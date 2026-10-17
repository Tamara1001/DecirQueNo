using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DecirQueNo
{
    public enum Ending
    {
        None,
        Happy,
        Neutral,
        Sad
    }

    public class GameManager : MonoBehaviour
    {
        public const string MainMenuScene = "MainMenu";
        public const string InstructionsScene = "Instructions";
        public const string GameScene = "Game";
        public const string SleepScene = "Sleep";
        public const string EndingScene = "Ending";

        private const string ConfigResourcePath = "GameConfig";
        private const float FadeSeconds = 0.35f;

        private ScreenFader _fader;
        private bool _loading;

        public static GameManager Instance { get; private set; }

        public GameConfig Config { get; private set; }
        public PlayerStats Stats { get; private set; }
        public int CurrentDay { get; private set; }
        public Ending Ending { get; private set; }

        // Seeds the distractors and bank order of this run.
        public int RunSeed { get; private set; }

        private readonly List<List<MessageData>> _schedule = new List<List<MessageData>>();

        // Messages queued for the next day (e.g. insistence after a No).
        public List<MessageData> CarryOverMessages { get; } = new List<MessageData>();

        public DayData CurrentDayData =>
            CurrentDay >= 1 && CurrentDay <= Config.days.Count ? Config.days[CurrentDay - 1] : null;

        public bool IsLastDay => CurrentDay >= Config.days.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            var go = new GameObject(nameof(GameManager));
            go.AddComponent<GameManager>();
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

            Config = Resources.Load<GameConfig>(ConfigResourcePath);
            if (Config == null)
            {
                Debug.LogError($"[GameManager] Missing Resources/{ConfigResourcePath} asset.");
            }

            _fader = ScreenFader.Create(transform);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        // Fades to black, loads the scene and fades back in.
        public void LoadScene(string sceneName)
        {
            if (_loading)
            {
                return;
            }

            StartCoroutine(FadeAndLoad(sceneName));
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            _loading = true;
            yield return _fader.FadeTo(1f, FadeSeconds);
            SceneManager.LoadScene(sceneName);
            _loading = false;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _fader.FadeTo(0f, FadeSeconds);
        }

        public DaySummary LastSummary { get; set; }

        public void StartNewGame()
        {
            ResetGame();
            LoadScene(GameScene);
        }

        // Lets the Game scene run on its own from the editor.
        public void EnsureGame()
        {
            if (Stats == null)
            {
                ResetGame();
            }
        }

        private void ResetGame()
        {
            Stats = new PlayerStats(Config);
            RunSeed = Random.Range(1, int.MaxValue);
            CurrentDay = 1;
            Ending = Ending.None;
            LastSummary = null;
            CarryOverMessages.Clear();
            BuildSchedule();
        }

        // Messages of the given day (1-based) for this run.
        public IReadOnlyList<MessageData> GetDayMessages(int day)
        {
            return day >= 1 && day <= _schedule.Count ? _schedule[day - 1] : new List<MessageData>();
        }

        // Shuffles the pool with the run seed and splits it across the days.
        private void BuildSchedule()
        {
            _schedule.Clear();
            var pool = new List<MessageData>();
            if (Config.pool != null)
            {
                pool.AddRange(Config.pool.messages);
            }

            var rng = new System.Random(RunSeed);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            int next = 0;
            foreach (int count in Config.messagesPerDay)
            {
                var day = new List<MessageData>();
                for (int i = 0; i < count && next < pool.Count; i++)
                {
                    day.Add(pool[next++]);
                }

                _schedule.Add(day);
            }
        }

        public void EndDay()
        {
            if (IsLastDay)
            {
                Ending = EvaluateEnding();
            }

            LoadScene(SleepScene);
        }

        private Ending EvaluateEnding()
        {
            if (Stats.Health >= Config.happyMinHealth && Stats.NoCount >= Config.happyMinNoCount)
            {
                return Ending.Happy;
            }

            return Stats.Health < Config.sadHealthBelow ? Ending.Sad : Ending.Neutral;
        }

        public void ContinueAfterSleep()
        {
            if (Ending != Ending.None)
            {
                LoadScene(EndingScene);
                return;
            }

            CurrentDay++;
            LoadScene(GameScene);
        }

        public void BackToMenu()
        {
            LoadScene(MainMenuScene);
        }
    }
}
