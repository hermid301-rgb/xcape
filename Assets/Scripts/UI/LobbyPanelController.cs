using UnityEngine;
using UnityEngine.UI;
using XCAPE.Networking;

namespace XCAPE.UI
{
    public class LobbyPanelController : MonoBehaviour
    {
        [SerializeField] private InputField lobbyNameField;
        [SerializeField] private InputField joinCodeField;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text joinCodeText;

        private RelayLobbyManager manager;

        void Awake()
        {
            manager = FindObjectOfType<RelayLobbyManager>() ?? new GameObject("RelayLobbyManager").AddComponent<RelayLobbyManager>();
            createButton?.onClick.AddListener(() => manager.CreateLobbyAndHost(lobbyNameField?.text?.Trim().Length > 0 ? lobbyNameField.text.Trim() : "XCAPE", 2));
            joinButton?.onClick.AddListener(() => manager.JoinLobbyByCodeAndClient(joinCodeField?.text?.Trim()));
            leaveButton?.onClick.AddListener(() => manager.LeaveLobby());

            manager.OnStatus += s => { if (statusText) statusText.text = s; if (manager.JoinCode != null && joinCodeText) joinCodeText.text = $"Code: {manager.JoinCode}"; };
            manager.OnError += e => { if (statusText) statusText.text = e; };
        }
    }
}
