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
    [Header("Style")]
    [SerializeField] private Color frameBg = new Color(1,1,1,0.05f);
    [SerializeField] private Color borderColor = new Color(1,1,1,0.25f);
    [SerializeField] private Color strikeColor = new Color(0.2f,1f,0.2f,1f);
    [SerializeField] private Color spareColor = new Color(0.2f,0.8f,1f,1f);

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
                    var c = InstantiateCell($"F{frame}R{r+1}");
                    c.GetComponentInParent<Image>().color = frameBg;
                    rollCells.Add(c);
                }
                // Total cell
                var totalCell = InstantiateCell($"F{frame}T");
                totalCell.GetComponentInParent<Image>().color = frameBg * 1.2f;
                frameTotals.Add(totalCell);
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
            var root = new GameObject("CellRoot");
            var bg = root.AddComponent<Image>();
            bg.color = new Color(1,1,1,0.08f);
            var go = new GameObject("Cell");
            go.transform.SetParent(root.transform, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.fontSize = 14;
            // Border (thin outline)
            var border = new GameObject("Border"); border.transform.SetParent(root.transform, false);
            var bi = border.AddComponent<Image>(); bi.color = borderColor;
            var rt = bi.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(-1,-1); rt.offsetMax = new Vector2(1,1);
            return root;
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
                            var t = rollCells[cellIdx++];
                            t.text = "X"; t.color = strikeColor;
                            rollCells[cellIdx++].text = "";
                            frameTotals[frame - 1].text = score.GetFrameScore(frame).ToString();
                            rollIdx += 1;
                        }
                        else
                        {
                            int a = rolls[rollIdx];
                            int b = (rollIdx + 1) < rolls.Count ? rolls[rollIdx + 1] : -1;
                            rollCells[cellIdx++].text = a.ToString();
                            var t = rollCells[cellIdx++];
                            if (b >= 0 && a + b == 10) { t.text = "/"; t.color = spareColor; } else { t.text = (b >= 0) ? b.ToString() : ""; t.color = Color.white; }
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
            if (val == 10) { rollCells[cellIdx].text = "X"; rollCells[cellIdx].color = strikeColor; }
                        else
                        {
                            // Check spare: needs previous in frame 10 context
                            if (r == 1 && rollCells[cellIdx - 1].text != "X" &&
                                int.TryParse(rollCells[cellIdx - 1].text, out var prev) && prev + val == 10)
                { rollCells[cellIdx].text = "/"; rollCells[cellIdx].color = spareColor; }
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
