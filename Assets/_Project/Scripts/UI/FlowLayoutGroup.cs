using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Lays children out left to right and wraps them into rows, like words in a paragraph.
    // Every child keeps its own preferred size (set it with a LayoutElement).
    public class FlowLayoutGroup : LayoutGroup
    {
        public float spacingX = 8f;
        public float spacingY = 8f;

        private struct Item
        {
            public RectTransform Rect;
            public float Width;
            public float Height;
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            float widest = 0f;
            foreach (RectTransform child in rectChildren)
            {
                widest = Mathf.Max(widest, LayoutUtility.GetPreferredSize(child, 0));
            }

            SetLayoutInputForAxis(widest + padding.horizontal, widest + padding.horizontal, -1f, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            float height = Arrange(false);
            SetLayoutInputForAxis(height, height, -1f, 1);
        }

        public override void SetLayoutHorizontal()
        {
            Arrange(true);
        }

        public override void SetLayoutVertical()
        {
            Arrange(true);
        }

        // Places the rows and returns the height they need. With apply false it only measures.
        private float Arrange(bool apply)
        {
            float available = rectTransform.rect.width - padding.horizontal;
            var rows = new List<List<Item>>();
            var current = new List<Item>();
            float currentWidth = 0f;

            foreach (RectTransform child in rectChildren)
            {
                var item = new Item
                {
                    Rect = child,
                    Width = LayoutUtility.GetPreferredSize(child, 0),
                    Height = LayoutUtility.GetPreferredSize(child, 1),
                };

                float needed = current.Count == 0 ? item.Width : currentWidth + spacingX + item.Width;
                if (current.Count > 0 && needed > available)
                {
                    rows.Add(current);
                    current = new List<Item>();
                    currentWidth = 0f;
                    needed = item.Width;
                }

                current.Add(item);
                currentWidth = needed;
            }

            if (current.Count > 0)
            {
                rows.Add(current);
            }

            float total = 0f;
            var rowHeights = new List<float>();
            foreach (List<Item> row in rows)
            {
                float rowHeight = 0f;
                foreach (Item item in row)
                {
                    rowHeight = Mathf.Max(rowHeight, item.Height);
                }

                rowHeights.Add(rowHeight);
                total += rowHeight;
            }

            if (rows.Count > 1)
            {
                total += spacingY * (rows.Count - 1);
            }

            float contentHeight = total + padding.vertical;
            if (!apply)
            {
                return contentHeight;
            }

            float y = padding.top;
            if (childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleRight)
            {
                y += Mathf.Max(0f, (rectTransform.rect.height - contentHeight) * 0.5f);
            }

            bool centered = childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter;
            for (int r = 0; r < rows.Count; r++)
            {
                float rowWidth = 0f;
                foreach (Item item in rows[r])
                {
                    rowWidth += item.Width;
                }

                rowWidth += spacingX * (rows[r].Count - 1);
                float x = padding.left + (centered ? Mathf.Max(0f, (available - rowWidth) * 0.5f) : 0f);
                foreach (Item item in rows[r])
                {
                    SetChildAlongAxis(item.Rect, 0, x, item.Width);
                    SetChildAlongAxis(item.Rect, 1, y + (rowHeights[r] - item.Height) * 0.5f, item.Height);
                    x += item.Width + spacingX;
                }

                y += rowHeights[r] + spacingY;
            }

            return contentHeight;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            spacingX = Mathf.Max(0f, spacingX);
            spacingY = Mathf.Max(0f, spacingY);
        }
#endif
    }
}
