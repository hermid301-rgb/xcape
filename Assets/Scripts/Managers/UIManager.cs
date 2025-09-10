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
    [Header("Score Grid (optional)")]
    [SerializeField] private GameObject scoreGrid;

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
            Show(mainMenuPanel);
            Hide(hudPanel);
            Hide(pausePanel);
            Hide(gameOverPanel);
            Hide(settingsPanel);
        }

        public void ShowHUD()
        {
            Hide(mainMenuPanel);
            Show(hudPanel);
            Hide(pausePanel);
            Hide(gameOverPanel);
            // settingsPanel permanece bajo control del usuario
        }

        public void ShowPauseMenu()
        {
            Show(pausePanel);
        }

        public void ShowGameOver()
        {
            Show(gameOverPanel);
            Hide(hudPanel);
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

        private void Show(GameObject panel) => Transition(panel, true);
        private void Hide(GameObject panel) => Transition(panel, false);

        private void Transition(GameObject panel, bool show)
        {
            if (!panel) return;
            var t = panel.GetComponent<XCAPE.UI.PanelTransition>() ?? panel.AddComponent<XCAPE.UI.PanelTransition>();
            if (show) t.Show(); else t.Hide();
        }
    }
}
