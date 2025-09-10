using UnityEngine;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

namespace XCAPE.Networking
{
    public class NetcodeBootstrap : MonoBehaviour
    {
        public void StartHost()
        {
#if UNITY_NETCODE_GAMEOBJECTS
            if (!EnsureNetworkManager()) return;
            NetworkManager.Singleton.StartHost();
            Debug.Log("[Netcode] Host started");
#else
            Debug.LogWarning("Netcode package not compiled. Install com.unity.netcode.gameobjects.");
#endif
        }

        public void StartClient()
        {
#if UNITY_NETCODE_GAMEOBJECTS
            if (!EnsureNetworkManager()) return;
            NetworkManager.Singleton.StartClient();
            Debug.Log("[Netcode] Client started");
#else
            Debug.LogWarning("Netcode package not compiled. Install com.unity.netcode.gameobjects.");
#endif
        }

#if UNITY_NETCODE_GAMEOBJECTS
        private bool EnsureNetworkManager()
        {
            if (NetworkManager.Singleton == null)
            {
                var go = new GameObject("NetworkManager");
                var nm = go.AddComponent<NetworkManager>();
                go.AddComponent<UnityTransport>();
            }
            return NetworkManager.Singleton != null;
        }
#endif
    }
}
