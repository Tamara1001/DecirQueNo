namespace DecirQueNo.Tests.Bots
{
    // Goal: followers. Says Yes to everything.
    public class InfluencerStrategy : IBotStrategy
    {
        public string Name => "Influencer";
        public float ReactionSeconds => 1f;
        public float ClickSeconds => 0.5f;

        public PhraseType Choose(BotContext context)
        {
            return PhraseType.Yes;
        }
    }

    // Never opens a notification: everything expires. Used to make sure ignoring is not a winning shortcut.
    public class IgnorerStrategy : IBotStrategy
    {
        public string Name => "Ignorer";
        public float ReactionSeconds => 0f;
        public float ClickSeconds => 0f;

        public PhraseType Choose(BotContext context)
        {
            return PhraseType.No;
        }

        public bool ShouldOpen(MessageData message)
        {
            return false;
        }
    }

    // Goal: sanity. Always goes for the No, fast and precise. Before the final message there is no real No,
    // so it takes the evasive answer (the lukewarm No).
    public class MentalHealthStrategy : IBotStrategy
    {
        public string Name => "MentalHealth";
        public float ReactionSeconds => 1.5f;
        public float ClickSeconds => 0.4f;

        public PhraseType Choose(BotContext context)
        {
            return PhraseType.No;
        }
    }

    // A person who is not fast: thinks for a while and clicks slowly, but always goes for the No.
    // If this bot cannot land the final No, the game is too hard for a real player.
    public class AverageStrategy : IBotStrategy
    {
        public string Name => "Average";
        public float ReactionSeconds => 4f;
        public float ClickSeconds => 1.2f;

        public PhraseType Choose(BotContext context)
        {
            return PhraseType.No;
        }
    }

    // Answers a fixed plan, one entry per message of the script (the final message uses the last entry
    // it can: a missing evasive answer falls back to the in-between one).
    public abstract class PlanStrategy : IBotStrategy
    {
        protected abstract PhraseType[] Plan { get; }

        public abstract string Name { get; }
        public float ReactionSeconds => 2f;
        public float ClickSeconds => 0.6f;

        public PhraseType Choose(BotContext context)
        {
            PlayerStats stats = context.Stats;
            int answered = stats.YesCount + stats.MaybeCount + stats.EvasiveCount + stats.NoCount + stats.ExpiredCount;
            return Plan[System.Math.Min(answered, Plan.Length - 1)];
        }
    }

    // Always the in-between answer: slow growth and a draining sanity. Ends in the middle ending.
    public class HesitantStrategy : PlanStrategy
    {
        public override string Name => "Hesitant";
        protected override PhraseType[] Plan { get; } = { PhraseType.Maybe };
    }

    // About 70% Yes and the rest in-between: reaches the goal and the bad ending.
    public class GoalChaserStrategy : PlanStrategy
    {
        public override string Name => "GoalChaser";

        protected override PhraseType[] Plan { get; } =
        {
            PhraseType.Yes, PhraseType.Yes, PhraseType.Maybe,
            PhraseType.Yes, PhraseType.Yes, PhraseType.Maybe,
            PhraseType.Yes, PhraseType.Yes, PhraseType.Maybe,
        };
    }

    // Says Yes at the beginning like everybody, then mostly refuses (half evasive or more), and lands the final No.
    public class BalancedStrategy : PlanStrategy
    {
        public override string Name => "Balanced";

        protected override PhraseType[] Plan { get; } =
        {
            PhraseType.Yes, PhraseType.Yes, PhraseType.Maybe,
            PhraseType.Evasive, PhraseType.Maybe, PhraseType.Evasive,
            PhraseType.Evasive, PhraseType.Evasive, PhraseType.No,
        };
    }
}
