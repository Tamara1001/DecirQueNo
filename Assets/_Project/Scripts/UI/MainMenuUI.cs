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
        private static readonly Color TitleColor = new Color(0.98f, 0.72f, 0.95f);

        private const string TitleFirstLine = "Still";
        private const string TitleSecondLine = "Typing";
        private const float DotSeconds = 0.45f;
        private const float BobHeight = 10f;
        private const float BobSpeed = 1.6f;

        [SerializeField] private Button _playButton;

        private TMP_Text _title;
        private Vector2 _titleRest;

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

            _title.enableAutoSizing = true;
            _title.fontSizeMin = 60f;
            _title.fontSizeMax = 210f;
            _title.fontStyle = FontStyles.Bold;
            _title.alignment = TextAlignmentOptions.Center;
            _title.lineSpacing = -18f;
            _title.enableWordWrapping = false;
            _title.color = TitleColor;
            _title.outlineWidth = 0.28f;
            _title.outlineColor = UiSprites.Navy;
            _title.raycastTarget = false;
            _title.text = $"{TitleFirstLine}\n{TitleSecondLine}...";

            StartCoroutine(AnimateTitle());
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
                }

                // A slow float and a soft pulse.
                rect.anchoredPosition = _titleRest + new Vector2(0f, Mathf.Sin(time * BobSpeed) * BobHeight);
                float pulse = 1f + 0.015f * Mathf.Sin(time * BobSpeed * 2f);
                rect.localScale = new Vector3(pulse, pulse, 1f);
                yield return null;
            }
        }
    }
}
