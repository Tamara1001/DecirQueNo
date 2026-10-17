using System.Collections.Generic;

namespace DecirQueNo
{
    public static class PhraseValidator
    {
        // Returns true when the words exactly match one of the phrases currently available.
        public static bool TryValidate(IReadOnlyList<PhraseOption> options, IList<string> words, out PhraseType type)
        {
            type = PhraseType.Yes;

            foreach (PhraseOption option in options)
            {
                if (Matches(option.Words, words))
                {
                    type = option.Type;
                    return true;
                }
            }

            return false;
        }

        private static bool Matches(IReadOnlyList<string> expected, IList<string> actual)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }

            for (int i = 0; i < expected.Count; i++)
            {
                if (expected[i] != actual[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
