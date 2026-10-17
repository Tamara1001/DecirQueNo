using System;
using System.Collections.Generic;

namespace DecirQueNo
{
    // Builds the word bank and the valid phrases for a message at the current No level.
    public static class BankBuilder
    {
        public class Bank
        {
            public List<string> Words = new List<string>();
            public List<PhraseOption> Options = new List<PhraseOption>();
            public NoLevelConfig Level;
            public int LevelIndex;
            public bool IsTutorial;
        }

        // forcedLevelIndex >= 0 overrides the level the health would give (used for the practice message).
        public static Bank Build(MessageData message, GameConfig config, int health, int seed, int forcedLevelIndex = -1)
        {
            int levelIndex = forcedLevelIndex >= 0 ? forcedLevelIndex : config.GetNoLevelIndex(health);
            var bank = new Bank
            {
                LevelIndex = levelIndex,
                Level = config.noLevels[levelIndex],
                IsTutorial = forcedLevelIndex >= 0,
            };

            int noCount = Math.Min(bank.Level.wordCount, message.noWords.Count);
            bank.Options.Add(new PhraseOption(PhraseType.Yes, new List<string>(message.yesWords)));
            bank.Options.Add(new PhraseOption(PhraseType.Maybe, new List<string>(message.maybeWords)));
            bank.Options.Add(new PhraseOption(PhraseType.No, message.noWords.GetRange(0, noCount)));

            foreach (PhraseOption option in bank.Options)
            {
                bank.Words.AddRange(option.Words);
            }

            var rng = new Random(seed + StableHash(message.name) + health);
            var pool = new List<string>(config.distractorPool);
            pool.RemoveAll(w => bank.Words.Contains(w));
            Shuffle(pool, rng);
            int distractors = Math.Min(bank.Level.distractors, pool.Count);
            bank.Words.AddRange(pool.GetRange(0, distractors));

            Shuffle(bank.Words, rng);
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
