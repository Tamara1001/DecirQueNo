using System;
using System.Collections.Generic;

namespace DecirQueNo
{
    // Builds the word bank and the valid phrases for a message. The bank is the union of the phrases, shuffled.
    public static class BankBuilder
    {
        public class Bank
        {
            public List<string> Words = new List<string>();
            public List<PhraseOption> Options = new List<PhraseOption>();
            public NoLevelConfig Level;
            public int LevelIndex;
        }

        public static Bank Build(MessageData message, GameConfig config, int health, int seed)
        {
            int levelIndex = config.GetNoLevelIndex(health);
            var bank = new Bank
            {
                LevelIndex = levelIndex,
                Level = config.noLevels[levelIndex],
            };

            bank.Options.Add(new PhraseOption(PhraseType.Yes, new List<string>(message.yesWords)));
            if (message.evasiveWords.Count > 0)
            {
                bank.Options.Add(new PhraseOption(PhraseType.Maybe, new List<string>(message.evasiveWords)));
            }

            if (message.noWords.Count > 0)
            {
                bank.Options.Add(new PhraseOption(PhraseType.No, new List<string>(message.noWords)));
            }

            foreach (PhraseOption option in bank.Options)
            {
                bank.Words.AddRange(option.Words);
            }

            // The order depends on the run and the message only, so it does not change while the player thinks.
            Shuffle(bank.Words, new Random(seed + StableHash(message.name)));
            return bank;
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // string.GetHashCode is not guaranteed stable between runs.
        private static int StableHash(string text)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }

                return hash;
            }
        }
    }
}
