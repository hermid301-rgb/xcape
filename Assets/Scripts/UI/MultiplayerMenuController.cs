using UnityEngine;
using UnityEngine.UI;
using XCAPE.Networking;

namespace XCAPE.UI
{
    public class MultiplayerMenuController : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private GameObject statusTextGO;
        private Text statusText;
        private NetcodeBootstrap net;

        void Awake()
        {
            if (statusTextGO) statusText = statusTextGO.GetComponent<Text>();
            net = FindObjectOfType<NetcodeBootstrap>();
            if (hostButton) hostButton.onClick.AddListener(() => { statusText?.SetText("Starting Host..."); net?.StartHost(); });
            if (joinButton) joinButton.onClick.AddListener(() => { statusText?.SetText("Starting Client..."); net?.StartClient(); });
        }
    }
}
