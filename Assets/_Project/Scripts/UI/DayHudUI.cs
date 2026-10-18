using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Everything that follows the clock of the game: the day card, the status bar of the phone (with the time),
    // the light of the room (day, sunset, night) and the button back to the main menu.
    public class DayHudUI : MonoBehaviour
    {
        private static readonly Color BorderColor = new Color(0.02f, 0.125f, 0.24f);
        private static readonly Color FillColor = new Color(0.98f, 0.8f, 0.96f);
        private static readonly Color Pink = new Color(0.98f, 0.67f, 0.94f);
        private const float BorderWidth = 7f;

        // Light of the room along the day (09:00 to 23:00): clear until the afternoon, golden at sunset, dark at night.
        private static readonly Color DayLight = new Color(1f, 0.95f, 0.8f, 0f);
        private static readonly Color AfternoonLight = new Color(1f, 0.8f, 0.38f, 0.2f);
        private static readonly Color SunsetLight = new Color(1f, 0.5f, 0.16f, 0.36f);
        private static readonly Color NightLight = new Color(0.1f, 0.15f, 0.55f, 0.52f);
        private const float AfternoonStartsAt = 13.5f;
        private const float AfternoonFullAt = 16.5f;
        private const float SunsetPeakAt = 18.5f;
        private const float NightFullAt = 21f;
        private const float LightFollowSpeed = 1.5f;

        [SerializeField] private DayController _day;
        [SerializeField] private TMP_Text _dayText;
        [SerializeField] private TMP_Text _clockText;

        private TMP_Text _phoneClock;
        private Image _light;
        private Color _lightTarget = DayLight;
        private float _savedTimeScale = 1f;
        private GameObject _confirm;

        private void Start()
        {
            Style();
            BuildStatusBar();
            BuildLight();
            BuildMenuButton();
            BuildDockIcons();
            _day.OnClockChanged += HandleClock;
            if (_day.ClockText != null)
            {
                HandleClock(_day.DayNumber, _day.ClockText);
            }
        }

        private void OnDestroy()
        {
            if (_day != null)
            {
                _day.OnClockChanged -= HandleClock;
            }

            // Never leave the game paused behind when the scene changes.
            if (_confirm != null && _confirm.activeSelf)
            {
                Time.timeScale = _savedTimeScale;
            }
        }

        private void Update()
        {
            if (_light != null)
            {
                _light.color = Color.Lerp(_light.color, _lightTarget, LightFollowSpeed * Time.unscaledDeltaTime);
            }
        }

        private void Style()
        {
            Transform panel = _dayText.transform.parent;
            var border = panel.GetComponent<Image>();
            if (border != null)
            {
                border.color = BorderColor;
                UiSprites.Round(border, 34f);
            }

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(panel, false);
            fill.transform.SetAsFirstSibling();
            var rect = (RectTransform)fill.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(BorderWidth, BorderWidth);
            rect.offsetMax = new Vector2(-BorderWidth, -BorderWidth);
            var image = fill.GetComponent<Image>();
            image.color = FillColor;
            image.raycastTarget = false;
            UiSprites.Round(image, 28f);

            // The card only says the day (the time is in the status bar of the phone) and sits next to the menu button.
            _clockText.gameObject.SetActive(false);
            _dayText.color = BorderColor;
            _dayText.fontStyle = FontStyles.Bold;
            _dayText.fontSize = 44f;
            _dayText.enableWordWrapping = false;
            _dayText.alignment = TextAlignmentOptions.Center;
            var dayRect = _dayText.rectTransform;
            dayRect.anchorMin = Vector2.zero;
            dayRect.anchorMax = Vector2.one;
            dayRect.offsetMin = Vector2.zero;
            dayRect.offsetMax = Vector2.zero;

            var card = (RectTransform)panel;
            card.anchorMin = card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(1f, 1f);
            card.sizeDelta = new Vector2(230f, 84f);
            card.anchoredPosition = new Vector2(-(40f + 84f + 14f), -26f);
        }

        // The status bar of the phone: the time on the left of the notch, signal, wifi and battery on the right.
        private void BuildStatusBar()
        {
            Transform phone = transform.Find("Phone");
            if (phone == null)
            {
                return;
            }

            var bar = new GameObject("StatusBar", typeof(RectTransform));
            bar.transform.SetParent(phone, false);
            Transform frame = phone.Find("Frame");
            if (frame != null)
            {
                bar.transform.SetSiblingIndex(frame.GetSiblingIndex());
            }

            var rect = (RectTransform)bar.transform;
            rect.anchorMin = new Vector2(0.1f, 0.926f);
            rect.anchorMax = new Vector2(0.9f, 0.958f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _phoneClock = NewText("Time", rect, 27f);
            _phoneClock.alignment = TextAlignmentOptions.MidlineLeft;
            var timeRect = _phoneClock.rectTransform;
            timeRect.anchorMin = Vector2.zero;
            timeRect.anchorMax = new Vector2(0.35f, 1f);
            timeRect.offsetMin = new Vector2(8f, 0f);
            timeRect.offsetMax = Vector2.zero;

            var icons = new GameObject("Icons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            icons.transform.SetParent(rect, false);
            var iconsRect = (RectTransform)icons.transform;
            iconsRect.anchorMin = new Vector2(0.62f, 0f);
            iconsRect.anchorMax = new Vector2(1f, 1f);
            iconsRect.offsetMin = Vector2.zero;
            iconsRect.offsetMax = new Vector2(-8f, 0f);
            var layout = icons.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            AddStatusIcon(iconsRect, "Icon_signal_cellular_alt");
            AddStatusIcon(iconsRect, "Icon_wifi");
            AddStatusIcon(iconsRect, "Icon_battery_full");
        }

        private static void AddStatusIcon(RectTransform parent, string icon)
        {
            var go = new GameObject(icon, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(28f, 28f);
            var image = go.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("UI/Icons/" + icon);
            image.color = BorderColor;
            image.raycastTarget = false;
        }

        // A tinted layer over the room (behind the phone and the board) that follows the hour of the day.
        private void BuildLight()
        {
            var go = new GameObject("TimeOfDayLight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            Transform phone = transform.Find("Phone");
            if (phone != null)
            {
                go.transform.SetSiblingIndex(phone.GetSiblingIndex());
            }

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _light = go.GetComponent<Image>();
            _light.raycastTarget = false;
            _light.color = DayLight;
        }

        // Round button in the corner: back to the main menu, after asking.
        private void BuildMenuButton()
        {
            var go = new GameObject("MenuButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ButtonFeedback));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-40f, -26f);
            rect.sizeDelta = new Vector2(84f, 84f);

            var button = go.GetComponent<Button>();
            UiSprites.CardButton(button, FillColor, 42f, 6f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(46f, 46f);
            var image = icon.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("UI/Icons/Icon_home");
            image.color = BorderColor;
            image.raycastTarget = false;

            button.onClick.AddListener(ShowConfirm);

            // The low-sanity filter covers everything of the HUD, this button included.
            Transform fog = transform.Find("Fog");
            Transform vignette = transform.Find("Vignette");
            Transform filter = fog != null ? fog : vignette;
            if (filter != null)
            {
                go.transform.SetSiblingIndex(filter.GetSiblingIndex());
            }
        }

        // The home button opens the pause menu: volume of the music and of the effects, and the way back to the main menu.
        private void ShowConfirm()
        {
            if (_confirm == null)
            {
                _confirm = BuildPauseMenu();
            }

            if (_confirm.activeSelf)
            {
                HideConfirm();
                return;
            }

            _confirm.SetActive(true);
            _confirm.transform.SetAsLastSibling();
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        private void HideConfirm()
        {
            _confirm.SetActive(false);
            Time.timeScale = _savedTimeScale;
        }

        private GameObject BuildPauseMenu()
        {
            var root = new GameObject("PauseMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            root.transform.SetParent(transform, false);
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.14f, 0.7f);

            // Tapping outside the card also closes the menu.
            var outside = root.GetComponent<Button>();
            outside.transition = Selectable.Transition.None;
            outside.onClick.AddListener(HideConfirm);

            var border = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(root.transform, false);
            var card = (RectTransform)border.transform;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 660f);
            var borderImage = border.GetComponent<Image>();
            borderImage.color = BorderColor;
            UiSprites.Round(borderImage, 46f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(card, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(8f, 8f);
            fillRect.offsetMax = new Vector2(-8f, -8f);
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = FillColor;
            UiSprites.Round(fillImage, 38f);

            TMP_Text title = NewText("Title", card, 68f);
            title.fontStyle = FontStyles.Bold;
            title.text = "PAUSA";
            Place(title.rectTransform, 0.05f, 0.95f, 0.8f, 0.96f);

            UiSliders.AddVolumeRows(card, _dayText.font, 0.62f, 0.44f);

            Button leave = NewButton("LeaveButton", card, "VOLVER AL MENÚ", Pink, 0.1f, 0.9f, 0.2f, 0.36f);
            leave.onClick.AddListener(() =>
            {
                Time.timeScale = _savedTimeScale;
                GameManager.Instance.BackToMenu();
            });

            Button stay = NewButton("StayButton", card, "Seguir jugando", FillColor, 0.25f, 0.75f, 0.05f, 0.17f);
            stay.onClick.AddListener(HideConfirm);

            root.SetActive(false);
            return root;
        }

        private Button NewButton(string name, Transform parent, string text, Color fill, float xMin, float xMax, float yMin, float yMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ButtonFeedback));
            go.transform.SetParent(parent, false);
            Place((RectTransform)go.transform, xMin, xMax, yMin, yMax);

            TMP_Text label = NewText("Label", go.transform, 34f);
            Place(label.rectTransform, 0f, 1f, 0f, 1f);
            label.text = text;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22f;
            label.fontSizeMax = 36f;

            var button = go.GetComponent<Button>();
            UiSprites.CardButton(button, fill, 36f, 6f);
            return button;
        }

        // Icons over the three squares of the row of apps at the bottom of the chat list: home, camera and chats.
        // They are only decoration.
        private void BuildDockIcons()
        {
            Transform dock = transform.Find("Phone/BottomBar");
            if (dock == null)
            {
                return;
            }

            AddDockIcon(dock, "Icon_home", 0.176f);
            AddDockIcon(dock, "Icon_camera", 0.508f);
            AddDockIcon(dock, "Icon_chat", 0.83f);
        }

        private static void AddDockIcon(Transform dock, string icon, float x)
        {
            var go = new GameObject(icon, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(dock, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(x, 0.5f);
            rect.sizeDelta = new Vector2(52f, 52f);
            var image = go.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("UI/Icons/" + icon);
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private TMP_Text NewText(string name, Transform parent, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = _dayText.font;
            label.fontSize = size;
            label.color = BorderColor;
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

        private void HandleClock(int day, string clock)
        {
            _dayText.text = $"Día {day}";
            _clockText.text = clock;
            if (_phoneClock != null)
            {
                _phoneClock.text = clock;
                _phoneClock.color = BorderColor;
                _phoneClock.fontStyle = FontStyles.Bold;
            }

            _lightTarget = LightAt(HoursOf(clock));
        }

        private static float HoursOf(string clock)
        {
            string[] parts = clock.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
            {
                return hours + minutes / 60f;
            }

            return 9f;
        }

        private static Color LightAt(float hours)
        {
            if (hours <= AfternoonStartsAt)
            {
                return DayLight;
            }

            if (hours < AfternoonFullAt)
            {
                return Color.Lerp(DayLight, AfternoonLight, (hours - AfternoonStartsAt) / (AfternoonFullAt - AfternoonStartsAt));
            }

            if (hours < SunsetPeakAt)
            {
                return Color.Lerp(AfternoonLight, SunsetLight, (hours - AfternoonFullAt) / (SunsetPeakAt - AfternoonFullAt));
            }

            if (hours < NightFullAt)
            {
                return Color.Lerp(SunsetLight, NightLight, (hours - SunsetPeakAt) / (NightFullAt - SunsetPeakAt));
            }

            return NightLight;
        }
    }
}
