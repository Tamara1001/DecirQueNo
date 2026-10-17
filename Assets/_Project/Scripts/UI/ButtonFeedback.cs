using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Hover and press feedback for any button: it grows a bit on hover and shrinks when pressed.
    [RequireComponent(typeof(Selectable))]
    public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _hoverScale = 1.07f;
        [SerializeField] private float _pressScale = 0.93f;
        [SerializeField] private float _speed = 20f;

        private Selectable _selectable;
        private bool _hover;
        private bool _pressed;

        // Set by UiFx while another animation owns the scale.
        public bool Locked { get; set; }

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        private void OnEnable()
        {
            _hover = false;
            _pressed = false;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (Locked)
            {
                return;
            }

            float target = 1f;
            if (_selectable != null && _selectable.interactable)
            {
                target = _pressed ? _pressScale : _hover ? _hoverScale : 1f;
            }

            float current = transform.localScale.x;
            float next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-_speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(next, next, 1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            _pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
        }
    }
}
