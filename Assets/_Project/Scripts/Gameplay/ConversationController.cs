using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    public class ConversationController : MonoBehaviour
    {
        [SerializeField] private ResponseTimer _timer;

        private MessageData _message;
        private bool _open;
        private bool _replying;

        public MessageData Message => _message;
        public PhraseBuilder Builder { get; private set; }
        public BankBuilder.Bank Bank { get; private set; }

        // True while the player is building an answer.
        public bool IsOpen => _open;

        // True from opening until the sender's reply has been read and the conversation closed.
        public bool IsBusy => _open || _replying;

        // Seconds the last answer took from opening the conversation.
        public float LastResponseSeconds { get; private set; }
        public ResponseTimer Timer => _timer;

        // How long the message waited before being opened, for the "Hace x" label.
        public float OpenedAgeSeconds { get; set; }

        // What the last answer was, for the reply screen.
        public PhraseType LastType { get; private set; }
        public IReadOnlyList<string> LastWords { get; private set; }

        public event Action<ConversationController> OnOpened;
        public event Action<MessageData, PhraseType, bool> OnResponded;

        // Raised when the reply was read and the conversation is over.
        public event Action OnClosed;
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

        public void Open(MessageData message, float seconds, PlayerStats stats, GameConfig config, int seed)
        {
            _message = message;
            Bank = BankBuilder.Build(message, config, stats.Health, seed);
            Builder = new PhraseBuilder(Bank.Words);
            Builder.OnChanged += () => OnCanSendChanged?.Invoke();
            _open = true;
            _replying = false;

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

        // Ends the reply screen. Called by the UI once the player read the sender's answer.
        public void Close()
        {
            if (!_replying)
            {
                return;
            }

            _replying = false;
            OnClosed?.Invoke();
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
            _replying = true;
            LastResponseSeconds = _timer.Duration - _timer.Remaining;
            _timer.StopTimer();

            // When time runs out the game answers Yes for the player.
            LastType = type;
            LastWords = timedOut ? new List<string>(_message.GetWords(PhraseType.Yes)) : Builder.GetWords();
            OnResponded?.Invoke(_message, type, timedOut);
        }
    }
}
