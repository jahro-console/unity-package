using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JahroConsole.Samples
{
    public static class FindMissingScriptsInScene
    {
        [JahroCommand(name: "find-missing-scripts", group: "Samples", description: "Lists GameObjects with missing MonoBehaviour references in current scene")]
        public static string FindMissingScriptsCommand()
        {
            try
            {
                return FindMissingScripts();
            }
            catch (Exception ex)
            {
                return $"Find missing scripts failed: {ex.Message}";
            }
        }

        private static string FindMissingScripts()
        {
            var stringBuilder = new StringBuilder(1024);
            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            var objectsWithMissingScripts = new List<(GameObject obj, int count)>();

            foreach (var root in rootObjects)
            {
                CheckGameObjectAndChildren(root, objectsWithMissingScripts);
            }

            stringBuilder.AppendLine($"Missing Scripts Report — {activeScene.name}");
            stringBuilder.AppendLine("--------------------------------");

            if (objectsWithMissingScripts.Count == 0)
            {
                stringBuilder.AppendLine("No missing scripts found.");
                return stringBuilder.ToString();
            }

            stringBuilder.AppendLine($"Found {objectsWithMissingScripts.Count} GameObjects with missing scripts:");
            foreach (var (obj, count) in objectsWithMissingScripts)
            {
                stringBuilder.AppendLine($" - {GetGameObjectPath(obj)} ({count} missing)");
            }

            return stringBuilder.ToString();
        }

        private static void CheckGameObjectAndChildren(GameObject obj, System.Collections.Generic.List<(GameObject obj, int count)> results)
        {
            var components = obj.GetComponents<MonoBehaviour>();
            var missingCount = 0;

            foreach (var component in components)
            {
                if (component == null)
                    missingCount++;
            }

            if (missingCount > 0)
            {
                results.Add((obj, missingCount));
            }

            foreach (Transform child in obj.transform)
            {
                CheckGameObjectAndChildren(child.gameObject, results);
            }
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            var path = obj.name;
            var parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return "/" + path;
        }
    }
}
