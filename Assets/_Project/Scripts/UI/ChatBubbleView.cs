using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Chat history made of message bubbles. Built in code inside the History panel of the phone.
    public class ChatBubbleView : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler
    {
        private const string SpriteFolder = "UI/";
        // The bubble sprites are big (their 9-slice borders are 90x62 px): drawing them at a fraction of their
        // size keeps the corners round and the tail small, so the bubble can hug the text.
        private const float PopSeconds = 0.25f;
        private const float SpriteScale = 2.5f;
        private const float MaxBubbleShare = 0.9f;
        private const float MinBubbleWidth = 110f;
        private const float MinBubbleHeight = 52f;
        private const float FontSize = 28f;
        private const float HeaderRatio = 0.7f;
        private const float MinHeaderSize = 20f;
        // Horizontal padding is larger on the side of the bubble tail.
        private const float TailSide = 34f;
        private const float PlainSide = 22f;
        private const float PaddingTop = 12f;
        private const float PaddingBottom = 16f;
        private static readonly Color HeaderColor = new Color(0.25f, 0.56f, 0.68f);

        private enum Kind { Header, Incoming, Outgoing, Hint }

        private struct Entry
        {
            public Kind Kind;
            public Sprite Sprite;
            public string Text;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private RectTransform _content;
        private TMP_FontAsset _font;
        private Coroutine _pop;

        // How far the player scrolled up from the newest message (0 = at the bottom).
        private float _scrollUp;
        private const float WheelStep = 40f;

        public static ChatBubbleView Create(RectTransform parent, TMP_FontAsset font)
        {
            var go = new GameObject("ChatView", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 12f);
            rect.offsetMax = new Vector2(-12f, -10f);
            go.AddComponent<RectMask2D>();

            // An invisible layer that catches the mouse wheel and the dragging, so long chats can be scrolled.
            var catcher = go.AddComponent<Image>();
            catcher.color = new Color(1f, 1f, 1f, 0f);
            catcher.raycastTarget = true;

            var view = go.AddComponent<ChatBubbleView>();
            view._font = font;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(rect, false);
            view._content = (RectTransform)content.transform;
            view._content.anchorMin = new Vector2(0f, 1f);
            view._content.anchorMax = new Vector2(1f, 1f);
            view._content.pivot = new Vector2(0.5f, 1f);
            view._content.offsetMin = Vector2.zero;
            view._content.offsetMax = Vector2.zero;

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return view;
        }

        public void Clear()
        {
            _entries.Clear();
            Build(FontSize);
        }

        // Small grey line above the incoming bubble ("Valerie (hace 2)").
        public void AddHeader(string text)
        {
            Add(Kind.Header, null, text);
        }

        public void AddIncoming(string sender, string text)
        {
            Add(Kind.Incoming, SpriteFor(sender), text);
        }

        public void AddOutgoing(string text)
        {
            Add(Kind.Outgoing, Load("Bubble_4_protagonista"), text);
        }

        // The "..." bubble shown while the sender is writing. Remove it with RemoveLast.
        public void AddTyping(string sender)
        {
            Add(Kind.Incoming, SpriteFor(sender), "...");
        }

        public void RemoveLast()
        {
            if (_entries.Count == 0)
            {
                return;
            }

            _entries.RemoveAt(_entries.Count - 1);
            Refresh();
        }

        // Plain tutorial line, no bubble.
        public void AddHint(string text)
        {
            Add(Kind.Hint, null, text);
        }

        private void Add(Kind kind, Sprite sprite, string text)
        {
            _entries.Add(new Entry { Kind = kind, Sprite = sprite, Text = text });
            Refresh();
            PopLast();
        }

        // The newest bubble pops in: it grows from a bit smaller and fades in.
        private void PopLast()
        {
            if (_content.childCount == 0 || !isActiveAndEnabled)
            {
                return;
            }

            Transform row = _content.GetChild(_content.childCount - 1);
            Transform target = row.Find("Bubble") != null ? row.Find("Bubble") : row;
            if (_pop != null)
            {
                StopCoroutine(_pop);
            }

            _pop = StartCoroutine(PopRoutine(target));
        }

        private IEnumerator PopRoutine(Transform target)
        {
            var group = target.gameObject.AddComponent<CanvasGroup>();
            float time = 0f;
            while (time < PopSeconds && target != null)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / PopSeconds);
                float back = 1f + 2.70158f * Mathf.Pow(k - 1f, 3f) + 1.70158f * Mathf.Pow(k - 1f, 2f);
                target.localScale = Vector3.one * Mathf.LerpUnclamped(0.7f, 1f, back);
                group.alpha = Mathf.Clamp01(k * 3f);
                yield return null;
            }

            if (target != null)
            {
                target.localScale = Vector3.one;
                group.alpha = 1f;
            }

            _pop = null;
        }

        // The text size never changes, so bubbles keep their shape when a new message arrives.
        // When the chat is longer than the panel it scrolls: the newest messages stay in view.
        private void Refresh()
        {
            Build(FontSize);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            // A new message brings the view back to the bottom.
            _scrollUp = 0f;
            ApplyScroll();
        }

        private float BottomOffset()
        {
            float viewport = ((RectTransform)transform).rect.height;
            return Mathf.Max(0f, _content.rect.height - viewport);
        }

        private void ApplyScroll()
        {
            float bottom = BottomOffset();
            _scrollUp = Mathf.Clamp(_scrollUp, 0f, bottom);
            _content.anchoredPosition = new Vector2(0f, bottom - _scrollUp);
        }

        public void OnScroll(PointerEventData eventData)
        {
            _scrollUp += Mathf.Clamp(eventData.scrollDelta.y, -3f, 3f) * WheelStep;
            ApplyScroll();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            _scrollUp += eventData.delta.y / Mathf.Max(0.01f, scale);
            ApplyScroll();
        }

        // Call after the panel changed size so the newest message stays in view.
        public void Relayout()
        {
            Canvas.ForceUpdateCanvases();
            Refresh();
        }

        // Leaves room above the chat for the header of the app.
        public void SetTopInset(float inset)
        {
            var rect = (RectTransform)transform;
            rect.offsetMax = new Vector2(rect.offsetMax.x, -inset);
            Refresh();
        }

        private void Build(float size)
        {
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                GameObject child = _content.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            foreach (Entry entry in _entries)
            {
                switch (entry.Kind)
                {
                    case Kind.Header:
                        BuildLine(entry.Text, Mathf.Max(MinHeaderSize, size * HeaderRatio), TextAlignmentOptions.Left, true);
                        break;
                    case Kind.Hint:
                        BuildLine(entry.Text, Mathf.Max(MinHeaderSize, size * HeaderRatio), TextAlignmentOptions.TopLeft, false);
                        break;
                    default:
                        BuildBubble(entry.Sprite, entry.Text, entry.Kind == Kind.Outgoing, size);
                        break;
                }
            }
        }

        private void BuildLine(string text, float size, TextAlignmentOptions alignment, bool singleLine)
        {
            TMP_Text label = NewText("Line", _content, size, HeaderColor);
            label.text = text;
            label.alignment = alignment;
            if (singleLine)
            {
                label.gameObject.AddComponent<LayoutElement>().preferredHeight = size + 6f;
            }
        }

        private void BuildBubble(Sprite sprite, string text, bool right, float size)
        {
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(_content, false);
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = right ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            var bubble = new GameObject("Bubble", typeof(RectTransform));
            bubble.transform.SetParent(row.transform, false);

            // The sprite lives in its own child that ignores the layout: otherwise its natural size
            // (it is a big picture) makes the bubble taller and wider than the text.
            var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            background.transform.SetParent(bubble.transform, false);
            background.GetComponent<LayoutElement>().ignoreLayout = true;
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            var image = background.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = SpriteScale;
            image.raycastTarget = false;

            float scale = size / FontSize;
            var layout = bubble.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt((right ? PlainSide : TailSide) * scale), Mathf.RoundToInt((right ? TailSide : PlainSide) * scale),
                Mathf.RoundToInt(PaddingTop * scale), Mathf.RoundToInt(PaddingBottom * scale));
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TMP_Text label = NewText("Text", bubble.transform, size, DarkTextFor(sprite));
            label.text = text;
            label.alignment = TextAlignmentOptions.TopLeft;

            // The bubble is as wide as its text, up to a share of the chat, and then the text wraps.
            float maxWidth = ((RectTransform)transform).rect.width * MaxBubbleShare;
            float horizontal = layout.padding.left + layout.padding.right;
            float textWidth = Mathf.Min(maxWidth - horizontal, label.GetPreferredValues(text, maxWidth - horizontal, 0f).x);
            var element = bubble.AddComponent<LayoutElement>();
            element.preferredWidth = Mathf.Max(Mathf.Ceil(textWidth) + horizontal + 2f, MinBubbleWidth * scale);
            element.minHeight = MinBubbleHeight * scale;
        }

        private TMP_Text NewText(string name, Transform parent, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = _font;
            label.fontSize = size;
            label.color = color;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            return label;
        }

        // Dark bubbles (the VIP contact) need light text; the rest are light.
        private static Color DarkTextFor(Sprite sprite)
        {
            bool dark = sprite != null && sprite.name == "Bubble_1_vip";
            return dark ? Color.white : new Color(0.02f, 0.125f, 0.24f);
        }

        private static Sprite SpriteFor(string sender)
        {
            string key = sender != null ? sender.ToLowerInvariant() : "";
            if (key.Contains("vip") || key.Contains("contacto"))
            {
                return Load("Bubble_1_vip");
            }

            if (key.Contains("christian") || key.Contains("cristian"))
            {
                return Load("Bubble_2_cristian");
            }

            return Load("Bubble_3_valerie");
        }

        private static Sprite Load(string name)
        {
            return Resources.Load<Sprite>(SpriteFolder + name);
        }
    }
}
