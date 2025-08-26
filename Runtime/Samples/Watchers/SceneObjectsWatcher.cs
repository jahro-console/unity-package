using UnityEngine;
using UnityEngine.SceneManagement;

namespace JahroConsole.Samples
{
    public static class SceneObjectsWatcher
    {

        [JahroWatch(name: "scene-count", group: "Samples", description: "Number of loaded scenes")]
        public static string SceneCount
        {
            get
            {
                if (!Application.isPlaying)
                    return null;

                var activeScene = SceneManager.GetActiveScene();
                var sceneCount = SceneManager.sceneCount;
                return $"Scenes: {sceneCount} (Active: {activeScene.name})";
            }
        }

    }
}