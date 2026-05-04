using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using UnityEditor;
using UnityEngine;

namespace JahroConsole.Editor
{

    [InitializeOnLoad]
    internal class JahroInstallManager
    {
        static JahroInstallManager()
        {
#if UNITY_EDITOR
            EditorApplication.projectChanged += onProjectChanged;
#endif
        }

        private static void onProjectChanged()
        {
            if (Application.isBatchMode || Application.isPlaying)
            {
                return;
            }

            string assetPath = $"{JahroConfig.ProjectResourcesFolder}/{JahroConfig.ProjectSettingFile}.asset";
            bool settingsExisted = File.Exists(assetPath);

            ValidateSettingsFile(JahroProjectSettings.Load());

            if (!settingsExisted)
            {
                JahroEditorView.isFreshInstall = true;
                JahroEditorView.ShowWindow();
            }
        }

        private static JahroProjectSettings ValidateSettingsFile(JahroProjectSettings settings)
        {
#if UNITY_EDITOR
            string assetPath = $"{JahroConfig.ProjectResourcesFolder}/{JahroConfig.ProjectSettingFile}.asset";

            if (File.Exists(assetPath))
            {
                return settings ?? AssetDatabase.LoadAssetAtPath<JahroProjectSettings>(assetPath);
            }

            if (settings == null)
            {
                settings = JahroProjectSettings.CreateDefault();
            }

            EnsureFolderExists(JahroConfig.ProjectBaseFolder);
            EnsureFolderExists(JahroConfig.ProjectResourcesFolder);

            if (AssetDatabase.LoadAssetAtPath<JahroProjectSettings>(assetPath) == null)
            {
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif
            return settings;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            string parentFolder = Path.GetDirectoryName(folderPath).Replace("\\", "/");
            string folderName = Path.GetFileName(folderPath);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
