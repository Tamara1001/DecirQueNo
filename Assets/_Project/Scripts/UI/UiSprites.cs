using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // A white rounded rectangle made at runtime, so any flat-colored Image can get round corners.
    // The Image color still tints it.
    public static class UiSprites
    {
        private const int Size = 64;
        private const int Radius = 24;
        private const int Border = 26;

        private static Sprite _rounded;

        public static Sprite Rounded
        {
            get
            {
                if (_rounded == null)
                {
                    _rounded = Create();
                }

                return _rounded;
            }
        }

        // Rounds an Image. radius is the size of the corners in UI units.
        public static void Round(Image image, float radius = 20f)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = Rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = Border / Mathf.Max(1f, radius);
        }

        public static readonly Color Navy = new Color(0.02f, 0.125f, 0.24f);

        // Styles a button like the day card: navy border, flat fill and bold navy text.
        // Safe to call again on a copy of a styled button to change its fill color.
        public static void CardButton(Button button, Color fill, float radius = 34f, float border = 7f)
        {
            var frame = button.GetComponent<Image>();
            frame.color = Navy;
            Round(frame, radius);

            Transform existing = button.transform.Find("Fill");
            Image inner;
            if (existing != null)
            {
                inner = existing.GetComponent<Image>();
            }
            else
            {
                var go = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(button.transform, false);
                go.transform.SetAsFirstSibling();
                var rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(border, border);
                rect.offsetMax = new Vector2(-border, -border);
                inner = go.GetComponent<Image>();
                inner.raycastTarget = false;
                Round(inner, radius - border);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.color = Navy;
                    label.fontStyle = FontStyles.Bold;
                    label.fontSize *= 1.1f;
                }
            }

            inner.color = fill;

            // Hover and press tint the fill, not the border.
            button.targetGraphic = inner;
        }

        private static Sprite Create()
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    // Distance to the closest corner circle, with a soft 1 px edge.
                    float cx = Mathf.Clamp(x + 0.5f, Radius, Size - Radius);
                    float cy = Mathf.Clamp(y + 0.5f, Radius, Size - Radius);
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float alpha = Mathf.Clamp01(Radius - distance + 0.5f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(Border, Border, Border, Border));
        }
    }
}
