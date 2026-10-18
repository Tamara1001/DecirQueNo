using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Volume sliders shared by the pause menu and the settings of the main menu.
    public static class UiSliders
    {
        private static readonly Color Navy = UiSprites.Navy;
        private static readonly Color Pink = new Color(0.98f, 0.67f, 0.94f);

        // A row "Label  o------" with a slider from 0 to 1, placed in a card by anchors (y is the bottom of the row).
        public static void AddRow(RectTransform card, TMP_FontAsset font, string label, float y, float value, UnityAction<float> onChange, Action onRelease = null)
        {
            var name = new GameObject(label + "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            name.transform.SetParent(card, false);
            var text = name.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = 36f;
            text.fontStyle = FontStyles.Bold;
            text.color = Navy;
            text.text = label;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            Place(text.rectTransform, 0.08f, 0.34f, y, y + 0.14f);

            var go = new GameObject(label + "Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(card, false);
            Place((RectTransform)go.transform, 0.36f, 0.92f, y, y + 0.14f);

            // Track.
            var track = new GameObject("Track", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            track.transform.SetParent(go.transform, false);
            var trackRect = (RectTransform)track.transform;
            trackRect.anchorMin = new Vector2(0f, 0.5f);
            trackRect.anchorMax = new Vector2(1f, 0.5f);
            trackRect.sizeDelta = new Vector2(0f, 20f);
            var trackImage = track.GetComponent<Image>();
            trackImage.color = new Color(Navy.r, Navy.g, Navy.b, 0.22f);
            UiSprites.Round(trackImage, 10f);

            // Filled part.
            var area = new GameObject("FillArea", typeof(RectTransform));
            area.transform.SetParent(go.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = new Vector2(0f, 0.5f);
            areaRect.anchorMax = new Vector2(1f, 0.5f);
            areaRect.sizeDelta = new Vector2(-44f, 20f);
            var fillBar = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillBar.transform.SetParent(area.transform, false);
            var barRect = (RectTransform)fillBar.transform;
            barRect.sizeDelta = new Vector2(15f, 0f);
            var barImage = fillBar.GetComponent<Image>();
            barImage.color = Pink;
            UiSprites.Round(barImage, 10f);

            // Handle: a navy ring with a white dot.
            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = (RectTransform)handleArea.transform;
            handleAreaRect.anchorMin = new Vector2(0f, 0.5f);
            handleAreaRect.anchorMax = new Vector2(1f, 0.5f);
            handleAreaRect.sizeDelta = new Vector2(-44f, 44f);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = (RectTransform)handle.transform;
            handleRect.sizeDelta = new Vector2(44f, 0f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = Navy;
            UiSprites.Round(handleImage, 22f);
            var inner = new GameObject("Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            inner.transform.SetParent(handle.transform, false);
            var innerRect = (RectTransform)inner.transform;
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(7f, 7f);
            innerRect.offsetMax = new Vector2(-7f, -7f);
            var innerImage = inner.GetComponent<Image>();
            innerImage.color = Color.white;
            innerImage.raycastTarget = false;
            UiSprites.Round(innerImage, 15f);

            var slider = go.GetComponent<Slider>();
            slider.fillRect = barRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(onChange);

            if (onRelease != null)
            {
                var up = go.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                entry.callback.AddListener(_ => onRelease());
                up.triggers.Add(entry);
            }
        }

        // The two sliders of the game: music and effects. A sample plays when the effects handle is released.
        public static void AddVolumeRows(RectTransform card, TMP_FontAsset font, float musicY, float sfxY)
        {
            AudioManager audio = AudioManager.Instance;
            AddRow(card, font, "Música", musicY, audio != null ? audio.MusicLevel : 0.5f, value =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.MusicLevel = value;
                }
            });

            AddRow(card, font, "Efectos", sfxY, audio != null ? audio.SfxLevel : 0.8f, value =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SfxLevel = value;
                }
            }, () => AudioManager.Play(Sfx.MenuSelect));
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
