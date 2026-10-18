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

    // Goal: grow at a steady pace without burning out. Yes when behind the follower pace, evasive when ahead,
    // and the real No on the final message when there is enough sanity left to build it in time.
    public class BalancedStrategy : IBotStrategy
    {
        private const int SaneEnoughForNo = 40;

        public string Name => "Balanced";
        public float ReactionSeconds => 2f;
        public float ClickSeconds => 0.6f;

        public PhraseType Choose(BotContext context)
        {
            GameConfig config = context.Config;
            PlayerStats stats = context.Stats;

            if (context.Message.isFinal)
            {
                float needed = ReactionSeconds + ClickSeconds * context.Message.noWords.Count;
                bool fits = needed < config.GetResponseSeconds(stats.Health);
                return stats.Health >= SaneEnoughForNo && fits ? PhraseType.No : PhraseType.Yes;
            }

            // Expected followers at the middle of the current day.
            float progress = (context.Day - 0.5f) / config.days.Count;
            float expected = config.startFollowers + (config.goalFollowers - config.startFollowers) * progress;
            return stats.Followers < expected ? PhraseType.Yes : PhraseType.Maybe;
        }
    }
}
