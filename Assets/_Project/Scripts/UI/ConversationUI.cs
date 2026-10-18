using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // The chat screen of the phone: header of the app, messages, the box where the phrase is built,
    // the bank of blocks, the time bar and the send button.
    public class ConversationUI : MonoBehaviour
    {
        private static readonly Color BankColor = new Color(0.56f, 0.76f, 0.84f);
        private static readonly Color UsedColor = new Color(0.2f, 0.42f, 0.52f);
        private static readonly Color PhraseColor = new Color(0.98f, 0.67f, 0.94f);
        private static readonly Color PhraseDropColor = new Color(1f, 0.85f, 0.97f);
        private static readonly Color PlaceholderColor = new Color(0.55f, 0.38f, 0.6f, 0.85f);
        private static readonly Color TimerFull = new Color(0.25f, 0.56f, 0.68f);
        private static readonly Color TimerEmpty = new Color(0.9f, 0.3f, 0.55f);

        private const float TypingSeconds = 0.7f;
        private const float BubbleGapSeconds = 1f;
        private const float ReplyHoldSeconds = 2.5f;
        private const float PulseBelowSeconds = 3f;
        private const float ReplySecondsPerChar = 0.04f;
        private const float ChipSpacing = 7f;
        private const float ExpandedHistoryMin = 0.02f;
        private const string PlaceholderText = "Toca o arrastra los bloques para armar tu respuesta";

        [SerializeField] private ConversationController _conversation;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _historyText;
        [SerializeField] private RectTransform _phraseContainer;
        [SerializeField] private RectTransform _bankContainer;
        [SerializeField] private WordButtonUI _wordButtonPrefab;
        [SerializeField] private Image _timerFill;
        [SerializeField] private RectTransform _timerBar;
        [SerializeField] private Button _sendButton;

        private Canvas _canvas;
        private Image _phraseImage;
        private Color _phraseIdleColor;
        private WordButtonUI _ghost;
        private int _popPosition = -1;
        private float _chipScale = 1f;

        private readonly List<WordButtonUI> _bankButtons = new List<WordButtonUI>();
        private readonly List<GameObject> _phraseButtons = new List<GameObject>();
        private Coroutine _typing;

        // Messages whose arrival was already shown: going back to the list and into the chat again does not replay it.
        private readonly HashSet<MessageData> _revealed = new HashSet<MessageData>();
        private Coroutine _shake;
        private Coroutine _reply;
        private Image _sendIcon;
        private TMP_Text _placeholder;
        private ChatBubbleView _chat;
        private ChatHeaderUI _header;
        private RectTransform _historyRect;
        private float _historyBottomAnchor;
        private GameObject _dock;
        private string _body = "";
        private bool _replying;
        private bool _readOnly;
        private int _lastPhraseCount;
        private int _longestOption;

        private bool ShowEmojis => GameManager.Instance.Config.showEmojis;

        public IReadOnlyList<WordButtonUI> BankButtons => _bankButtons;
        public bool SendInteractable => _sendButton.interactable;

        // Raised when the player leaves an expired message they were reading.
        public event System.Action OnReadOnlyClosed;

        // An expired message can still be read, but nothing can be answered anymore.
        public void ShowReadOnly(MessageData message)
        {
            _readOnly = true;
            _root.SetActive(true);
            if (_dock != null)
            {
                _dock.SetActive(false);
            }

            StopReply();
            if (_typing != null)
            {
                StopCoroutine(_typing);
                _typing = null;
            }

            _header.Show(message.sender);
            _header.SetTyping(false);
            SetBuilderVisible(false);
            _sendButton.gameObject.SetActive(false);
            SetHistoryExpanded(true);
            _body = TextDisplay.ForScreen(message.body, ShowEmojis);
            _chat.Clear();
            AddIncomingBubbles(message.sender, _body);
            _chat.AddHint("Este mensaje venció. Ya no puedes responderlo.");
        }

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
            _canvas = GetComponentInParent<Canvas>().rootCanvas;

            // Round corners for the whole phone UI.
            UiSprites.Round(_historyText.transform.parent.GetComponent<Image>(), 24f);
            UiSprites.Round(_phraseContainer.GetComponent<Image>(), 22f);
            UiSprites.Round(_bankContainer.GetComponent<Image>(), 22f);

            // The timer bar is clipped to a rounded shape, so its fill has round ends too.
            Image timerBackground = _timerBar.GetComponent<Image>();
            if (timerBackground != null)
            {
                UiSprites.Round(timerBackground, 13f);
                _timerBar.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            }

            _phraseImage = _phraseContainer.GetComponent<Image>();
            if (_phraseImage != null)
            {
                _phraseIdleColor = _phraseImage.color;
            }

            // Blocks are as wide as their text and wrap into rows, like words in a sentence.
            SetupFlow(_bankContainer, TextAnchor.UpperCenter);
            SetupFlow(_phraseContainer, TextAnchor.UpperLeft);
            BuildPlaceholder();

            StyleSendButton();
            _sendButton.onClick.AddListener(HandleSendClicked);

            // The bar must shrink as time runs out, not only change color.
            UiFx.MakeHorizontalFill(_timerFill);

            // The chat is drawn with message bubbles under the header of the messaging app.
            _historyRect = (RectTransform)_historyText.transform.parent;
            _historyBottomAnchor = _historyRect.anchorMin.y;
            _chat = ChatBubbleView.Create(_historyRect, _historyText.font);
            _chat.SetTopInset(ChatHeaderUI.Height + 6f);
            _header = ChatHeaderUI.Create(_historyRect, _historyText.font);
            _header.BackClicked += HandleBackClicked;
            _historyText.gameObject.SetActive(false);

            // The row of apps at the bottom of the phone is only for the list of chats.
            Transform dock = transform.Find("Phone/BottomBar");
            _dock = dock != null ? dock.gameObject : null;
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

        private static void SetupFlow(RectTransform container, TextAnchor alignment)
        {
            var grid = container.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                DestroyImmediate(grid);
            }

            var flow = container.gameObject.AddComponent<FlowLayoutGroup>();
            flow.spacingX = ChipSpacing;
            flow.spacingY = ChipSpacing;
            flow.padding = new RectOffset(14, 14, 12, 12);
            flow.childAlignment = alignment;
        }

        // Faint hint inside the empty phrase box. It is the only instruction the player needs in the chat.
        private void BuildPlaceholder()
        {
            var go = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(_phraseContainer, false);
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(22f, 8f);
            rect.offsetMax = new Vector2(-22f, -8f);
            _placeholder = go.GetComponent<TextMeshProUGUI>();
            _placeholder.font = _historyText.font;
            _placeholder.fontSize = 25f;
            _placeholder.fontStyle = FontStyles.Italic;
            _placeholder.color = PlaceholderColor;
            _placeholder.alignment = TextAlignmentOptions.Center;
            _placeholder.raycastTarget = false;
            _placeholder.text = PlaceholderText;
        }

        // A round button with the paper plane of the icon pack.
        private void StyleSendButton()
        {
            var sendFeedback = _sendButton.GetComponent<ButtonFeedback>();
            if (sendFeedback != null)
            {
                sendFeedback.ClickSound = false;
            }

            UiSprites.CardButton(_sendButton, PhraseColor, 46f, 6f);
            TMP_Text label = _sendButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.gameObject.SetActive(false);
            }

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(_sendButton.transform, false);
            var rect = (RectTransform)icon.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(46f, 46f);
            _sendIcon = icon.GetComponent<Image>();
            _sendIcon.raycastTarget = false;
            SetSendIcon("Icon_send");
        }

        private void SetSendIcon(string icon)
        {
            _sendIcon.sprite = Resources.Load<Sprite>("UI/Icons/" + icon);
            _sendIcon.color = UiSprites.Navy;
        }

        private void HandleOpened(ConversationController conversation)
        {
            _readOnly = false;
            _root.SetActive(true);
            if (_dock != null)
            {
                _dock.SetActive(false);
            }

            MessageData message = conversation.Message;
            _header.Show(message.sender);

            // Back from the reply screen of the previous conversation.
            StopReply();
            SetBuilderVisible(true);
            _sendButton.gameObject.SetActive(true);
            SetHistoryExpanded(false);
            SetSendIcon("Icon_send");

            // The messages arrive one at a time.
            if (_typing != null)
            {
                StopCoroutine(_typing);
            }

            _typing = StartCoroutine(ShowMessage(message));

            _chipScale = conversation.Bank.Level.buttonScale;
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
                button.Setup(bank[i], () => conversation.Builder.Add(bankIndex), ShowEmojis, _chipScale);
                AttachDrag(button, bankIndex, -1);
                _bankButtons.Add(button);
            }

            Refresh();
        }

        private IEnumerator ShowMessage(MessageData message)
        {
            _body = TextDisplay.ForScreen(message.body, ShowEmojis);
            _chat.Clear();

            if (_revealed.Contains(message))
            {
                AddIncomingBubbles(message.sender, _body);
                _header.SetTyping(false);
                _typing = null;
                yield break;
            }

            // Bubble 1, a second of "writing...", bubble 2, and so on.
            string[] lines = Lines(_body);
            for (int i = 0; i < lines.Length; i++)
            {
                _header.SetTyping(true);
                _chat.AddTyping(message.sender);
                yield return new WaitForSeconds(i == 0 ? TypingSeconds : BubbleGapSeconds);
                _chat.RemoveLast();
                _chat.AddIncoming(message.sender, lines[i]);
                AudioManager.Play(Sfx.Type);
            }

            _revealed.Add(message);
            _header.SetTyping(false);
            _typing = null;
        }

        // Each line of the script is its own bubble, so long messages read as several short ones.
        private void AddIncomingBubbles(string sender, string text)
        {
            foreach (string line in Lines(text))
            {
                _chat.AddIncoming(sender, line);
            }
        }

        private static string[] Lines(string text)
        {
            var lines = new List<string>();
            foreach (string line in text.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line.Trim());
                }
            }

            return lines.ToArray();
        }

        // ---- Drag and drop: drag a block into the phrase, reorder it there, or drag it out to take it back ----

        private void AttachDrag(WordButtonUI button, int bankIndex, int phrasePosition)
        {
            WordDrag drag = button.GetComponent<WordDrag>();
            if (drag == null)
            {
                drag = button.gameObject.AddComponent<WordDrag>();
            }

            CanvasGroup group = button.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = button.gameObject.AddComponent<CanvasGroup>();
            }

            drag.CanDrag = () => button.Interactable && _conversation.IsOpen;
            drag.Began = e => BeginDrag(button, group);
            drag.Moved = e => MoveGhost(e.position);
            drag.Ended = e => EndDrag(e.position, group, bankIndex, phrasePosition);
        }

        private Camera CanvasCamera => _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        private void BeginDrag(WordButtonUI source, CanvasGroup sourceGroup)
        {
            sourceGroup.alpha = 0.35f;
            AudioManager.Play(Sfx.Type, AudioManager.PlayerTypePitch);

            _ghost = Instantiate(_wordButtonPrefab, _canvas.transform);
            _ghost.Setup(source.Word, null, ShowEmojis, _chipScale);
            _ghost.SetState(true, PhraseColor);
            var feedback = _ghost.GetComponent<ButtonFeedback>();
            if (feedback != null)
            {
                feedback.enabled = false;
            }

            var group = _ghost.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var rect = (RectTransform)_ghost.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = ((RectTransform)source.transform).rect.size;
            rect.localScale = Vector3.one * 1.12f;
            _ghost.transform.SetAsLastSibling();
        }

        private void MoveGhost(Vector2 screenPosition)
        {
            if (_ghost == null)
            {
                return;
            }

            var canvasRect = (RectTransform)_canvas.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, CanvasCamera, out Vector2 local))
            {
                ((RectTransform)_ghost.transform).anchoredPosition = local;
            }

            // The phrase area lights up while a block is over it.
            if (_phraseImage != null)
            {
                bool over = RectTransformUtility.RectangleContainsScreenPoint(_phraseContainer, screenPosition, CanvasCamera);
                _phraseImage.color = over ? PhraseDropColor : _phraseIdleColor;
            }
        }

        private void EndDrag(Vector2 screenPosition, CanvasGroup sourceGroup, int bankIndex, int phrasePosition)
        {
            if (_ghost != null)
            {
                Destroy(_ghost.gameObject);
                _ghost = null;
            }

            if (_phraseImage != null)
            {
                _phraseImage.color = _phraseIdleColor;
            }

            if (sourceGroup != null)
            {
                sourceGroup.alpha = 1f;
            }

            PhraseBuilder builder = _conversation.Builder;
            if (builder == null || !_conversation.IsOpen)
            {
                return;
            }

            bool inside = RectTransformUtility.RectangleContainsScreenPoint(_phraseContainer, screenPosition, CanvasCamera);
            AudioManager.Play(Sfx.Type, AudioManager.PlayerTypePitch);
            if (phrasePosition < 0)
            {
                // From the bank into the phrase, at the spot where it was dropped.
                if (inside)
                {
                    int index = InsertIndex(screenPosition, -1);
                    _popPosition = index;
                    builder.Insert(bankIndex, index);
                }
            }
            else if (inside)
            {
                int index = InsertIndex(screenPosition, phrasePosition);
                _popPosition = index;
                builder.Move(phrasePosition, index);
            }
            else
            {
                // Dropped outside the phrase: the block goes back to the bank.
                builder.RemoveAt(phrasePosition);
            }
        }

        // How many of the blocks already in the phrase come before the drop point, reading left to right, top to bottom.
        private int InsertIndex(Vector2 screenPosition, int ignorePosition)
        {
            int before = 0;
            for (int i = 0; i < _phraseButtons.Count; i++)
            {
                if (i == ignorePosition)
                {
                    continue;
                }

                var rect = (RectTransform)_phraseButtons[i].transform;
                Vector2 center = RectTransformUtility.WorldToScreenPoint(CanvasCamera, rect.position);
                float halfRow = rect.rect.height * _canvas.scaleFactor * 0.5f;
                bool above = center.y > screenPosition.y + halfRow;
                bool sameRow = Mathf.Abs(center.y - screenPosition.y) <= halfRow;
                if (above || (sameRow && center.x < screenPosition.x))
                {
                    before++;
                }
            }

            return before;
        }

        private void HandleSendClicked()
        {
            if (_replying)
            {
                CloseReply();
                return;
            }

            AudioManager.Play(Sfx.Send);
            _conversation.Send();
        }

        // The arrow of the header: back to the list of chats without answering.
        private void HandleBackClicked()
        {
            if (_readOnly)
            {
                AudioManager.Play(Sfx.MenuSelect);
                _readOnly = false;
                _root.SetActive(false);
                ShowDock();
                OnReadOnlyClosed?.Invoke();
                return;
            }

            if (_replying || !_conversation.IsOpen)
            {
                return;
            }

            AudioManager.Play(Sfx.MenuSelect);
            _revealed.Add(_conversation.Message);
            if (_typing != null)
            {
                StopCoroutine(_typing);
                _typing = null;
            }

            _conversation.Abandon();
            _root.SetActive(false);
            ShowDock();
        }

        private void ShowDock()
        {
            if (_dock != null)
            {
                _dock.SetActive(true);
            }
        }

        // While the reply shows, the history takes the room of the word builder.
        private void SetHistoryExpanded(bool expanded)
        {
            Vector2 min = _historyRect.anchorMin;
            min.y = expanded ? ExpandedHistoryMin : _historyBottomAnchor;
            _historyRect.anchorMin = min;
            _chat.Relayout();
        }

        private void SetBuilderVisible(bool visible)
        {
            _bankContainer.gameObject.SetActive(visible);
            _phraseContainer.gameObject.SetActive(visible);
            _timerBar.gameObject.SetActive(visible);
        }

        // Shows what Sam sent and the sender's answer. It closes by itself or when the player taps the arrow.
        private IEnumerator ShowReply(MessageData message, PhraseType type, IReadOnlyList<string> words)
        {
            var phrase = new List<string>();
            foreach (string word in words)
            {
                phrase.Add(TextDisplay.ForScreen(word, ShowEmojis));
            }

            string reply = TextDisplay.ForScreen(message.GetReply(type), ShowEmojis);
            SetHistoryExpanded(true);
            _chat.Clear();
            AddIncomingBubbles(message.sender, _body);
            _chat.AddOutgoing(string.Join(" ", phrase));
            AudioManager.Play(Sfx.Send);

            // After answering there is nothing left to send: the chat closes by itself.
            SetBuilderVisible(false);
            _sendButton.gameObject.SetActive(false);

            // The closing text of the last message is Sam's own monologue, shown in the ending screen:
            // the contact does not say it in the chat.
            string[] lines = message.isFinal ? new string[0] : Lines(reply);

            // The sender answers one bubble at a time, with a pause of "writing..." before each one.
            for (int i = 0; i < lines.Length; i++)
            {
                _header.SetTyping(true);
                _chat.AddTyping(message.sender);
                yield return new WaitForSeconds(i == 0 ? TypingSeconds + 0.3f : BubbleGapSeconds);
                _chat.RemoveLast();
                _chat.AddIncoming(message.sender, lines[i]);
                AudioManager.Play(Sfx.Type);
            }

            _header.SetTyping(false);
            yield return new WaitForSeconds(ReplyHoldSeconds + (lines.Length > 0 ? lines[lines.Length - 1].Length * ReplySecondsPerChar : 0f));
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
            ShowDock();
            _conversation.Close();
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
                button.Setup(builder.Bank[builder.Picked[position]], () => builder.RemoveAt(phrasePosition), ShowEmojis, _chipScale);
                button.SetState(true, PhraseColor);
                AttachDrag(button, builder.Picked[position], position);
                _phraseButtons.Add(button.gameObject);

                // Only the word that was just added (or dropped in) pops.
                int popAt = _popPosition >= 0 ? _popPosition : builder.Count - 1;
                if ((grew || _popPosition >= 0) && position == popAt)
                {
                    button.PlayPop();
                }
            }

            _popPosition = -1;
            _lastPhraseCount = builder.Count;
            _placeholder.gameObject.SetActive(builder.Count == 0);

            bool canSend = _conversation.CanSend();
            _sendButton.interactable = canSend;
            Color icon = UiSprites.Navy;
            icon.a = canSend ? 1f : 0.4f;
            _sendIcon.color = icon;

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
