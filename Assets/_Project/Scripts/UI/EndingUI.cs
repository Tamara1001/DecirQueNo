using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class EndingUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            PlayerStats stats = manager.Stats;

            string title = manager.Ending == Ending.Happy ? "FINAL: DECIR QUE NO" : "FINAL: NO DIJE QUE NO";
            string closing = manager.LastFinalText;

            _text.text =
                $"{title}\n" +
                (string.IsNullOrEmpty(closing) ? "" : TextDisplay.ForScreen(closing, manager.Config.showEmojis) + "\n") +
                $"Seguidores: {stats.Followers:N0} / {manager.Config.goalFollowers:N0}   Cordura: {stats.Health}\n" +
                $"Si: {stats.YesCount}   Evasivas: {stats.EvasiveCount}   No: {stats.NoCount}   Sin abrir: {stats.ExpiredCount}\n" +
                $"Se acabo el tiempo: {stats.TimeoutCount}   Tiempo promedio de respuesta: {stats.AverageResponseSeconds:0.0} s\n" +
                $"Semilla: {manager.RunSeed}";

            _restartButton.onClick.AddListener(manager.StartNewGame);
            _menuButton.onClick.AddListener(manager.BackToMenu);
        }
    }
}
