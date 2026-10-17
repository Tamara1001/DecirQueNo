using TMPro;
using UnityEngine;

namespace DecirQueNo
{
    public class MonitorUI : MonoBehaviour
    {
        private const float CountSpeedFraction = 3f;
        private const float MinCountPerSecond = 2000f;
        private const float PunchScale = 0.15f;
        private const float PunchDecay = 6f;

        [SerializeField] private TMP_Text _followersText;
        [SerializeField] private TMP_Text _healthDebugText;
        [SerializeField] private bool _showHealthDebug = true;

        private PlayerStats _stats;
        private float _displayed;
        private int _target;
        private int _shownInt = -1;
        private float _punch;

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _stats = manager.Stats;
            _stats.OnFollowersChanged += HandleFollowers;
            _stats.OnHealthChanged += HandleHealth;

            // The health is hidden from players: the debug text only shows in the editor and debug builds.
            _healthDebugText.gameObject.SetActive(_showHealthDebug && (Application.isEditor || Debug.isDebugBuild));

            _displayed = _stats.Followers;
            _target = _stats.Followers;
            RefreshFollowers();
            HandleHealth(_stats.Health);
        }

        private void OnDestroy()
        {
            if (_stats == null)
            {
                return;
            }

            _stats.OnFollowersChanged -= HandleFollowers;
            _stats.OnHealthChanged -= HandleHealth;
        }

        private void Update()
        {
            if (_stats == null)
            {
                return;
            }

            // Count up (or down) towards the real value, and let the punch settle.
            if (!Mathf.Approximately(_displayed, _target))
            {
                float step = Mathf.Max(MinCountPerSecond, Mathf.Abs(_target - _displayed) * CountSpeedFraction) * Time.unscaledDeltaTime;
                _displayed = Mathf.MoveTowards(_displayed, _target, step);
                RefreshFollowers();
            }

            if (_punch > 0f)
            {
                _punch = Mathf.Max(0f, _punch - PunchDecay * Time.unscaledDeltaTime);
            }

            float scale = 1f + PunchScale * _punch;
            _followersText.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void HandleFollowers(int followers)
        {
            _target = followers;
            _punch = 1f;
        }

        private void RefreshFollowers()
        {
            int shown = Mathf.RoundToInt(_displayed);
            if (shown == _shownInt)
            {
                return;
            }

            _shownInt = shown;
            int goal = GameManager.Instance.Config.goalFollowers;
            _followersText.text = $"Seguidores\n{shown:N0} / {goal:N0}";
        }

        private void HandleHealth(int health)
        {
            _healthDebugText.text = $"[DEBUG] Salud {health}  |  No nivel {_stats.NoLevelIndex + 1}";
        }
    }
}
