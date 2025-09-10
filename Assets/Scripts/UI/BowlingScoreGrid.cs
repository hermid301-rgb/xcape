using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;

namespace XCAPE.UI
{
    public class BowlingScoreGrid : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private GameObject cellPrefab; // simple Text cell

        private readonly List<Text> rollCells = new(); // 21 cells (2*9 + 3)
        private readonly List<Text> frameTotals = new(); // 10 cells
        private ScoreManager score;

        void Awake()
        {
            score = FindObjectOfType<ScoreManager>();
            BuildIfNeeded();
            if (score != null)
            {
                score.OnTotalUpdated += _ => Refresh();
                score.OnFrameUpdated += (_, __) => Refresh();
            }
        }

        private void BuildIfNeeded()
        {
            if (grid == null)
            {
                var go = new GameObject("ScoreGrid");
                go.transform.SetParent(transform, false);
                grid = go.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(48, 28);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 33; // 3 columns per frame + spacer
                grid.spacing = new Vector2(2, 2);
            }
            if (cellPrefab == null)
            {
                cellPrefab = CreateDefaultCellPrefab();
            }

            rollCells.Clear(); frameTotals.Clear();
            for (int frame = 1; frame <= 10; frame++)
            {
                // Rolls
                int rollsInFrame = (frame == 10) ? 3 : 2;
                for (int r = 0; r < rollsInFrame; r++)
                {
                    rollCells.Add(InstantiateCell($"F{frame}R{r+1}"));
                }
                // Total cell
                frameTotals.Add(InstantiateCell($"F{frame}T"));
                // Spacer
                InstantiateCell("Spacer").text = "";
            }
        }

        private Text InstantiateCell(string name)
        {
            var go = Instantiate(cellPrefab, grid.transform);
            go.name = name;
            return go.GetComponent<Text>();
        }

        private GameObject CreateDefaultCellPrefab()
        {
            var go = new GameObject("Cell");
            var img = go.AddComponent<Image>();
            img.color = new Color(1,1,1,0.08f);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 14;
            return go;
        }

        void OnEnable() => Refresh();

        public void Refresh()
        {
            if (score == null) return;
            var rolls = score.GetRolls();
            int rollIdx = 0;
            int cellIdx = 0;

            // Clear all
            foreach (var c in rollCells) c.text = "";
            foreach (var t in frameTotals) t.text = "";

            for (int frame = 1; frame <= 10; frame++)
            {
                if (frame < 10)
                {
                    if (rollIdx < rolls.Count)
                    {
                        if (rolls[rollIdx] == 10)
                        {
                            rollCells[cellIdx++].text = "X"; // strike
                            rollCells[cellIdx++].text = "";
                            frameTotals[frame - 1].text = score.GetFrameScore(frame).ToString();
                            rollIdx += 1;
                        }
                        else
                        {
                            int a = rolls[rollIdx];
                            int b = (rollIdx + 1) < rolls.Count ? rolls[rollIdx + 1] : -1;
                            rollCells[cellIdx++].text = a.ToString();
                            rollCells[cellIdx++].text = (b >= 0) ? ((a + b == 10) ? "/" : b.ToString()) : "";
                            frameTotals[frame - 1].text = score.GetFrameScore(frame).ToString();
                            rollIdx += 2;
                        }
                    }
                    else
                    {
                        cellIdx += 2;
                    }
                    cellIdx++; // skip total cell already assigned by index
                }
                else
                {
                    // 10th frame up to 3 rolls
                    for (int r = 0; r < 3; r++)
                    {
                        if (rollIdx >= rolls.Count) { cellIdx++; continue; }
                        int val = rolls[rollIdx++];
                        if (val == 10) rollCells[cellIdx].text = "X";
                        else
                        {
                            // Check spare: needs previous in frame 10 context
                            if (r == 1 && rollCells[cellIdx - 1].text != "X" &&
                                int.TryParse(rollCells[cellIdx - 1].text, out var prev) && prev + val == 10)
                                rollCells[cellIdx].text = "/";
                            else rollCells[cellIdx].text = val.ToString();
                        }
                        cellIdx++;
                    }
                    frameTotals[frame - 1].text = score.GetFrameScore(frame).ToString();
                    cellIdx++; // spacer
                }
            }
        }
    }
}
