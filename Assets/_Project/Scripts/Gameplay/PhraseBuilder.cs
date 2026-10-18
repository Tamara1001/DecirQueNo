using System;
using System.Collections.Generic;

namespace DecirQueNo
{
    // Holds the words the player picked, in order. Words are referenced by bank index
    // so the bank can contain repeated words.
    public class PhraseBuilder
    {
        private readonly List<string> _bank;
        private readonly List<int> _picked = new List<int>();

        public event Action OnChanged;

        public IReadOnlyList<string> Bank => _bank;
        public IReadOnlyList<int> Picked => _picked;
        public int Count => _picked.Count;

        public PhraseBuilder(IEnumerable<string> bank)
        {
            _bank = new List<string>(bank);
        }

        public bool IsPicked(int bankIndex)
        {
            return _picked.Contains(bankIndex);
        }

        public void Add(int bankIndex)
        {
            if (bankIndex < 0 || bankIndex >= _bank.Count || IsPicked(bankIndex))
            {
                return;
            }

            _picked.Add(bankIndex);
            OnChanged?.Invoke();
        }

        // Puts a bank word at a position of the phrase (used when the player drags it in).
        public void Insert(int bankIndex, int phrasePosition)
        {
            if (bankIndex < 0 || bankIndex >= _bank.Count || IsPicked(bankIndex))
            {
                return;
            }

            _picked.Insert(Math.Clamp(phrasePosition, 0, _picked.Count), bankIndex);
            OnChanged?.Invoke();
        }

        // Reorders a word of the phrase. toPosition is the position it ends up at.
        public void Move(int fromPosition, int toPosition)
        {
            if (fromPosition < 0 || fromPosition >= _picked.Count)
            {
                return;
            }

            toPosition = Math.Clamp(toPosition, 0, _picked.Count - 1);
            if (toPosition == fromPosition)
            {
                return;
            }

            int bankIndex = _picked[fromPosition];
            _picked.RemoveAt(fromPosition);
            _picked.Insert(toPosition, bankIndex);
            OnChanged?.Invoke();
        }

        public void RemoveAt(int phrasePosition)
        {
            if (phrasePosition < 0 || phrasePosition >= _picked.Count)
            {
                return;
            }

            _picked.RemoveAt(phrasePosition);
            OnChanged?.Invoke();
        }

        public void Clear()
        {
            _picked.Clear();
            OnChanged?.Invoke();
        }

        public List<string> GetWords()
        {
            var words = new List<string>(_picked.Count);
            foreach (int index in _picked)
            {
                words.Add(_bank[index]);
            }

            return words;
        }
    }
}
