using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // The look of low sanity, without bars or numbers. From about 60% sanity down the picture goes cold and
    // grey, slow murky clouds drift over it, a fine film grain flickers, the corners close in with a deep
    // indigo shade, and below 30% the shade breathes like a slow heartbeat that bleeds a little red.
    [RequireComponent(typeof(Image))]
    public class HealthVignetteUI : MonoBehaviour
    {
        private static readonly Color ShadeColor = new Color(0.06f, 0.02f, 0.16f);
        private static readonly Color BeatColor = new Color(0.2f, 0.01f, 0.09f);
        private static readonly Color FogColor = new Color(0.42f, 0.48f, 0.62f);
        private static readonly Color MurkColorA = new Color(0.1f, 0.14f, 0.24f);
        private static readonly Color MurkColorB = new Color(0.2f, 0.1f, 0.26f);

        private const float StartsBelow = 0.62f;
        private const float FullBelow = 0.08f;
        private const float MaxShade = 0.86f;
        private const float MaxFog = 0.26f;
        private const float MaxMurk = 0.42f;
        private const float MaxGrain = 0.085f;
        private const float BreatheBelow = 0.3f;
        private const float HeartRate = 1.15f;
        private const float FadeSpeed = 1.2f;
        private const float GrainStepSeconds = 0.07f;

        private Image _shade;
        private Image _fog;
        private RawImage _murkA;
        private RawImage _murkB;
        private RawImage _grain;
        private PlayerStats _stats;
        private float _danger;
        private float _targetDanger;
        private float _grainTimer;

        private void Awake()
        {
            _shade = GetComponent<Image>();
            _shade.raycastTarget = false;
            _shade.sprite = UiNoise.EdgeShade();
            _shade.color = new Color(ShadeColor.r, ShadeColor.g, ShadeColor.b, 0f);

            // The layers sit under the shade, in this order: cold flat fog, two drifting clouds, grain.
            _fog = NewLayer<Image>("Fog");
            _fog.color = new Color(FogColor.r, FogColor.g, FogColor.b, 0f);
            _murkA = NewLayer<RawImage>("MurkA");
            _murkA.texture = UiNoise.Cloud();
            _murkB = NewLayer<RawImage>("MurkB");
            _murkB.texture = UiNoise.Cloud();
            _grain = NewLayer<RawImage>("Grain");
            _grain.texture = UiNoise.Grain();
        }

        private T NewLayer<T>(string name) where T : Graphic
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(T));
            go.transform.SetParent(transform.parent, false);
            go.transform.SetSiblingIndex(transform.GetSiblingIndex());
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var graphic = go.GetComponent<T>();
            graphic.raycastTarget = false;
            graphic.color = new Color(1f, 1f, 1f, 0f);
            return graphic;
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
            float time = Time.unscaledTime;
            float health01 = _stats != null ? Mathf.Clamp01(_stats.Health / (float)GameManager.Instance.Config.maxHealth) : 1f;
            float shade = MaxShade * _danger;
            Color shadeColor = ShadeColor;
            if (health01 < BreatheBelow)
            {
                // Two quick beats and a rest, like a heart: the shade closes in and bleeds a little red.
                float phase = Mathf.Repeat(time * HeartRate, 1f);
                float beat = Pulse(phase, 0.05f) + 0.7f * Pulse(phase, 0.25f);
                shade *= 0.93f + 0.07f * Mathf.Sin(time * 1.7f) + 0.06f * beat;
                shadeColor = Color.Lerp(ShadeColor, BeatColor, Mathf.Clamp01(beat) * 0.6f);
            }

            _shade.color = new Color(shadeColor.r, shadeColor.g, shadeColor.b, Mathf.Clamp01(shade));
            _fog.color = new Color(FogColor.r, FogColor.g, FogColor.b, MaxFog * _danger);

            // Two cloud layers drifting in different directions: the picture looks like it is seen through smoke.
            float murk = MaxMurk * _danger;
            DriftClouds(_murkA, MurkColorA, murk, new Vector2(1.7f, 1f), new Vector2(0.011f, 0.004f), time, 0f);
            DriftClouds(_murkB, MurkColorB, murk * 0.8f, new Vector2(1.1f, 0.65f), new Vector2(-0.007f, 0.009f), time, 0.37f);

            // Film grain: it jumps to a random place every few hundredth of a second.
            _grainTimer -= Time.unscaledDeltaTime;
            if (_grainTimer <= 0f)
            {
                _grainTimer = GrainStepSeconds;
                _grain.uvRect = new Rect(Random.value, Random.value, 7.5f, 4.2f);
            }

            _grain.color = new Color(1f, 1f, 1f, MaxGrain * _danger);
        }

        private static void DriftClouds(RawImage image, Color color, float alpha, Vector2 size, Vector2 speed, float time, float offset)
        {
            image.color = new Color(color.r, color.g, color.b, alpha);
            image.uvRect = new Rect(offset + speed.x * time, offset + speed.y * time + 0.05f * Mathf.Sin(time * 0.3f + offset * 9f), size.x, size.y);
        }

        // A short bump of the heartbeat: 1 at the given point of the cycle, quickly falling to 0.
        private static float Pulse(float phase, float at)
        {
            float x = (phase - at) / 0.09f;
            return Mathf.Exp(-x * x);
        }
    }
}
