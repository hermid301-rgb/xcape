using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;

namespace XCAPE.UI
{
    public class GameOverPanelController : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text strikesText;
        [SerializeField] private Text sparesText;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            if (playAgainButton) playAgainButton.onClick.AddListener(PlayAgain);
            if (menuButton) menuButton.onClick.AddListener(ReturnToMenu);
        }

        void OnEnable()
        {
            PopulateSummary();
        }

        private void PopulateSummary()
        {
            var gm = GameManager.Instance;
            var sm = gm?.ScoreManager ?? Object.FindObjectOfType<ScoreManager>();
            if (sm == null) return;

            int total = sm.GetTotalScore();
            int high = gm != null ? gm.GetHighScore() : total;
            int strikes = 0, spares = 0;
            var rolls = sm.GetRolls();
            int idx = 0;
            for (int frame = 1; frame <= ScoreManager.FramesPerGame && idx < rolls.Count; frame++)
            {
                if (rolls[idx] == 10)
                {
                    strikes++; idx += (frame == 10 ? 1 : 1);
                    if (frame == 10)
                    {
                        // consume dos más si existen
                        idx = Mathf.Min(idx + 2, rolls.Count);
                    }
                }
                else
                {
                    int a = rolls[idx];
                    int b = (idx + 1) < rolls.Count ? rolls[idx + 1] : 0;
                    if (a + b == 10) spares++;
                    idx += (frame == 10 && a + b >= 10) ? 3 : 2;
                }
            }

            if (finalScoreText) finalScoreText.text = $"Score: {total}";
            if (highScoreText) highScoreText.text = $"High: {high}";
            if (strikesText) strikesText.text = $"Strikes: {strikes}";
            if (sparesText) sparesText.text = $"Spares: {spares}";
        }

        private void PlayAgain()
        {
            var gm = GameManager.Instance;
            gm?.StartNewGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
        }

        private void ReturnToMenu()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }
    }
}
