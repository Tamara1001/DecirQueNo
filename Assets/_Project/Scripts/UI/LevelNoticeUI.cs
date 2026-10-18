using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Tells the player when saying No got easier or harder, which is the point of the game.
    // A small notification with the look of the rest of the UI that rises from below the screen, waits and floats up
    // while it fades out.
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelNoticeUI : MonoBehaviour
    {
        private const float ShowSeconds = 2.6f;
        private const float FadeSeconds = 0.5f;
        private const float CornerMargin = 48f;
        private const float RiseSeconds = 0.6f;
        private const float FloatAwayDistance = 70f;
        private static readonly Vector2 CardSize = new Vector2(470f, 76f);

        private static readonly Color EasierColor = new Color(0.62f, 0.9f, 0.95f);
        private static readonly Color HarderColor = new Color(0.98f, 0.67f, 0.94f);

        [SerializeField] private TMP_Text _text;

        private CanvasGroup _group;
        private PlayerStats _stats;
        private Coroutine _routine;
        private Image _fill;
        private RectTransform _rect;
        private Vector2 _restPosition;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _rect = (RectTransform)transform;

            // The old card could be stretched by its anchors: it becomes a small fixed-size card with the same center.
            Vector3 center = _rect.TransformPoint(_rect.rect.center);
            _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.sizeDelta = CardSize;
            _rect.position = center;
            _restPosition = _rect.anchoredPosition;
            BuildCard();
        }

        private void BuildCard()
        {
            var border = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(transform, false);
            border.transform.SetAsFirstSibling();
            var borderRect = (RectTransform)border.transform;
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            var borderImage = border.GetComponent<Image>();
            borderImage.color = UiSprites.Navy;
            borderImage.raycastTarget = false;
            UiSprites.Round(borderImage, 30f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(border.transform, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(6f, 6f);
            fillRect.offsetMax = new Vector2(-6f, -6f);
            _fill = fill.GetComponent<Image>();
            _fill.raycastTarget = false;
            UiSprites.Round(_fill, 24f);

            _text.color = UiSprites.Navy;
            _text.fontStyle = FontStyles.Bold;
            _text.alignment = TextAlignmentOptions.Center;
            _text.enableAutoSizing = true;
            _text.fontSizeMin = 18f;
            _text.fontSizeMax = 30f;
            var textRect = _text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 8f);
            textRect.offsetMax = new Vector2(-18f, -8f);
        }

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _stats = manager.Stats;
            _stats.OnNoLevelChanged += HandleLevelChanged;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnNoLevelChanged -= HandleLevelChanged;
            }
        }

        private void HandleLevelChanged(int oldIndex, int newIndex)
        {
            bool easier = newIndex < oldIndex;
            _text.text = easier ? "Decir que no te cuesta menos" : "Decir que no te cuesta más";
            _fill.color = easier ? EasierColor : HarderColor;

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(Show());
        }

        private IEnumerator Show()
        {
            // Its place is the bottom right corner of the screen. It starts just below the bottom edge and rises to it
            // with a little overshoot.
            PlaceInCorner();
            AudioManager.Play(Sfx.Notification, 1.25f);
            float rise = DistanceFromBelowScreen();
            _group.alpha = 1f;
            float time = 0f;
            while (time < RiseSeconds)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / RiseSeconds);
                float back = 1f + 1.2f * Mathf.Pow(k - 1f, 3f) + 0.2f * Mathf.Pow(k - 1f, 2f);
                _rect.anchoredPosition = _restPosition - new Vector2(0f, rise * (1f - back));
                yield return null;
            }

            _rect.anchoredPosition = _restPosition;

            float wait = 0f;
            while (wait < ShowSeconds)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            // It keeps going up while it fades away.
            time = 0f;
            while (time < FadeSeconds)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / FadeSeconds);
                _group.alpha = 1f - k;
                _rect.anchoredPosition = _restPosition + new Vector2(0f, FloatAwayDistance * k);
                yield return null;
            }

            _group.alpha = 0f;
            _rect.anchoredPosition = _restPosition;
            _routine = null;
        }

        private void PlaceInCorner()
        {
            var canvasRect = _rect.root as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            Rect area = canvasRect.rect;
            var local = new Vector3(area.xMax - CardSize.x * 0.5f - CornerMargin, area.yMin + CardSize.y * 0.5f + CornerMargin, 0f);
            _rect.position = canvasRect.TransformPoint(local);
            _restPosition = _rect.anchoredPosition;
        }

        // How far below its resting place the bottom edge of the screen plus the height of the card is.
        private float DistanceFromBelowScreen()
        {
            var canvasRect = _rect.root as RectTransform;
            if (canvasRect == null)
            {
                return 400f;
            }

            Vector3 local = canvasRect.InverseTransformPoint(_rect.position);
            return local.y + canvasRect.rect.height * 0.5f + CardSize.y;
        }
    }
}
