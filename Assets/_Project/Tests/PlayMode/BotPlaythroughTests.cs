using System.Collections;
using System.IO;
using DecirQueNo.Tests.Bots;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DecirQueNo.Tests
{
    // Full playthroughs by bots. See BOTS.md for what each bot does and the latest results.
    public class BotPlaythroughTests
    {
        private const float TimeScale = 30f;
        private const float MaxDeltaTime = 1f;

        private float _oldTimeScale;
        private float _oldMaxDelta;

        [SetUp]
        public void SetUp()
        {
            _oldTimeScale = Time.timeScale;
            _oldMaxDelta = Time.maximumDeltaTime;
            Time.maximumDeltaTime = MaxDeltaTime;
            Time.timeScale = TimeScale;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _oldTimeScale;
            Time.maximumDeltaTime = _oldMaxDelta;
        }

        [UnityTest]
        public IEnumerator InfluencerBot_BurnsOut()
        {
            var report = new BotReport();
            yield return new BotPlayer(new InfluencerStrategy(), report).Play();

            AssertInvariants(report);
            Assert.AreEqual(Ending.Sad, report.Ending, "Saying Yes to everything must end badly.");
            Assert.GreaterOrEqual(report.FinalFollowers, report.GoalFollowers, "Saying Yes to everything reaches the goal.");
        }

        [UnityTest]
        public IEnumerator MentalHealthBot_GetsHappyEnding()
        {
            var report = new BotReport();
            yield return new BotPlayer(new MentalHealthStrategy(), report).Play();

            AssertInvariants(report);
            Assert.AreEqual(1, report.No, "The real No exists only in the final message.");
            Assert.AreEqual(8, report.Evasive, "Before the final message the bot takes the lukewarm No.");
            Assert.AreEqual(Ending.Happy, report.Ending, "Learning to say No must lead to the happy ending.");
        }

        [UnityTest]
        public IEnumerator AverageBot_CanSayNo()
        {
            var report = new BotReport();
            yield return new BotPlayer(new AverageStrategy(), report).Play();

            AssertInvariants(report);
            Assert.AreEqual(1, report.No, "A slow, average player must be able to land the final No.");
            Assert.AreEqual(Ending.Happy, report.Ending, "Going for the No at a human pace must still reach the happy ending.");
        }

        [UnityTest]
        public IEnumerator IgnorerBot_DoesNotWin()
        {
            var report = new BotReport();
            yield return new BotPlayer(new IgnorerStrategy(), report).Play();

            AssertInvariants(report);
            Assert.Greater(report.Expired, 0, "Every notification should have expired.");
            Assert.AreEqual(9, report.Expired, "Every message of the script expires.");
            Assert.AreEqual(0, report.Yes + report.Maybe + report.Evasive + report.No, "The ignorer never answers anything.");
            Assert.AreEqual(Ending.Neutral, report.Ending, "Ignoring everything ends in the middle: no goal and no No.");
        }

        [UnityTest]
        public IEnumerator HesitantBot_GetsTheMiddleEnding()
        {
            var report = new BotReport();
            yield return new BotPlayer(new HesitantStrategy(), report).Play();

            AssertInvariants(report);
            Assert.AreEqual(9, report.Maybe, "The hesitant bot answers the in-between phrase every time.");
            Assert.Less(report.FinalFollowers, report.GoalFollowers, "Only saying 'maybe' must not reach the goal.");
            Assert.AreEqual(Ending.Neutral, report.Ending, "Saying maybe to everything ends in the middle.");
        }

        [UnityTest]
        public IEnumerator GoalChaserBot_ReachesTheGoalWithAboutSeventyPercentYes()
        {
            var report = new BotReport();
            yield return new BotPlayer(new GoalChaserStrategy(), report).Play();

            AssertInvariants(report);
            Assert.GreaterOrEqual(report.FinalFollowers, report.GoalFollowers, "6 Yes out of 9 and the rest 'maybe' must reach the goal.");
            Assert.AreEqual(Ending.Sad, report.Ending);
        }

        [UnityTest]
        public IEnumerator BalancedBot_GetsHappyEnding()
        {
            var report = new BotReport();
            yield return new BotPlayer(new BalancedStrategy(), report).Play();

            AssertInvariants(report);
            Assert.LessOrEqual(report.Yes, 3, "The good path says Yes only at the beginning.");
            Assert.GreaterOrEqual(report.Evasive + report.No, 5, "The good path refuses at least half of the messages.");
            Assert.AreEqual(1, report.No);
            Assert.AreEqual(Ending.Happy, report.Ending, "Some Yes at the start, mostly refusals and the final No is the good ending.");
        }

        private static void SaveReport(BotReport report)
        {
            Debug.Log(report);
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BotReports"));
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, report.Bot + ".txt"), report.ToString());
        }

        private static void AssertInvariants(BotReport report)
        {
            SaveReport(report);
            Assert.AreNotEqual(Ending.None, report.Ending, "The game must end with an ending.");
            Assert.GreaterOrEqual(report.Days.Count, 1, "At least one day must be played.");
            Assert.LessOrEqual(report.Days.Count, 3, "No more than 3 days.");
            Assert.GreaterOrEqual(report.FinalHealth, 0);
            Assert.GreaterOrEqual(report.FinalFollowers, 0);
            Assert.AreEqual(0, report.Timeouts, "Bots answer before the timer runs out.");
        }
    }
}
