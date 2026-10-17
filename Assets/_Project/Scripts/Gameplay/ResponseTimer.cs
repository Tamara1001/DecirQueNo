using System;
using UnityEngine;

namespace DecirQueNo
{
    public class ResponseTimer : MonoBehaviour
    {
        private float _duration;
        private float _remaining;
        private bool _running;

        // Normalized remaining time, 1 = full, 0 = expired.
        public event Action<float> OnTick;
        public event Action OnTimeout;

        public float Remaining => _remaining;
        public float Duration => _duration;
        public bool IsRunning => _running;

        public void StartTimer(float seconds)
        {
            _duration = Mathf.Max(0.01f, seconds);
            _remaining = _duration;
            _running = true;
            OnTick?.Invoke(1f);
        }

        public void StopTimer()
        {
            _running = false;
        }

        private void Update()
        {
            if (!_running)
            {
                return;
            }

            _remaining -= Time.deltaTime;
            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _running = false;
                OnTick?.Invoke(0f);
                OnTimeout?.Invoke();
                return;
            }

            OnTick?.Invoke(_remaining / _duration);
        }
    }
}
