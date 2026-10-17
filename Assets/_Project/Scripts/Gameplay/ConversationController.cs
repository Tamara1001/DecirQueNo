using System;
using UnityEngine;

namespace DecirQueNo
{
    public class ConversationController : MonoBehaviour
    {
        [SerializeField] private ResponseTimer _timer;

        private MessageData _message;
        private bool _open;

        public MessageData Message => _message;
        public PhraseBuilder Builder { get; private set; }
        public BankBuilder.Bank Bank { get; private set; }
        public bool IsOpen => _open;

        // Seconds the last answer took from opening the conversation.
        public float LastResponseSeconds { get; private set; }
        public ResponseTimer Timer => _timer;

        public event Action<ConversationController> OnOpened;
        public event Action<MessageData, PhraseType, bool> OnResponded;
        public event Action OnCanSendChanged;

        private void Awake()
        {
            _timer.OnTimeout += HandleTimeout;
        }

        private void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.OnTimeout -= HandleTimeout;
            }
        }

        public void Open(MessageData message, float seconds, PlayerStats stats, GameConfig config, int seed, int forcedLevelIndex = -1)
        {
            _message = message;
            Bank = BankBuilder.Build(message, config, stats.Health, seed, forcedLevelIndex);
            Builder = new PhraseBuilder(Bank.Words);
            Builder.OnChanged += () => OnCanSendChanged?.Invoke();
            _open = true;

            OnOpened?.Invoke(this);
            _timer.StartTimer(seconds);
        }

        public bool CanSend()
        {
            return _open && PhraseValidator.TryValidate(Bank.Options, Builder.GetWords(), out _);
        }

        public void Send()
        {
            if (!_open)
            {
                return;
            }

            if (!PhraseValidator.TryValidate(Bank.Options, Builder.GetWords(), out PhraseType type))
            {
                return;
            }

            Finish(type, false);
        }

        // Used by tests and by the day controller when it needs to close the conversation.
        public void ForceTimeout()
        {
            HandleTimeout();
        }

        private void HandleTimeout()
        {
            if (_open)
            {
                Finish(PhraseType.Yes, true);
            }
        }

        private void Finish(PhraseType type, bool timedOut)
        {
            _open = false;
            LastResponseSeconds = _timer.Duration - _timer.Remaining;
            _timer.StopTimer();
            OnResponded?.Invoke(_message, type, timedOut);
        }
    }
}
