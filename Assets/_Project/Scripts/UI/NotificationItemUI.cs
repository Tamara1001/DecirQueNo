using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class NotificationItemUI : MonoBehaviour
    {
        private static readonly Color FullLifeColor = new Color(0.91f, 0.96f, 0.97f);
        private static readonly Color ExpiredColor = new Color(0.74f, 0.74f, 0.78f);
        private static readonly Color ExpiredTextColor = new Color(0.38f, 0.38f, 0.43f);
        private const float BannerCornerScale = 2.3f;
        private const float LifeBarMargin = 0.08f;
        private const float LifeBarBottom = 9f;
        private const float LifeBarHeight = 6f;
        private static readonly Color LowLifeColor = new Color(1f, 0.35f, 0.5f);

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _lifeFill;
        [SerializeField] private CanvasGroup _group;

        private Notification _notification;
        private bool _expiring;
        private bool _expired;
        private DayController _dayController;

        public bool IsExpired => _expired;

        public MessageData Message => _notification?.Message;

        // Same path a mouse click takes. Also used by the test bots.
        public void Click()
        {
            if (!_expiring && !_expired && _button.interactable)
            {
                _button.onClick.Invoke();
            }
        }

        public void Setup(Notification notification, Action<Notification> onClick, bool animate)
        {
            _notification = notification;
            MessageData message = notification.Message;

            ApplyBanner(message.sender);
            StyleLifeBar();
            RefreshLabel();

            var feedback = GetComponent<ButtonFeedback>();
            if (feedback != null)
            {
                feedback.ClickSound = false;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(notification));
            UpdateLife();

            if (animate && gameObject.activeInHierarchy)
            {
                StartCoroutine(UiFx.Pop(transform));
            }
        }

        // The banner of the sender (teal, orchid or navy) replaces the flat background.
        private void ApplyBanner(string sender)
        {
            var background = GetComponent<Image>();
            Sprite banner = SenderStyle.Banner(sender);
            if (background != null && banner != null)
            {
                background.sprite = banner;
                background.type = Image.Type.Sliced;
                // The banner corners are big: drawn smaller they are rounded but not a pill.
                background.pixelsPerUnitMultiplier = BannerCornerScale;
                background.color = Color.white;
            }

            _label.alignment = TextAlignmentOptions.Center;

            _label.color = SenderStyle.BannerText(sender);
        }

        // A thin rounded bar inside the banner that empties as the notification runs out.
        private void StyleLifeBar()
        {
            if (_lifeFill == null)
            {
                return;
            }

            UiSprites.Round(_lifeFill, 3f);
            _lifeFill.raycastTarget = false;
            var rect = _lifeFill.rectTransform;
            rect.pivot = new Vector2(0f, 0f);
            rect.anchorMin = new Vector2(LifeBarMargin, 0f);
            rect.anchoredPosition = new Vector2(0f, LifeBarBottom);
            rect.sizeDelta = new Vector2(0f, LifeBarHeight);
        }

        // The message ran out of time: it stays in the list in grey, readable but no longer answerable.
        public void MarkExpired(Action<MessageData> onRead)
        {
            _expired = true;
            var background = GetComponent<Image>();
            if (background != null)
            {
                // A plain grey pill instead of the colored banner of the sender.
                background.pixelsPerUnitMultiplier = 1f;
                UiSprites.Round(background, 30f);
                background.color = ExpiredColor;
            }

            _label.color = ExpiredTextColor;
            _label.text = $"{_notification.Message.sender}  (vencido)";
            if (_lifeFill != null)
            {
                _lifeFill.gameObject.SetActive(false);
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onRead?.Invoke(_notification.Message));
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
            RefreshLabel();
        }

        // The "Hace x" part follows the phone's clock, so waiting is visible.
        private void RefreshLabel()
        {
            if (_notification == null || _expiring || _expired)
            {
                return;
            }

            // The label follows the phone's clock, so it always matches the time of arrival.
            if (_dayController == null)
            {
                _dayController = FindFirstObjectByType<DayController>();
            }

            string ago = TimeAgo.FormatMinutes(_dayController != null ? _dayController.ClockMinutes - _notification.ArrivedAtMinutes : 0f);
            string text = $"{_notification.Message.sender}  ({ago})";
            if (text != _label.text)
            {
                _label.text = text;
            }
        }

        private void UpdateLife()
        {
            if (_notification == null || _lifeFill == null || _expiring || _expired)
            {
                return;
            }

            float life = Mathf.Clamp01(_notification.Remaining01);
            _lifeFill.rectTransform.anchorMax = new Vector2(LifeBarMargin + (1f - 2f * LifeBarMargin) * life, 0f);
            _lifeFill.color = Color.Lerp(LowLifeColor, FullLifeColor, life);
        }
    }
}
