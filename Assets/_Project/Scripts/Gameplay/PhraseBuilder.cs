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
