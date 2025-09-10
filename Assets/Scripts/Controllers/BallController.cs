using UnityEngine;
using System.Collections;
using XCAPE.Core;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

namespace XCAPE.Gameplay
{
    /// <summary>
    /// Controlador de bola de boliche con física realista, controles táctiles y efectos de spin
    /// Simula comportamiento real de una bola de boliche con momentum, fricción y colisiones precisas
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
    public class BallController : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float ballMass = 7.26f; // Peso real de bola de boliche (16 lbs)
        [SerializeField] private float launchForceMultiplier = 15f;
        [SerializeField] private float maxLaunchSpeed = 22f; // Velocidad máxima realista (mph convertido)
        [SerializeField] private float minLaunchSpeed = 5f;
        [SerializeField] private float spinMultiplier = 8f;
        [SerializeField] private float gutterBounceForce = 3f;
        
        [Header("Friction & Rolling")]
        [SerializeField] private float laneRollingFriction = 0.02f;
        [SerializeField] private float approachFriction = 0.3f;
        [SerializeField] private float gutterFriction = 0.8f;
        [SerializeField] private PhysicMaterial ballPhysicMaterial;
        [SerializeField] private PhysicMaterial lanePhysicMaterial;
        
        [Header("Touch Controls")]
        [SerializeField] private float touchSensitivity = 2f;
        [SerializeField] private float powerChargeTime = 2f;
        [SerializeField] private float maxSpinAngle = 45f;
        [SerializeField] private bool enableTrajectoryPreview = true;
        
        [Header("Visual Effects")]
        [SerializeField] private TrailRenderer ballTrail;
        [SerializeField] private ParticleSystem strikeEffect;
        [SerializeField] private LineRenderer trajectoryLine;
        [SerializeField] private int trajectoryPoints = 30;
        [SerializeField] private float trajectoryTimeStep = 0.1f;

        [Header("Audio")]
        [SerializeField] private AudioClip rollingSFX;
        [SerializeField] private AudioClip launchSFX;
        [SerializeField] private AudioClip gutterSFX;
        [SerializeField] private AudioClip pinCollisionSFX;
        
        // Internal state
        private Rigidbody _rb;
        private SphereCollider _collider;
        private AudioSource _audioSource;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        
        // Control state
        private bool _isLaunched = false;
        private bool _isCharging = false;
        private bool _canLaunch = true;
        private float _currentPower = 0f;
        private float _currentSpin = 0f;
        private Vector3 _launchDirection = Vector3.forward;
        private Vector2 _touchStartPos;
        private Vector2 _touchCurrentPos;
        
        // Physics state
        private bool _isOnLane = false;
        private bool _isInGutter = false;
        private bool _hasHitPins = false;
        private float _currentSpeed = 0f;
        
        // Events
        public System.Action<float> OnPowerChanged;
        public System.Action<float> OnSpinChanged;
        public System.Action OnBallLaunched;
        public System.Action OnBallStopped;
        public System.Action OnBallInGutter;
        public System.Action<int> OnPinsHit;

        #region Unity Lifecycle
        void Awake()
        {
            InitializeComponents();
            SetupPhysics();
            StoreInitialTransform();
        }

        void Start()
        {
            InitializeTrajectoryLine();
            if (ballTrail) ballTrail.enabled = false;
        }

        void Update()
        {
            if (HasServerAuthority())
            {
                HandleTouchInput();
                UpdatePhysics();
                UpdateAudio();
                UpdateTrajectoryPreview();
                CheckBallState();
            }
        }

        void FixedUpdate()
        {
            if (HasServerAuthority())
            {
                ApplyCustomPhysics();
            }
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
            _audioSource = GetComponent<AudioSource>();
            
            if (!ballPhysicMaterial)
            {
                ballPhysicMaterial = new PhysicMaterial("BallMaterial")
                {
                    dynamicFriction = 0.3f,
                    staticFriction = 0.4f,
                    bounciness = 0.2f,
                    frictionCombine = PhysicMaterialCombine.Average,
                    bounceCombine = PhysicMaterialCombine.Average
                };
            }
        }

