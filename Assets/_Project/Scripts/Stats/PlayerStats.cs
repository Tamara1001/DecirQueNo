using System;
using UnityEngine;

namespace DecirQueNo
{
    // Health is the hidden sanity. Evasive answers from level 2 on are the lukewarm No.
    public class PlayerStats
    {
        private readonly GameConfig _config;
        private float _totalResponseSeconds;
        private int _responses;

        public int Followers { get; private set; }
        public int Health { get; private set; }

        public int YesCount { get; private set; }
        public int MaybeCount { get; private set; }
        public int EvasiveCount { get; private set; }

        // Real No answers (only the final message has one).
        public int NoCount { get; private set; }
        public int ExpiredCount { get; private set; }
        public int TimeoutCount { get; private set; }

        public float AverageResponseSeconds => _responses == 0 ? 0f : _totalResponseSeconds / _responses;

        public event Action<int> OnFollowersChanged;
        public event Action<int> OnHealthChanged;

        // Old and new difficulty tier. A lower index means an easier tier.
        public event Action<int, int> OnNoLevelChanged;

        // 0 = easiest tier (high sanity), up to noLevels.Count - 1 (low sanity).
        public int NoLevelIndex => _config.GetNoLevelIndex(Health);

        public PlayerStats(GameConfig config)
        {
            _config = config;
            Followers = config.startFollowers;
            Health = config.startHealth;
        }

        // Applies the effects of an answer the player gave.
        public void Apply(PhraseType type, MessageData message, float responseSeconds = 0f, bool timedOut = false)
        {
            switch (type)
            {
                case PhraseType.Yes: YesCount++; break;
                case PhraseType.Maybe: MaybeCount++; break;
                case PhraseType.Evasive: EvasiveCount++; break;
                default: NoCount++; break;
            }

            if (timedOut)
            {
                TimeoutCount++;
            }

            _responses++;
            _totalResponseSeconds += responseSeconds;

            AddFollowers(_config.GetFollowers(type, message));
            AddHealth(_config.GetHealth(type, message));
        }

        // A notification expired without being opened. It is not an answer the player gave.
        public void ApplyExpired(MessageData message)
        {
            ExpiredCount++;
            AddFollowers(_config.GetExpiredFollowers(message));
        }

        public void AddFollowers(int amount)
        {
            Followers = Mathf.Max(0, Followers + amount);
            OnFollowersChanged?.Invoke(Followers);
        }

        public void AddHealth(int amount)
        {
            int levelBefore = NoLevelIndex;
            Health = Mathf.Clamp(Health + amount, 0, _config.maxHealth);
            OnHealthChanged?.Invoke(Health);

            int levelAfter = NoLevelIndex;
            if (levelAfter != levelBefore)
            {
                OnNoLevelChanged?.Invoke(levelBefore, levelAfter);
            }
        }
    }
}
