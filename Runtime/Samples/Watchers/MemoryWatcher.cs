using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace JahroConsole.Samples
{
    public static class MemoryWatcher
    {
        private const float UPDATE_INTERVAL = 1f; // 1Hz update rate
        private const float MB = 1024f * 1024f;
        private const float WARNING_THRESHOLD = 0.8f; // 80% of available memory

        private static float _lastUpdateTime;
        private static long _lastManagedMemory;
        private static long _lastNativeMemory;
        private static string _cachedManagedValue;
        private static string _cachedNativeValue;
        private static string _cachedDeltaValue;

        static MemoryWatcher()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        private static void OnBeforeRender()
        {
            if (!Application.isPlaying)
                return;

            var currentTime = Time.unscaledTime;
            if (currentTime - _lastUpdateTime < UPDATE_INTERVAL)
                return;

            var managedMemory = GC.GetTotalMemory(false);
            var nativeMemory = Profiler.GetTotalAllocatedMemoryLong();
            var totalMemory = managedMemory + nativeMemory;

            var managedDelta = managedMemory - _lastManagedMemory;
            var nativeDelta = nativeMemory - _lastNativeMemory;
            var totalDelta = managedDelta + nativeDelta;

            _lastManagedMemory = managedMemory;
            _lastNativeMemory = nativeMemory;
            _lastUpdateTime = currentTime;

            var isWarning = false;
#if UNITY_ANDROID || UNITY_IOS
            var systemMemory = SystemInfo.systemMemorySize * MB;
            isWarning = totalMemory > systemMemory * WARNING_THRESHOLD;
#endif

            var color = isWarning ? "red" : "#17E96B";
            var deltaColor = totalDelta > 0 ? "yellow" : "#17E96B";

            var managedPercent = totalMemory > 0 ? (managedMemory * 100f / totalMemory) : 0f;
            var nativePercent = totalMemory > 0 ? (nativeMemory * 100f / totalMemory) : 0f;

            _cachedManagedValue = $"Managed: {managedMemory / MB:F1} MB ({managedPercent:F0}%)";
            _cachedNativeValue = $"Native: {nativeMemory / MB:F1} MB ({nativePercent:F0}%)";
            _cachedDeltaValue = $"<color={deltaColor}>Δ {(totalDelta > 0 ? "+" : "")}{totalDelta / MB:F1} MB</color>";
        }

        [JahroWatch(name: "memory-managed", group: "Samples", description: "Managed heap memory usage")]
        public static string ManagedMemory => _cachedManagedValue;

        [JahroWatch(name: "memory-native", group: "Samples", description: "Native memory usage (textures, meshes, etc)")]
        public static string NativeMemory => _cachedNativeValue;

        [JahroWatch(name: "memory-delta", group: "Samples", description: "Memory change since last update")]
        public static string MemoryDelta => _cachedDeltaValue;
    }
}
