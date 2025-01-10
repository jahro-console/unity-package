using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Jahro.Core.Data
{

    public class JahroProjectSettings : ScriptableObject, IProjectSettings
    {
        [SerializeField]
        private string _APIKey;

        [SerializeField]
        private bool _jahroEnabled;

        [SerializeField]
        private bool _useLaunchKeyboardShortcut;

        [SerializeField]
        private bool _useLaunchTapArea;

        [SerializeField]
        private KeyCode _launchKey;

        [SerializeField]
        private List<string> _activeAssemblies;

        [SerializeField]
        private bool _duplicateToUnityConsole;

        public bool JahroEnabled { get => _jahroEnabled; set => _jahroEnabled = value; }

        public bool UseLaunchKeyboardShortcut { get => _useLaunchKeyboardShortcut; set => _useLaunchKeyboardShortcut = value; }

        public bool UseLaunchTapArea { get => _useLaunchTapArea; set => _useLaunchTapArea = value; }

        public KeyCode LaunchKey { get => _launchKey; set => _launchKey = value; }

        public List<string> ActiveAssemblies { get => _activeAssemblies; set => _activeAssemblies = value; }

        public bool DuplicateToUnityConsole { get => _duplicateToUnityConsole; set => _duplicateToUnityConsole = value; }

        public string APIKey { get => _APIKey; set => _APIKey = value; }

        public static JahroProjectSettings LoadOrCreate()
        {
            var settings = Resources.Load<JahroProjectSettings>(FileManager.ProjectSettingFile);

            if (settings == null)
            {
                settings = CreateDefault();
            }
            ValidateSettingsFile(settings);
            return settings;
        }

        public static JahroProjectSettings CreateDefault()
        {
            var settings = ScriptableObject.CreateInstance<JahroProjectSettings>();
            settings._APIKey = "";
            settings.JahroEnabled = true;
            settings._useLaunchKeyboardShortcut = true;
            settings._useLaunchTapArea = true;
            settings._activeAssemblies = new List<string>();
            settings._duplicateToUnityConsole = false;
            settings._launchKey = KeyCode.BackQuote;

#if UNITY_EDITOR
            var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player);
            foreach (var assembly in assemblies)
            {
                settings._activeAssemblies.Add(assembly.name);
            }
#endif
            return settings;
        }

        public static void ValidateSettingsFile(JahroProjectSettings settings)
        {
#if UNITY_EDITOR

            var assetsResult = AssetDatabase.FindAssets(" t:JahroProjectSettings " + FileManager.ProjectSettingFile);

            if (assetsResult == null || assetsResult.Length == 0)
            {
                // if (AssetDatabase.IsValidFolder(FileManager.PackageSettingPath))
                // {
                //     CreateAssetsFolder();
                // }
                AssetDatabase.CreateAsset(settings, FileManager.AssetsSettingPath + "/" + FileManager.ProjectSettingFile + ".asset");
                AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}