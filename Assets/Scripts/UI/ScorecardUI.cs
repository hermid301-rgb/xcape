using System.Text;
using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;

namespace XCAPE.UI
{
    public class ScorecardUI : MonoBehaviour
    {
        [SerializeField] private Text scorecardText;
        private ScoreManager score;

        void Awake()
        {
            score = FindObjectOfType<ScoreManager>();
            if (score != null)
            {
                score.OnTotalUpdated += _ => Refresh();
                score.OnFrameUpdated += (_, __) => Refresh();
            }
        }

        void OnEnable() => Refresh();

        public void Refresh()
        {
            if (scorecardText == null || score == null) return;
            var rolls = score.GetRolls();
            var sb = new StringBuilder();
            int rollIndex = 0;
            for (int frame = 1; frame <= ScoreManager.FramesPerGame; frame++)
            {
                if (rollIndex >= rolls.Count)
                {
                    sb.AppendLine($"F{frame:00}: - | -   (Total: {score.GetFrameScore(frame)})");
                    continue;
                }

                if (rolls[rollIndex] == 10) // strike
                {
                    sb.AppendLine($"F{frame:00}: X |   (Total: {score.GetFrameScore(frame)})");
                    rollIndex += 1;
                }
                else
                {
                    int a = rolls[rollIndex];
                    int b = (rollIndex + 1) < rolls.Count ? rolls[rollIndex + 1] : -1;
                    string second = b >= 0 ? ((a + b == 10) ? "/" : b.ToString()) : "-";
                    sb.AppendLine($"F{frame:00}: {a} | {second}   (Total: {score.GetFrameScore(frame)})");
                    rollIndex += 2;
                }
            }
            scorecardText.text = sb.ToString();
        }
    }
}
