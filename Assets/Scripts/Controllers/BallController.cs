using UnityEngine;

namespace XCAPE.Gameplay
{
    /// <summary>
    /// Stub for bowling ball controller. Full physics and touch controls will be implemented in Phase 2.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        [Header("Physics")]
        public float launchForce = 20f;
        public float maxSpeed = 22f;
        public float spinStrength = 3f;

        private Rigidbody _rb;
        private bool _launched;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.maxAngularVelocity = 50f;
        }

        void Update()
        {
            // Clamp velocity
            if (_rb.velocity.magnitude > maxSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * maxSpeed;
            }
        }

        public void Launch(Vector3 direction, float power, float spin)
        {
            if (_launched) return;
            _launched = true;
            _rb.AddForce(direction.normalized * launchForce * Mathf.Clamp01(power), ForceMode.VelocityChange);
            _rb.AddTorque(Vector3.up * spin * spinStrength, ForceMode.VelocityChange);
        }

        public void ResetBall(Vector3 position)
        {
            _launched = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.position = position;
            transform.rotation = Quaternion.identity;
        }
    }
}
