using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Replaces the system cursor with the Kenney cursor pack (arrow, and a pointing hand over buttons).
    // Starts by itself on boot and survives scene loads.
    public class GameCursor : MonoBehaviour
    {
        private const string ArrowPath = "UI/Cursor/cursor_arrow";
        private const string HandPath = "UI/Cursor/cursor_hand";

        // Tip of the arrow and of the index finger, in texture pixels (top-left origin).
        private static readonly Vector2 ArrowHotspot = new Vector2(19f, 13f);
        private static readonly Vector2 HandHotspot = new Vector2(19f, 5f);

        private Texture2D _arrow;
        private Texture2D _hand;
        private bool _overButton;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject("GameCursor");
            DontDestroyOnLoad(go);
            go.AddComponent<GameCursor>();
        }

        private void Awake()
        {
            _arrow = Resources.Load<Texture2D>(ArrowPath);
            _hand = Resources.Load<Texture2D>(HandPath);
            Apply(false);
        }

        private void Update()
        {
            bool over = IsOverButton();
            if (over != _overButton) Apply(over);
        }

        private bool IsOverButton()
        {
            if (EventSystem.current == null || Mouse.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
            _hits.Clear();
            EventSystem.current.RaycastAll(data, _hits);
            foreach (RaycastResult hit in _hits)
            {
                var selectable = hit.gameObject.GetComponentInParent<Selectable>();
                if (selectable != null) return selectable.IsInteractable();
            }
            return false;
        }

        private void Apply(bool hand)
        {
            _overButton = hand;
            Texture2D tex = hand ? _hand : _arrow;
            if (tex == null) return;
            Cursor.SetCursor(tex, hand ? HandHotspot : ArrowHotspot, CursorMode.Auto);
        }
    }
}
