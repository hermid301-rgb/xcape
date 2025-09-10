using UnityEngine;

namespace XCAPE.Gameplay
{
    /// <summary>
    /// Simple pin behavior stub for Phase 1. Full collision/settle detection in Phase 2.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PinController : MonoBehaviour
    {
        private Rigidbody _rb;
        private Quaternion _startRot;
        private Vector3 _startPos;

        public bool IsKnockedDown => Vector3.Dot(transform.up, Vector3.up) < 0.7f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _startPos = transform.position;
            _startRot = transform.rotation;
        }

        public void ResetPin()
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(_startPos, _startRot);
        }
    }
}