        private void SetupPhysics()
        {
            _rb.mass = ballMass;
            _rb.drag = 0.1f;
            _rb.angularDrag = 2f;
            _rb.maxAngularVelocity = 50f;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            _collider.material = ballPhysicMaterial;
            _collider.radius = 0.108f; // Radio real de bola de boliche
        }

        private void StoreInitialTransform()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        private void InitializeTrajectoryLine()
        {
            if (!trajectoryLine)
            {
                GameObject lineObj = new GameObject("TrajectoryLine");
                lineObj.transform.parent = transform;
                trajectoryLine = lineObj.AddComponent<LineRenderer>();
            }
            
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.color = new Color(1f, 1f, 0f, 0.6f);
            trajectoryLine.startWidth = 0.05f;
            trajectoryLine.endWidth = 0.02f;
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.enabled = false;
        }
        #endregion

        #region Touch Input System
        private void HandleTouchInput()
        {
            if (!_canLaunch || _isLaunched) return;

            var inputManager = GameManager.Instance?.InputManager;
            if (inputManager == null) return;

            // Detectar inicio de toque
            if (inputManager.IsTouching && !_isCharging)
            {
                StartCharging(inputManager.TouchDelta);
            }
            // Actualizar mientras se toca
            else if (inputManager.IsTouching && _isCharging)
            {
                UpdateCharging(inputManager.TouchDelta);
            }
            // Soltar para lanzar
            else if (!inputManager.IsTouching && _isCharging)
            {
                LaunchBall();
            }
        }

        private void StartCharging(Vector2 touchPos)
        {
            _isCharging = true;
            _touchStartPos = touchPos;
            _currentPower = 0f;
            _currentSpin = 0f;
            
            if (trajectoryLine && enableTrajectoryPreview)
            {
                trajectoryLine.enabled = true;
            }
            
            StartCoroutine(ChargePower());
        }

        private void UpdateCharging(Vector2 touchPos)
        {
            _touchCurrentPos = touchPos;
            
            // Calcular spin basado en movimiento horizontal
            Vector2 delta = _touchCurrentPos - _touchStartPos;
            float spinInput = Mathf.Clamp(delta.x * touchSensitivity, -1f, 1f);
            _currentSpin = spinInput;
            
            // Calcular dirección de lanzamiento
            float angle = _currentSpin * maxSpinAngle;
            _launchDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            
            OnSpinChanged?.Invoke(_currentSpin);
        }

        private IEnumerator ChargePower()
        {
            float chargeTime = 0f;
            
            while (_isCharging && chargeTime < powerChargeTime)
            {
                chargeTime += Time.deltaTime;
                _currentPower = Mathf.Clamp01(chargeTime / powerChargeTime);
                OnPowerChanged?.Invoke(_currentPower);
                yield return null;
            }
            
            // Auto-lanzar si se mantiene presionado demasiado tiempo
            if (_isCharging)
            {
                LaunchBall();
            }
        }
        #endregion

        #region Ball Launch System
        public void LaunchBall()
        {
            if (!_canLaunch || _isLaunched) return;
            if (!HasServerAuthority()) return; // solo servidor lanza
            
            _isCharging = false;
            _isLaunched = true;
            _canLaunch = false;
            
            // Calcular velocidad de lanzamiento
            float launchSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, _currentPower);
            Vector3 launchVelocity = _launchDirection * launchSpeed;
            
            // Aplicar fuerza de lanzamiento
            _rb.velocity = launchVelocity;
            
            // Aplicar spin
            Vector3 spinTorque = new Vector3(0, _currentSpin * spinMultiplier, 0);
            _rb.angularVelocity = spinTorque;
            
