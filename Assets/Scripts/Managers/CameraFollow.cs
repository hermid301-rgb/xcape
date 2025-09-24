using UnityEngine;
using XCAPE.Gameplay;

namespace XCAPE.Core
{
    /// <summary>
    /// Cámara que sigue a la bola suavemente y hace tilt en función del spin
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private BallController target;
        [SerializeField] private Vector3 offset = new Vector3(0, 2.2f, -4.5f);
        [SerializeField] private float followSmooth = 5f;
        [SerializeField] private float lookAheadFactor = 0.35f;
        [SerializeField] private float maxTilt = 8f;

        void LateUpdate()
        {
            if (!target) return;
            var vel = target.GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero;
            var lookAhead = vel * lookAheadFactor;
            var targetPos = target.transform.position + offset + new Vector3(lookAhead.x, 0, lookAhead.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);

            // Mirar hacia adelante
            var lookPoint = target.transform.position + new Vector3(0, 0.5f, 2f) + lookAhead;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPoint - transform.position, Vector3.up), Time.deltaTime * followSmooth);

            // Pequeño tilt por spin
            float tilt = Mathf.Clamp(target.CurrentSpin * maxTilt, -maxTilt, maxTilt);
            transform.Rotate(Vector3.forward, -tilt * Time.deltaTime, Space.Self);
        }
    }
}
