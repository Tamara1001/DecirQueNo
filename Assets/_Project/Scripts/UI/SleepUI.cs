using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Night: day summary and the mirror at bedtime. Morning: the mirror again when the next day starts.
    public class SleepUI : MonoBehaviour
    {
        private const string CharacterFolder = "Characters/";
        private static readonly Color NightTint = new Color(0.72f, 0.72f, 0.9f);
        private static readonly Color NightText = new Color(0.93f, 0.97f, 1f);
        private static readonly Color DayText = new Color(0.02f, 0.125f, 0.24f);

        [SerializeField] private TMP_Text _summaryText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Image _mirror;
        [SerializeField] private Image[] _tinted;

        private static readonly Color Lilac = new Color(0.98f, 0.8f, 0.96f);
        private static readonly Color NightLilac = new Color(0.74f, 0.7f, 0.93f);
        private static readonly Color Pink = new Color(0.98f, 0.67f, 0.94f);

        private TMP_Text _continueLabel;
        private Image _cardFill;
        private bool _morning;
        private bool _changing;

        // Between the night and the next morning: fade out, a second of black, fade in.
        private const float FadeSeconds = 0.6f;
        private const float BlackSeconds = 1f;

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _continueLabel = _continueButton.GetComponentInChildren<TMP_Text>();
            _continueButton.onClick.AddListener(HandleContinue);
            BuildCard();
            ShowNight();
        }

        // Same look as the opening slides: a card with a navy border next to the mirror.
        private void BuildCard()
        {
            var border = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(transform, false);
            var card = (RectTransform)border.transform;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = new Vector2(560f, 0f);
            card.sizeDelta = new Vector2(760f, 560f);
            var borderImage = border.GetComponent<Image>();
            borderImage.color = UiSprites.Navy;
            borderImage.raycastTarget = false;
            UiSprites.Round(borderImage, 46f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(card, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(8f, 8f);
            fillRect.offsetMax = new Vector2(-8f, -8f);
            _cardFill = fill.GetComponent<Image>();
            _cardFill.color = Lilac;
            _cardFill.raycastTarget = false;
            UiSprites.Round(_cardFill, 38f);

            _summaryText.transform.SetParent(card, false);
            var textRect = _summaryText.rectTransform;
            textRect.anchorMin = new Vector2(0.06f, 0.32f);
            textRect.anchorMax = new Vector2(0.94f, 0.94f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            _summaryText.enableAutoSizing = true;
            _summaryText.fontSizeMin = 30f;
            _summaryText.fontSizeMax = 52f;
            _summaryText.alignment = TextAlignmentOptions.Center;
            _summaryText.color = UiSprites.Navy;
            _summaryText.lineSpacing = 14f;

            _continueButton.transform.SetParent(card, false);
            var buttonRect = (RectTransform)_continueButton.transform;
            buttonRect.anchorMin = new Vector2(0.25f, 0.07f);
            buttonRect.anchorMax = new Vector2(0.75f, 0.25f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            UiSprites.CardButton(_continueButton, Pink);
        }

        private void ShowNight()
        {
            GameManager manager = GameManager.Instance;
            DaySummary s = manager.LastSummary;
            PlayerStats stats = manager.Stats;
            _summaryText.text = s == null
                ? "Fin del día"
                : $"<size=125%><b>Día {s.Day} terminado</b></size>\nSeguidores: {stats.Followers:N0}\n<size=80%>({s.FollowersDelta:+#,0;-#,0;0} hoy)</size>";
            SetLook(true);
            SetLabel(manager.Ending != Ending.None ? "Continuar" : "Dormir");
        }

        private void ShowMorning()
        {
            GameManager manager = GameManager.Instance;
            _morning = true;
            _summaryText.text = $"<size=140%><b>Día {manager.CurrentDay + 1}</b></size>\nUn día nuevo.";
            SetLook(false);
            SetLabel("Empezar el día");
        }

        private void HandleContinue()
        {
            if (_changing)
            {
                return;
            }

            GameManager manager = GameManager.Instance;

            // After the last day there is no morning: the ending follows the night mirror.
            if (_morning || manager.Ending != Ending.None)
            {
                manager.ContinueAfterSleep();
                return;
            }

            StartCoroutine(NightToMorning());
        }

        private IEnumerator NightToMorning()
        {
            _changing = true;
            GameManager manager = GameManager.Instance;
            yield return manager.FadeTo(1f, FadeSeconds);
            yield return new WaitForSecondsRealtime(BlackSeconds);
            ShowMorning();
            yield return manager.FadeTo(0f, FadeSeconds);
            _changing = false;
        }

        // The mirror shows the health without numbers: the expression of Sam. Night uses the shadowed set.
        private void SetLook(bool night)
        {
            GameManager manager = GameManager.Instance;
            _mirror.sprite = Resources.Load<Sprite>(CharacterFolder + ExpressionName(manager.Stats.Health, manager.Config, night));
            foreach (Image image in _tinted)
            {
                image.color = night ? NightTint : Color.white;
            }

            if (_cardFill != null)
            {
                _cardFill.color = night ? NightLilac : Lilac;
            }
        }

        // High sanity: happy. Middle: blushing and uneasy. Low: sad.
        public static string ExpressionName(int health, GameConfig config, bool night)
        {
            // Same sanity tiers as the No buttons: 0 = healthy, 1 = shaken, 2 = broken.
            int tier = config.GetNoLevelIndex(health);
            string expression = tier == 0 ? "Feliz" : tier == 1 ? "Blush" : "Triste";

            return $"Sam_{expression}_{(night ? "Sombra" : "Dia")}";
        }

        private void SetLabel(string text)
        {
            if (_continueLabel != null)
            {
                _continueLabel.text = text;
            }
        }
    }
}
