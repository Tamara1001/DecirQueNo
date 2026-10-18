using System.Collections.Generic;
using UnityEngine;

namespace DecirQueNo
{
    [CreateAssetMenu(menuName = "DecirQueNo/Day", fileName = "Day_")]
    public class DayData : ScriptableObject
    {
        public int dayNumber = 1;

        [Tooltip("Real seconds the in-game clock takes from 09:00 to 23:00. The day ends when its messages are done, not by the clock.")]
        public float dayLength = 240f;
    }
}
