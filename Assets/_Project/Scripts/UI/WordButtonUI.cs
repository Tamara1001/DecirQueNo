using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class WordButtonUI : MonoBehaviour
    {
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

        public void Setup(string word, Action onClick, bool showEmojis = false)
        {
            Word = word;
            _label.text = TextDisplay.ForScreen(word, showEmojis);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void SetState(bool interactable, Color color)
        {
            _button.interactable = interactable;
            _background.color = color;
        }
    }
}
