using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        private readonly HashSet<Notification> _expiring = new HashSet<Notification>();
        private bool _subscribed;

        private void Start()
        {
            _day.Notifications.OnChanged += Refresh;
            _day.Notifications.OnExpired += HandleExpired;
            _conversation.OnOpened += HandleOpened;
            _conversation.OnClosed += HandleClosed;
            _subscribed = true;
            Refresh();
        }

        private void OnDestroy()
        {
            if (!_subscribed)
            {
                return;
            }

            _day.Notifications.OnChanged -= Refresh;
            _day.Notifications.OnExpired -= HandleExpired;
            if (_conversation != null)
            {
                _conversation.OnOpened -= HandleOpened;
                _conversation.OnClosed -= HandleClosed;
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

        private void HandleExpired(Notification notification)
        {
            _expiring.Add(notification);
        }

        private void Refresh()
        {
            // Remove the items whose notification is gone: expired ones fade out, opened ones just disappear.
            var gone = new List<Notification>();
            foreach (KeyValuePair<Notification, NotificationItemUI> pair in _items)
            {
                if (!_day.Notifications.Contains(pair.Key))
                {
                    gone.Add(pair.Key);
                }
            }

            foreach (Notification notification in gone)
            {
                NotificationItemUI item = _items[notification];
                _items.Remove(notification);
                if (_expiring.Remove(notification))
                {
                    item.PlayExpire();
                }
                else
                {
                    Destroy(item.gameObject);
                }
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

            _emptyText.gameObject.SetActive(_items.Count == 0);
        }
    }
}
