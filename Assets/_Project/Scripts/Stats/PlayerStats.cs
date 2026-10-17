using System;
using UnityEngine;

namespace DecirQueNo
{
    public class PlayerStats
    {
        private readonly GameConfig _config;
        private int _noStreak;
        private float _totalResponseSeconds;
        private int _responses;

        public int Followers { get; private set; }
        public int Health { get; private set; }

        public int YesCount { get; private set; }
        public int MaybeCount { get; private set; }
        public int NoCount { get; private set; }
        public int ExpiredCount { get; private set; }
        public int TimeoutCount { get; private set; }
        public int BestNoStreak { get; private set; }

        public float AverageResponseSeconds => _responses == 0 ? 0f : _totalResponseSeconds / _responses;

        public event Action<int> OnFollowersChanged;
        public event Action<int> OnHealthChanged;

        // 0 = easiest No (high health), up to noLevels.Count - 1 (low health).
        public int NoLevelIndex => _config.GetNoLevelIndex(Health);

        public PlayerStats(GameConfig config)
        {
            _config = config;
            Followers = config.startFollowers;
            Health = config.startHealth;
        }

        // Applies the effects of an answer the player gave.
        public void Apply(PhraseType type, float multiplier = 1f, float responseSeconds = 0f, bool timedOut = false)
        {
            switch (type)
            {
                case PhraseType.Yes:
                    YesCount++;
                    _noStreak = 0;
                    break;
                case PhraseType.Maybe:
                    MaybeCount++;
                    _noStreak = 0;
                    break;
                default:
                    NoCount++;
                    _noStreak++;
                    BestNoStreak = Mathf.Max(BestNoStreak, _noStreak);
                    break;
            }

            if (timedOut)
            {
                TimeoutCount++;
            }

            _responses++;
            _totalResponseSeconds += responseSeconds;

            AddFollowers(_config.GetFollowers(type, multiplier));
            AddHealth(_config.GetHealth(type, multiplier));
        }

        // A notification expired without being opened. It is not a No the player said.
        public void ApplyExpired(float multiplier = 1f)
        {
            ExpiredCount++;
            AddFollowers(Mathf.RoundToInt(_config.expireFollowers * multiplier));
            AddHealth(Mathf.RoundToInt(_config.expireHealth * multiplier));
        }

        public void AddFollowers(int amount)
        {
            Followers = Mathf.Max(0, Followers + amount);
            OnFollowersChanged?.Invoke(Followers);
        }

        public void AddHealth(int amount)
        {
            Health = Mathf.Clamp(Health + amount, 0, _config.maxHealth);
            OnHealthChanged?.Invoke(Health);
        }
    }
}
