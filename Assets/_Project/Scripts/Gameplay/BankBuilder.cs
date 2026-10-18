using System;
using System.Collections.Generic;

namespace DecirQueNo
{
    // Builds the word bank and the valid phrases for a message. The bank is the union of the phrases.
    // Sanity only changes how the blocks are laid out (see BankArrangement) and how big they are.
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
            if (message.maybeWords.Count > 0)
            {
                bank.Options.Add(new PhraseOption(PhraseType.Maybe, new List<string>(message.maybeWords)));
            }

            if (message.evasiveWords.Count > 0)
            {
                bank.Options.Add(new PhraseOption(PhraseType.Evasive, new List<string>(message.evasiveWords)));
            }

            if (message.noWords.Count > 0)
            {
                bank.Options.Add(new PhraseOption(PhraseType.No, new List<string>(message.noWords)));
            }

            // The order depends on the run and the message only, so it does not change while the player thinks.
            var rng = new Random(seed + StableHash(message.name));
            Arrange(bank, rng);
            return bank;
        }

        private static void Arrange(Bank bank, Random rng)
        {
            var easy = new List<PhraseOption>();
            var refusals = new List<PhraseOption>();
            foreach (PhraseOption option in bank.Options)
            {
                (IsRefusal(option.Type) ? refusals : easy).Add(option);
            }

            if (bank.Level.arrangement == BankArrangement.Mixed)
            {
                foreach (PhraseOption option in bank.Options)
                {
                    bank.Words.AddRange(option.Words);
                }

                Shuffle(bank.Words, rng);
                return;
            }

            // Yes and Maybe are never hard to build: their blocks always come together and in order.
            var groups = new List<List<string>>();
            foreach (PhraseOption option in easy)
            {
                groups.Add(new List<string>(option.Words));
            }

            foreach (PhraseOption option in refusals)
            {
                var words = new List<string>(option.Words);
                if (bank.Level.arrangement == BankArrangement.Scrambled)
                {
                    ShuffleUntilChanged(words, rng);
                }

                groups.Add(words);
            }

            Shuffle(groups, rng);
            foreach (List<string> group in groups)
            {
                bank.Words.AddRange(group);
            }
        }

        private static bool IsRefusal(PhraseType type)
        {
            return type == PhraseType.Evasive || type == PhraseType.No;
        }

        // A scrambled group that came out in the right order would defeat the purpose.
        private static void ShuffleUntilChanged(List<string> words, Random rng)
        {
            var original = new List<string>(words);
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Shuffle(words, rng);
                if (!Same(words, original))
                {
                    return;
                }
            }
        }

        private static bool Same(List<string> a, List<string> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
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
