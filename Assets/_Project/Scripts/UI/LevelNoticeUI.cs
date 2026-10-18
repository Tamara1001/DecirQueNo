using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Tells the player when saying No got easier or harder, which is the point of the game.
    // A card with the look of the rest of the UI that slides in, waits and fades out.
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelNoticeUI : MonoBehaviour
    {
        private const float ShowSeconds = 3.5f;
        private const float FadeSeconds = 0.5f;
        private const float SlideSeconds = 0.35f;
        private const float SlideDistance = 160f;

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
            UiSprites.Round(borderImage, 34f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(border.transform, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(7f, 7f);
            fillRect.offsetMax = new Vector2(-7f, -7f);
            _fill = fill.GetComponent<Image>();
            _fill.raycastTarget = false;
            UiSprites.Round(_fill, 28f);

            _text.color = UiSprites.Navy;
            _text.fontStyle = FontStyles.Bold;
            _text.alignment = TextAlignmentOptions.Center;
            _text.enableAutoSizing = true;
            _text.fontSizeMin = 20f;
            _text.fontSizeMax = 42f;
            var textRect = _text.rectTransform;
            textRect.offsetMin = new Vector2(24f, 12f);
            textRect.offsetMax = new Vector2(-24f, -12f);
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
            // Slides in from the right with a little overshoot.
            float time = 0f;
            while (time < SlideSeconds)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / SlideSeconds);
                float back = 1f + 2.70158f * Mathf.Pow(k - 1f, 3f) + 1.70158f * Mathf.Pow(k - 1f, 2f);
                _rect.anchoredPosition = _restPosition + new Vector2(SlideDistance * (1f - back), 0f);
                _group.alpha = Mathf.Clamp01(k * 3f);
                yield return null;
            }

            _rect.anchoredPosition = _restPosition;
            _group.alpha = 1f;

            float wait = 0f;
            while (wait < ShowSeconds)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
            }

            time = 0f;
            while (time < FadeSeconds)
            {
                time += Time.unscaledDeltaTime;
                _group.alpha = 1f - Mathf.Clamp01(time / FadeSeconds);
                yield return null;
            }

            _group.alpha = 0f;
            _routine = null;
        }
    }
}
