using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#endif
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace XCAPE.Networking
{
    public class RelayLobbyManager : MonoBehaviour
    {
        public event Action<string> OnStatus;
        public event Action<Lobby> OnLobbyCreated;
        public event Action<Lobby> OnLobbyJoined;
    public event Action<Lobby> OnLobbyUpdated;
        public event Action OnLobbyLeft;
        public event Action<string> OnError;

        public Lobby CurrentLobby { get; private set; }
        public string JoinCode { get; private set; }

    private Coroutine _heartbeat;
    private Coroutine _poll;

        async void Awake()
        {
            await EnsureServicesAsync();
        }

        public async Task EnsureServicesAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Application.cloudProjectId))
                {
                    OnError?.Invoke("UGS: Proyecto no vinculado (Application.cloudProjectId vacío). Abre Tools > XCAPE > Setup Wizard > Link to UGS.");
                    return;
                }
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    OnStatus?.Invoke($"Signed in: {AuthenticationService.Instance.PlayerId}");
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke($"Services init/auth failed: {e.Message}");
            }
        }

        public async void CreateLobbyAndHost(string lobbyName, int maxPlayers)
        {
            try
            {
                await EnsureServicesAsync();

                // Allocate Relay for host
                Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                JoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

#if UNITY_NETCODE_GAMEOBJECTS
                ConfigureTransportAsHost(alloc);
                StartHost();
#endif
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new System.Collections.Generic.Dictionary<string, DataObject>
                    {
                        {"joinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode)}
                    }
                };
                CurrentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                OnLobbyCreated?.Invoke(CurrentLobby);
                OnStatus?.Invoke($"Lobby created. JoinCode: {JoinCode}");

                _heartbeat = StartCoroutine(Heartbeat());
                _poll = StartCoroutine(PollLobby());
            }
            catch (Exception e)
            {
                OnError?.Invoke($"CreateLobby failed: {e.Message}");
            }
        }

        public async void JoinLobbyByCodeAndClient(string joinCode)
        {
            try
            {
                await EnsureServicesAsync();

                // Fetch Relay join data and start client first for faster connection
                JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(joinCode);
#if UNITY_NETCODE_GAMEOBJECTS
                ConfigureTransportAsClient(join);
                StartClient();
#endif
                var lr = await Lobbies.Instance.JoinLobbyByCodeAsync(joinCode);
                CurrentLobby = lr;
                OnLobbyJoined?.Invoke(CurrentLobby);
                OnStatus?.Invoke("Joined lobby");
                _poll = StartCoroutine(PollLobby());
            }
            catch (Exception e)
            {
                OnError?.Invoke($"JoinLobby failed: {e.Message}");
            }
        }

        public async void LeaveLobby()
        {
            try
            {
                if (CurrentLobby != null)
                {
                    await Lobbies.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                }
            }
            catch { }
            finally
            {
                if (_heartbeat != null) StopCoroutine(_heartbeat);
                _heartbeat = null;
                if (_poll != null) StopCoroutine(_poll);
                _poll = null;
                CurrentLobby = null;
                OnLobbyLeft?.Invoke();
            }
        }

        private IEnumerator Heartbeat()
        {
            var wait = new WaitForSecondsRealtime(15f);
            while (CurrentLobby != null)
            {
                var task = Lobbies.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                task.ContinueWith(t => { if (t.IsFaulted && t.Exception != null) OnError?.Invoke($"Heartbeat failed: {t.Exception.Message}"); });
                yield return wait;
            }
        }

        private IEnumerator PollLobby()
        {
            var wait = new WaitForSecondsRealtime(1.75f);
            while (CurrentLobby != null)
            {
                var task = Lobbies.Instance.GetLobbyAsync(CurrentLobby.Id);
                yield return new WaitUntil(() => task.IsCompleted);
                if (task.IsFaulted)
                {
                    var ex = task.Exception;
                    OnError?.Invoke($"Lobby poll failed: {ex?.Message}");
                    // If the lobby no longer exists or player removed, leave
                    LeaveLobby();
                    yield break;
                }
                CurrentLobby = task.Result;
                OnLobbyUpdated?.Invoke(CurrentLobby);
                // Detect start flag
                if (CurrentLobby.Data != null && CurrentLobby.Data.TryGetValue("started", out var started) && started.Value == "1")
                {
#if UNITY_NETCODE_GAMEOBJECTS
                    // Optionally, perform a scene transition here if needed
                    if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsClient)
                    {
                        // Clients can just rely on host scene management; nothing to do here
                    }
#endif
                }
                yield return wait;
            }
        }

        public async void SetReady(bool ready)
        {
            try
            {
                if (CurrentLobby == null) return;
                var data = new Dictionary<string, PlayerDataObject>
                {
                    { "ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ready ? "1" : "0") }
                };
                await Lobbies.Instance.UpdatePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions { Data = data });
                OnStatus?.Invoke($"Ready: {(ready ? "Yes" : "No")}");
            }
            catch (Exception e)
            {
                OnError?.Invoke($"SetReady failed: {e.Message}");
            }
        }

        public bool IsHost()
        {
            try
            {
                return CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
            }
            catch { return false; }
        }

        public async void StartGame()
        {
            try
            {
                if (CurrentLobby == null) return;
                if (!IsHost()) { OnStatus?.Invoke("Only host can start"); return; }
                var lobbyData = new Dictionary<string, DataObject>
                {
                    { "started", new DataObject(DataObject.VisibilityOptions.Public, "1") }
                };
                var options = new UpdateLobbyOptions { Data = lobbyData };
                CurrentLobby = await Lobbies.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);
                OnStatus?.Invoke("Game starting...");
#if UNITY_NETCODE_GAMEOBJECTS
                // Host can transition to GamePlay scene and use Netcode SceneManager if desired
                if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsHost)
                {
                    var sceneName = "GamePlay";
                    if (Unity.Netcode.NetworkManager.Singleton.SceneManager != null)
                        Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                    else
                        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                }
#endif
            }
            catch (Exception e)
            {
                OnError?.Invoke($"StartGame failed: {e.Message}");
            }
        }

#if UNITY_NETCODE_GAMEOBJECTS
        private void ConfigureTransportAsHost(Allocation alloc)
        {
            var utp = FindObjectOfType<UnityTransport>() ?? new GameObject("UnityTransport").AddComponent<UnityTransport>();
            utp.SetRelayServerData(new RelayServerData(alloc, "dtls"));
        }

        private void ConfigureTransportAsClient(JoinAllocation join)
        {
            var utp = FindObjectOfType<UnityTransport>() ?? new GameObject("UnityTransport").AddComponent<UnityTransport>();
            utp.SetRelayServerData(new RelayServerData(join, "dtls"));
        }

        private void StartHost()
        {
            if (NetworkManager.Singleton == null)
            {
                var nm = new GameObject("NetworkManager").AddComponent<NetworkManager>();
                nm.gameObject.AddComponent<UnityTransport>();
            }
            NetworkManager.Singleton.StartHost();
        }

        private void StartClient()
        {
            if (NetworkManager.Singleton == null)
            {
                var nm = new GameObject("NetworkManager").AddComponent<NetworkManager>();
                nm.gameObject.AddComponent<UnityTransport>();
            }
            NetworkManager.Singleton.StartClient();
        }
#endif
    }
}
