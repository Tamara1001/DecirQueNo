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

            string title = manager.Ending == Ending.Happy ? "FINAL FELIZ"
                : manager.Ending == Ending.Neutral ? "FINAL NEUTRO"
                : "FINAL TRISTE";

            _text.text =
                $"{title}\n" +
                $"Seguidores: {stats.Followers:N0} / {manager.Config.goalFollowers:N0}   Salud: {stats.Health}\n" +
                $"Si: {stats.YesCount}   Tal vez: {stats.MaybeCount}   No: {stats.NoCount}   Expiradas: {stats.ExpiredCount}\n" +
                $"Mejor racha de No: {stats.BestNoStreak}   Timeouts: {stats.TimeoutCount}\n" +
                $"Tiempo promedio de respuesta: {stats.AverageResponseSeconds:0.0} s   Nivel de No final: {stats.NoLevelIndex + 1}\n" +
                $"Semilla: {manager.RunSeed}";

            _restartButton.onClick.AddListener(manager.StartNewGame);
            _menuButton.onClick.AddListener(manager.BackToMenu);
        }
    }
}
