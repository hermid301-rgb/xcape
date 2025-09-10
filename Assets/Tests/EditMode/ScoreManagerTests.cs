using NUnit.Framework;
using UnityEngine;
using XCAPE.Core;

namespace XCAPE.Tests
{
    /// <summary>
    /// Tests unitarios para ScoreManager
    /// Valida scoring oficial de bowling incluyendo strikes, spares y frame 10
    /// </summary>
    public class ScoreManagerTests
    {
        private GameObject gameObject;
        private ScoreManager scoreManager;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("TestScoreManager");
            scoreManager = gameObject.AddComponent<ScoreManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (gameObject != null)
                Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void TestPerfectGame300()
        {
            // 12 strikes = 300 puntos
            for (int i = 0; i < 12; i++)
            {
                scoreManager.RecordRoll(10);
            }

            Assert.AreEqual(300, scoreManager.GetTotalScore(), "Perfect game should score 300");
            Assert.IsTrue(scoreManager.IsGameOver, "Game should be over after perfect game");
        }

        [Test]
        public void TestAllSpares150()
        {
            // 10 frames de spare (5+5) + bonus final 5 = 150
            for (int frame = 0; frame < 10; frame++)
            {
                scoreManager.RecordRoll(5);
                scoreManager.RecordRoll(5);
            }
            scoreManager.RecordRoll(5); // Bonus del último spare

            Assert.AreEqual(150, scoreManager.GetTotalScore(), "All spares should score 150");
        }

        [Test]
        public void TestGutterGame()
        {
            // 20 tiradas de 0 pinos
            for (int i = 0; i < 20; i++)
            {
                scoreManager.RecordRoll(0);
            }

            Assert.AreEqual(0, scoreManager.GetTotalScore(), "Gutter game should score 0");
            Assert.IsTrue(scoreManager.IsGameOver, "Game should be over after 20 rolls");
        }

        [Test]
        public void TestMixedGame()
        {
            // Frame 1: Strike (10 + next 2 rolls)
            scoreManager.RecordRoll(10);
            
            // Frame 2: Spare 7+3 (10 + next 1 roll)
            scoreManager.RecordRoll(7);
            scoreManager.RecordRoll(3);
            
            // Frame 3: Normal 4+2
            scoreManager.RecordRoll(4);
            scoreManager.RecordRoll(2);
            
            // Frames 4-9: Normal scoring
            for (int frame = 4; frame <= 9; frame++)
            {
                scoreManager.RecordRoll(3);
                scoreManager.RecordRoll(4);
            }
            
            // Frame 10: Normal 2+3
            scoreManager.RecordRoll(2);
            scoreManager.RecordRoll(3);

            // Frame 1: 10 + 7 + 3 = 20
            // Frame 2: 7 + 3 + 4 = 14
            // Frame 3: 4 + 2 = 6
            // Frames 4-9: 7 * 6 = 42
            // Frame 10: 2 + 3 = 5
            // Total: 20 + 14 + 6 + 42 + 5 = 87

            Assert.AreEqual(87, scoreManager.GetTotalScore(), "Mixed game should score 87");
        }

        [Test]
        public void TestFrame10Scenarios()
        {
            // Setup: 9 frames normales (3+4 cada uno)
            for (int frame = 1; frame <= 9; frame++)
            {
                scoreManager.RecordRoll(3);
                scoreManager.RecordRoll(4);
            }

            // Scenario A: Frame 10 strike
            var tempGameObject = new GameObject("TempScoreManager");
            var tempScoreManager = tempGameObject.AddComponent<ScoreManager>();
            
            // Replicate 9 frames
            for (int frame = 1; frame <= 9; frame++)
            {
                tempScoreManager.RecordRoll(3);
                tempScoreManager.RecordRoll(4);
            }
            
            // Frame 10: Strike + 2 bonus rolls
            tempScoreManager.RecordRoll(10);
            tempScoreManager.RecordRoll(5);
            tempScoreManager.RecordRoll(3);
            
            // 9 frames * 7 + frame 10 (10+5+3) = 63 + 18 = 81
            Assert.AreEqual(81, tempScoreManager.GetTotalScore(), "Frame 10 strike scenario");
            Assert.IsTrue(tempScoreManager.IsGameOver, "Game should be over");
            
            Object.DestroyImmediate(tempGameObject);
        }

        [Test]
        public void TestStrikeBonus()
        {
            // Strike en frame 1
            scoreManager.RecordRoll(10);
            
            // Frame 2: 4+3
            scoreManager.RecordRoll(4);
            scoreManager.RecordRoll(3);
            
            // El frame 1 debe valer 10 + 4 + 3 = 17
            Assert.AreEqual(17, scoreManager.GetFrameScore(1), "Strike bonus should be next 2 rolls");
        }

        [Test]
        public void TestSpareBonus()
        {
            // Spare en frame 1: 6+4
            scoreManager.RecordRoll(6);
            scoreManager.RecordRoll(4);
            
            // Frame 2: primer roll 5
            scoreManager.RecordRoll(5);
            
            // El frame 1 debe valer 6 + 4 + 5 = 15
            Assert.AreEqual(15, scoreManager.GetFrameScore(1), "Spare bonus should be next 1 roll");
        }

        [Test]
        public void TestGameStateProgression()
        {
            Assert.IsFalse(scoreManager.IsGameOver, "Game should not be over at start");
            Assert.AreEqual(1, scoreManager.CurrentFrame, "Should start at frame 1");
            Assert.IsTrue(scoreManager.IsFirstRoll, "Should start with first roll");
            
            // First roll
            scoreManager.RecordRoll(5);
            Assert.IsFalse(scoreManager.IsFirstRoll, "Should be second roll after first");
            
            // Second roll
            scoreManager.RecordRoll(3);
            Assert.AreEqual(2, scoreManager.CurrentFrame, "Should advance to frame 2");
            Assert.IsTrue(scoreManager.IsFirstRoll, "Should be first roll of new frame");
        }

        [Test]
        public void TestInvalidRollHandling()
        {
            // Test negative roll
            scoreManager.RecordRoll(-5);
            Assert.AreEqual(0, scoreManager.GetTotalScore(), "Negative rolls should be clamped to 0");
            
            // Test roll over 10
            scoreManager.RecordRoll(15);
            Assert.AreEqual(10, scoreManager.GetTotalScore(), "Rolls over 10 should be clamped to 10");
            
            // Reset for next test
            scoreManager.ResetGame();
            
            // Test sum over 10 in same frame
            scoreManager.RecordRoll(7);
            scoreManager.RecordRoll(8); // This should be clamped to 3 (10-7)
            
            Assert.AreEqual(10, scoreManager.GetFrameScore(1), "Frame total should not exceed 10");
        }

        [Test]
        public void TestResetGame()
        {
            // Play some rolls
            scoreManager.RecordRoll(5);
            scoreManager.RecordRoll(4);
            scoreManager.RecordRoll(10);
            
            // Reset
            scoreManager.ResetGame();
            
            Assert.AreEqual(0, scoreManager.GetTotalScore(), "Score should be 0 after reset");
            Assert.AreEqual(1, scoreManager.CurrentFrame, "Should be back to frame 1");
            Assert.IsTrue(scoreManager.IsFirstRoll, "Should be first roll");
            Assert.IsFalse(scoreManager.IsGameOver, "Game should not be over");
        }
    }
}
