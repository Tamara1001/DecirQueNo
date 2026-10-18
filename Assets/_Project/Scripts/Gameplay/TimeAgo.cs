using UnityEngine;

namespace DecirQueNo
{
    // "Hace N min" label for a message, derived from the in-game clock so it moves with the phone's time.
    public static class TimeAgo
    {
        private const float DayMinutes = 14f * 60f;

        public static string Format(float ageSeconds, float dayLengthSeconds)
        {
            float minutesPerSecond = DayMinutes / Mathf.Max(1f, dayLengthSeconds);
            int minutes = Mathf.FloorToInt(ageSeconds * minutesPerSecond / 10f) * 10;
            return minutes <= 0 ? "Ahora" : $"Hace {minutes} min";
        }
    }
}
