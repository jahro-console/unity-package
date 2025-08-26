using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JahroConsole.Samples
{
    public static class ListLoadedScenes
    {
        [JahroCommand(name: "list-loaded-scenes", group: "Samples", description: "Shows current scene situation - which scenes are loaded, active, and available")]
        public static string ListLoadedScenesCommand()
        {
            try
            {
                return GetSceneSituation();
            }
            catch (Exception ex)
            {
                return $"Scene situation check failed: {ex.Message}";
            }
        }

        private static string GetSceneSituation()
        {
            var stringBuilder = new StringBuilder(1024);

            var activeScene = SceneManager.GetActiveScene();
            var loadedScenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            stringBuilder.AppendLine("Scenes");
            stringBuilder.AppendLine("--------------------------------");

            stringBuilder.AppendLine($"Active Scene: {activeScene.name}");
            stringBuilder.AppendLine($"Loaded Scenes: {loadedScenes.Length}");
            stringBuilder.AppendLine();

            if (loadedScenes.Length > 0)
            {
                stringBuilder.AppendLine("Currently Loaded:");
                for (int i = 0; i < loadedScenes.Length; i++)
                {
                    var scene = loadedScenes[i];
                    var status = scene == activeScene ? "ACTIVE" : "Loaded";
                    stringBuilder.AppendLine($" • {scene.name} ({status})");
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }
    }
}
