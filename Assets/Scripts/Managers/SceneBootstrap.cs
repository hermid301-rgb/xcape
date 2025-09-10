using UnityEngine;

namespace XCAPE.Core
{
    // Ensures a GameManager exists when entering any scene directly
    public class SceneBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureManagers()
        {
            if (FindObjectOfType<GameManager>() == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
                Object.DontDestroyOnLoad(go);
            }
        }
    }
}
