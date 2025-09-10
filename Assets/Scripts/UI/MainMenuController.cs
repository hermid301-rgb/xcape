using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;

namespace XCAPE.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private GameManager gm;

        void Awake()
        {
            gm = GameManager.Instance;
            if (playButton) playButton.onClick.AddListener(Play);
            if (settingsButton) settingsButton.onClick.AddListener(ToggleSettings);
            if (quitButton) quitButton.onClick.AddListener(Quit);
            if (settingsPanel) settingsPanel.SetActive(false);
        }

        public void Play()
        {
            gm?.StartNewGame();
            // Asumimos que la escena de gameplay ya está configurada en Build Settings (índice 1)
            UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
        }

        public void ToggleSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        public void Quit()
        {
            gm?.QuitGame();
        }
    }
}
