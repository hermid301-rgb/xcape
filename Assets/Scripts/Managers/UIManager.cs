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

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject settingsPanel;

        private ScoreManager score;

        void Awake()
        {
            score = FindObjectOfType<ScoreManager>();
            if (score)
            {
                score.OnTotalUpdated += t => { if (totalText) totalText.text = $"Total: {t}"; };
                score.OnFrameUpdated += (f, _) => { if (frameText) frameText.text = $"Frame: {f}"; };
            }

            var gm = GameManager.Instance;
            if (gm)
            {
                gm.OnGameStateChanged += OnGameStateChanged;
            }

            ShowMainMenu();
        }

        public void UpdatePower(float v) { if (powerText) powerText.text = $"Power: {(int)(v*100)}%"; }
        public void UpdateSpin(float v) { if (spinText) spinText.text = $"Spin: {v:F2}"; }

        public void ShowMainMenu()
        {
            SetPanel(mainMenuPanel, true);
            SetPanel(hudPanel, false);
            SetPanel(pausePanel, false);
            SetPanel(gameOverPanel, false);
            SetPanel(settingsPanel, false);
        }

        public void ShowHUD()
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(hudPanel, true);
            SetPanel(pausePanel, false);
            SetPanel(gameOverPanel, false);
            // settingsPanel permanece bajo control del usuario
        }

        public void ShowPauseMenu()
        {
            SetPanel(pausePanel, true);
        }

        public void ShowGameOver()
        {
            SetPanel(gameOverPanel, true);
            SetPanel(hudPanel, false);
        }

        private void OnGameStateChanged(GameManager.GameState state)
        {
            switch (state)
            {
                case GameManager.GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameManager.GameState.Gameplay:
                    ShowHUD();
                    break;
                case GameManager.GameState.Paused:
                    ShowPauseMenu();
                    break;
                case GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        private void SetPanel(GameObject panel, bool enable)
        {
            if (panel) panel.SetActive(enable);
        }
    }
}
