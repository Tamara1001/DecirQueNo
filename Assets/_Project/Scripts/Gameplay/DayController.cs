using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    // Plays one day of the script: the messages arrive one at a time, always in the same order.
    public class DayController : MonoBehaviour
    {
        private const int StartHour = 9;
        private const int EndHour = 23;
        private const float FirstMessageDelay = 3f;
        private const float MessageGap = 5f;

        [SerializeField] private ConversationController _conversation;

        private readonly DaySummary _summary = new DaySummary();

        private GameManager _manager;
        private PlayerStats _stats;
        private DayData _day;
        private IReadOnlyList<MessageData> _messages;
        private int _nextIndex;
        private float _nextArrivalIn;
        private float _elapsed;
        private int _startFollowers;
        private int _startHealth;
        private string _clockText;
        private bool _ended;

        public NotificationSystem Notifications { get; } = new NotificationSystem();
        public int DayNumber => _day != null ? _day.dayNumber : 0;
        public string ClockText => _clockText;

        public event Action<int, string> OnClockChanged;

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

            _messages = _manager.GetDayMessages(_day.dayNumber);
            _nextArrivalIn = FirstMessageDelay;
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
            foreach (Notification expired in Notifications.Tick(Time.deltaTime))
            {
                Expire(expired.Message);
            }

            DeliverNext();
            UpdateClock();

            if (_conversation.IsBusy)
            {
                return;
            }

            // Reaching the goal ends the run right away.
            if (_stats.Followers >= _manager.Config.goalFollowers)
            {
                EndDay(true);
                return;
            }

            // The day ends once every message of the script was answered or expired.
            if (_nextIndex >= _messages.Count && Notifications.Count == 0)
            {
                EndDay();
            }
        }

        public void Select(Notification notification)
        {
            if (_ended || _conversation.IsBusy || !Notifications.Contains(notification))
            {
                return;
            }

            MessageData message = notification.Message;
            Notifications.Remove(notification);
            _conversation.OpenedAgeSeconds = notification.Lifetime - notification.Remaining;
            _conversation.Open(message, _manager.Config.GetResponseSeconds(_stats.Health), _stats, _manager.Config, _manager.RunSeed);
        }

        // The next message arrives only after the previous one was answered or expired, so the story keeps its order.
        private void DeliverNext()
        {
            if (_nextIndex >= _messages.Count || Notifications.Count > 0 || _conversation.IsBusy)
            {
                return;
            }

            _nextArrivalIn -= Time.deltaTime;
            if (_nextArrivalIn > 0f)
            {
                return;
            }

            Notifications.Add(_messages[_nextIndex], _manager.Config.notificationLifetime);
            _nextIndex++;
            _nextArrivalIn = MessageGap;
        }

        private void HandleResponded(MessageData message, PhraseType type, bool timedOut)
        {
            _stats.Apply(type, message, _conversation.LastResponseSeconds, timedOut);
            switch (type)
            {
                case PhraseType.Yes: _summary.Yes++; break;
                case PhraseType.Maybe: _summary.Maybe++; break;
                default: _summary.No++; break;
            }

            if (message.isFinal)
            {
                _manager.LastFinalText = message.GetReply(type);
            }
        }

        // A notification that expired without being opened: it counts as an evasive answer with a surcharge.
        private void Expire(MessageData message)
        {
            _stats.ApplyExpired(message);
            _summary.Expired++;
        }

        private void UpdateClock()
        {
            float t = Mathf.Clamp01(_elapsed / _day.dayLength);
            int totalMinutes = Mathf.RoundToInt(StartHour * 60 + t * (EndHour - StartHour) * 60);

            // Only tens of minutes are shown: the exact minutes would add pressure for nothing.
            totalMinutes -= totalMinutes % 10;
            string text = $"{totalMinutes / 60:00}:{totalMinutes % 60:00}";
            if (text == _clockText)
            {
                return;
            }

            _clockText = text;
            OnClockChanged?.Invoke(_day.dayNumber, text);
        }

        private void EndDay(bool goalReached = false)
        {
            _ended = true;

            // Notifications still waiting when the day ends expire, unless the run ended by reaching the goal.
            foreach (Notification notification in Notifications.TakeAll())
            {
                if (!goalReached)
                {
                    Expire(notification.Message);
                }
            }

            _summary.FollowersDelta = _stats.Followers - _startFollowers;
            _summary.HealthDelta = _stats.Health - _startHealth;
            _manager.LastSummary = _summary;
            _manager.EndDay(goalReached);
        }
    }
}
