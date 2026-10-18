using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    // Difficulty tier driven by sanity (the hidden "health"): lower sanity, smaller buttons.
    [Serializable]
    public class NoLevelConfig
    {
        [Tooltip("This tier applies while sanity is at least this value.")]
        public int minHealth;
        public float buttonScale = 1f;

        public NoLevelConfig(int minHealth, float buttonScale)
        {
            this.minHealth = minHealth;
            this.buttonScale = buttonScale;
        }
    }

    // Effects of the answers to the messages of one antagonist level.
    [Serializable]
    public class AntagonistLevelConfig
    {
        public int yesFollowers;
        public int evasiveFollowers;
        public int yesHealth;
        public int evasiveHealth;

        public AntagonistLevelConfig(int yesFollowers, int evasiveFollowers, int yesHealth, int evasiveHealth)
        {
            this.yesFollowers = yesFollowers;
            this.evasiveFollowers = evasiveFollowers;
            this.yesHealth = yesHealth;
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
            new NoLevelConfig(70, 1f),
            new NoLevelConfig(40, 0.8f),
            new NoLevelConfig(0, 0.6f),
        };

        [Header("Response timer (seconds, scales with sanity)")]
        public float timerMinSeconds = 20f;
        public float timerMaxSeconds = 60f;

        [Header("Effects by antagonist level (index 0 = level 1)")]
        public List<AntagonistLevelConfig> levels = new List<AntagonistLevelConfig>
        {
            new AntagonistLevelConfig(30000, -3000, -15, 0),
            new AntagonistLevelConfig(100000, -10000, -20, 5),
            new AntagonistLevelConfig(230000, -23000, -25, 5),
        };

        [Header("Final No")]
        public int noFollowers = -100000;
        public int noHealth = 30;

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

        public float GetResponseSeconds(int health)
        {
            return Mathf.Lerp(timerMinSeconds, timerMaxSeconds, Mathf.Clamp01(health / (float)maxHealth));
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
                case PhraseType.Maybe: return Level(message).evasiveFollowers;
                default: return noFollowers;
            }
        }

        public int GetHealth(PhraseType type, MessageData message)
        {
            switch (type)
            {
                case PhraseType.Yes: return Level(message).yesHealth;
                case PhraseType.Maybe: return Level(message).evasiveHealth;
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
