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
    [SerializeField] private RectTransform playersListRoot;
    [SerializeField] private Toggle readyToggle;
    [SerializeField] private Button startButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text joinCodeText;

        private RelayLobbyManager manager;
    private GameObject playerRowPrefab;

        void Awake()
        {
            manager = FindObjectOfType<RelayLobbyManager>() ?? new GameObject("RelayLobbyManager").AddComponent<RelayLobbyManager>();
            createButton?.onClick.AddListener(() => manager.CreateLobbyAndHost(lobbyNameField?.text?.Trim().Length > 0 ? lobbyNameField.text.Trim() : "XCAPE", 2));
            joinButton?.onClick.AddListener(() => manager.JoinLobbyByCodeAndClient(joinCodeField?.text?.Trim()));
            leaveButton?.onClick.AddListener(() => manager.LeaveLobby());
            readyToggle?.onValueChanged.AddListener(v => manager.SetReady(v));
            startButton?.onClick.AddListener(() => manager.StartGame());

            manager.OnStatus += s => { if (statusText) statusText.text = s; if (!string.IsNullOrEmpty(manager.JoinCode) && joinCodeText) joinCodeText.text = $"Code: {manager.JoinCode}"; };
            manager.OnError += e => { if (statusText) statusText.text = e; };
            manager.OnLobbyJoined += _ => RefreshPlayers();
            manager.OnLobbyUpdated += _ => RefreshPlayers();
            manager.OnLobbyLeft += () => ClearPlayers();

            // If the project is not linked to UGS, show a clear warning and disable interactions
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                if (statusText) statusText.text = "UGS no vinculado. Abre Tools > XCAPE > Setup Wizard y vincula el proyecto.";
                SetInteractable(false);
            }
            else
            {
                SetInteractable(true);
            }
        }

        private void SetInteractable(bool enabled)
        {
            if (createButton) createButton.interactable = enabled;
            if (joinButton) joinButton.interactable = enabled;
            if (leaveButton) leaveButton.interactable = enabled;
            if (readyToggle) readyToggle.interactable = enabled;
            if (startButton) startButton.interactable = enabled && manager != null && manager.IsHost();
            if (joinCodeField) joinCodeField.interactable = enabled;
            if (lobbyNameField) lobbyNameField.interactable = enabled;
        }

        private void ClearPlayers()
        {
            if (playersListRoot == null) return;
            for (int i = playersListRoot.childCount - 1; i >= 0; i--)
                Destroy(playersListRoot.GetChild(i).gameObject);
            if (startButton) startButton.interactable = false;
        }

        private void RefreshPlayers()
        {
            if (playersListRoot == null || manager.CurrentLobby == null) return;
            ClearPlayers();
            bool allReady = true;
            foreach (var p in manager.CurrentLobby.Players)
            {
                var row = CreatePlayerRow(playersListRoot);
                var name = p.Id;
                string ready = "0";
                if (p.Data != null && p.Data.TryGetValue("ready", out var r)) ready = r.Value;
                var text = row.GetComponentInChildren<Text>();
                text.text = (p.Id == manager.CurrentLobby.HostId ? "[HOST] " : "") + name + (ready == "1" ? "  ✅" : "  ⏳");
                if (ready != "1") allReady = false;
            }
            if (startButton)
            {
                startButton.interactable = manager.IsHost() && manager.CurrentLobby != null && manager.CurrentLobby.Players.Count > 0 && allReady;
            }
        }

        private GameObject CreatePlayerRow(Transform parent)
        {
            var go = new GameObject("PlayerRow");
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>(); bg.color = new Color(1,1,1,0.04f);
            var rt = bg.rectTransform; rt.sizeDelta = new Vector2(0, 34);
            var txtGO = new GameObject("Text"); txtGO.transform.SetParent(go.transform, false);
            var txt = txtGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 16; txt.alignment = TextAnchor.MiddleLeft; txt.color = Color.white;
            var trt = txt.rectTransform; trt.anchorMin = new Vector2(0,0); trt.anchorMax = new Vector2(1,1); trt.offsetMin = new Vector2(8,0); trt.offsetMax = new Vector2(-8,0);
            return go;
        }
    }
}
