using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DecirQueNo
{
    // Lets a word block be dragged with the pointer. The conversation screen decides what dropping means.
    public class WordDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Func<bool> CanDrag { get; set; }
        public Action<PointerEventData> Began { get; set; }
        public Action<PointerEventData> Moved { get; set; }
        public Action<PointerEventData> Ended { get; set; }

        private bool _dragging;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CanDrag != null && !CanDrag())
            {
                return;
            }

            _dragging = true;
            Began?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragging)
            {
                Moved?.Invoke(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            Ended?.Invoke(eventData);
        }

        private void OnDisable()
        {
            _dragging = false;
        }
    }
}
