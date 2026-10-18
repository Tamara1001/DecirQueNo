using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    // How the blocks of the bank are laid out. This, and the size of the blocks, is all that sanity changes.
    public enum BankArrangement
    {
        // The blocks of every phrase together and in the right order.
        Ordered,

        // The negative phrases (evasive and No) keep their own group, but their blocks are scrambled.
        Scrambled,

        // Every block of every phrase is scrambled together.
        Mixed
    }

    // Difficulty tier driven by sanity (the hidden "health"): lower sanity, smaller and more scrambled blocks.
    [Serializable]
    public class NoLevelConfig
    {
        [Tooltip("This tier applies while sanity is at least this value.")]
        public int minHealth;
        public float buttonScale = 1f;
        public BankArrangement arrangement = BankArrangement.Ordered;

        public NoLevelConfig(int minHealth, float buttonScale, BankArrangement arrangement)
        {
            this.minHealth = minHealth;
            this.buttonScale = buttonScale;
            this.arrangement = arrangement;
        }
    }

    // Effects of the answers to the messages of one antagonist level.
    [Serializable]
    public class AntagonistLevelConfig
    {
        public int yesFollowers;
        public int maybeFollowers;
        public int evasiveFollowers;
        public int yesHealth;
        public int maybeHealth;
        public int evasiveHealth;

        public AntagonistLevelConfig(int yesFollowers, int maybeFollowers, int evasiveFollowers, int yesHealth, int maybeHealth, int evasiveHealth)
        {
            this.yesFollowers = yesFollowers;
            this.maybeFollowers = maybeFollowers;
            this.evasiveFollowers = evasiveFollowers;
            this.yesHealth = yesHealth;
            this.maybeHealth = maybeHealth;
            this.evasiveHealth = evasiveHealth;
        }
    }

    [CreateAssetMenu(menuName = "DecirQueNo/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Stats (health = sanity, hidden from the player)")]
        public int startFollowers = 320;
        public int goalFollowers = 1000000;
        public int startHealth = 70;
        public int maxHealth = 100;

        [Header("Difficulty by sanity (ordered from highest to lowest)")]
        public List<NoLevelConfig> noLevels = new List<NoLevelConfig>
        {
            new NoLevelConfig(70, 1f, BankArrangement.Ordered),
            new NoLevelConfig(40, 0.9f, BankArrangement.Scrambled),
            new NoLevelConfig(0, 0.8f, BankArrangement.Mixed),
        };

        [Header("Response timer (seconds, the same for everybody)")]
        public float responseSeconds = 46f;

        [Header("Effects by antagonist level (index 0 = level 1)")]
        public List<AntagonistLevelConfig> levels = new List<AntagonistLevelConfig>
        {
            new AntagonistLevelConfig(120000, 60000, -3000, -15, -7, 0),
            new AntagonistLevelConfig(140000, 70000, -5000, -20, -10, 5),
            new AntagonistLevelConfig(155000, 77500, -10000, -25, -12, 5),
        };

        [Header("Audio")]
        [Range(0f, 1f)] public float musicVolume = 0.3f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        public float musicCrossfadeSeconds = 1f;

        [Header("Final No")]
        public int noFollowers = -100000;
        public int noHealth = 30;

        [Header("Endings")]
        [Tooltip("The good ending needs the final No and at most this many Yes answers.")]
        public int goodMaxYes = 3;
        [Tooltip("The good ending needs this share of the messages answered with an evasive answer or a No.")]
        [Range(0f, 1f)] public float goodMinNegativeShare = 0.5f;

        [Header("Notifications")]
        [Tooltip("Seconds an unopened notification lives before it expires.")]
        public float notificationLifetime = 30f;
        [Tooltip("An expired notification costs the evasive followers times this.")]
        public float expireFollowersMultiplier = 1.5f;

        [Header("Days")]
        public List<DayData> days = new List<DayData>();
        public MessagePool pool;

        [Tooltip("How many messages of the pool (in order) each day plays.")]
        public List<int> messagesPerDay = new List<int> { 3, 3, 3 };

        [Header("Text")]
        [Tooltip("Emojis need a font that has them. Until the TMP font is added they are stripped from the screen.")]
        public bool showEmojis;

        // Index into noLevels (0 = easiest) for the given sanity.
        public int GetNoLevelIndex(int health)
        {
            for (int i = 0; i < noLevels.Count; i++)
            {
                if (health >= noLevels[i].minHealth)
                {
                    return i;
                }
            }

            return noLevels.Count - 1;
        }

        public NoLevelConfig GetNoLevel(int health)
        {
            return noLevels[GetNoLevelIndex(health)];
        }

        public int TotalMessages
        {
            get
            {
                int total = 0;
                foreach (int count in messagesPerDay)
                {
                    total += count;
                }

                return total;
            }
        }

        private AntagonistLevelConfig Level(MessageData message)
        {
            int index = Mathf.Clamp(message.level - 1, 0, levels.Count - 1);
            return levels[index];
        }

        public int GetFollowers(PhraseType type, MessageData message)
        {
            switch (type)
            {
                case PhraseType.Yes: return Level(message).yesFollowers;
                case PhraseType.Maybe: return Level(message).maybeFollowers;
                case PhraseType.Evasive: return Level(message).evasiveFollowers;
                default: return noFollowers;
            }
        }

        public int GetHealth(PhraseType type, MessageData message)
        {
            switch (type)
            {
                case PhraseType.Yes: return Level(message).yesHealth;
                case PhraseType.Maybe: return Level(message).maybeHealth;
                case PhraseType.Evasive: return Level(message).evasiveHealth;
                default: return noHealth;
            }
        }

        // An unopened message that expires counts as an evasive answer with a surcharge, and gives no sanity.
        public int GetExpiredFollowers(MessageData message)
        {
            return Mathf.RoundToInt(Level(message).evasiveFollowers * expireFollowersMultiplier);
        }
    }
}
