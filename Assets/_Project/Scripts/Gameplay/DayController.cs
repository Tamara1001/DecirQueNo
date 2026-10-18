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

        // The clock always runs at the same pace. After answering a message it only speeds up (this fraction of the
        // day per second) until it reaches the part of the day that the story has reached.
        private const float ClockFastForward = 0.03f;

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
        private int _resolved;
        private float _shownProgress;
        private Notification _openNotification;
        private float _openedAt;

        public NotificationSystem Notifications { get; } = new NotificationSystem();
        public int DayNumber => _day != null ? _day.dayNumber : 0;
        public string ClockText => _clockText;

        // Minutes since midnight on the phone's clock, without the rounding of the text.
        public float ClockMinutes { get; private set; } = StartHour * 60f;

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
            _conversation.OnAbandoned += HandleAbandoned;
            UpdateClock();
        }

        private void OnDestroy()
        {
            if (_conversation != null)
            {
                _conversation.OnResponded -= HandleResponded;
                _conversation.OnAbandoned -= HandleAbandoned;
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
            _openNotification = notification;
            _openedAt = Time.time;
            Notifications.Remove(notification);
            _conversation.OpenedAgeSeconds = notification.Lifetime - notification.Remaining;
            _conversation.Open(message, _manager.Config.responseSeconds, _stats, _manager.Config, _manager.RunSeed);
        }

        // The player left the chat without answering: the notification goes back to the list, and the time
        // spent inside counts against its life, so going back and forth is not a way to stall.
        private void HandleAbandoned()
        {
            if (_openNotification == null)
            {
                return;
            }

            Notification notification = _openNotification;
            _openNotification = null;
            notification.Remaining -= Time.time - _openedAt;
            if (notification.Remaining <= 0f)
            {
                Expire(notification.Message);
                Notifications.AddExpired(notification);
            }
            else
            {
                Notifications.Restore(notification);
            }
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

            Notification arrived = Notifications.Add(_messages[_nextIndex], _manager.Config.notificationLifetime);
            arrived.ArrivedAtMinutes = ClockMinutes;
            _nextIndex++;
            _nextArrivalIn = MessageGap;
        }

        private void HandleResponded(MessageData message, PhraseType type, bool timedOut)
        {
            _resolved++;
            _stats.Apply(type, message, _conversation.LastResponseSeconds, timedOut);
            switch (type)
            {
                case PhraseType.Yes: _summary.Yes++; break;
                case PhraseType.Maybe: _summary.Maybe++; break;
                case PhraseType.Evasive: _summary.Evasive++; break;
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
            _resolved++;
            _stats.ApplyExpired(message);
            _summary.Expired++;
        }

        private void UpdateClock()
        {
            // Constant pace in real time. Every answered message brings the evening closer: the clock then speeds up
            // for a while to catch up, so the player still gets to see the afternoon and the night.
            float total = Mathf.Max(1f, _messages.Count);
            float messageProgress = Mathf.Clamp01(_resolved / total);
            float step = Time.deltaTime / _day.dayLength;
            float behind = Mathf.Max(0f, messageProgress - (_shownProgress + step));
            step += Mathf.Min(ClockFastForward * Time.deltaTime, behind);
            _shownProgress = Mathf.Clamp01(_shownProgress + step);
            float t = _shownProgress;
            ClockMinutes = StartHour * 60 + t * (EndHour - StartHour) * 60;
            int totalMinutes = Mathf.RoundToInt(ClockMinutes);

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
