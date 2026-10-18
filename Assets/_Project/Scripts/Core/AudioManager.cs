using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DecirQueNo
{
    public enum Sfx
    {
        Notification,
        Type,
        Send,
        MenuSelect
    }

    // Plays the music of each scene and the UI sounds. Clips live in Resources/Audio.
    public class AudioManager : MonoBehaviour
    {
        private const string Folder = "Audio/";
        private const float MixFollowSeconds = 3f;

        // The sliders are perceptual: the level is squared and then scaled down, so the whole range is quiet and the
        // low end of the slider is really low.
        private const float MusicMaster = 0.4f;
        private const float SfxMaster = 0.5f;

        // The music of a run starts a few seconds after the scene, and the "bad" track only joins in
        // when the sanity is really low.
        private const float MixStartDelaySeconds = 3.5f;
        private const float BadTrackStartsBelow = 0.55f;
        private const float BadTrackFullBelow = 0.1f;
        private const float BadTrackMaxShare = 0.8f;
        private const string HealthMix = "HEALTH_MIX";

        // The sound of typing is higher when it is the player who types; the characters use the default pitch.
        public const float PlayerTypePitch = 1.3f;

        private const int SfxVoices = 6;
        private const float TypeCooldownSeconds = 0.11f;
        private const float DefaultCooldownSeconds = 0.04f;

        private static readonly Dictionary<Sfx, string> SfxNames = new Dictionary<Sfx, string>
        {
            { Sfx.Notification, "SFX_Notificacion" },
            { Sfx.Type, "SFX_Escribir" },
            { Sfx.Send, "SFX_Enviar_Mensaje" },
            { Sfx.MenuSelect, "SFX_Seleccion_Menu" },
        };

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioSource _musicA;
        private AudioSource _musicB;
        private readonly List<AudioSource> _sfxVoices = new List<AudioSource>();
        private readonly Dictionary<Sfx, float> _lastPlayed = new Dictionary<Sfx, float>();
        private int _nextVoice;
        private float _musicLevel = -1f;
        private float _sfxLevel = -1f;
        private AudioSource _activeMusic;
        private string _currentMusic;
        private float _mixAudibleAt;
        private AudioSource _good;
        private AudioSource _bad;

        public static AudioManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            new GameObject(nameof(AudioManager)).AddComponent<AudioManager>();
        }

        public static void Play(Sfx sfx, float pitch = 1f)
        {
            if (Instance != null)
            {
                Instance.PlaySfx(sfx, pitch);
            }
        }

        // Volume chosen by the player in the pause menu (0 to 1). Until they touch it, it is the value of GameConfig.
        public float MusicLevel
        {
            get
            {
                if (_musicLevel < 0f)
                {
                    GameManager manager = GameManager.Instance;
                    _musicLevel = manager != null && manager.Config != null ? manager.Config.musicVolume : 0.3f;
                }

                return _musicLevel;
            }
            set
            {
                _musicLevel = Mathf.Clamp01(value);
                if (_currentMusic != HealthMix && _activeMusic != null && _activeMusic.isPlaying)
                {
                    _activeMusic.volume = MusicGain;
                }
            }
        }

        private float MusicGain => MusicLevel * MusicLevel * MusicMaster;

        private float SfxGain => SfxLevel * SfxLevel * SfxMaster;

        public float SfxLevel
        {
            get
            {
                if (_sfxLevel < 0f)
                {
                    GameManager manager = GameManager.Instance;
                    _sfxLevel = manager != null && manager.Config != null ? manager.Config.sfxVolume : 1f;
                }

                return _sfxLevel;
            }
            set => _sfxLevel = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicA = CreateSource(true);
            _musicB = CreateSource(true);
            for (int i = 0; i < SfxVoices; i++)
            {
                AudioSource voice = CreateSource(false);
                voice.volume = 1f;
                _sfxVoices.Add(voice);
            }

            _good = CreateSource(true);
            _bad = CreateSource(true);
            _activeMusic = _musicA;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            PlayForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        private AudioSource CreateSource(bool loop)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = 0f;
            return source;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayForScene(scene.name);
        }

        // In Game and Sleep both health tracks play in sync: the good one follows the health and the bad one
        // gets what is missing (100% health = only the good track, 0% = only the bad track).
        private void Update()
        {
            bool mix = _currentMusic == HealthMix;
            float volume = MusicVolume();
            float good = 0f;
            float bad = 0f;
            if (mix)
            {
                float health01 = Health01();
                good = volume * Mathf.Lerp(1f, 0.55f, 1f - health01);

                // 0 while the sanity is fine, 1 when it is almost gone, smoothly in between.
                float danger = Mathf.Clamp01((BadTrackStartsBelow - health01) / (BadTrackStartsBelow - BadTrackFullBelow));
                bad = volume * BadTrackMaxShare * danger * danger * (3f - 2f * danger);

                if (Time.unscaledTime < _mixAudibleAt)
                {
                    good = 0f;
                    bad = 0f;
                }
            }

            float step = Time.unscaledDeltaTime / MixFollowSeconds;
            _good.volume = Mathf.MoveTowards(_good.volume, good, step);
            _bad.volume = Mathf.MoveTowards(_bad.volume, bad, step);
            if (!mix)
            {
                StopIfSilent(_good);
                StopIfSilent(_bad);
            }
        }

        private static void StopIfSilent(AudioSource source)
        {
            if (source.isPlaying && source.volume <= 0f)
            {
                source.Stop();
            }
        }

        private static float Health01()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null || manager.Stats == null || manager.Config == null)
            {
                return 1f;
            }

            return Mathf.Clamp01(manager.Stats.Health / (float)manager.Config.maxHealth);
        }

        private float MusicVolume()
        {
            return MusicGain;
        }

        private void PlayForScene(string scene)
        {
            switch (scene)
            {
                case GameManager.GameScene:
                case GameManager.SleepScene:
                    StartHealthMix();
                    break;
                case GameManager.EndingScene:
                    GameManager manager = GameManager.Instance;
                    PlayMusic(EndingTrack(manager != null ? manager.Ending : Ending.Sad), false);
                    break;
                default:
                    PlayMusic("BGM_Menu");
                    break;
            }
        }

        // Good ending: win theme. Middle ending: its own theme if the clip exists. Bad ending: lose theme.
        private string EndingTrack(Ending ending)
        {
            if (ending == Ending.Happy)
            {
                return "BGM_Ganar";
            }

            if (ending == Ending.Neutral && Resources.Load<AudioClip>(Folder + "BGM_Neutral") != null)
            {
                return "BGM_Neutral";
            }

            return "BGM_Perder";
        }

        private void StartHealthMix()
        {
            if (_currentMusic == HealthMix)
            {
                return;
            }

            AudioClip good = GetClip("BGM_Buena_Salud");
            AudioClip bad = GetClip("BGM_Poca_Salud");
            if (good == null || bad == null)
            {
                return;
            }

            // Fades out whatever was playing; the mix fades in from silence.
            AudioSource from = _activeMusic;
            StopAllCoroutines();
            StartCoroutine(Crossfade(from, null));
            _good.clip = good;
            _bad.clip = bad;
            _good.time = 0f;
            _bad.time = 0f;
            _good.Play();
            _bad.Play();
            _currentMusic = HealthMix;
            _mixAudibleAt = Time.unscaledTime + MixStartDelaySeconds;
        }

        private void PlayMusic(string clipName, bool loop = true)
        {
            if (clipName == _currentMusic)
            {
                return;
            }

            AudioClip clip = GetClip(clipName);
            if (clip == null)
            {
                return;
            }

            AudioSource from = _activeMusic;
            AudioSource to = from == _musicA ? _musicB : _musicA;
            to.clip = clip;
            to.loop = loop;
            to.time = 0f;
            to.Play();
            _activeMusic = to;
            _currentMusic = clipName;
            StopAllCoroutines();
            StartCoroutine(Crossfade(from, to));
        }

        private System.Collections.IEnumerator Crossfade(AudioSource from, AudioSource to)
        {
            float fromStart = from.volume;
            float toStart = to != null ? to.volume : 0f;
            float volume = MusicGain;
            float seconds = GameManager.Instance != null && GameManager.Instance.Config != null
                ? GameManager.Instance.Config.musicCrossfadeSeconds
                : 1f;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                from.volume = Mathf.Lerp(fromStart, 0f, k);
                if (to != null)
                {
                    to.volume = Mathf.Lerp(toStart, volume, k);
                }

                yield return null;
            }

            from.volume = 0f;
            from.Stop();
            if (to != null)
            {
                to.volume = volume;
            }
        }

        private void PlaySfx(Sfx sfx, float pitch)
        {
            // A cooldown per sound: picking blocks quickly must not stack the same sound four times.
            float now = Time.unscaledTime;
            float cooldown = sfx == Sfx.Type ? TypeCooldownSeconds : DefaultCooldownSeconds;
            if (_lastPlayed.TryGetValue(sfx, out float last) && now - last < cooldown)
            {
                return;
            }

            AudioClip clip = GetClip(SfxNames[sfx]);
            if (clip == null)
            {
                return;
            }

            _lastPlayed[sfx] = now;

            // Every sound gets a voice of its own, so each can have its pitch.
            AudioSource voice = _sfxVoices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _sfxVoices.Count;
            voice.Stop();
            voice.pitch = pitch;
            voice.volume = SfxGain;
            voice.clip = clip;
            voice.Play();
        }

        private AudioClip GetClip(string clipName)
        {
            if (!_clips.TryGetValue(clipName, out AudioClip clip))
            {
                clip = Resources.Load<AudioClip>(Folder + clipName);
                if (clip == null)
                {
                    Debug.LogWarning($"[AudioManager] Missing Resources/{Folder}{clipName}");
                }

                _clips[clipName] = clip;
            }

            return clip;
        }
    }
}
