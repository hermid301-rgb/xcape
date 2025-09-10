using UnityEngine;

namespace XCAPE.Core
{
    /// <summary>
    /// Minimal scoring manager placeholder. Full official rules in Phase 2.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private int total;
        public void ResetGame() { total = 0; }
        public void AddPoints(int p) { total += Mathf.Max(0, p); }
        public int GetTotalScore() => total;
    }
}