            // Efectos visuales y audio
            if (ballTrail) ballTrail.enabled = true;
            if (trajectoryLine) trajectoryLine.enabled = false;
            
            PlayLaunchEffects();
            
            OnBallLaunched?.Invoke();
            
            Debug.Log($"Ball launched: Power={_currentPower:F2}, Spin={_currentSpin:F2}, Speed={launchSpeed:F1}");
        }

        private void PlayLaunchEffects()
        {
            if (_audioSource && launchSFX)
            {
                _audioSource.PlayOneShot(launchSFX);
            }
        }
        #endregion

        #region Physics Updates
        private void UpdatePhysics()
        {
            _currentSpeed = _rb.velocity.magnitude;
            
            // Aplicar fricción personalizada basada en superficie
            if (_isOnLane && !_isInGutter)
            {
                ApplyLaneFriction();
            }
            else if (_isInGutter)
            {
                ApplyGutterFriction();
            }
        }

        private void ApplyCustomPhysics()
        {
            if (!_isLaunched) return;
            
            // Simular resistencia del aire
            if (_rb.velocity.magnitude > 0.1f)
            {
                Vector3 airResistance = -_rb.velocity.normalized * (_rb.velocity.sqrMagnitude * 0.001f);
                _rb.AddForce(airResistance, ForceMode.Force);
            }
            
            // Aplicar efecto del spin en el movimiento (hook)
            if (_rb.angularVelocity.magnitude > 0.1f && _isOnLane)
            {
                Vector3 spinEffect = Vector3.Cross(_rb.angularVelocity, Vector3.up) * 0.1f;
                _rb.AddForce(spinEffect, ForceMode.Force);
            }
        }

        private void ApplyLaneFriction()
        {
            if (_rb.velocity.magnitude > 0.1f)
            {
                Vector3 frictionForce = -_rb.velocity.normalized * (laneRollingFriction * _rb.mass * 9.81f);
                _rb.AddForce(frictionForce, ForceMode.Force);
            }
        }

        private void ApplyGutterFriction()
        {
            if (_rb.velocity.magnitude > 0.1f)
            {
                Vector3 frictionForce = -_rb.velocity.normalized * (gutterFriction * _rb.mass * 9.81f);
                _rb.AddForce(frictionForce, ForceMode.Force);
            }
        }
        #endregion

        #region Trajectory Preview
        private void UpdateTrajectoryPreview()
        {
            if (!trajectoryLine || !_isCharging || !enableTrajectoryPreview) return;
            
            Vector3[] points = CalculateTrajectory();
            trajectoryLine.positionCount = points.Length;
            trajectoryLine.SetPositions(points);
        }

        private Vector3[] CalculateTrajectory()
        {
            Vector3[] points = new Vector3[trajectoryPoints];
            Vector3 startPos = transform.position;
            Vector3 velocity = _launchDirection * Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, _currentPower);
            
            for (int i = 0; i < trajectoryPoints; i++)
            {
                float time = i * trajectoryTimeStep;
                points[i] = startPos + velocity * time + 0.5f * Physics.gravity * time * time;
                
                // Aplicar efecto de fricción simplificado
                velocity *= (1f - laneRollingFriction * trajectoryTimeStep);
            }
            
            return points;
        }
        #endregion

        #region Audio System
        private void UpdateAudio()
        {
            if (!_audioSource) return;
            
            // Audio de rodadura
            if (_isLaunched && _currentSpeed > 1f && rollingSFX)
            {
                if (!_audioSource.isPlaying || _audioSource.clip != rollingSFX)
                {
                    _audioSource.clip = rollingSFX;
                    _audioSource.loop = true;
                    _audioSource.Play();
                }
                
                // Ajustar volumen y pitch basado en velocidad
                _audioSource.volume = Mathf.Clamp01(_currentSpeed / maxLaunchSpeed);
                _audioSource.pitch = Mathf.Clamp(0.5f + (_currentSpeed / maxLaunchSpeed), 0.5f, 2f);
            }
            else if (_audioSource.isPlaying && _audioSource.clip == rollingSFX)
            {
                _audioSource.Stop();
            }
        }
        #endregion

        #region State Checking
        private void CheckBallState()
        {
            // Verificar si la bola se ha detenido
            if (_isLaunched && _currentSpeed < 0.5f && _rb.velocity.magnitude < 0.1f)
            {
                StartCoroutine(CheckForStop());
            }
        }

        private IEnumerator CheckForStop()
        {
            yield return new WaitForSeconds(1f);
            
            if (_rb.velocity.magnitude < 0.1f)
            {
                StopBall();
            }
        }

        private void StopBall()
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            if (ballTrail) ballTrail.enabled = false;
            if (_audioSource.isPlaying) _audioSource.Stop();
            
            OnBallStopped?.Invoke();
            
            Debug.Log("Ball stopped");
        }
        #endregion

        #region Collision Detection
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Lane"))
            {
                _isOnLane = true;
                _isInGutter = false;
            }
            else if (other.CompareTag("Gutter"))
            {
                _isOnLane = false;
                _isInGutter = true;
                OnBallInGutter?.Invoke();
                
                if (_audioSource && gutterSFX)
                {
                    _audioSource.PlayOneShot(gutterSFX);
                }
            }
            else if (other.CompareTag("PinArea"))
            {
                _hasHitPins = true;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Pin"))
            {
                HandlePinCollision(collision);
            }
            else if (collision.gameObject.CompareTag("Gutter"))
            {
                HandleGutterBounce(collision);
            }
        }

        private void HandlePinCollision(Collision collision)
        {
            if (_audioSource && pinCollisionSFX)
            {
                _audioSource.PlayOneShot(pinCollisionSFX, 0.7f);
            }
            
            // Notificar colisión con pino
            var pinController = collision.gameObject.GetComponent<PinController>();
            if (pinController)
            {
                pinController.OnHitByBall(_rb.velocity.magnitude);
            }
        }

        private void HandleGutterBounce(Collision collision)
        {
            // Aplicar rebote suave en gutter
            Vector3 bounceDirection = Vector3.Reflect(_rb.velocity.normalized, collision.contacts[0].normal);
            _rb.velocity = bounceDirection * Mathf.Min(_rb.velocity.magnitude, gutterBounceForce);
        }
        #endregion

        #region Public Interface
        public void ResetBall()
        {
            if (!HasServerAuthority()) return; // solo servidor resetea
            _isLaunched = false;
            _isCharging = false;
            _canLaunch = true;
            _isOnLane = false;
            _isInGutter = false;
            _hasHitPins = false;
            _currentPower = 0f;
            _currentSpin = 0f;
            _currentSpeed = 0f;
            
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(_startPosition, _startRotation);
            
            if (ballTrail) ballTrail.enabled = false;
            if (trajectoryLine) trajectoryLine.enabled = false;
            if (_audioSource.isPlaying) _audioSource.Stop();
            
            Debug.Log("Ball reset to starting position");
        }

        public bool IsLaunched => _isLaunched;
        public bool CanLaunch => _canLaunch;
        public float CurrentSpeed => _currentSpeed;
        public float CurrentPower => _currentPower;
        public float CurrentSpin => _currentSpin;
        public bool HasHitPins => _hasHitPins;
        public bool IsInGutter => _isInGutter;
        #endregion

        #region Debug
        void OnDrawGizmosSelected()
        {
            // Visualizar dirección de lanzamiento
            if (_isCharging)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, _launchDirection * 5f);
                
                // Visualizar fuerza
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f + _currentPower);
            }
        }
        #endregion

    private bool HasServerAuthority()
    {
#if UNITY_NETCODE_GAMEOBJECTS
        if (NetworkManager.Singleton == null) return true; // modo offline
        return NetworkManager.Singleton.IsServer;
#else
        return true;
#endif
    }
    }
}
