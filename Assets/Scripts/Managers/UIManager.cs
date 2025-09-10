using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;
using XCAPE.Gameplay;

namespace XCAPE.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Text powerText;
        [SerializeField] private Text spinText;
        [SerializeField] private Text frameText;
        [SerializeField] private Text totalText;

        private ScoreManager score;

        void Awake()
        {
            score = FindObjectOfType<ScoreManager>();
            if (score)
            {
                score.OnTotalUpdated += t => { if (totalText) totalText.text = $"Total: {t}"; };
                score.OnFrameUpdated += (f, _) => { if (frameText) frameText.text = $"Frame: {f}"; };
            }
        }

        public void UpdatePower(float v) { if (powerText) powerText.text = $"Power: {(int)(v*100)}%"; }
        public void UpdateSpin(float v) { if (spinText) spinText.text = $"Spin: {v:F2}"; }

        public void ShowMainMenu() { }
        public void ShowHUD() { }
        public void ShowPauseMenu() { }
        public void ShowGameOver() { }
    }
}
