using System.Collections;
using UnityEngine;

namespace DecirQueNo
{
    // Tiny tweens for game feel. They use unscaled time so they look the same at any time scale.
    public static class UiFx
    {
        public static IEnumerator Pop(Transform target, float from = 0.6f, float duration = 0.18f)
        {
            var feedback = target.GetComponent<ButtonFeedback>();
            if (feedback != null)
            {
                feedback.Locked = true;
            }

            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / duration);
                target.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, EaseOutBack(k));
                yield return null;
            }

            target.localScale = Vector3.one;
            if (feedback != null)
            {
                feedback.Locked = false;
            }
        }

        public static IEnumerator Shake(RectTransform target, float magnitude = 14f, float duration = 0.28f)
        {
            Vector2 origin = target.anchoredPosition;
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(time / duration);
                target.anchoredPosition = origin + new Vector2(Mathf.Sin(time * 70f) * magnitude * k, 0f);
                yield return null;
            }

            target.anchoredPosition = origin;
        }

        public static IEnumerator FadeAndShrink(CanvasGroup group, Transform target, float duration = 0.3f)
        {
            var feedback = target.GetComponent<ButtonFeedback>();
            if (feedback != null)
            {
                feedback.Locked = true;
            }

            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / duration);
                if (group != null)
                {
                    group.alpha = 1f - k;
                }

                target.localScale = new Vector3(1f - 0.15f * k, 1f - 0.15f * k, 1f);
                yield return null;
            }
        }

        private static float EaseOutBack(float k)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float t = k - 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }
    }
}
