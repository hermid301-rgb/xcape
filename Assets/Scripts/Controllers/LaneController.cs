using UnityEngine;
using XCAPE.Gameplay;
using XCAPE.Core;

namespace XCAPE.Gameplay
{
    /// <summary>
    /// Control de la pista: límites (gutter), área de pinos, reset de frame y agrupación de pinos.
    /// </summary>
    public class LaneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform ballSpawnPoint;
        [SerializeField] private BallController ball;
        [SerializeField] private PinController[] pins;
        [SerializeField] private Transform[] pinPositions; // 10 posiciones oficiales
        [SerializeField] private Collider laneTrigger;
        [SerializeField] private Collider leftGutterTrigger;
        [SerializeField] private Collider rightGutterTrigger;
        [SerializeField] private Collider pinAreaTrigger;

        [Header("Frame Control")]
        [SerializeField] private float settleWaitTime = 3.0f;
        private bool _frameActive;
        private int _pinsDownThisFrame;

        private ScoreManager _scoreManager;

        void Start()
        {
            _scoreManager = FindObjectOfType<ScoreManager>();
            if (HasServerAuthority())
            {
                ResetFrame(true);
                SubscribeBallEvents();
            }
        }

        private void SubscribeBallEvents()
        {
            if (!ball) return;
            ball.OnBallStopped += OnBallStopped;
            ball.OnBallInGutter += OnBallGutter;
        }

        public void ResetFrame(bool fullRack)
        {
            if (!HasServerAuthority()) return;
            // Reset bola
            if (ball && ballSpawnPoint)
            {
                ball.transform.position = ballSpawnPoint.position;
                ball.ResetBall();
            }

            // Reset pinos
            if (pins != null && pinPositions != null && pins.Length == 10 && pinPositions.Length == 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    pins[i].SetPinNumber(i + 1);
                    if (fullRack)
                    {
                        pins[i].SetOriginalTransform(pinPositions[i].position, pinPositions[i].rotation);
                        pins[i].ResetPin();
                    }
                    else if (pins[i].IsKnockedDown)
                    {
                        // Dejar caídos si no es full rack
                    }
                }
            }

            _frameActive = true;
            _pinsDownThisFrame = 0;
        }

        private void OnBallStopped()
        {
            if (!HasServerAuthority()) return;
            if (!_frameActive) return;
            Invoke(nameof(ScoreAndMaybeReset), settleWaitTime);
        }

        private void OnBallGutter()
        {
            // Nada especial por ahora; el conteo se hace al detenerse
        }

        private void ScoreAndMaybeReset()
        {
            if (!HasServerAuthority()) return;
            // Contar pinos caídos
            int down = 0;
            foreach (var p in pins)
            {
                if (p.IsKnockedDown) down++;
            }
            int knockedThisRoll = down - _pinsDownThisFrame; // pinos nuevos caídos
            _pinsDownThisFrame = down;

            // Registrar en ScoreManager
            if (_scoreManager)
            {
                _scoreManager.RecordRoll(Mathf.Clamp(knockedThisRoll, 0, 10));
                if (_scoreManager.IsGameOver)
                {
                    _frameActive = false;
                    return;
                }

                // Si fue strike o terminó el frame, hacer reset parcial o total
                if (_scoreManager.IsFirstRoll)
                {
                    // Acaba de avanzar de frame; reponer full rack
                    ResetFrame(true);
                }
                else
                {
                    // Siguiente tiro dentro del mismo frame; NO reponer pinos
                    ball.transform.position = ballSpawnPoint.position;
                    ball.ResetBall();
                }
            }
        }

    private bool HasServerAuthority()
    {
#if UNITY_NETCODE_GAMEOBJECTS
        if (Unity.Netcode.NetworkManager.Singleton == null) return true;
        return Unity.Netcode.NetworkManager.Singleton.IsServer;
#else
        return true;
#endif
    }
    }
}
