using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Top bar of the chat, like a messaging app: back arrow, round profile picture with the green "online" dot,
    // the name of the contact with its status, and the call and menu buttons.
    public class ChatHeaderUI : MonoBehaviour
    {
        public const float Height = 104f;

        private static readonly Color OnlineGreen = new Color(0.36f, 0.9f, 0.5f);
        private static readonly Color StatusColor = new Color(0.86f, 1f, 0.93f);

        private TMP_Text _name;
        private TMP_Text _status;
        private TMP_Text _initial;
        private Image _avatar;
        private Image _dot;

        public event Action BackClicked;

        // Builds the bar at the top of a panel. The panel keeps its own background.
        public static ChatHeaderUI Create(RectTransform panel, TMP_FontAsset font)
        {
            var go = new GameObject("ChatHeader", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ChatHeaderUI));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, Height);

            var background = go.GetComponent<Image>();
            background.sprite = Resources.Load<Sprite>("UI/Chat_HeaderBar");
            background.color = Color.white;
            background.raycastTarget = true;
            if (background.sprite == null)
            {
                background.color = new Color(0f, 0.58f, 0.72f);
            }

            var header = go.GetComponent<ChatHeaderUI>();
            header.Build(rect, font);
            return header;
        }

        private void Build(RectTransform bar, TMP_FontAsset font)
        {
            // Back arrow.
            Button back = NewIconButton("Back", bar, "Icon_arrow_back", 46f);
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0.5f);
            backRect.pivot = new Vector2(0f, 0.5f);
            backRect.anchoredPosition = new Vector2(10f, 0f);
            backRect.sizeDelta = new Vector2(64f, 64f);
            back.onClick.AddListener(() => BackClicked?.Invoke());

            // Profile picture: a round placeholder with the initial until the artists send the photos.
            var avatarGo = new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            avatarGo.transform.SetParent(bar, false);
            var avatarRect = (RectTransform)avatarGo.transform;
            avatarRect.anchorMin = avatarRect.anchorMax = new Vector2(0f, 0.5f);
            avatarRect.pivot = new Vector2(0f, 0.5f);
            avatarRect.anchoredPosition = new Vector2(80f, 0f);
            avatarRect.sizeDelta = new Vector2(66f, 66f);
            _avatar = avatarGo.GetComponent<Image>();
            _avatar.raycastTarget = false;
            UiSprites.Round(_avatar, 33f);

            _initial = NewText("Initial", avatarRect, font, 34f, FontStyles.Bold, Color.white);
            Stretch((RectTransform)_initial.transform);

            // The green "online" dot, with a white ring so it reads on any picture.
            var ring = new GameObject("DotRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ring.transform.SetParent(avatarRect, false);
            var ringRect = (RectTransform)ring.transform;
            ringRect.anchorMin = ringRect.anchorMax = new Vector2(1f, 0f);
            ringRect.pivot = new Vector2(0.5f, 0.5f);
            ringRect.anchoredPosition = new Vector2(-6f, 8f);
            ringRect.sizeDelta = new Vector2(24f, 24f);
            var ringImage = ring.GetComponent<Image>();
            ringImage.color = Color.white;
            ringImage.raycastTarget = false;
            UiSprites.Round(ringImage, 12f);

            var dot = new GameObject("Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dot.transform.SetParent(ringRect, false);
            Stretch((RectTransform)dot.transform, 4f);
            _dot = dot.GetComponent<Image>();
            _dot.color = OnlineGreen;
            _dot.raycastTarget = false;
            UiSprites.Round(_dot, 8f);

            // Name and status.
            _name = NewText("Name", bar, font, 36f, FontStyles.Bold, Color.white);
            _name.alignment = TextAlignmentOptions.BottomLeft;
            _name.enableWordWrapping = false;
            _name.overflowMode = TextOverflowModes.Ellipsis;
            var nameRect = _name.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 0.45f);
            nameRect.anchorMax = new Vector2(1f, 0.95f);
            nameRect.offsetMin = new Vector2(160f, 0f);
            nameRect.offsetMax = new Vector2(-130f, 0f);

            _status = NewText("Status", bar, font, 24f, FontStyles.Normal, StatusColor);
            _status.alignment = TextAlignmentOptions.TopLeft;
            _status.enableWordWrapping = false;
            var statusRect = _status.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 0.08f);
            statusRect.anchorMax = new Vector2(1f, 0.46f);
            statusRect.offsetMin = new Vector2(160f, 0f);
            statusRect.offsetMax = new Vector2(-130f, 0f);

            // Call and menu (three dots). They are part of the look; the game does not use them.
            Button menu = NewIconButton("Menu", bar, "Icon_more_vert", 40f);
            var menuRect = (RectTransform)menu.transform;
            menuRect.anchorMin = menuRect.anchorMax = new Vector2(1f, 0.5f);
            menuRect.pivot = new Vector2(1f, 0.5f);
            menuRect.anchoredPosition = new Vector2(-8f, 0f);
            menuRect.sizeDelta = new Vector2(56f, 64f);

            Button call = NewIconButton("Call", bar, "Icon_call", 38f);
            var callRect = (RectTransform)call.transform;
            callRect.anchorMin = callRect.anchorMax = new Vector2(1f, 0.5f);
            callRect.pivot = new Vector2(1f, 0.5f);
            callRect.anchoredPosition = new Vector2(-66f, 0f);
            callRect.sizeDelta = new Vector2(60f, 64f);
        }

        public void Show(string sender)
        {
            _name.text = sender;

            // The picture of the contact when the artists made one (UI/Avatar_<name>), else a colored circle with the initial.
            Sprite portrait = Resources.Load<Sprite>("UI/Avatar_" + ContactKey(sender));
            if (portrait != null)
            {
                _avatar.sprite = portrait;
                _avatar.type = Image.Type.Simple;
                _avatar.preserveAspect = true;
                _avatar.color = Color.white;
                _initial.gameObject.SetActive(false);
            }
            else
            {
                UiSprites.Round(_avatar, 33f);
                _avatar.color = AvatarColor(sender);
                _initial.text = Initial(sender);
                _initial.gameObject.SetActive(true);
            }

            SetTyping(false);
        }

        private static string ContactKey(string sender)
        {
            string key = sender != null ? sender.ToLowerInvariant() : "";
            if (key.Contains("vip") || key.Contains("contacto"))
            {
                return "Vip";
            }

            return key.Contains("christian") || key.Contains("cristian") ? "Christian" : "Valerie";
        }

        // "Escribiendo..." while the contact writes, "En línea" the rest of the time.
        public void SetTyping(bool typing)
        {
            _status.text = typing ? "Escribiendo..." : "En línea";
        }

        private static string Initial(string sender)
        {
            string key = sender != null ? sender.ToLowerInvariant() : "";
            if (key.Contains("vip") || key.Contains("contacto"))
            {
                return "VIP";
            }

            return string.IsNullOrEmpty(sender) ? "?" : sender.Substring(0, 1).ToUpperInvariant();
        }

        private static Color AvatarColor(string sender)
        {
            string key = sender != null ? sender.ToLowerInvariant() : "";
            if (key.Contains("vip") || key.Contains("contacto"))
            {
                return new Color(0.12f, 0.18f, 0.4f);
            }

            return key.Contains("christian") || key.Contains("cristian")
                ? new Color(0.25f, 0.5f, 0.62f)
                : new Color(0.84f, 0.5f, 0.82f);
        }

        private Button NewIconButton(string name, Transform parent, string icon, float iconSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ButtonFeedback));
            go.transform.SetParent(parent, false);
            var hit = go.GetComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            var image = iconGo.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("UI/Icons/" + icon);
            image.color = Color.white;
            image.raycastTarget = false;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            go.GetComponent<ButtonFeedback>().ClickSound = false;
            return button;
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

        private static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
