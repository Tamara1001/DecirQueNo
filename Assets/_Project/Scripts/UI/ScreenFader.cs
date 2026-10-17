using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Full-screen black overlay used to fade between scenes. Created and owned by GameManager.
    public class ScreenFader : MonoBehaviour
    {
        private Image _image;
        private Coroutine _current;

        public static ScreenFader Create(Transform parent)
        {
            var go = new GameObject("ScreenFader", typeof(Canvas));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var imageGo = new GameObject("Black", typeof(RectTransform), typeof(Image));
            imageGo.transform.SetParent(go.transform, false);
            var rt = (RectTransform)imageGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var fader = go.AddComponent<ScreenFader>();
            fader._image = imageGo.GetComponent<Image>();
            fader._image.color = new Color(0f, 0f, 0f, 0f);
            fader._image.raycastTarget = false;
            return fader;
        }

        public Coroutine FadeTo(float alpha, float duration)
        {
            if (_current != null)
            {
                StopCoroutine(_current);
            }

            _current = StartCoroutine(Fade(alpha, duration));
            return _current;
        }

        private IEnumerator Fade(float alpha, float duration)
        {
            float start = _image.color.a;
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(start, alpha, Mathf.Clamp01(time / duration)));
                yield return null;
            }

            SetAlpha(alpha);
            _current = null;
        }

        private void SetAlpha(float alpha)
        {
            Color c = _image.color;
            c.a = alpha;
            _image.color = c;
        }
    }
}
