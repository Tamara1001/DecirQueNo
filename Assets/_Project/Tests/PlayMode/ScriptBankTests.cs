using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace DecirQueNo.Tests
{
    // Checks the script data (see GUION.md): every phrase can be built from the bank and the board matches the game.
    public class ScriptBankTests
    {
        private static readonly PhraseType[] AllTypes = { PhraseType.Yes, PhraseType.Maybe, PhraseType.Evasive, PhraseType.No };

        private static IReadOnlyList<MessageData> LoadMessages()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            Assert.IsNotNull(config, "Missing Resources/GameConfig.");
            Assert.IsNotNull(config.pool, "GameConfig has no message pool.");
            return config.pool.messages;
        }

        [Test]
        public void Pool_HasThreeDaysOfThreeMessagesInOrder()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            IReadOnlyList<MessageData> messages = LoadMessages();

            Assert.AreEqual(9, messages.Count);
            Assert.AreEqual(new[] { 3, 3, 3 }, config.messagesPerDay.ToArray());
            for (int i = 0; i < messages.Count; i++)
            {
                Assert.AreEqual(i / 3 + 1, messages[i].level, $"{messages[i].name} is in the wrong day.");
            }

            Assert.IsTrue(messages[8].isFinal, "The last message must be the final one.");
        }

        [Test]
        public void EveryMessage_HasPhrasesRepliesAndLevel()
        {
            foreach (MessageData message in LoadMessages())
            {
                Assert.IsNotEmpty(message.body, $"{message.name} has no incoming text.");
                Assert.IsNotEmpty(message.yesWords, $"{message.name} has no Yes phrase.");
                Assert.IsNotEmpty(message.yesReply, $"{message.name} has no reply to Yes.");
                Assert.IsNotEmpty(message.maybeWords, $"{message.name} has no in-between phrase.");
                Assert.IsNotEmpty(message.maybeReply, $"{message.name} has no reply to the in-between answer.");
                Assert.That(message.level, Is.InRange(1, 3), message.name);

                if (message.isFinal)
                {
                    Assert.IsNotEmpty(message.noWords, $"{message.name} (final) needs the real No.");
                    Assert.IsNotEmpty(message.noReply, $"{message.name} (final) needs the reply to No.");
                }
                else
                {
                    Assert.IsNotEmpty(message.evasiveWords, $"{message.name} has no evasive phrase.");
                    Assert.IsNotEmpty(message.evasiveReply, $"{message.name} has no reply to the evasive answer.");
                    Assert.IsEmpty(message.noWords, $"{message.name}: only the final message has a real No.");
                }

                foreach (PhraseType type in AllTypes)
                {
                    foreach (string word in message.GetWords(type))
                    {
                        Assert.IsFalse(string.IsNullOrWhiteSpace(word), $"{message.name} has an empty fragment.");
                    }
                }
            }
        }

        [Test]
        public void EveryPhrase_CanBeBuiltFromItsBank()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            foreach (MessageData message in LoadMessages())
            {
                BankBuilder.Bank bank = BankBuilder.Build(message, config, config.startHealth, 1234);

                foreach (PhraseOption option in bank.Options)
                {
                    var builder = new PhraseBuilder(bank.Words);
                    foreach (string word in option.Words)
                    {
                        int index = FindFreeIndex(bank.Words, builder, word);
                        Assert.GreaterOrEqual(index, 0, $"{message.name}: fragment '{word}' of the {option.Type} phrase is not in the bank.");
                        builder.Add(index);
                    }

                    Assert.IsTrue(PhraseValidator.TryValidate(bank.Options, builder.GetWords(), out PhraseType type), $"{message.name}: the {option.Type} phrase is not valid.");
                    Assert.AreEqual(option.Type, type, message.name);
                }
            }
        }

        [Test]
        public void NoPhraseIsAPrefixOfAnotherPhrase()
        {
            // Otherwise Send would light up halfway through the longer phrase.
            foreach (MessageData message in LoadMessages())
            {
                var all = new List<IReadOnlyList<string>>();
                foreach (PhraseType type in AllTypes)
                {
                    if (message.GetWords(type).Count > 0)
                    {
                        all.Add(message.GetWords(type));
                    }
                }

                for (int a = 0; a < all.Count; a++)
                {
                    for (int b = 0; b < all.Count; b++)
                    {
                        if (a != b && all[a].Count < all[b].Count)
                        {
                            bool prefix = true;
                            for (int i = 0; i < all[a].Count; i++)
                            {
                                prefix &= all[a][i] == all[b][i];
                            }

                            Assert.IsFalse(prefix, $"{message.name}: a phrase is the beginning of another one.");
                        }
                    }
                }
            }
        }

        [Test]
        public void HighSanity_KeepsTheRefusalsTogetherAndInOrder()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            foreach (MessageData message in LoadMessages())
            {
                BankBuilder.Bank bank = BankBuilder.Build(message, config, config.maxHealth, 77);
                Assert.AreEqual(BankArrangement.Ordered, bank.Level.arrangement);
                foreach (PhraseOption option in bank.Options)
                {
                    Assert.GreaterOrEqual(IndexOfSequence(bank.Words, option.Words), 0, $"{message.name}: the {option.Type} phrase is not together and in order.");
                }
            }
        }

        [Test]
        public void MediumSanity_ScramblesTheRefusalsInsideTheirOwnGroup()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            foreach (MessageData message in LoadMessages())
            {
                BankBuilder.Bank bank = BankBuilder.Build(message, config, config.noLevels[1].minHealth, 77);
                Assert.AreEqual(BankArrangement.Scrambled, bank.Level.arrangement);
                foreach (PhraseOption option in bank.Options)
                {
                    bool refusal = option.Type == PhraseType.Evasive || option.Type == PhraseType.No;
                    bool inOrder = IndexOfSequence(bank.Words, option.Words) >= 0;
                    Assert.AreNotEqual(refusal, inOrder, $"{message.name}: the {option.Type} phrase has the wrong order for this tier.");
                }
            }
        }

        [Test]
        public void LowSanity_MixesEverythingWithoutLosingBlocks()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            bool someMixed = false;
            foreach (MessageData message in LoadMessages())
            {
                BankBuilder.Bank bank = BankBuilder.Build(message, config, 0, 77);
                Assert.AreEqual(BankArrangement.Mixed, bank.Level.arrangement);

                int expected = 0;
                foreach (PhraseOption option in bank.Options)
                {
                    expected += option.Words.Count;
                    someMixed |= IndexOfSequence(bank.Words, option.Words) < 0;
                }

                Assert.AreEqual(expected, bank.Words.Count, message.name);
            }

            Assert.IsTrue(someMixed, "At the lowest sanity the blocks must come out mixed.");
        }

        [Test]
        public void ResponseTime_DoesNotDependOnSanity()
        {
            var config = Resources.Load<GameConfig>("GameConfig");
            Assert.Greater(config.responseSeconds, 0f);
        }

        // Index where the words appear one after the other in the bank, or -1.
        private static int IndexOfSequence(IReadOnlyList<string> bank, IReadOnlyList<string> words)
        {
            for (int start = 0; start + words.Count <= bank.Count; start++)
            {
                bool match = true;
                for (int i = 0; match && i < words.Count; i++)
                {
                    match = bank[start + i] == words[i];
                }

                if (match)
                {
                    return start;
                }
            }

            return -1;
        }

        private static int FindFreeIndex(IReadOnlyList<string> bank, PhraseBuilder builder, string word)
        {
            for (int i = 0; i < bank.Count; i++)
            {
                if (bank[i] == word && !builder.IsPicked(i))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
