using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DecirQueNo.Tests.Bots
{
    // Plays the whole game through the real scenes and UI: MainMenu -> Game/Sleep x days -> Ending.
    public class BotPlayer
    {
        private const float RealTimeBudgetSeconds = 240f;

        private readonly IBotStrategy _strategy;
        private readonly BotReport _report;
        private float _startedAt;

        public BotPlayer(IBotStrategy strategy, BotReport report)
        {
            _strategy = strategy;
            _report = report;
            _report.Bot = strategy.Name;
        }

        public IEnumerator Play()
        {
            _startedAt = Time.realtimeSinceStartup;

            SceneManager.LoadScene(GameManager.MainMenuScene);
            yield return WaitForScene(GameManager.MainMenuScene);
            ClickOnlyButton();

            while (SceneManager.GetActiveScene().name != GameManager.EndingScene)
            {
                GuardBudget();

                switch (SceneManager.GetActiveScene().name)
                {
                    case GameManager.InstructionsScene:
                        ClickOnlyButton();
                        yield return WaitForSceneChange(GameManager.InstructionsScene);
                        break;
                    case GameManager.GameScene:
                        yield return PlayGameFrame();
                        break;
                    case GameManager.SleepScene:
                        RecordSummary();
                        ClickOnlyButton();
                        yield return WaitForSceneChange(GameManager.SleepScene);
                        break;
                    default:
                        yield return null;
                        break;
                }
            }

            GameManager manager = GameManager.Instance;
            _report.Ending = manager.Ending;
            _report.FinalFollowers = manager.Stats.Followers;
            _report.FinalHealth = manager.Stats.Health;
            _report.GoalFollowers = manager.Config.goalFollowers;
        }

        private IEnumerator PlayGameFrame()
        {
            var conversation = UnityEngine.Object.FindFirstObjectByType<ConversationController>();
            var conversationUi = UnityEngine.Object.FindFirstObjectByType<ConversationUI>();
            if (conversation == null || conversationUi == null)
            {
                yield return null;
                yield break;
            }

            if (conversation.IsOpen)
            {
                yield return Answer(conversation, conversationUi);
                yield break;
            }

            NotificationItemUI[] items = UnityEngine.Object.FindObjectsByType<NotificationItemUI>(FindObjectsSortMode.None);
            foreach (NotificationItemUI item in items)
            {
                if (_strategy.ShouldOpen(item.Message))
                {
                    item.Click();
                    break;
                }
            }

            yield return null;
        }

        private IEnumerator Answer(ConversationController conversation, ConversationUI ui)
        {
            GameManager manager = GameManager.Instance;
            MessageData message = conversation.Message;
            var context = new BotContext
            {
                Stats = manager.Stats,
                Config = manager.Config,
                Message = message,
                Day = manager.CurrentDay,
            };

            PhraseOption phrase = PickOption(conversation.Bank.Options, _strategy.Choose(context));
            _report.Choices.Add(phrase.Type);

            // Thinking plus clicking every word takes game time; long No phrases may not fit.
            yield return new WaitForSeconds(_strategy.ReactionSeconds + _strategy.ClickSeconds * phrase.Words.Count);

            if (!conversation.IsOpen)
            {
                // The timer ran out first and the game answered Yes for the bot.
                _report.Timeouts++;
                yield break;
            }

            foreach (string word in phrase.Words)
            {
                WordButtonUI button = FindBankButton(ui, word);
                if (button == null)
                {
                    throw new InvalidOperationException($"[{_strategy.Name}] No clickable bank button for word '{word}'.");
                }

                button.Click();
            }

            if (!ui.SendInteractable)
            {
                throw new InvalidOperationException($"[{_strategy.Name}] Send button not interactable for a valid phrase.");
            }

            ui.ClickSend();
            yield return null;
        }

        // Only the final message has a real No, and it has no evasive answer. When the wanted phrase is not
        // there the bot takes the closest one: No -> evasive -> Yes.
        private static PhraseOption PickOption(System.Collections.Generic.IReadOnlyList<PhraseOption> options, PhraseType wanted)
        {
            PhraseType[] order = wanted == PhraseType.No
                ? new[] { PhraseType.No, PhraseType.Maybe, PhraseType.Yes }
                : wanted == PhraseType.Maybe
                    ? new[] { PhraseType.Maybe, PhraseType.Yes }
                    : new[] { PhraseType.Yes };

            foreach (PhraseType type in order)
            {
                foreach (PhraseOption option in options)
                {
                    if (option.Type == type)
                    {
                        return option;
                    }
                }
            }

            throw new InvalidOperationException($"No {wanted} phrase available in the current bank.");
        }

        private static WordButtonUI FindBankButton(ConversationUI ui, string word)
        {
            foreach (WordButtonUI button in ui.BankButtons)
            {
                if (button.Word == word && button.Interactable)
                {
                    return button;
                }
            }

            return null;
        }

        private void RecordSummary()
        {
            DaySummary summary = GameManager.Instance.LastSummary;
            if (summary != null && !_report.Days.Exists(d => d.Day == summary.Day))
            {
                _report.Days.Add(summary);
            }
        }

        private static void ClickOnlyButton()
        {
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            if (buttons.Length != 1)
            {
                throw new InvalidOperationException($"Expected exactly 1 button in '{SceneManager.GetActiveScene().name}', found {buttons.Length}.");
            }

            buttons[0].onClick.Invoke();
        }

        private IEnumerator WaitForScene(string sceneName)
        {
            while (SceneManager.GetActiveScene().name != sceneName)
            {
                GuardBudget();
                yield return null;
            }

            yield return null;
        }

        private IEnumerator WaitForSceneChange(string fromScene)
        {
            while (SceneManager.GetActiveScene().name == fromScene)
            {
                GuardBudget();
                yield return null;
            }

            yield return null;
        }

        private void GuardBudget()
        {
            if (Time.realtimeSinceStartup - _startedAt > RealTimeBudgetSeconds)
            {
                throw new TimeoutException($"[{_strategy.Name}] Bot exceeded the {RealTimeBudgetSeconds}s real-time budget.");
            }
        }
    }
}
