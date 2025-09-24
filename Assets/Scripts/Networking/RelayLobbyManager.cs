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
using Unity.Services.Multiplayer; // Nuevo paquete
// using Unity.Services.Lobbies;
// using Unity.Services.Lobbies.Models;
// using Unity.Services.Relay;
// using Unity.Services.Relay.Models;

namespace XCAPE.Networking
{
    public class RelayLobbyManager : MonoBehaviour
    {
        // ...existing code...
        // Comentamos los eventos y propiedades dependientes de Lobby/Relay
        // public event Action<string> OnStatus;
        // public event Action<Lobby> OnLobbyCreated;
        // public event Action<Lobby> OnLobbyJoined;
        // public event Action<Lobby> OnLobbyUpdated;
        // public event Action OnLobbyLeft;
        // public event Action<string> OnError;

        // public Lobby CurrentLobby { get; private set; }
        // public string JoinCode { get; private set; }

        // private Coroutine _heartbeat;
        // private Coroutine _poll;

        async void Awake()
        {
            await EnsureServicesAsync();
        }

        public async Task EnsureServicesAsync()
        {
            try
            {
                // Migración: inicializar Multiplayer Services
                if (Unity.Services.Multiplayer.MultiplayerService.Instance.State == MultiplayerServiceState.Uninitialized)
                {
                    await Unity.Services.Multiplayer.MultiplayerService.Instance.InitializeAsync();
                }
                // Autenticación y otros servicios se gestionan desde MultiplayerService
            }
            catch (Exception e)
            {
                Debug.LogError($"Multiplayer Services init/auth failed: {e.Message}");
            }
        }

        // TODO: Migrar métodos de lobby y relay a Multiplayer Services
        // public async void CreateLobbyAndHost(string lobbyName, int maxPlayers) { ... }
        // public async void JoinLobbyByCodeAndClient(string joinCode) { ... }
        // public async void LeaveLobby() { ... }
        // private IEnumerator Heartbeat() { ... }
        // private IEnumerator PollLobby() { ... }
        // public async void SetReady(bool ready) { ... }
        // public bool IsHost() { ... }
        // public async void StartGame() { ... }

        // ...existing code for Netcode transport setup remains unchanged...
    }
}
