using System;
using System.Collections;
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
        public event Action OnLobbyLeft;
        public event Action<string> OnError;

        public Lobby CurrentLobby { get; private set; }
        public string JoinCode { get; private set; }

        private Coroutine _heartbeat;

        async void Awake()
        {
            await EnsureServicesAsync();
        }

        public async Task EnsureServicesAsync()
        {
            try
            {
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
