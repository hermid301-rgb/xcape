using UnityEngine;

namespace XCAPE.Core
{
    public class AnalyticsManager : MonoBehaviour
    {
        public void Initialize() { Debug.Log("Analytics initialized"); }
        public void LogEvent(string name) { Debug.Log($"Event: {name}"); }
        public void LogEvent(string name, string key, object value) { Debug.Log($"Event: {name} {key}={value}"); }
    }
}
