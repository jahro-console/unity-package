using System;
using UnityEngine;

namespace JahroConsole.Samples
{
    public static class FpsWatcher
    {
        private const float UI_REFRESH_RATE = 0.25f; // 4Hz
        private const float EMA_ALPHA = 0.1f; // Smoothing factor for EMA
        private const float RESET_INTERVAL = 60f; // Reset min/max every minute

        private static float _lastUpdateTime;
        private static float _lastResetTime;
        private static float _emaFps;
        private static float _minFps = float.MaxValue;
        private static float _maxFps;
        private static string _cachedRangeValue;
        private static string _cachedHealthValue;

        static FpsWatcher()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        private static void OnBeforeRender()
        {
            if (!Application.isPlaying || Time.unscaledDeltaTime <= 0)
                return;

            var currentTime = Time.unscaledTime;
            var currentFps = 1f / Time.unscaledDeltaTime;

            _emaFps = _emaFps == 0
                ? currentFps
                : _emaFps + EMA_ALPHA * (currentFps - _emaFps);

            if (currentTime - _lastResetTime >= RESET_INTERVAL)
            {
                _minFps = currentFps;
                _maxFps = currentFps;
                _lastResetTime = currentTime;
            }

            _minFps = Mathf.Min(_minFps, currentFps);
            _maxFps = Mathf.Max(_maxFps, currentFps);

            if (currentTime - _lastUpdateTime >= UI_REFRESH_RATE)
            {
                UpdateCachedValues();
                _lastUpdateTime = currentTime;
            }
        }

        private static void UpdateCachedValues()
        {
            _cachedRangeValue = $"Range (60s): {_minFps:F0}-{_maxFps:F0} FPS";

            var color = _emaFps >= 60f ? "#17E96B" : (_emaFps >= 30f ? "yellow" : "red");
            _cachedHealthValue = $"<color={color}>{_emaFps:F1} FPS</color>";
        }

        [JahroWatch(name: "fps-range", group: "Samples", description: "FPS range (min/max) over 60 seconds")]
        public static string FpsRange => _cachedRangeValue;

        [JahroWatch(name: "fps-health", group: "Samples", description: "FPS health status with color indicator")]
        public static string FpsHealth => _cachedHealthValue;

    }
}
