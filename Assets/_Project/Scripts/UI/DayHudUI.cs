using TMPro;
using UnityEngine;

namespace DecirQueNo
{
    public class DayHudUI : MonoBehaviour
    {
        [SerializeField] private DayController _day;
        [SerializeField] private TMP_Text _dayText;
        [SerializeField] private TMP_Text _clockText;

        private void Start()
        {
            _day.OnClockChanged += HandleClock;
            if (_day.ClockText != null)
            {
                HandleClock(_day.DayNumber, _day.ClockText);
            }
        }

        private void OnDestroy()
        {
            if (_day != null)
            {
                _day.OnClockChanged -= HandleClock;
            }
        }

        private void HandleClock(int day, string clock)
        {
            _dayText.text = $"Dia {day}";
            _clockText.text = clock;
        }
    }
}
