using System;
using System.Collections.Generic;

namespace DecirQueNo
{
    public class Notification
    {
        public MessageData Message { get; }
        public float Lifetime { get; }
        public float Remaining { get; set; }

        // Phone clock (minutes since midnight) when it arrived, for the "Hace x" label.
        public float ArrivedAtMinutes { get; set; }

        // 1 = just arrived, 0 = about to expire.
        public float Remaining01 => Lifetime <= 0f ? 0f : Remaining / Lifetime;

        public Notification(MessageData message, float lifetime)
        {
            Message = message;
            Lifetime = lifetime;
            Remaining = lifetime;
        }
    }

    public class NotificationSystem
    {
        private readonly List<Notification> _pending = new List<Notification>();

        // Messages that ran out of time. They stay in the chat list (greyed out) so they can still be read.
        private readonly List<Notification> _expiredList = new List<Notification>();

        public IReadOnlyList<Notification> Pending => _pending;
        public IReadOnlyList<Notification> ExpiredList => _expiredList;
        public int Count => _pending.Count;

        public event Action<Notification> OnNotificationAdded;
        public event Action<Notification> OnExpired;
        public event Action OnChanged;

        public bool Contains(Notification notification)
        {
            return _pending.Contains(notification);
        }

        public bool HasExpired(Notification notification)
        {
            return _expiredList.Contains(notification);
        }

        public Notification Add(MessageData message, float lifetime)
        {
            var notification = new Notification(message, lifetime);
            _pending.Add(notification);
            OnNotificationAdded?.Invoke(notification);
            OnChanged?.Invoke();
            return notification;
        }

        // Puts back a notification the player opened and then left, keeping what is left of its life.
        public void Restore(Notification notification)
        {
            if (_pending.Contains(notification))
            {
                return;
            }

            _pending.Insert(0, notification);
            OnChanged?.Invoke();
        }

        // For a notification that expired while the player had it open and then left.
        public void AddExpired(Notification notification)
        {
            if (!_expiredList.Contains(notification))
            {
                _expiredList.Add(notification);
                OnChanged?.Invoke();
            }
        }

        public void Remove(Notification notification)
        {
            if (_pending.Remove(notification))
            {
                OnChanged?.Invoke();
            }
        }

        // Advances every notification and returns the ones that expired (already removed).
        public List<Notification> Tick(float deltaTime)
        {
            List<Notification> expired = null;
            foreach (Notification notification in _pending)
            {
                notification.Remaining -= deltaTime;
                if (notification.Remaining <= 0f)
                {
                    expired = expired ?? new List<Notification>();
                    expired.Add(notification);
                }
            }

            if (expired == null)
            {
                return new List<Notification>();
            }

            foreach (Notification notification in expired)
            {
                _pending.Remove(notification);
                _expiredList.Add(notification);
                OnExpired?.Invoke(notification);
            }

            OnChanged?.Invoke();
            return expired;
        }

        public List<Notification> TakeAll()
        {
            var all = new List<Notification>(_pending);
            _pending.Clear();
            OnChanged?.Invoke();
            return all;
        }
    }
}
