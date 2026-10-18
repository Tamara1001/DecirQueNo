using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class MainMenuUI : MonoBehaviour
    {
        private static readonly Color PlayFill = new Color(0.98f, 0.67f, 0.94f);
        private static readonly Color CreditsFill = new Color(0.98f, 0.8f, 0.96f);
        private static readonly Color TitleTop = new Color(1f, 0.84f, 0.97f);
        private static readonly Color TitleBottom = new Color(0.98f, 0.68f, 0.94f);
        private static readonly Color TitleOutline = new Color(0.04f, 0.1f, 0.3f);

        private const string TitleFirstLine = "Still";
        private const string TitleSecondLine = "Typing";
        private const float DotSeconds = 0.45f;
        private const float BobHeight = 3f;
        private const float BobSpeed = 1.2f;
        private const int OutlineDirections = 12;
        private const float OutlineWidth = 5f;

        [SerializeField] private Button _playButton;

        private TMP_Text _title;
        private Vector2 _titleRest;
        private Image _glow;
        private readonly System.Collections.Generic.List<TMP_Text> _outline = new System.Collections.Generic.List<TMP_Text>();
        private readonly System.Collections.Generic.List<Vector2> _outlineOffsets = new System.Collections.Generic.List<Vector2>();

        private void Start()
        {
            _playButton.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.InstructionsScene));
            UiSprites.CardButton(_playButton, PlayFill);

            // Settings and credits are copies of Play, placed under it.
            TMP_Text playLabel = _playButton.GetComponentInChildren<TMP_Text>();
            CreditsPanel credits = CreditsPanel.Create(transform, _playButton, playLabel.font);
            SettingsPanel settings = SettingsPanel.Create(transform, _playButton, playLabel.font);

            var playRect = (RectTransform)_playButton.transform;
            float x0 = playRect.anchorMin.x;
            float x1 = playRect.anchorMax.x;
            const float rowHeight = 0.095f;
            const float rowStep = 0.115f;
            float top = 0.37f;
            SetRow(playRect, x0, x1, top - rowHeight, top);

            Button settingsButton = Instantiate(_playButton, _playButton.transform.parent);
            settingsButton.name = "SettingsButton";
            SetRow((RectTransform)settingsButton.transform, x0, x1, top - rowStep - rowHeight, top - rowStep);
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(settings.Show);
            settingsButton.GetComponentInChildren<TMP_Text>().text = "Ajustes";
            UiSprites.CardButton(settingsButton, CreditsFill);

            Button creditsButton = Instantiate(_playButton, _playButton.transform.parent);
            creditsButton.name = "CreditsButton";
            SetRow((RectTransform)creditsButton.transform, x0, x1, top - 2f * rowStep - rowHeight, top - 2f * rowStep);
            creditsButton.onClick.RemoveAllListeners();
            creditsButton.onClick.AddListener(credits.Show);
            creditsButton.GetComponentInChildren<TMP_Text>().text = "Créditos";
            UiSprites.CardButton(creditsButton, CreditsFill);

            StyleTitle();
        }

        private static void SetRow(RectTransform rect, float xMin, float xMax, float yMin, float yMax)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // "Still Typing..." big, with the dots writing themselves like a chat that is about to send something.
        // The letters have a pink gradient, a light outline and a soft pink glow so they stand out on the dark background.
        private void StyleTitle()
        {
            Transform old = transform.Find("Text");
            if (old == null)
            {
                return;
            }

            _title = old.GetComponent<TMP_Text>();
            var rect = _title.rectTransform;
            rect.anchorMin = new Vector2(0.6f, 0.4f);
            rect.anchorMax = new Vector2(0.99f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _titleRest = rect.anchoredPosition;

            // Rounded display font (Fredoka, OFL), friendlier and closer to a social app than the default one.
            var logoFont = Resources.Load<TMP_FontAsset>("Fonts/Fredoka SDF");
            if (logoFont != null)
            {
                _title.font = logoFont;
            }

            _title.enableAutoSizing = true;
            _title.fontSizeMin = 60f;
            _title.fontSizeMax = 210f;
            _title.fontStyle = FontStyles.Bold;
            _title.alignment = TextAlignmentOptions.Center;
            _title.lineSpacing = -18f;
            _title.enableWordWrapping = false;
            _title.raycastTarget = false;
            _title.text = $"{TitleFirstLine}\n{TitleSecondLine}...";

            // Vertical gradient: pale pink on top, saturated pink at the bottom.
            _title.enableVertexGradient = true;
            _title.colorGradient = new VertexGradient(TitleTop, TitleTop, TitleBottom, TitleBottom);
            _title.color = Color.white;

            int baseIndex = old.GetSiblingIndex();
            BuildOutline(baseIndex);

            // A soft pink glow behind the letters, breathing slowly.
            var glow = new GameObject("TitleGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glow.transform.SetParent(transform, false);
            glow.transform.SetSiblingIndex(baseIndex);
            var glowRect = (RectTransform)glow.transform;
            glowRect.anchorMin = new Vector2(0.52f, 0.28f);
            glowRect.anchorMax = new Vector2(1.07f, 1.02f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;
            _glow = glow.GetComponent<Image>();
            _glow.sprite = UiNoise.Glow();
            _glow.color = new Color(0.98f, 0.5f, 0.92f, 0.3f);
            _glow.raycastTarget = false;

            StartCoroutine(AnimateTitle());
        }

        // The outline is made of copies of the title in navy, a few pixels around the real one. The outline of the SDF
        // material breaks with this font's dynamic atlas, and this one also works the same in WebGL.
        private void BuildOutline(int index)
        {
            for (int i = 0; i < OutlineDirections; i++)
            {
                float angle = i * Mathf.PI * 2f / OutlineDirections;
                var go = new GameObject("TitleOutline", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(transform, false);
                go.transform.SetSiblingIndex(index);
                var rect = (RectTransform)go.transform;
                var source = _title.rectTransform;
                rect.anchorMin = source.anchorMin;
                rect.anchorMax = source.anchorMax;
                rect.offsetMin = source.offsetMin;
                rect.offsetMax = source.offsetMax;

                var copy = go.GetComponent<TextMeshProUGUI>();
                copy.font = _title.font;
                copy.fontStyle = _title.fontStyle;
                copy.enableAutoSizing = true;
                copy.fontSizeMin = _title.fontSizeMin;
                copy.fontSizeMax = _title.fontSizeMax;
                copy.alignment = _title.alignment;
                copy.lineSpacing = _title.lineSpacing;
                copy.enableWordWrapping = false;
                copy.color = TitleOutline;
                copy.raycastTarget = false;
                copy.text = _title.text;
                _outline.Add(copy);
                _outlineOffsets.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * OutlineWidth);
            }
        }

        private IEnumerator AnimateTitle()
        {
            var rect = _title.rectTransform;
            float time = 0f;
            float dotTimer = 0f;
            int dots = 0;
            while (true)
            {
                time += Time.unscaledDeltaTime;
                dotTimer += Time.unscaledDeltaTime;

                // The dots appear one by one, rest a moment, and start over. Hidden dots keep their space.
                if (dotTimer >= DotSeconds)
                {
                    dotTimer = 0f;
                    dots = (dots + 1) % 5;
                    int shown = Mathf.Min(dots, 3);
                    string visible = new string('.', shown);
                    string hidden = new string('.', 3 - shown);
                    _title.text = $"{TitleFirstLine}\n{TitleSecondLine}{visible}<alpha=#00>{hidden}";
                    foreach (TMP_Text copy in _outline)
                    {
                        copy.text = _title.text;
                    }
                }

                // A slow float and a soft pulse; the glow breathes with it.
                Vector2 position = _titleRest + new Vector2(0f, Mathf.Sin(time * BobSpeed) * BobHeight);
                float pulse = 1f + 0.005f * Mathf.Sin(time * BobSpeed * 2f);
                rect.anchoredPosition = position;
                rect.localScale = new Vector3(pulse, pulse, 1f);
                for (int i = 0; i < _outline.Count; i++)
                {
                    _outline[i].rectTransform.anchoredPosition = position + _outlineOffsets[i];
                    _outline[i].rectTransform.localScale = rect.localScale;
                }

                _glow.color = new Color(0.98f, 0.5f, 0.92f, 0.28f + 0.1f * Mathf.Sin(time * BobSpeed * 1.5f));
                yield return null;
            }
        }
    }
}
