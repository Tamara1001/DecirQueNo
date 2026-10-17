using System;
using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    [Serializable]
    public class NoLevelConfig
    {
        [Tooltip("This level applies while health is at least this value.")]
        public int minHealth;
        public int wordCount;
        public int distractors;
        public float buttonScale = 1f;

        public NoLevelConfig(int minHealth, int wordCount, int distractors, float buttonScale)
        {
            this.minHealth = minHealth;
            this.wordCount = wordCount;
            this.distractors = distractors;
            this.buttonScale = buttonScale;
        }
    }

    [CreateAssetMenu(menuName = "DecirQueNo/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Stats")]
        public int startFollowers = 10000;
        public int goalFollowers = 100000;
        public int startHealth = 45;
        public int maxHealth = 100;

        [Header("No difficulty (ordered from easiest to hardest)")]
        public List<NoLevelConfig> noLevels = new List<NoLevelConfig>
        {
            new NoLevelConfig(80, 2, 2, 1f),
            new NoLevelConfig(65, 4, 3, 0.85f),
            new NoLevelConfig(50, 6, 4, 0.7f),
            new NoLevelConfig(0, 8, 5, 0.6f),
        };

        public List<string> distractorPool = new List<string>
        {
            "Banana", "Mesa", "Perro", "Luz", "Sol", "Casa", "Nube", "Auto", "Rio", "Pan", "Luna", "Mar", "Palabra_1", "Palabra_2"
        };

        [Header("Response timer (seconds, scales with health)")]
        public float timerMinSeconds = 45f;
        public float timerMaxSeconds = 60f;

        [Header("Endings")]
        public int happyMinHealth = 65;
        public int happyMinNoCount = 5;
        public int sadHealthBelow = 30;

        [Header("Effects: Yes")]
        public int yesFollowers = 7500;
        public int yesHealth = -12;

        [Header("Effects: Maybe")]
        public int maybeFollowers = 3750;
        public int maybeHealth = -6;

        [Header("Effects: No")]
        public int noFollowers = -500;
        public int noHealth = 8;

        [Tooltip("Extra health for each No in a row after the first one.")]
        public int noStreakBonusHealth = 2;
        [Tooltip("The streak bonus stops growing after this many Nos in a row.")]
        public int noStreakBonusCap = 4;

        [Header("Notifications")]
        [Tooltip("Seconds an unopened notification lives before it expires.")]
        public float notificationLifetime = 30f;
        public int expireFollowers = -1500;
        public int expireHealth = 2;

        [Header("Days")]
        public List<DayData> days = new List<DayData>();
        public MessagePool pool;

        [Tooltip("How many messages each day takes from the shuffled pool.")]
        public List<int> messagesPerDay = new List<int> { 4, 5, 5 };

        // Index into noLevels (0 = easiest) for the given health.
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

        public int GetFollowers(PhraseType type, float multiplier = 1f)
        {
            return Mathf.RoundToInt(BaseFollowers(type) * multiplier);
        }

        public int GetHealth(PhraseType type, float multiplier = 1f)
        {
            return Mathf.RoundToInt(BaseHealth(type) * multiplier);
        }

        private int BaseFollowers(PhraseType type)
        {
            switch (type)
            {
                case PhraseType.Yes: return yesFollowers;
                case PhraseType.Maybe: return maybeFollowers;
                default: return noFollowers;
            }
        }

        private int BaseHealth(PhraseType type)
        {
            switch (type)
            {
                case PhraseType.Yes: return yesHealth;
                case PhraseType.Maybe: return maybeHealth;
                default: return noHealth;
            }
        }
    }
}
