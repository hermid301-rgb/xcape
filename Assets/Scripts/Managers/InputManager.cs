using UnityEngine;

namespace XCAPE.Core
{
    public class InputManager : MonoBehaviour
    {
        public Vector2 TouchDelta { get; private set; }
        public bool IsTouching { get; private set; }

        public void UpdateInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            IsTouching = Input.GetMouseButton(0);
            TouchDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                IsTouching = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
                TouchDelta = t.deltaPosition;
            }
            else
            {
                IsTouching = false;
                TouchDelta = Vector2.zero;
            }
#endif
        }
    }
}
