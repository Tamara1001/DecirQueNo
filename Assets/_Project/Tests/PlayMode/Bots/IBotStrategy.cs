namespace DecirQueNo.Tests.Bots
{
    public struct BotContext
    {
        public PlayerStats Stats;
        public GameConfig Config;
        public MessageData Message;
        public int Day;
    }

    // A strategy only decides WHAT to answer. The BotPlayer does the clicking.
    public interface IBotStrategy
    {
        string Name { get; }

        // In-game seconds the bot "thinks" before it starts clicking.
        float ReactionSeconds { get; }

        // In-game seconds per word it clicks. Long No phrases take real time.
        float ClickSeconds { get; }

        PhraseType Choose(BotContext context);

        // Whether the bot opens this notification at all. When false it is left to expire.
        bool ShouldOpen(MessageData message) => true;
    }
}
