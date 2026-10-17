using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    [CreateAssetMenu(menuName = "DecirQueNo/Day", fileName = "Day_")]
    public class DayData : ScriptableObject
    {
        public int dayNumber = 1;

        [Tooltip("Real seconds the day lasts at most. The in-game clock runs from 09:00 to 23:00.")]
        public float dayLength = 150f;

        [Tooltip("Seconds after the day starts when each message slot arrives. One entry per message of the day.")]
        public List<float> arrivalTimes = new List<float>();
    }
}
