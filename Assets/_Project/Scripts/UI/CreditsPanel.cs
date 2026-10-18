using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Credits window of the main menu. Built in code with the same look as the day card:
    // a lilac card with a navy border over a dimmed screen.
    public class CreditsPanel : MonoBehaviour
    {
        private static readonly Color Navy = new Color(0.02f, 0.125f, 0.24f);
        private static readonly Color Lilac = new Color(0.98f, 0.8f, 0.96f);
        private static readonly Color RoleColor = new Color(0.62f, 0.25f, 0.62f);

        private const float BorderWidth = 8f;

        // Role, then one line per person.
        private const string Credits =
            "<b><color=#9E409E>Programación</color></b>\n" +
            "Tamara D'Angelo  <size=75%>@Tamara1001</size>\n\n" +
            "<b><color=#9E409E>Game Design</color></b>\n" +
            "Eugenia Vullioud  <size=75%>@madness_returns</size>\n\n" +
            "<b><color=#9E409E>Arte 2D, UI/UX</color></b>\n" +
            "Massiel Colín  <size=75%>@LittlePoorBoy</size>\n" +
            "Jessie  <size=75%>@jessie033305</size>\n" +
            "Aldana  <size=75%>@aldana.rp</size>\n\n" +
            "<b><color=#9E409E>Narrativa</color></b>\n" +
            "Manuel Martinez Miraglia  <size=75%>@ManuSMC</size>\n" +
            "Jessica Briseño  <size=75%>@JessBriss</size>\n\n" +
            "<b><color=#9E409E>Audio para Videojuegos</color></b>\n" +
            "Oriana Oseguera  <size=75%>@Ori</size>\n\n" +
            "<size=65%>Iconos: Material Icons (Google), licencia Apache 2.0</size>";

        // Creates the panel (hidden) under parent. backTemplate is cloned for the close button.
        public static CreditsPanel Create(Transform parent, Button backTemplate, TMP_FontAsset font)
        {
            var root = new GameObject("CreditsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CreditsPanel));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform, 0f, 0f, 0f, 0f);

            // Dimmed screen; clicking it closes the window too.
            var dim = root.GetComponent<Image>();
            dim.color = new Color(0.02f, 0.05f, 0.14f, 0.7f);
            var close = root.AddComponent<Button>();
            close.transition = Selectable.Transition.None;
            close.onClick.AddListener(() => root.SetActive(false));

            // Navy border and lilac fill.
            Image border = NewImage("Card", root.transform, Navy);
            var card = (RectTransform)border.transform;
            card.anchorMin = new Vector2(0.27f, 0.07f);
            card.anchorMax = new Vector2(0.73f, 0.93f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            UiSprites.Round(border, 44f);
            border.raycastTarget = true;

            Image fill = NewImage("Fill", card, Lilac);
            Stretch((RectTransform)fill.transform, BorderWidth, BorderWidth, BorderWidth, BorderWidth);
            UiSprites.Round(fill, 38f);

            TMP_Text title = NewText("Title", card, font, "Créditos", 64f, FontStyles.Bold);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.05f, 0.88f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TMP_Text body = NewText("Body", card, font, Credits, 36f, FontStyles.Normal);
            body.enableAutoSizing = true;
            body.fontSizeMin = 18f;
            body.fontSizeMax = 38f;
            var bodyRect = body.rectTransform;
            bodyRect.anchorMin = new Vector2(0.06f, 0.2f);
            bodyRect.anchorMax = new Vector2(0.94f, 0.87f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            Button back = Instantiate(backTemplate, card);
            back.name = "BackButton";
            back.onClick.RemoveAllListeners();
            back.onClick.AddListener(() => root.SetActive(false));
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(0.3f, 0.05f);
            backRect.anchorMax = new Vector2(0.7f, 0.15f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            back.GetComponentInChildren<TMP_Text>().text = "Volver";

            root.SetActive(false);
            return root.GetComponent<CreditsPanel>();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private static Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text NewText(string name, Transform parent, TMP_FontAsset font, string text, float size, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = Navy;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
