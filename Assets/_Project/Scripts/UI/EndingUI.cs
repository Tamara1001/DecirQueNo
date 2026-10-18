using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // The last screen: the cutscene of the ending (good, bad) with a card with how the run ended. The middle ending
    // has no cutscene yet and keeps Sam in the shadowed mirror, like the opening slides and the Sleep scene.
    public class EndingUI : MonoBehaviour
    {
        private const string ClosingHeader = "Final de la ruta";
        private static readonly Color NightTint = new Color(0.62f, 0.64f, 0.88f);
        private static readonly Color NightLilac = new Color(0.86f, 0.78f, 0.97f);
        private static readonly Color Pink = new Color(0.98f, 0.67f, 0.94f);
        private static readonly Color Accent = new Color(0.62f, 0.25f, 0.62f);

        private const float CutsceneSeconds = 1.6f;
        private const float CutsceneZoom = 1.05f;

        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Sprite _mirrorFrame;

        private RectTransform _cutscene;

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();

            HideOld("TextPanel");
            BuildNight(manager);
            RectTransform card = BuildCard(out Image fill);
            BuildContent(manager, card);
            BuildButtons(manager, card);
            StartCoroutine(PopIn(card, _cutscene != null ? CutsceneSeconds : 0f));
        }

        private void HideOld(string name)
        {
            Transform old = transform.Find(name);
            if (old != null)
            {
                old.gameObject.SetActive(false);
            }
        }

        // Night background and Sam in shadow, with the expression that matches how the run went.
        private void BuildNight(GameManager manager)
        {
            if (BuildCutscene(manager.Ending))
            {
                return;
            }

            Transform background = transform.Find("Background");
            if (background != null)
            {
                background.GetComponent<Image>().color = NightTint;
            }

            var sam = new GameObject("Mirror", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sam.transform.SetParent(transform, false);
            var rect = (RectTransform)sam.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(-288f, -610f);
            rect.sizeDelta = new Vector2(636f, 900f);
            var image = sam.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.sprite = Resources.Load<Sprite>("Characters/Sam_" + Expression(manager.Ending) + "_Sombra");

            if (_mirrorFrame != null)
            {
                var frame = new GameObject("MirrorFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                frame.transform.SetParent(transform, false);
                var frameRect = (RectTransform)frame.transform;
                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;
                frameRect.offsetMin = Vector2.zero;
                frameRect.offsetMax = Vector2.zero;
                var frameImage = frame.GetComponent<Image>();
                frameImage.sprite = _mirrorFrame;
                frameImage.color = NightTint;
                frameImage.raycastTarget = false;
            }
        }

        // The full-screen picture of the ending, under the card. False when this ending has none.
        private bool BuildCutscene(Ending ending)
        {
            string name = ending == Ending.Happy ? "Final_Bueno" : ending == Ending.Neutral ? "Final_Neutral" : "Final_Malo";
            Sprite sprite = name != null ? Resources.Load<Sprite>("Cutscenes/" + name) : null;
            if (sprite == null)
            {
                return false;
            }

            var go = new GameObject("Cutscene", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(transform, false);
            go.transform.SetSiblingIndex(0);
            _cutscene = (RectTransform)go.transform;
            Place(_cutscene, 0f, 1f, 0f, 1f);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            StartCoroutine(RevealCutscene(_cutscene, go.GetComponent<CanvasGroup>()));
            HideOld("Background");
            return true;
        }

        // The picture fades in from black and drifts slowly closer.
        private static IEnumerator RevealCutscene(RectTransform picture, CanvasGroup group)
        {
            float time = 0f;
            while (time < CutsceneSeconds)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / CutsceneSeconds);
                group.alpha = k;
                picture.localScale = Vector3.one * Mathf.Lerp(CutsceneZoom, 1f, k);
                yield return null;
            }

            group.alpha = 1f;
            picture.localScale = Vector3.one;
        }

        private static string Expression(Ending ending)
        {
            switch (ending)
            {
                case Ending.Happy: return "Feliz";
                case Ending.Neutral: return "Blush";
                default: return "Triste";
            }
        }

        private RectTransform BuildCard(out Image fill)
        {
            var border = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            border.transform.SetParent(transform, false);
            var card = (RectTransform)border.transform;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = new Vector2(520f, 0f);
            card.sizeDelta = new Vector2(860f, 900f);
            var borderImage = border.GetComponent<Image>();
            borderImage.color = UiSprites.Navy;
            borderImage.raycastTarget = false;
            UiSprites.Round(borderImage, 46f);

            var inner = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            inner.transform.SetParent(card, false);
            var fillRect = (RectTransform)inner.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(8f, 8f);
            fillRect.offsetMax = new Vector2(-8f, -8f);
            fill = inner.GetComponent<Image>();
            fill.color = NightLilac;
            fill.raycastTarget = false;
            UiSprites.Round(fill, 38f);
            return card;
        }

        private void BuildContent(GameManager manager, RectTransform card)
        {
            PlayerStats stats = manager.Stats;
            string title;
            string epilogue;
            switch (manager.Ending)
            {
                case Ending.Happy:
                    title = "Decir que no";
                    epilogue = "Perdiste números. Recuperaste tu voz.";
                    break;
                case Ending.Neutral:
                    title = "A medias";
                    epilogue = "No llegaste al millón y tampoco dijiste que no. Sigues en el mismo lugar, esperando la próxima notificación.";
                    break;
                default:
                    title = "No dije que no";
                    epilogue = "Llegaste al millón. Ya no queda nadie en el espejo.";
                    break;
            }

            // The closing monologue of the script, without its "Final de la ruta" header.
            string closing = manager.LastFinalText;
            if (!string.IsNullOrEmpty(closing))
            {
                closing = TextDisplay.ForScreen(closing, manager.Config.showEmojis).Trim();
                if (closing.StartsWith(ClosingHeader))
                {
                    closing = closing.Substring(ClosingHeader.Length).Trim();
                }
            }

            TMP_Text heading = NewText("Heading", card, _text.font, 30f, FontStyles.Bold, Accent);
            heading.text = "FINAL";
            Place(heading.rectTransform, 0.06f, 0.94f, 0.9f, 0.97f);

            TMP_Text name = NewText("Title", card, _text.font, 72f, FontStyles.Bold, UiSprites.Navy);
            name.text = title;
            name.enableAutoSizing = true;
            name.fontSizeMin = 44f;
            name.fontSizeMax = 76f;
            Place(name.rectTransform, 0.06f, 0.94f, 0.78f, 0.9f);

            // The old scene text is reused for the body, inside the card.
            _text.transform.SetParent(card, false);
            _text.enableAutoSizing = true;
            _text.fontSizeMin = 24f;
            _text.fontSizeMax = 40f;
            _text.alignment = TextAlignmentOptions.Top;
            _text.color = UiSprites.Navy;
            _text.lineSpacing = 10f;
            _text.enableWordWrapping = true;
            _text.text =
                (string.IsNullOrEmpty(closing) ? "" : $"<i>{closing}</i>\n\n") +
                $"<b>{epilogue}</b>\n\n" +
                $"<color=#9E409E>Seguidores: {stats.Followers:N0}\nSalud mental: {Mathf.RoundToInt(100f * stats.Health / manager.Config.maxHealth)}%</color>";
            Place(_text.rectTransform, 0.08f, 0.92f, 0.22f, 0.77f);
        }

        private void BuildButtons(GameManager manager, RectTransform card)
        {
            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(manager.StartNewGame);
            _restartButton.transform.SetParent(card, false);
            Place((RectTransform)_restartButton.transform, 0.05f, 0.48f, 0.04f, 0.17f);
            UiSprites.CardButton(_restartButton, Pink);
            _restartButton.GetComponentInChildren<TMP_Text>().text = "Reiniciar";

            _menuButton.onClick.RemoveAllListeners();
            _menuButton.onClick.AddListener(manager.BackToMenu);
            _menuButton.transform.SetParent(card, false);
            Place((RectTransform)_menuButton.transform, 0.52f, 0.95f, 0.04f, 0.17f);
            UiSprites.CardButton(_menuButton, new Color(0.98f, 0.8f, 0.96f));
            _menuButton.GetComponentInChildren<TMP_Text>().text = "Volver al menú";
        }

        // The card fades in and settles from a slightly smaller size.
        private IEnumerator PopIn(RectTransform card, float delay)
        {
            var group = card.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            for (float wait = 0f; wait < delay; wait += Time.unscaledDeltaTime)
            {
                yield return null;
            }

            const float seconds = 0.5f;
            float time = 0f;
            while (time < seconds)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / seconds);
                float back = 1f + 2.70158f * Mathf.Pow(k - 1f, 3f) + 1.70158f * Mathf.Pow(k - 1f, 2f);
                card.localScale = Vector3.one * Mathf.LerpUnclamped(0.9f, 1f, back);
                group.alpha = Mathf.Clamp01(k * 2.5f);
                yield return null;
            }

            card.localScale = Vector3.one;
            group.alpha = 1f;
        }

        private static TMP_Text NewText(string name, Transform parent, TMP_FontAsset font, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static void Place(RectTransform rect, float xMin, float xMax, float yMin, float yMax)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
