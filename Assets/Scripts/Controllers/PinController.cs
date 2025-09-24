using UnityEngine;
using System.Collections;
#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

namespace XCAPE.Gameplay
{
    /// <summary>
    /// Controlador de pino de boliche con detección de caída realista y física calibrada
    /// Simula comportamiento oficial de pinos de boliche con materiales y colisiones precisas
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PinController : MonoBehaviour
    {
        [Header("Pin Physics")]
        [SerializeField] private float pinMass = 1.59f; // Peso oficial de pino (3.5 lbs)
        [SerializeField] private float knockDownAngle = 30f; // Ángulo para considerar caído
        [SerializeField] private float settlementTime = 3f; // Tiempo para considerar establecido
        [SerializeField] private float minimumHitForce = 2f; // Fuerza mínima para knock down
        
        [Header("Pin Dimensions")]
        [SerializeField] private float pinHeight = 0.381f; // Altura oficial (15 pulgadas)
        [SerializeField] private float pinRadius = 0.06f; // Radio en la parte más ancha
        [SerializeField] private PhysicsMaterial pinPhysicMaterial;
        
        [Header("Visual States")]
        [SerializeField] private Material standingMaterial;
        [SerializeField] private Material knockedDownMaterial;
        [SerializeField] private ParticleSystem knockDownEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioClip knockDownSFX;
        [SerializeField] private AudioClip pinCollisionSFX;
        [SerializeField] private AudioClip pinSettleSFX;
        
        // Internal state
        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private AudioSource _audioSource;
        private Renderer _renderer;
        
        // Position and rotation tracking
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        
        // Pin state
        private bool _isKnockedDown = false;
        private bool _isSettled = false;
        private bool _wasHitThisFrame = false;
        private float _timeUnsettled = 0f;
        private float _lastMovementTime = 0f;
        
        // Pin numbering (1-10 for official setup)
        [Header("Pin Identification")]
        [SerializeField] private int pinNumber = 1;
        [SerializeField] private bool isHeadPin = false; // Pin 1 (cabecera)
        
        // Events
        public System.Action<PinController, bool> OnPinStateChanged;
        public System.Action<PinController> OnPinKnockedDown;
        public System.Action<PinController> OnPinSettled;
        
        #region Unity Lifecycle
        void Awake()
        {
            InitializeComponents();
            SetupPhysics();
            StoreOriginalTransform();
        }

        void Start()
        {
            InitializeAudio();
            SetVisualState(false);
        }

        void Update()
        {
            if (HasServerAuthority())
            {
                CheckPinState();
                UpdateSettlement();
                TrackMovement();
            }
        }

        void FixedUpdate()
        {
            ApplyCustomPhysics();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _renderer = GetComponent<Renderer>();
            
            // Crear AudioSource si no existe
            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Crear material físico si no existe
            if (!pinPhysicMaterial)
            {
                pinPhysicMaterial = new PhysicsMaterial("PinMaterial")
                {
                    dynamicFriction = 0.4f,
                    staticFriction = 0.6f,
                    bounciness = 0.1f,
                    frictionCombine = PhysicsMaterialCombine.Average,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
            }
        }

        private void SetupPhysics()
        {
            // Configurar Rigidbody con valores realistas
            _rb.mass = pinMass;
            _rb.linearDamping = 0.5f;
            _rb.angularDamping = 3f;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            // Configurar Collider con dimensiones oficiales
            _collider.height = pinHeight;
            _collider.radius = pinRadius;
            _collider.material = pinPhysicMaterial;
            _collider.center = new Vector3(0, pinHeight * 0.5f, 0);
            
            // Centro de masa ligeramente hacia abajo para estabilidad
            _rb.centerOfMass = new Vector3(0, pinHeight * 0.4f, 0);
        }

        private void StoreOriginalTransform()
        {
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
        }

        private void InitializeAudio()
        {
            _audioSource.spatialBlend = 1f; // Audio 3D
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _audioSource.maxDistance = 20f;
        }
        #endregion

        #region Pin State Detection
        private void CheckPinState()
        {
            bool wasKnockedDown = _isKnockedDown;
            
            // Verificar ángulo de inclinación
            float angleFromVertical = Vector3.Angle(transform.up, Vector3.up);
            _isKnockedDown = angleFromVertical > knockDownAngle;
            
            // Verificar si está fuera de posición significativamente
            float distanceFromOriginal = Vector3.Distance(transform.position, _originalPosition);
            if (distanceFromOriginal > pinRadius * 2f)
            {
                _isKnockedDown = true;
            }
            
            // Notificar cambio de estado
            if (_isKnockedDown != wasKnockedDown)
            {
                OnPinStateChanged?.Invoke(this, _isKnockedDown);
                
                if (_isKnockedDown)
                {
                    OnPinKnockedDown?.Invoke(this);
                    PlayKnockDownEffects();
                }
                
                SetVisualState(_isKnockedDown);
            }
        }

        private void UpdateSettlement()
        {
            // Verificar si el pino está en movimiento
            bool isMoving = _rb.linearVelocity.magnitude > 0.1f || _rb.angularVelocity.magnitude > 0.1f;
            
            if (isMoving)
            {
                _timeUnsettled = 0f;
                _lastMovementTime = Time.time;
                _isSettled = false;
            }
            else
            {
                _timeUnsettled += Time.deltaTime;
                
                // Considerar establecido después del tiempo de estabilización
                if (!_isSettled && _timeUnsettled >= settlementTime)
                {
                    _isSettled = true;
                    OnPinSettled?.Invoke(this);
                    PlaySettleEffects();
                }
            }
        }

        private void TrackMovement()
        {
            // Detectar movimiento significativo para efectos de audio
            float positionDelta = Vector3.Distance(transform.position, _lastPosition);
            float rotationDelta = Quaternion.Angle(transform.rotation, _lastRotation);
            
            if (positionDelta > 0.01f || rotationDelta > 1f)
            {
                _lastPosition = transform.position;
                _lastRotation = transform.rotation;
            }
        }
        #endregion

        #region Physics Customization
        private void ApplyCustomPhysics()
        {
            if (!HasServerAuthority()) return;
            // Aplicar estabilización cuando está de pie
            if (!_isKnockedDown && _rb.linearVelocity.magnitude < 0.5f)
            {
                // Pequeña fuerza correctiva hacia la posición vertical
                Vector3 uprightTorque = Vector3.Cross(transform.up, Vector3.up) * 10f;
                _rb.AddTorque(uprightTorque, ForceMode.Force);
            }
            
            // Limitar velocidad angular excesiva
            if (_rb.angularVelocity.magnitude > 20f)
            {
                _rb.angularVelocity = _rb.angularVelocity.normalized * 20f;
            }
        }
        #endregion

        #region Collision Handling
        void OnCollisionEnter(Collision collision)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            
            if (collision.gameObject.CompareTag("Ball"))
            {
                HandleBallCollision(collision, impactForce);
            }
            else if (collision.gameObject.CompareTag("Pin"))
            {
                HandlePinCollision(collision, impactForce);
            }
            else if (collision.gameObject.CompareTag("Lane") || collision.gameObject.CompareTag("Gutter"))
            {
                HandleLaneCollision(collision, impactForce);
            }
        }

        private void HandleBallCollision(Collision collision, float impactForce)
        {
            // La bola siempre puede tumbar un pino con suficiente fuerza
            if (impactForce >= minimumHitForce)
            {
                _wasHitThisFrame = true;
                
                // Aplicar fuerza adicional basada en el impacto
                Vector3 forceDirection = collision.contacts[0].normal;
                _rb.AddForceAtPosition(-forceDirection * impactForce * 2f, collision.contacts[0].point, ForceMode.Impulse);
                
                PlayHitEffects(impactForce);
            }
        }

        private void HandlePinCollision(Collision collision, float impactForce)
        {
            // Pino golpeando a otro pino
            if (impactForce > 1f)
            {
                PlayPinCollisionEffects(impactForce);
                
                // Notificar al otro pino que fue golpeado
                var otherPin = collision.gameObject.GetComponent<PinController>();
                otherPin?.OnHitByPin(impactForce);
            }
        }

        private void HandleLaneCollision(Collision collision, float impactForce)
        {
            // Pino cayendo en la pista o canaleta
            if (impactForce > 2f)
            {
                PlayLandingEffects(impactForce);
            }
        }

        public void OnHitByBall(float ballSpeed)
        {
            // Llamado por BallController cuando detecta colisión
            _wasHitThisFrame = true;
            Debug.Log($"Pin {pinNumber} hit by ball at {ballSpeed:F1} speed");
        }

        public void OnHitByPin(float force)
        {
            // Llamado por otro pino en colisión
            if (force > minimumHitForce * 0.5f) // Los pinos requieren menos fuerza entre ellos
            {
                _wasHitThisFrame = true;
            }
        }
        #endregion

        #region Visual and Audio Effects
        private void SetVisualState(bool isKnockedDown)
        {
            if (_renderer && standingMaterial && knockedDownMaterial)
            {
                _renderer.material = isKnockedDown ? knockedDownMaterial : standingMaterial;
            }
        }

        private void PlayKnockDownEffects()
        {
            if (knockDownEffect)
            {
                knockDownEffect.Play();
            }
            
            if (_audioSource && knockDownSFX)
            {
                _audioSource.PlayOneShot(knockDownSFX, 0.8f);
            }
            
            Debug.Log($"Pin {pinNumber} knocked down!");
        }

        private void PlayHitEffects(float impactForce)
        {
            if (_audioSource && pinCollisionSFX)
            {
                float volume = Mathf.Clamp01(impactForce / 10f);
                _audioSource.PlayOneShot(pinCollisionSFX, volume);
            }
        }

        private void PlayPinCollisionEffects(float impactForce)
        {
            if (_audioSource && pinCollisionSFX)
            {
                float volume = Mathf.Clamp01(impactForce / 5f) * 0.6f;
                _audioSource.PlayOneShot(pinCollisionSFX, volume);
            }
        }

        private void PlayLandingEffects(float impactForce)
        {
            if (_audioSource && pinCollisionSFX)
            {
                float volume = Mathf.Clamp01(impactForce / 8f) * 0.4f;
                _audioSource.PlayOneShot(pinCollisionSFX, volume);
            }
        }

        private void PlaySettleEffects()
        {
            if (_audioSource && pinSettleSFX)
            {
                _audioSource.PlayOneShot(pinSettleSFX, 0.3f);
            }
        }
        #endregion

        #region Public Interface
        public void ResetPin()
        {
            if (!HasServerAuthority()) return;
            // Resetear estado físico
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(_originalPosition, _originalRotation);
            
            // Resetear estado interno
            _isKnockedDown = false;
            _isSettled = true;
            _wasHitThisFrame = false;
            _timeUnsettled = 0f;
            _lastMovementTime = 0f;
            
            // Resetear efectos visuales
            SetVisualState(false);
            
            if (knockDownEffect && knockDownEffect.isPlaying)
            {
                knockDownEffect.Stop();
            }
            
            Debug.Log($"Pin {pinNumber} reset to original position");
        }

        public void SetPinNumber(int number)
        {
            pinNumber = number;
            isHeadPin = (number == 1);
            gameObject.name = $"Pin_{number:D2}";
        }

        public void SetOriginalTransform(Vector3 position, Quaternion rotation)
        {
            _originalPosition = position;
            _originalRotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }

        // Propiedades públicas
        public bool IsKnockedDown => _isKnockedDown;
        public bool IsSettled => _isSettled;
        public bool WasHitThisFrame => _wasHitThisFrame;
        public int PinNumber => pinNumber;
        public bool IsHeadPin => isHeadPin;
        public Vector3 OriginalPosition => _originalPosition;
        public float TimeSinceLastMovement => Time.time - _lastMovementTime;
        
        // Métodos de utilidad
        public float GetAngleFromVertical()
        {
            return Vector3.Angle(transform.up, Vector3.up);
        }

        public float GetDistanceFromOriginal()
        {
            return Vector3.Distance(transform.position, _originalPosition);
        }
        #endregion

        #region Debug
        void OnDrawGizmosSelected()
        {
            // Visualizar posición original
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_originalPosition, new Vector3(pinRadius * 2, pinHeight, pinRadius * 2));
            
            // Visualizar estado actual
            if (_isKnockedDown)
            {
                Gizmos.color = Color.red;
            }
            else if (_isSettled)
            {
                Gizmos.color = Color.blue;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }
            
            Gizmos.DrawWireSphere(transform.position, pinRadius);
            
            // Mostrar número del pino (solo en editor)
            if (pinNumber > 0)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * pinHeight, $"Pin {pinNumber}");
#endif
            }
        }
        #endregion

    private bool HasServerAuthority()
    {
#if UNITY_NETCODE_GAMEOBJECTS
        if (NetworkManager.Singleton == null) return true; // offline
        return NetworkManager.Singleton.IsServer;
#else
        return true;
#endif
    }
    }
}
