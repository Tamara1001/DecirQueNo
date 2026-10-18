using UnityEngine;

namespace DecirQueNo
{
    // Procedural textures for the screen filters (low sanity, sunset). Built once and shared.
    public static class UiNoise
    {
        private static Texture2D _cloud;
        private static Texture2D _grain;
        private static Sprite _radial;
        private static Sprite _soft;

        // Soft tileable clouds: white with a smooth alpha, to be tinted and scrolled.
        public static Texture2D Cloud(int size = 128, float frequency = 3f, int seed = 11)
        {
            if (_cloud != null)
            {
                return _cloud;
            }

            var texture = NewTexture(size, FilterMode.Bilinear);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float n = 0.6f * Tileable(u, v, frequency, seed) + 0.3f * Tileable(u, v, frequency * 2f, seed + 7) + 0.1f * Tileable(u, v, frequency * 4f, seed + 13);
                    float alpha = Mathf.SmoothStep(0.25f, 0.8f, n);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _cloud = texture;
            return _cloud;
        }

        // Film grain: random grey dots, point filtered so each dot stays crisp.
        public static Texture2D Grain(int size = 128)
        {
            if (_grain != null)
            {
                return _grain;
            }

            var texture = NewTexture(size, FilterMode.Point);
            var random = new System.Random(5);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                float v = (float)random.NextDouble();
                pixels[i] = new Color(v, v, v, 1f);
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _grain = texture;
            return _grain;
        }

        // Clear in the middle, 1 at the corners.
        public static Sprite EdgeShade()
        {
            if (_radial != null)
            {
                return _radial;
            }

            const int size = 128;
            var texture = NewTexture(size, FilterMode.Bilinear);
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Pow(Mathf.SmoothStep(0.45f, 1.35f, distance), 1.3f));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _radial = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _radial;
        }

        // A very soft, wide vignette: no hard edge, it just darkens gradually away from the center.
        public static Sprite SoftEdgeShade()
        {
            if (_soft != null)
            {
                return _soft;
            }

            const int size = 128;
            var texture = NewTexture(size, FilterMode.Bilinear);
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.SmoothStep(0.25f, 1.55f, distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _soft = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _soft;
        }

        // 1 in the middle, fading to 0 at the edge: a soft glow.
        public static Sprite Glow()
        {
            const int size = 128;
            var texture = NewTexture(size, FilterMode.Bilinear);
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                    float a = 1f - Mathf.SmoothStep(0f, 1f, distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a * a);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Texture2D NewTexture(int size, FilterMode filter)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = filter,
                hideFlags = HideFlags.HideAndDontSave,
            };
        }

        // Perlin noise that wraps around: blends the four shifted copies at the borders.
        private static float Tileable(float u, float v, float frequency, int seed)
        {
            float a = Mathf.PerlinNoise(seed + u * frequency, seed + v * frequency);
            float b = Mathf.PerlinNoise(seed + (u - 1f) * frequency, seed + v * frequency);
            float c = Mathf.PerlinNoise(seed + (u - 1f) * frequency, seed + (v - 1f) * frequency);
            float d = Mathf.PerlinNoise(seed + u * frequency, seed + (v - 1f) * frequency);
            return a * (1f - u) * (1f - v) + b * u * (1f - v) + c * u * v + d * (1f - u) * v;
        }
    }
}
