using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class NotificationListUI : MonoBehaviour
    {
        [SerializeField] private DayController _day;
        [SerializeField] private ConversationController _conversation;
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _container;
        [SerializeField] private NotificationItemUI _itemPrefab;
        [SerializeField] private TMP_Text _emptyText;

        private readonly Dictionary<Notification, NotificationItemUI> _items = new Dictionary<Notification, NotificationItemUI>();
        private ConversationUI _conversationUi;
        private bool _subscribed;

        private void Start()
        {
            UiSprites.Round(_container.GetComponent<Image>(), 22f);
            _day.Notifications.OnChanged += Refresh;
            _day.Notifications.OnNotificationAdded += HandleAdded;
            _conversation.OnOpened += HandleOpened;
            _conversation.OnClosed += HandleClosed;
            _conversation.OnAbandoned += HandleClosed;
            _conversationUi = FindFirstObjectByType<ConversationUI>();
            if (_conversationUi != null)
            {
                _conversationUi.OnReadOnlyClosed += HandleClosed;
            }

            _subscribed = true;
            Refresh();
        }

        private void HandleAdded(Notification notification)
        {
            AudioManager.Play(Sfx.Notification);
        }

        private void OnDestroy()
        {
            if (!_subscribed)
            {
                return;
            }

            _day.Notifications.OnChanged -= Refresh;
            _day.Notifications.OnNotificationAdded -= HandleAdded;
            if (_conversationUi != null)
            {
                _conversationUi.OnReadOnlyClosed -= HandleClosed;
            }

            if (_conversation != null)
            {
                _conversation.OnOpened -= HandleOpened;
                _conversation.OnClosed -= HandleClosed;
                _conversation.OnAbandoned -= HandleClosed;
            }
        }

        private void HandleOpened(ConversationController conversation)
        {
            _root.SetActive(false);
        }

        private void HandleClosed()
        {
            _root.SetActive(true);
            Refresh();
        }

        // Opens an expired message to read it, without the word builder.
        private void ReadExpired(MessageData message)
        {
            if (_conversation.IsBusy || _conversationUi == null)
            {
                return;
            }

            _root.SetActive(false);
            _conversationUi.ShowReadOnly(message);
        }

        private void Refresh()
        {
            // Remove the items whose notification is gone (opened ones just disappear). Expired ones stay, in grey.
            var gone = new List<Notification>();
            foreach (KeyValuePair<Notification, NotificationItemUI> pair in _items)
            {
                if (!_day.Notifications.Contains(pair.Key) && !_day.Notifications.HasExpired(pair.Key))
                {
                    gone.Add(pair.Key);
                }
            }

            foreach (Notification notification in gone)
            {
                Destroy(_items[notification].gameObject);
                _items.Remove(notification);
            }

            // Add the new arrivals, with a pop only when the list is visible.
            foreach (Notification notification in _day.Notifications.Pending)
            {
                if (_items.ContainsKey(notification))
                {
                    continue;
                }

                NotificationItemUI item = Instantiate(_itemPrefab, _container);
                item.Setup(notification, _day.Select, _root.activeInHierarchy);
                _items[notification] = item;
            }

            foreach (Notification notification in _day.Notifications.ExpiredList)
            {
                if (!_items.TryGetValue(notification, out NotificationItemUI item))
                {
                    item = Instantiate(_itemPrefab, _container);
                    item.Setup(notification, _day.Select, false);
                    _items[notification] = item;
                }

                if (!item.IsExpired)
                {
                    item.MarkExpired(ReadExpired);
                }
            }

            _emptyText.gameObject.SetActive(_items.Count == 0);
        }
    }
}
