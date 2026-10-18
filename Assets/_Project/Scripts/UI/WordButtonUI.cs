using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class WordButtonUI : MonoBehaviour
    {
        // A block is as wide as its text: font, height and padding scale with the difficulty tier.
        private const float BaseFontSize = 26f;
        private const float BaseHeight = 46f;
        private const float PaddingX = 20f;

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _background;

        // The original fragment (what the phrase validator compares), not what is shown on screen.
        public string Word { get; private set; }
        public bool Interactable => _button.interactable;

        // Same path a mouse click takes. Also used by the test bots.
        public void Click()
        {
            if (_button.interactable)
            {
                _button.onClick.Invoke();
            }
        }

        public void PlayPop()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(UiFx.Pop(transform, 0.7f, 0.15f));
            }
        }

        public void Setup(string word, Action onClick, bool showEmojis = false, float scale = 1f)
        {
            Word = word;
            UiSprites.Round(_background, 18f * scale);
            _label.enableAutoSizing = false;
            _label.enableWordWrapping = false;
            _label.overflowMode = TextOverflowModes.Overflow;
            _label.fontSize = BaseFontSize * scale;
            _label.alignment = TextAlignmentOptions.Center;
            _label.text = TextDisplay.ForScreen(word, showEmojis);

            // Inset the text so it never touches the edge of the block.
            var labelRect = _label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(PaddingX * 0.5f * scale, 2f);
            labelRect.offsetMax = new Vector2(-PaddingX * 0.5f * scale, -2f);

            var element = GetComponent<LayoutElement>();
            if (element == null)
            {
                element = gameObject.AddComponent<LayoutElement>();
            }

            element.preferredWidth = Mathf.Ceil(_label.GetPreferredValues(_label.text).x) + PaddingX * scale;
            element.preferredHeight = BaseHeight * scale;

            DisableClickSound();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                AudioManager.Play(Sfx.Type, AudioManager.PlayerTypePitch);
                onClick?.Invoke();
            });
        }

        private void DisableClickSound()
        {
            var feedback = GetComponent<ButtonFeedback>();
            if (feedback != null)
            {
                feedback.ClickSound = false;
            }
        }

        public void SetState(bool interactable, Color color)
        {
            _button.interactable = interactable;
            _background.color = color;
        }
    }
}
