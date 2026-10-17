using System.Collections.Generic;

namespace DecirQueNo
{
    // A phrase the player can build right now for the open message.
    public class PhraseOption
    {
        public PhraseType Type;
        public IReadOnlyList<string> Words;

        public PhraseOption(PhraseType type, IReadOnlyList<string> words)
        {
            Type = type;
            Words = words;
        }
    }
}
