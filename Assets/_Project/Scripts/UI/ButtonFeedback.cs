using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Hover and press feedback for any button, with a springy feel: it grows and tilts a little on hover,
    // squashes when pressed and bounces back when released.
    [RequireComponent(typeof(Selectable))]
    public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _pressScale = 0.92f;

        // Spring: stiffness pulls to the target, damping calms the bounce.
        private const float Stiffness = 380f;
        private const float Damping = 22f;
        private const float HoverTiltDegrees = 1.6f;
        private const float ClickKick = 5f;

        private Selectable _selectable;
        private bool _hover;
        private bool _pressed;
        private Vector2 _scale = Vector2.one;
        private Vector2 _scaleVelocity;
        private float _tilt;
        private float _tiltVelocity;

        // Plays the menu select sound on click; word, send and notification buttons have their own sounds.
        public bool ClickSound { get; set; } = true;

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
            _scale = Vector2.one;
            _scaleVelocity = Vector2.zero;
            _tilt = 0f;
            _tiltVelocity = 0f;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        private void OnDisable()
        {
            transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (Locked)
            {
                // Another animation owns the scale; keep the spring in sync with it.
                _scale = new Vector2(transform.localScale.x, transform.localScale.y);
                _scaleVelocity = Vector2.zero;
                return;
            }

            bool active = _selectable != null && _selectable.interactable;
            Vector2 targetScale = Vector2.one;
            float targetTilt = 0f;
            if (active && _pressed)
            {
                // A squash: wider and flatter.
                targetScale = new Vector2(_pressScale + 0.03f, _pressScale - 0.05f);
            }
            else if (active && _hover)
            {
                targetScale = Vector2.one * _hoverScale;
                targetTilt = HoverTiltDegrees;
            }

            // Fixed small steps keep the spring stable at any frame rate.
            float remaining = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            while (remaining > 0f)
            {
                float dt = Mathf.Min(remaining, 1f / 120f);
                remaining -= dt;
                _scaleVelocity += ((targetScale - _scale) * Stiffness - _scaleVelocity * Damping) * dt;
                _scale += _scaleVelocity * dt;
                _tiltVelocity += ((targetTilt - _tilt) * Stiffness - _tiltVelocity * Damping) * dt;
                _tilt += _tiltVelocity * dt;
            }

            transform.localScale = new Vector3(_scale.x, _scale.y, 1f);
            transform.localRotation = Quaternion.Euler(0f, 0f, _tilt);
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

        public void OnPointerClick(PointerEventData eventData)
        {
            // A little kick so the button pops back out after the press.
            _scaleVelocity += Vector2.one * ClickKick;

            if (ClickSound && _selectable != null && _selectable.interactable)
            {
                AudioManager.Play(Sfx.MenuSelect);
            }
        }
    }
}
