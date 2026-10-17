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

    // Goal: mental health. Always goes for the No, fast and precise.
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

    // Goal: reach the follower goal at a steady pace without burning out.
    // Tries the No when health is low and the phrase fits in the response time.
    // Otherwise Yes when behind the follower pace and Maybe when ahead.
    public class BalancedStrategy : IBotStrategy
    {
        private const int SafeHealth = 55;

        public string Name => "Balanced";
        public float ReactionSeconds => 2f;
        public float ClickSeconds => 0.6f;

        public PhraseType Choose(BotContext context)
        {
            GameConfig config = context.Config;
            PlayerStats stats = context.Stats;

            if (stats.Health < SafeHealth)
            {
                int noWords = System.Math.Min(config.GetNoLevel(stats.Health).wordCount, context.Message.noWords.Count);
                float needed = ReactionSeconds + ClickSeconds * noWords;
                if (needed < config.GetResponseSeconds(stats.Health))
                {
                    return PhraseType.No;
                }
            }

            // Expected followers at the middle of the current day.
            float progress = (context.Day - 0.5f) / config.days.Count;
            float expected = config.startFollowers + (config.goalFollowers - config.startFollowers) * progress;
            return stats.Followers < expected ? PhraseType.Yes : PhraseType.Maybe;
        }
    }
}
