using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DecirQueNo
{
    public enum Ending
    {
        None,

        // Learned to say No: the final No, few Yes and mostly refusals.
        Happy,

        // Gave in a little to everything and did not reach the goal. Close to the sad one.
        Neutral,

        // Reached the goal by saying Yes.
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
        // The screen goes to black, holds a moment, and fades back in: between scenes and between days.
        private const float FadeSeconds = 0.6f;
        private const float BlackHoldSeconds = 0.25f;

        private ScreenFader _fader;
        private bool _loading;

        public static GameManager Instance { get; private set; }

        public GameConfig Config { get; private set; }
        public PlayerStats Stats { get; private set; }
        public int CurrentDay { get; private set; }
        public Ending Ending { get; private set; }

        // Seeds the bank order of this run.
        public int RunSeed { get; private set; }

        private readonly List<List<MessageData>> _schedule = new List<List<MessageData>>();

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
            yield return new WaitForSecondsRealtime(BlackHoldSeconds);
            SceneManager.LoadScene(sceneName);
            _loading = false;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _fader.FadeTo(0f, FadeSeconds);
        }

        public DaySummary LastSummary { get; set; }

        // Closing monologue of the final message, shown in the ending.
        public string LastFinalText { get; set; }

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
            LastFinalText = null;
            BuildSchedule();
        }

        // Messages of the given day (1-based) for this run.
        public IReadOnlyList<MessageData> GetDayMessages(int day)
        {
            return day >= 1 && day <= _schedule.Count ? _schedule[day - 1] : new List<MessageData>();
        }

        // Splits the pool across the days keeping the script order.
        private void BuildSchedule()
        {
            _schedule.Clear();
            var pool = new List<MessageData>();
            if (Config.pool != null)
            {
                pool.AddRange(Config.pool.messages);
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

        // Reaching the follower goal ends the run on the spot, whatever the day.
        public void EndDay(bool goalReached = false)
        {
            if (IsLastDay || goalReached)
            {
                Ending = EvaluateEnding();
            }

            LoadScene(SleepScene);
        }

        private Ending EvaluateEnding()
        {
            if (Stats.Followers >= Config.goalFollowers)
            {
                return Ending.Sad;
            }

            int refusals = Stats.EvasiveCount + Stats.NoCount;
            int neededRefusals = Mathf.CeilToInt(Config.TotalMessages * Config.goodMinNegativeShare);
            bool good = Stats.NoCount > 0 && Stats.YesCount <= Config.goodMaxYes && refusals >= neededRefusals;
            return good ? Ending.Happy : Ending.Neutral;
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
