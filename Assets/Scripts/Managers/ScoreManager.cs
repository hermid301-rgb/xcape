using System;
using System.Collections.Generic;
using UnityEngine;

namespace XCAPE.Core
{
    /// <summary>
    /// Sistema de puntuación oficial de boliche (10 frames, strikes, spares)
    /// API:
    /// - StartNewGame()
    /// - RecordRoll(pinsKnocked)
    /// - GetFrameScore(frameIndex), GetCumulativeScore(frameIndex), GetTotalScore()
    /// - IsGameOver, CurrentFrame, IsFirstRoll
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public const int FramesPerGame = 10;
        private List<int> rolls = new List<int>(21); // máx 21 tiros

        public int CurrentFrame { get; private set; } = 1;
        public bool IsFirstRoll { get; private set; } = true;
        public bool IsGameOver { get; private set; } = false;

        public event Action<int, int> OnFrameUpdated; // frameIndex, frameScore
        public event Action<int> OnTotalUpdated; // total
        public event Action OnGameCompleted;

        public void StartNewGame()
        {
            rolls.Clear();
            CurrentFrame = 1;
            IsFirstRoll = true;
            IsGameOver = false;
            NotifyTotals();
        }

        public void ResetGame() => StartNewGame();

        public void RecordRoll(int pinsKnocked)
        {
            if (IsGameOver) return;
            pinsKnocked = Mathf.Clamp(pinsKnocked, 0, 10);
            rolls.Add(pinsKnocked);

            // Avance de frame/roll segun reglas
            if (CurrentFrame < FramesPerGame)
            {
                if (IsFirstRoll)
                {
                    if (pinsKnocked == 10) // strike
                    {
                        AdvanceFrame();
                    }
                    else
                    {
                        IsFirstRoll = false;
                    }
                }
                else
                {
                    AdvanceFrame();
                }
            }
            else
            {
                // Frame 10 tiene reglas especiales
                HandleTenthFrameProgress();
            }

            NotifyTotals();
        }

        private void AdvanceFrame()
        {
            CurrentFrame++;
            IsFirstRoll = true;
            if (CurrentFrame > FramesPerGame)
            {
                CurrentFrame = FramesPerGame;
                // Juego puede no haber terminado aún si faltan tiros del frame 10, se maneja aparte
            }
        }

        private void HandleTenthFrameProgress()
        {
            // Para evaluar si terminar, contamos tiros usados en frame 10
            int startIndex = GetRollIndexForFrame(FramesPerGame);
            int rollsInTenth = rolls.Count - startIndex;
            if (rollsInTenth < 2)
            {
                // Aún falta segundo tiro
                return;
            }

            int first = rolls[startIndex];
            int second = rolls[startIndex + 1];
            bool strike = first == 10;
            bool spare = !strike && (first + second == 10);

            if (strike || spare)
            {
                // Se permite un tercer tiro
                if (rollsInTenth >= 3)
                {
                    EndGame();
                }
            }
            else
            {
                // Sin strike/spare, termina al segundo tiro
                EndGame();
            }
        }

        private void EndGame()
        {
            IsGameOver = true;
            OnGameCompleted?.Invoke();
        }

        // Scoring oficial
        public int GetTotalScore()
        {
            int score = 0;
            int rollIndex = 0;
            for (int frame = 1; frame <= FramesPerGame; frame++)
            {
                if (rollIndex >= rolls.Count) break;
                if (IsStrike(rollIndex))
                {
                    score += 10 + StrikeBonus(rollIndex);
                    rollIndex += 1;
                }
                else if (IsSpare(rollIndex))
                {
                    score += 10 + SpareBonus(rollIndex);
                    rollIndex += 2;
                }
                else
                {
                    score += SumOfBallsInFrame(rollIndex);
                    rollIndex += 2;
                }
            }
            return score;
        }

        public int GetFrameScore(int frameIndex)
        {
            frameIndex = Mathf.Clamp(frameIndex, 1, FramesPerGame);
            int score = 0;
            int rollIndex = 0;
            for (int frame = 1; frame <= frameIndex; frame++)
            {
                if (rollIndex >= rolls.Count) break;
                if (IsStrike(rollIndex))
                {
                    score += 10 + StrikeBonus(rollIndex);
                    rollIndex += 1;
                }
                else if (IsSpare(rollIndex))
                {
                    score += 10 + SpareBonus(rollIndex);
                    rollIndex += 2;
                }
                else
                {
                    score += SumOfBallsInFrame(rollIndex);
                    rollIndex += 2;
                }
            }
            return score;
        }

        public int GetCumulativeScore(int frameIndex) => GetFrameScore(frameIndex);

        private bool IsStrike(int rollIndex)
        {
            return rollIndex < rolls.Count && rolls[rollIndex] == 10;
        }

        private bool IsSpare(int rollIndex)
        {
            return (rollIndex + 1) < rolls.Count && (rolls[rollIndex] + rolls[rollIndex + 1] == 10);
        }

        private int StrikeBonus(int rollIndex)
        {
            int b1 = (rollIndex + 1) < rolls.Count ? rolls[rollIndex + 1] : 0;
            int b2 = (rollIndex + 2) < rolls.Count ? rolls[rollIndex + 2] : 0;
            return b1 + b2;
        }

        private int SpareBonus(int rollIndex)
        {
            int b = (rollIndex + 2) < rolls.Count ? rolls[rollIndex + 2] : 0;
            return b;
        }

        private int SumOfBallsInFrame(int rollIndex)
        {
            int a = rollIndex < rolls.Count ? rolls[rollIndex] : 0;
            int b = (rollIndex + 1) < rolls.Count ? rolls[rollIndex + 1] : 0;
            return a + b;
        }

        private int GetRollIndexForFrame(int frame)
        {
            int rollIndex = 0;
            for (int f = 1; f < frame && rollIndex < rolls.Count; f++)
            {
                if (IsStrike(rollIndex)) rollIndex += 1;
                else rollIndex += 2;
            }
            return rollIndex;
        }

        private void NotifyTotals()
        {
            int total = GetTotalScore();
            OnTotalUpdated?.Invoke(total);
            OnFrameUpdated?.Invoke(CurrentFrame, GetFrameScore(CurrentFrame));
        }

        // Utilidad para UI
        public IReadOnlyList<int> GetRolls() => rolls;
    }
}
