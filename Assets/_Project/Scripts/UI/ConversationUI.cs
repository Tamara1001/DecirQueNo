using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class ConversationUI : MonoBehaviour
    {
        private static readonly Color BankColor = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color UsedColor = new Color(0.4f, 0.4f, 0.4f);
        private static readonly Color PhraseColor = new Color(0.6f, 0.85f, 1f);
        private static readonly Color TimerFull = new Color(0.35f, 0.8f, 0.4f);
        private static readonly Color TimerEmpty = new Color(0.9f, 0.25f, 0.25f);

        private const float TypingSeconds = 0.4f;
        private const float PulseBelowSeconds = 3f;
        private const float ReplyMinSeconds = 3f;
        private const float ReplySecondsPerChar = 0.04f;

        [SerializeField] private ConversationController _conversation;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _historyText;
        [SerializeField] private RectTransform _phraseContainer;
        [SerializeField] private RectTransform _bankContainer;
        [SerializeField] private GridLayoutGroup _phraseGrid;
        [SerializeField] private GridLayoutGroup _bankGrid;
        [SerializeField] private WordButtonUI _wordButtonPrefab;
        [SerializeField] private Image _timerFill;
        [SerializeField] private RectTransform _timerBar;
        [SerializeField] private Button _sendButton;
        [SerializeField] private Vector2 _baseCellSize = new Vector2(160f, 70f);
        [SerializeField] private float _baseSpacing = 12f;

        private readonly List<WordButtonUI> _bankButtons = new List<WordButtonUI>();
        private readonly List<GameObject> _phraseButtons = new List<GameObject>();
        private Coroutine _typing;
        private Coroutine _shake;
        private Coroutine _reply;
        private TMP_Text _sendLabel;
        private string _historyBase = "";
        private bool _replying;
        private int _lastPhraseCount;
        private int _longestOption;

        private bool ShowEmojis => GameManager.Instance.Config.showEmojis;

        public IReadOnlyList<WordButtonUI> BankButtons => _bankButtons;
        public bool SendInteractable => _sendButton.interactable;

        // Same path a mouse click takes. Also used by the test bots.
        public void ClickSend()
        {
            if (_sendButton.interactable)
            {
                _sendButton.onClick.Invoke();
            }
        }

        private void Awake()
        {
            _conversation.OnOpened += HandleOpened;
            _conversation.OnResponded += HandleResponded;
            _conversation.OnCanSendChanged += Refresh;
            _conversation.Timer.OnTick += HandleTick;
            _sendButton.onClick.AddListener(HandleSendClicked);
            _sendLabel = _sendButton.GetComponentInChildren<TMP_Text>();

            // The bar must shrink as time runs out, not only change color.
            UiFx.MakeHorizontalFill(_timerFill);
            _root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_conversation == null)
            {
                return;
            }

            _conversation.OnOpened -= HandleOpened;
            _conversation.OnResponded -= HandleResponded;
            _conversation.OnCanSendChanged -= Refresh;
            _conversation.Timer.OnTick -= HandleTick;
        }

        private void HandleOpened(ConversationController conversation)
        {
            _root.SetActive(true);
            MessageData message = conversation.Message;

            // Back from the reply screen of the previous conversation.
            StopReply();
            SetBuilderVisible(true);
            SetSendLabel("Enviar");

            // The message "types" for a moment before it shows up.
            if (_typing != null)
            {
                StopCoroutine(_typing);
            }

            _typing = StartCoroutine(ShowMessage(message));

            ApplyButtonScale(conversation.Bank.Level.buttonScale);

            _longestOption = 0;
            foreach (PhraseOption option in conversation.Bank.Options)
            {
                _longestOption = Mathf.Max(_longestOption, option.Words.Count);
            }

            foreach (WordButtonUI button in _bankButtons)
            {
                Destroy(button.gameObject);
            }

            _bankButtons.Clear();
            _lastPhraseCount = 0;

            IReadOnlyList<string> bank = conversation.Builder.Bank;
            for (int i = 0; i < bank.Count; i++)
            {
                int bankIndex = i;
                WordButtonUI button = Instantiate(_wordButtonPrefab, _bankContainer);
                button.Setup(bank[i], () => conversation.Builder.Add(bankIndex), ShowEmojis);
                _bankButtons.Add(button);
            }

            Refresh();
        }

        private IEnumerator ShowMessage(MessageData message)
        {
            string ago = TimeAgo.Format(_conversation.OpenedAgeSeconds, GameManager.Instance.CurrentDayData.dayLength);
            string header = $"{message.sender} ({ago})\n";
            _historyText.text = header + "...";
            yield return new WaitForSeconds(TypingSeconds);
            _historyBase = header + TextDisplay.ForScreen(message.body, ShowEmojis);
            _historyText.text = IsFirstConversation() ? _historyBase + "\n\n" + TutorialHint : _historyBase;
            _typing = null;
        }

        // Short tutorial: only the very first conversation of the run explains what to do.
        private const string TutorialHint =
            "Toca los fragmentos para armar una de las frases posibles y toca Enviar. Cada frase tiene un costo distinto. Cuidado con el tiempo.";

        private static bool IsFirstConversation()
        {
            PlayerStats stats = GameManager.Instance.Stats;
            return stats.YesCount + stats.EvasiveCount + stats.NoCount + stats.ExpiredCount == 0;
        }

        private void HandleSendClicked()
        {
            if (_replying)
            {
                CloseReply();
                return;
            }

            _conversation.Send();
        }

        private void SetSendLabel(string text)
        {
            if (_sendLabel != null)
            {
                _sendLabel.text = text;
            }
        }

        private void SetBuilderVisible(bool visible)
        {
            _bankContainer.gameObject.SetActive(visible);
            _phraseContainer.gameObject.SetActive(visible);
            _timerBar.gameObject.SetActive(visible);
        }

        // Shows what Sam sent and the sender's answer. It closes by itself or when the player taps Continuar.
        private IEnumerator ShowReply(MessageData message, PhraseType type, IReadOnlyList<string> words)
        {
            var phrase = new List<string>();
            foreach (string word in words)
            {
                phrase.Add(TextDisplay.ForScreen(word, ShowEmojis));
            }

            string reply = TextDisplay.ForScreen(message.GetReply(type), ShowEmojis);
            string speaker = message.isFinal ? "" : message.sender + ": ";
            _historyText.text = _historyBase + "\n\nSam: " + string.Join(" ", phrase) + "\n\n" + speaker + reply;

            SetBuilderVisible(false);
            _sendButton.interactable = true;
            SetSendLabel("Continuar");

            yield return new WaitForSeconds(ReplyMinSeconds + reply.Length * ReplySecondsPerChar);
            CloseReply();
        }

        private void StopReply()
        {
            if (_reply != null)
            {
                StopCoroutine(_reply);
                _reply = null;
            }

            _replying = false;
        }

        private void CloseReply()
        {
            if (!_replying)
            {
                return;
            }

            StopReply();
            _root.SetActive(false);
            _conversation.Close();
        }

        private void ApplyButtonScale(float scale)
        {
            Vector2 cell = _baseCellSize * scale;
            Vector2 spacing = new Vector2(_baseSpacing, _baseSpacing) * scale;
            _bankGrid.cellSize = cell;
            _bankGrid.spacing = spacing;
            _phraseGrid.cellSize = cell;
            _phraseGrid.spacing = spacing;
        }

        private void HandleResponded(MessageData message, PhraseType type, bool timedOut)
        {
            if (_typing != null)
            {
                StopCoroutine(_typing);
                _typing = null;
            }

            _timerBar.localScale = Vector3.one;

            // The conversation stays open to show the sender's reply; CloseReply ends it.
            StopReply();
            _replying = true;
            _reply = StartCoroutine(ShowReply(message, type, _conversation.LastWords));
        }

        private void HandleTick(float normalized)
        {
            _timerFill.fillAmount = normalized;
            _timerFill.color = Color.Lerp(TimerEmpty, TimerFull, normalized);

            // The bar throbs during the last seconds.
            float seconds = normalized * _conversation.Timer.Duration;
            if (seconds > 0f && seconds <= PulseBelowSeconds)
            {
                float pulse = 1f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 9f));
                _timerBar.localScale = new Vector3(1f, pulse, 1f);
            }
            else
            {
                _timerBar.localScale = Vector3.one;
            }
        }

        private void Refresh()
        {
            if (_conversation.Builder == null || _replying)
            {
                return;
            }

            PhraseBuilder builder = _conversation.Builder;

            for (int i = 0; i < _bankButtons.Count; i++)
            {
                bool picked = builder.IsPicked(i);
                _bankButtons[i].SetState(!picked, picked ? UsedColor : BankColor);
            }

            foreach (GameObject go in _phraseButtons)
            {
                Destroy(go);
            }

            _phraseButtons.Clear();

            bool grew = builder.Count > _lastPhraseCount;
            for (int position = 0; position < builder.Count; position++)
            {
                int phrasePosition = position;
                WordButtonUI button = Instantiate(_wordButtonPrefab, _phraseContainer);
                button.Setup(builder.Bank[builder.Picked[position]], () => builder.RemoveAt(phrasePosition), ShowEmojis);
                button.SetState(true, PhraseColor);
                _phraseButtons.Add(button.gameObject);

                // Only the word that was just added pops.
                if (grew && position == builder.Count - 1)
                {
                    button.PlayPop();
                }
            }

            _lastPhraseCount = builder.Count;
            bool canSend = _conversation.CanSend();
            _sendButton.interactable = canSend;

            // A phrase as long as the longest answer that still does not match is plainly wrong.
            if (grew && !canSend && builder.Count >= _longestOption)
            {
                if (_shake != null)
                {
                    StopCoroutine(_shake);
                }

                _shake = StartCoroutine(UiFx.Shake(_phraseContainer));
            }
        }
    }
}
