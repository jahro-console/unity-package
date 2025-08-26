using System;
using System.Text;
using UnityEngine;

namespace JahroConsole.Samples
{
    public static class DumpPhysicsState
    {
        [JahroCommand(name: "dump-physics-state", group: "Samples", description: "Shows current physics state and settings")]
        public static string DumpPhysicsStateCommand()
        {
            try
            {
                return GetPhysicsSnapshot();
            }
            catch (Exception ex)
            {
                return $"Physics state dump failed: {ex.Message}";
            }
        }

        private static string GetPhysicsSnapshot()
        {
            var stringBuilder = new StringBuilder(1024);

            stringBuilder.AppendLine("Physics Snapshot");
            stringBuilder.AppendLine("--------------------------------");

            var allRigidbodies = GameObject.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            var activeRigidbodies = 0;
            var sleepingRigidbodies = 0;

            foreach (var rb in allRigidbodies)
            {
                if (rb.IsSleeping())
                    sleepingRigidbodies++;
                else
                    activeRigidbodies++;
            }

            var allColliders = GameObject.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            var activeColliders = 0;

            foreach (var col in allColliders)
            {
                if (col.enabled)
                    activeColliders++;
            }

            stringBuilder.AppendLine($"Active Rigidbodies: {activeRigidbodies}");
            stringBuilder.AppendLine($"Sleeping Rigidbodies: {sleepingRigidbodies}");
            stringBuilder.AppendLine($"Active Colliders: {activeColliders}");

            stringBuilder.AppendLine("Physics settings:");
            stringBuilder.AppendLine($" - gravity: {Physics.gravity}");
            stringBuilder.AppendLine($" - fixedDeltaTime: {Time.fixedDeltaTime:F3}");

            return stringBuilder.ToString();
        }
    }
}
