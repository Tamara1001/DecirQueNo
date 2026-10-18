using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Settings window of the main menu: music and effects volume. Same look as the pause menu.
    public class SettingsPanel : MonoBehaviour
    {
        private static readonly Color Navy = UiSprites.Navy;
        private static readonly Color Lilac = new Color(0.98f, 0.8f, 0.96f);

        public static SettingsPanel Create(Transform parent, Button backTemplate, TMP_FontAsset font)
        {
            var root = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SettingsPanel));
            root.transform.SetParent(parent, false);
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.14f, 0.7f);

            // Tapping outside the card closes it too.
            var outside = root.AddComponent<Button>();
            outside.transition = Selectable.Transition.None;
            outside.onClick.AddListener(() => root.SetActive(false));

            var border = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(root.transform, false);
            var card = (RectTransform)border.transform;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 560f);
            var borderImage = border.GetComponent<Image>();
            borderImage.color = Navy;
            UiSprites.Round(borderImage, 46f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(card, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(8f, 8f);
            fillRect.offsetMax = new Vector2(-8f, -8f);
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = Lilac;
            fillImage.raycastTarget = false;
            UiSprites.Round(fillImage, 38f);

            var title = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            title.transform.SetParent(card, false);
            var titleText = title.GetComponent<TextMeshProUGUI>();
            titleText.font = font;
            titleText.fontSize = 68f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Navy;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = "AJUSTES";
            titleText.raycastTarget = false;
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.05f, 0.76f);
            titleRect.anchorMax = new Vector2(0.95f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            UiSliders.AddVolumeRows(card, font, 0.55f, 0.35f);

            Button back = Instantiate(backTemplate, card);
            back.name = "BackButton";
            back.onClick.RemoveAllListeners();
            back.onClick.AddListener(() => root.SetActive(false));
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(0.28f, 0.07f);
            backRect.anchorMax = new Vector2(0.72f, 0.24f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            back.GetComponentInChildren<TMP_Text>().text = "Volver";

            root.SetActive(false);
            return root.GetComponent<SettingsPanel>();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }
    }
}
