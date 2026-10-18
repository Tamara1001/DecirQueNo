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
        private const float DeltaSeconds = 3.5f;
        private const float DeltaOffsetY = 60f;

        [SerializeField] private TMP_Text _followersText;
        [SerializeField] private TMP_Text _healthDebugText;
        [SerializeField] private bool _showHealthDebug = true;

        private PlayerStats _stats;
        private float _displayed;
        private int _target;
        private int _shownInt = -1;
        private float _punch;
        private TMP_Text _deltaText;
        private float _deltaShownFor;

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

            UpdateDelta();

            if (_punch > 0f)
            {
                _punch = Mathf.Max(0f, _punch - PunchDecay * Time.unscaledDeltaTime);
            }

            float scale = 1f + PunchScale * _punch;
            _followersText.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void HandleFollowers(int followers)
        {
            int delta = followers - _target;
            _target = followers;
            _punch = 1f;
            ShowDelta(delta, _stats.LastFollowersReason);
        }

        // Says why the counter moved, since it can move while another chat is open.
        private void ShowDelta(int delta, string reason)
        {
            if (delta == 0)
            {
                return;
            }

            if (_deltaText == null)
            {
                _deltaText = Instantiate(_followersText, _followersText.transform.parent);
                _deltaText.name = "FollowersDelta";
                _deltaText.rectTransform.anchoredPosition += new Vector2(0f, -DeltaOffsetY);
                _deltaText.fontSize = _followersText.fontSize * 0.8f;
                _deltaText.raycastTarget = false;
            }

            string sign = delta > 0 ? "+" : "-";
            string text = $"{sign}{Mathf.Abs(delta):N0}";
            _deltaText.text = string.IsNullOrEmpty(reason) ? text : $"{text}  ({reason})";
            _deltaText.color = delta > 0 ? new Color(0.6f, 1f, 0.7f) : new Color(1f, 0.55f, 0.55f);
            _deltaShownFor = DeltaSeconds;
        }

        private void UpdateDelta()
        {
            if (_deltaText == null)
            {
                return;
            }

            _deltaShownFor = Mathf.Max(0f, _deltaShownFor - Time.unscaledDeltaTime);
            Color color = _deltaText.color;
            color.a = Mathf.Clamp01(_deltaShownFor / 0.6f);
            _deltaText.color = color;
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
