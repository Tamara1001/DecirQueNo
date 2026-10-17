using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class NotificationItemUI : MonoBehaviour
    {
        private static readonly Color FullLifeColor = new Color(0.35f, 0.8f, 0.4f);
        private static readonly Color LowLifeColor = new Color(0.9f, 0.3f, 0.3f);
        private static readonly Color InsistenceColor = new Color(1f, 0.75f, 0.75f);

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _lifeFill;
        [SerializeField] private CanvasGroup _group;

        private Notification _notification;
        private bool _expiring;

        public MessageData Message => _notification?.Message;

        // Same path a mouse click takes. Also used by the test bots.
        public void Click()
        {
            if (!_expiring && _button.interactable)
            {
                _button.onClick.Invoke();
            }
        }

        public void Setup(Notification notification, Action<Notification> onClick, bool animate)
        {
            _notification = notification;
            MessageData message = notification.Message;

            // Insistence messages are marked in red so they stand out.
            bool insistent = message.pressure > 0;
            _label.text = insistent ? $"!  {message.sender}  ({message.timeAgo})" : $"{message.sender}  ({message.timeAgo})";
            var background = GetComponent<Image>();
            if (insistent && background != null)
            {
                background.color = InsistenceColor;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(notification));
            UpdateLife();

            if (animate && gameObject.activeInHierarchy)
            {
                StartCoroutine(UiFx.Pop(transform));
            }
        }

        // Fades and shrinks the item before it disappears. Used when the notification expired.
        public void PlayExpire()
        {
            _expiring = true;
            _button.interactable = false;
            if (!gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }

            StartCoroutine(ExpireRoutine());
        }

        private IEnumerator ExpireRoutine()
        {
            yield return UiFx.FadeAndShrink(_group, transform);
            Destroy(gameObject);
        }

        private void Update()
        {
            UpdateLife();
        }

        private void UpdateLife()
        {
            if (_notification == null || _lifeFill == null || _expiring)
            {
                return;
            }

            float life = Mathf.Clamp01(_notification.Remaining01);
            _lifeFill.fillAmount = life;
            _lifeFill.color = Color.Lerp(LowLifeColor, FullLifeColor, life);
        }
    }
}
