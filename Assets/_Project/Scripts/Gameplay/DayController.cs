using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    public class DayController : MonoBehaviour
    {
        private const int StartHour = 9;
        private const int EndHour = 23;
        private const float InsistenceDelay = 8f;
        private const float InsistenceCutoff = 15f;
        private const float CarryOverStart = 3f;
        private const float CarryOverGap = 4f;

        [SerializeField] private ConversationController _conversation;

        private readonly List<Scheduled> _scheduled = new List<Scheduled>();
        private readonly DaySummary _summary = new DaySummary();

        private GameManager _manager;
        private PlayerStats _stats;
        private DayData _day;
        private float _elapsed;
        private int _startFollowers;
        private int _startHealth;
        private string _clockText;
        private bool _ended;

        public NotificationSystem Notifications { get; } = new NotificationSystem();
        public int DayNumber => _day != null ? _day.dayNumber : 0;
        public string ClockText => _clockText;

        public event Action<int, string> OnClockChanged;

        private class Scheduled
        {
            public float Time;
            public MessageData Message;
        }

        private void Start()
        {
            _manager = GameManager.Instance;
            _manager.EnsureGame();
            _stats = _manager.Stats;
            _day = _manager.CurrentDayData;
            if (_day == null)
            {
                Debug.LogError("[DayController] No DayData for the current day.");
                enabled = false;
                return;
            }

            _startFollowers = _stats.Followers;
            _startHealth = _stats.Health;
            _summary.Day = _day.dayNumber;

            IReadOnlyList<MessageData> messages = _manager.GetDayMessages(_day.dayNumber);
            for (int i = 0; i < messages.Count; i++)
            {
                float arrival = i < _day.arrivalTimes.Count ? _day.arrivalTimes[i] : _day.arrivalTimes[_day.arrivalTimes.Count - 1];
                Schedule(arrival, messages[i]);
            }

            for (int i = 0; i < _manager.CarryOverMessages.Count; i++)
            {
                Schedule(CarryOverStart + i * CarryOverGap, _manager.CarryOverMessages[i]);
            }

            _manager.CarryOverMessages.Clear();
            _conversation.OnResponded += HandleResponded;
            UpdateClock();
        }

        private void OnDestroy()
        {
            if (_conversation != null)
            {
                _conversation.OnResponded -= HandleResponded;
            }
        }

        private void Update()
        {
            if (_ended)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            Deliver();
            foreach (Notification expired in Notifications.Tick(Time.deltaTime))
            {
                Expire(expired.Message);
            }

            UpdateClock();

            // A conversation already open is never cut off by the clock: the response timer bounds it.
            bool timeIsUp = _elapsed >= _day.dayLength && !_conversation.IsOpen;
            bool allDone = _scheduled.Count == 0 && Notifications.Count == 0 && !_conversation.IsOpen;
            if (timeIsUp || allDone)
            {
                EndDay();
            }
        }

        public void Select(Notification notification)
        {
            if (_ended || _conversation.IsOpen || !Notifications.Contains(notification))
            {
                return;
            }

            MessageData message = notification.Message;
            Notifications.Remove(notification);
            _conversation.Open(message,_manager.Config.GetResponseSeconds(_stats.Health), _stats, _manager.Config, _manager.RunSeed);
        }

        private void Schedule(float time, MessageData message)
        {
            _scheduled.Add(new Scheduled { Time = time, Message = message });
        }

        private void Deliver()
        {
            for (int i = _scheduled.Count - 1; i >= 0; i--)
            {
                if (_scheduled[i].Time <= _elapsed)
                {
                    Notifications.Add(_scheduled[i].Message, _manager.Config.notificationLifetime);
                    _scheduled.RemoveAt(i);
                }
            }
        }

        private void HandleResponded(MessageData message, PhraseType type, bool timedOut)
        {
            _stats.Apply(type, message.effectMultiplier, _conversation.LastResponseSeconds, timedOut);
            switch (type)
            {
                case PhraseType.Yes: _summary.Yes++; break;
                case PhraseType.Maybe: _summary.Maybe++; break;
                default: _summary.No++; break;
            }

            if (type == PhraseType.No)
            {
                QueueInsistence(message);
            }
        }

        // A notification that expired without being opened: costs followers, gives a little health
        // and brings the insistence, but it is not a No the player said.
        private void Expire(MessageData message)
        {
            _stats.ApplyExpired(message.effectMultiplier);
            _summary.Expired++;
            QueueInsistence(message);
        }

        private void QueueInsistence(MessageData message)
        {
            if (message.insistence == null)
            {
                return;
            }

            float arrival = _elapsed + InsistenceDelay;
            if (arrival < _day.dayLength - InsistenceCutoff)
            {
                Schedule(arrival, message.insistence);
            }
            else
            {
                _manager.CarryOverMessages.Add(message.insistence);
            }
        }

        private void UpdateClock()
        {
            float t = Mathf.Clamp01(_elapsed / _day.dayLength);
            int totalMinutes = Mathf.RoundToInt(StartHour * 60 + t * (EndHour - StartHour) * 60);
            string text = $"{totalMinutes / 60:00}:{totalMinutes % 60:00}";
            if (text == _clockText)
            {
                return;
            }

            _clockText = text;
            OnClockChanged?.Invoke(_day.dayNumber, text);
        }

        private void EndDay()
        {
            _ended = true;

            // Notifications still waiting when the day ends expire.
            foreach (Notification notification in Notifications.TakeAll())
            {
                Expire(notification.Message);
            }

            _summary.FollowersDelta = _stats.Followers - _startFollowers;
            _summary.HealthDelta = _stats.Health - _startHealth;
            _manager.LastSummary = _summary;
            _manager.EndDay();
        }
    }
}
