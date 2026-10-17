using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Darkens the edges of the screen as the health drops. Shows the health without a bar or numbers.
    [RequireComponent(typeof(Image))]
    public class HealthVignetteUI : MonoBehaviour
    {
        private const float MaxAlpha = 0.85f;
        private const float FadeSpeed = 2f;
        private const int TextureSize = 128;

        private Image _image;
        private PlayerStats _stats;
        private float _alpha;
        private float _targetAlpha;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.raycastTarget = false;
            _image.sprite = BuildSprite();
            _image.color = new Color(0f, 0f, 0f, 0f);
        }

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _stats = manager.Stats;
            _stats.OnHealthChanged += HandleHealth;
            _alpha = _targetAlpha = AlphaFor(_stats.Health);
            Apply();
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnHealthChanged -= HandleHealth;
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_alpha, _targetAlpha))
            {
                return;
            }

            _alpha = Mathf.MoveTowards(_alpha, _targetAlpha, FadeSpeed * Time.unscaledDeltaTime);
            Apply();
        }

        private void HandleHealth(int health)
        {
            _targetAlpha = AlphaFor(health);
        }

        private float AlphaFor(int health)
        {
            float health01 = Mathf.Clamp01(health / (float)GameManager.Instance.Config.maxHealth);
            return Mathf.Lerp(MaxAlpha, 0f, health01);
        }

        private void Apply()
        {
            _image.color = new Color(0f, 0f, 0f, _alpha);
        }

        // Radial gradient: transparent in the middle, opaque black at the corners.
        private static Sprite BuildSprite()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float dx = (x + 0.5f) / TextureSize * 2f - 1f;
                    float dy = (y + 0.5f) / TextureSize * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.SmoothStep(0.35f, 1.25f, distance);
                    texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f));
        }
    }
}
