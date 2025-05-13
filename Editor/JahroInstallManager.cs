using System;
using System.Diagnostics;
using System.IO;
using JahroConsole.Core.Data;
using JahroConsole.Editor;
using UnityEditor;
using UnityEngine;

namespace JahroConsole.Core.Registry
{

    [InitializeOnLoad]
    internal class JahroInstallManager : AssetPostprocessor
    {

        private const string BaseFolder = "Assets/Jahro";
        private const string ResourcesFolder = "Assets/Jahro/Resources";

        static JahroInstallManager()
        {
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                bool isSettingsFileExists = JahroProjectSettings.isSettingsFileExists();
                var settings = JahroProjectSettings.LoadOrCreate();
                ValidateSettingsFile(settings);
                if (!isSettingsFileExists)
                {
                    JahroEditorView.isFreshInstall = !isSettingsFileExists;
                    JahroEditorView.ShowWindow();
                }
            }
        }

        public static void ValidateSettingsFile(JahroProjectSettings settings)
        {
#if UNITY_EDITOR
            EnsureFolderExists(BaseFolder);
            EnsureFolderExists(ResourcesFolder);

            string assetPath = $"{ResourcesFolder}/{FileManager.ProjectSettingFile}.asset";

            if (!AssetExists<JahroProjectSettings>(assetPath))
            {
                CreateAsset(settings, assetPath);
            }
#endif
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

        private static bool AssetExists<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath) != null;
        }

        private static void CreateAsset<T>(T asset, string assetPath) where T : UnityEngine.Object
        {
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}