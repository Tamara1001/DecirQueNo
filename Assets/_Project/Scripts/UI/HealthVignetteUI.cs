using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // The look of low sanity, without bars or numbers: from about 60% sanity down the picture gets cold and
    // grey, the corners close in with a deep indigo shade, and below 30% the shade slowly breathes.
    [RequireComponent(typeof(Image))]
    public class HealthVignetteUI : MonoBehaviour
    {
        private static readonly Color ShadeColor = new Color(0.07f, 0.02f, 0.17f);
        private static readonly Color FogColor = new Color(0.45f, 0.5f, 0.68f);

        private const float StartsBelow = 0.62f;
        private const float FullBelow = 0.08f;
        private const float MaxShade = 0.82f;
        private const float MaxFog = 0.2f;
        private const float BreatheBelow = 0.3f;
        private const float BreatheSpeed = 1.7f;
        private const float FadeSpeed = 1.2f;
        private const int TextureSize = 128;

        private Image _shade;
        private Image _fog;
        private PlayerStats _stats;
        private float _danger;
        private float _targetDanger;

        private void Awake()
        {
            _shade = GetComponent<Image>();
            _shade.raycastTarget = false;
            _shade.sprite = BuildSprite();
            _shade.color = new Color(ShadeColor.r, ShadeColor.g, ShadeColor.b, 0f);

            // A flat cold layer under the shade: it washes the colors out a little.
            var fogGo = new GameObject("Fog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fogGo.transform.SetParent(transform.parent, false);
            fogGo.transform.SetSiblingIndex(transform.GetSiblingIndex());
            var rect = (RectTransform)fogGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _fog = fogGo.GetComponent<Image>();
            _fog.raycastTarget = false;
            _fog.color = new Color(FogColor.r, FogColor.g, FogColor.b, 0f);
        }

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            _stats = manager.Stats;
            _stats.OnHealthChanged += HandleHealth;
            _danger = _targetDanger = DangerFor(_stats.Health);
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
            _danger = Mathf.MoveTowards(_danger, _targetDanger, FadeSpeed * Time.unscaledDeltaTime);
            Apply();
        }

        private void HandleHealth(int health)
        {
            _targetDanger = DangerFor(health);
        }

        // 0 while the sanity is fine, 1 when it is almost gone.
        private static float DangerFor(int health)
        {
            float health01 = Mathf.Clamp01(health / (float)GameManager.Instance.Config.maxHealth);
            float t = Mathf.Clamp01((StartsBelow - health01) / (StartsBelow - FullBelow));
            return t * t * (3f - 2f * t);
        }

        private void Apply()
        {
            float shade = MaxShade * _danger;
            float health01 = _stats != null ? Mathf.Clamp01(_stats.Health / (float)GameManager.Instance.Config.maxHealth) : 1f;
            if (health01 < BreatheBelow)
            {
                // The shade slowly closes in and opens up again, like uneasy breathing.
                shade *= 0.9f + 0.1f * Mathf.Sin(Time.unscaledTime * BreatheSpeed);
            }

            _shade.color = new Color(ShadeColor.r, ShadeColor.g, ShadeColor.b, shade);
            _fog.color = new Color(FogColor.r, FogColor.g, FogColor.b, MaxFog * _danger);
        }

        // Soft radial gradient: clear in the middle, closing in towards the corners.
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
                    float alpha = Mathf.Pow(Mathf.SmoothStep(0.45f, 1.35f, distance), 1.3f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f));
        }
    }
}
