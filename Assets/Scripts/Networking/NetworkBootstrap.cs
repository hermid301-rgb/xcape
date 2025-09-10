#if UNITY_NETCODE
using Unity.Netcode;
#endif
using UnityEngine;

namespace XCAPE.Core
{
    public class NetworkBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
#if UNITY_NETCODE
            Debug.Log("Netcode present");
#else
            Debug.Log("Netcode not enabled yet");
#endif
        }
    }
}
