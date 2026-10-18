using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    public class SleepUI : MonoBehaviour
    {
        private static readonly Color MirrorLow = new Color(0.35f, 0.08f, 0.1f);
        private static readonly Color MirrorHigh = new Color(0.75f, 0.92f, 1f);

        [SerializeField] private TMP_Text _summaryText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Image _mirror;

        private void Start()
        {
            GameManager manager = GameManager.Instance;
            manager.EnsureGame();
            DaySummary s = manager.LastSummary;
            PlayerStats stats = manager.Stats;
            _summaryText.text = s == null
                ? "Resumen"
                : $"Dia {s.Day} terminado\nSi: {s.Yes}  Evasiva: {s.Maybe}  No: {s.No}  Expirados: {s.Expired}\n" +
                  $"Seguidores: {stats.Followers:N0} ({s.FollowersDelta:+#,0;-#,0;0})";

            // The mirror shows the health without numbers: dark red when low, bright when high.
            float health01 = Mathf.Clamp01(stats.Health / (float)manager.Config.maxHealth);
            _mirror.color = Color.Lerp(MirrorLow, MirrorHigh, health01);

            _continueButton.onClick.AddListener(manager.ContinueAfterSleep);
        }
    }
}
