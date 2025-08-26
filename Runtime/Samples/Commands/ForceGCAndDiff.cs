using System;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace JahroConsole.Samples
{
    public static class ForceGCAndDiff
    {
        [JahroCommand(name: "force-gc", group: "Samples", description: "Forces garbage collection and compares memory before/after to identify potential retained objects")]
        public static string ForceGCAndDiffCommand()
        {
            try
            {
                return PerformGCAndDiff();
            }
            catch (Exception ex)
            {
                return $"GC and diff failed: {ex.Message}";
            }
        }

        private static string PerformGCAndDiff()
        {
            var stringBuilder = new StringBuilder(1024);

            long beforeManaged = GC.GetTotalMemory(false);
            long beforeNative = Profiler.GetTotalAllocatedMemoryLong();
            long beforeTotal = beforeManaged + beforeNative;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long afterManaged = GC.GetTotalMemory(false);
            long afterNative = Profiler.GetTotalAllocatedMemoryLong();
            long afterTotal = afterManaged + afterNative;

            stringBuilder.AppendLine("Force GC & Diff");
            stringBuilder.AppendLine("--------------------------------");
            stringBuilder.AppendLine($"Before: {FormatBytes(beforeTotal)}");
            stringBuilder.AppendLine($"After: {FormatBytes(afterTotal)}");

            long delta = afterTotal - beforeTotal;
            stringBuilder.AppendLine($"Delta: {(delta >= 0 ? "+" : "")}{FormatBytes(delta)}");

            return stringBuilder.ToString();
        }

        private static string FormatBytes(long bytes)
        {
            const double mb = 1024.0 * 1024.0;
            return $"{bytes / mb:F1} MB";
        }
    }
}
