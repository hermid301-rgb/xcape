using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;

namespace XCAPE.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        void Awake()
        {
            var gm = GameManager.Instance;
            if (resumeButton) resumeButton.onClick.AddListener(() => gm?.ResumeGame());
            if (restartButton) restartButton.onClick.AddListener(() => gm?.StartNewGame());
            if (quitButton) quitButton.onClick.AddListener(() => gm?.ReturnToMainMenu());
        }
    }
}
