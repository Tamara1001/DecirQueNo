using System.Collections;
using TMPro;
using UnityEngine;

namespace DecirQueNo
{
    // The followers board: the big counter plus a floating "+120.000" every time it changes.
    public class MonitorUI : MonoBehaviour
    {
        private static readonly Color GainColor = new Color(0.62f, 0.96f, 0.82f);
        private static readonly Color LossColor = new Color(1f, 0.5f, 0.72f);
        private static readonly Color OutlineColor = new Color(0.02f, 0.125f, 0.24f);

        private const float CountSpeedFraction = 3f;
        private const float MinCountPerSecond = 2000f;
        private const float PunchScale = 0.18f;
        private const float PunchDecay = 5f;
        private const float FollowersTextScale = 1.8f;
        private const float FollowersLift = 0.12f;
        private const float FloatSizeRatio = 0.42f;
        private const float FloatRise = 70f;
        private const float FloatStartOffset = 60f;
        private const float FloatSeconds = 1.9f;
        private const float FlashDecay = 2.2f;

        [SerializeField] private TMP_Text _followersText;

        private PlayerStats _stats;
        private float _displayed;
        private int _target;
        private int _shownInt = -1;
        private float _punch;
        private float _flash;
        private Color _flashColor = Color.white;
        private Color _baseColor;

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _stats = manager.Stats;
            _stats.OnFollowersChanged += HandleFollowers;

            // Just the current count, big and high on the board.
            _followersText.enableAutoSizing = false;
            _followersText.fontSize *= FollowersTextScale;
            _followersText.enableWordWrapping = false;
            _followersText.overflowMode = TextOverflowModes.Overflow;
            _followersText.alignment = TextAlignmentOptions.Center;
            _followersText.fontStyle = FontStyles.Bold;
            RectTransform followersRect = _followersText.rectTransform;
            followersRect.anchorMin += new Vector2(0f, FollowersLift);
            followersRect.anchorMax += new Vector2(0f, FollowersLift);
            _baseColor = _followersText.color;

            _displayed = _stats.Followers;
            _target = _stats.Followers;
            RefreshFollowers();
        }

        private void OnDestroy()
        {
            if (_stats == null)
            {
                return;
            }

            _stats.OnFollowersChanged -= HandleFollowers;
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

            // The counter flashes green when it grows and pink when it drops, then goes back to normal.
            if (_flash > 0f)
            {
                _flash = Mathf.Max(0f, _flash - FlashDecay * Time.unscaledDeltaTime);
            }

            _followersText.color = Color.Lerp(_baseColor, _flashColor, _flash);
            float scale = 1f + PunchScale * _punch;
            _followersText.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void HandleFollowers(int followers)
        {
            int delta = followers - _target;
            _target = followers;
            if (delta == 0)
            {
                return;
            }

            _punch = 1f;
            _flash = 1f;
            _flashColor = delta > 0 ? GainColor : LossColor;
            StartCoroutine(FloatDelta(delta));
        }

        // A label that pops under the counter, rises and fades. Several can be on screen at once.
        private IEnumerator FloatDelta(int delta)
        {
            TMP_Text label = Instantiate(_followersText, _followersText.transform.parent);
            label.name = "FollowersDelta";
            label.raycastTarget = false;
            label.fontSize = _followersText.fontSize * FloatSizeRatio;
            label.fontStyle = FontStyles.Bold;
            label.text = delta > 0 ? $"+{delta:N0}" : $"-{-delta:N0}";
            Color color = delta > 0 ? GainColor : LossColor;
            label.color = color;
            label.outlineWidth = 0.22f;
            label.outlineColor = OutlineColor;

            RectTransform rect = label.rectTransform;
            Vector2 start = rect.anchoredPosition + new Vector2(Random.Range(-30f, 30f), FloatStartOffset);
            rect.anchoredPosition = start;

            float time = 0f;
            while (time < FloatSeconds)
            {
                time += Time.unscaledDeltaTime;
                float k = time / FloatSeconds;

                // Quick pop in, slow rise, fade at the end.
                float pop = k < 0.12f ? Mathf.Lerp(0.5f, 1.25f, k / 0.12f) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((k - 0.12f) / 0.15f));
                rect.localScale = Vector3.one * pop;
                rect.anchoredPosition = start + new Vector2(0f, FloatRise * (1f - Mathf.Pow(1f - k, 2.2f)));
                color.a = k < 0.7f ? 1f : Mathf.Clamp01(1f - (k - 0.7f) / 0.3f);
                label.color = color;
                yield return null;
            }

            Destroy(label.gameObject);
        }

        private void RefreshFollowers()
        {
            int shown = Mathf.RoundToInt(_displayed);
            if (shown == _shownInt)
            {
                return;
            }

            _shownInt = shown;
            _followersText.text = $"{shown:N0}";
        }
    }
}
