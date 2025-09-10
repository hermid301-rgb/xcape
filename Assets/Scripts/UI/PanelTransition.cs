using System.Collections;
using UnityEngine;

namespace XCAPE.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelTransition : MonoBehaviour
    {
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);
        [SerializeField] private bool scalePunch = true;
        private CanvasGroup cg;
        private Coroutine current;

        void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        }

        public void Show(bool immediate = false)
        {
            if (current != null) StopCoroutine(current);
            gameObject.SetActive(true);
            if (immediate) { cg.alpha = 1f; transform.localScale = Vector3.one; return; }
            current = StartCoroutine(Fade(0f, 1f, true));
        }

        public void Hide(bool immediate = false)
        {
            if (current != null) StopCoroutine(current);
            if (immediate) { cg.alpha = 0f; gameObject.SetActive(false); return; }
            current = StartCoroutine(Fade(1f, 0f, false));
        }

        private IEnumerator Fade(float from, float to, bool activating)
        {
            cg.alpha = from;
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one;
            if (scalePunch && activating) { startScale = Vector3.one * 0.95f; endScale = Vector3.one; }

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = curve.Evaluate(Mathf.Clamp01(t / duration));
                cg.alpha = Mathf.Lerp(from, to, k);
                if (scalePunch && activating)
                {
                    transform.localScale = Vector3.Lerp(startScale, endScale, k);
                }
                yield return null;
            }
            cg.alpha = to;
            if (!activating) gameObject.SetActive(false);
            current = null;
        }
    }
}
