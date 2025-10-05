using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using JahroConsole.Core.Context;

namespace JahroConsole.Core.Data
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
        private bool _autoDisableInRelease;

        [SerializeField]
        private IProjectSettings.SnapshotMode _snapshotMode;

        public event Action OnSettingsChanged;

        public bool JahroEnabled
        {
            get => _jahroEnabled;
            set
            {
                if (_jahroEnabled != value)
                {
                    _jahroEnabled = value;
                    NotifySettingsChanged();
                }
            }
        }

        public bool UseLaunchKeyboardShortcut
        {
            get => _useLaunchKeyboardShortcut;
            set
            {
                if (_useLaunchKeyboardShortcut != value)
                {
                    _useLaunchKeyboardShortcut = value;
                    NotifySettingsChanged();
                }
            }
        }

        public bool UseLaunchTapArea
        {
            get => _useLaunchTapArea;
            set
            {
                if (_useLaunchTapArea != value)
                {
                    _useLaunchTapArea = value;
                    NotifySettingsChanged();
                }
            }
        }

        public KeyCode LaunchKey
        {
            get => _launchKey;
            set
            {
                if (_launchKey != value)
                {
                    _launchKey = value;
                    NotifySettingsChanged();
                }
            }
        }

        public List<string> ActiveAssemblies
        {
            get => _activeAssemblies;
            set
            {
                if (_activeAssemblies != value)
                {
                    _activeAssemblies = value;
                    NotifySettingsChanged();
                }
            }
        }

        public string APIKey
        {
            get => _APIKey;
            set
            {
                if (_APIKey != value)
                {
                    _APIKey = value;
                    NotifySettingsChanged();
                }
            }
        }

        public bool AutoDisableInRelease
        {
            get => _autoDisableInRelease;
            set
            {
                if (_autoDisableInRelease != value)
                {
                    _autoDisableInRelease = value;
                    NotifySettingsChanged();
                }
            }
        }

        public IProjectSettings.SnapshotMode SnapshotingMode
        {
            get => _snapshotMode;
            set
            {
                if (_snapshotMode != value)
                {
                    _snapshotMode = value;
                    NotifySettingsChanged();
                }
            }
        }

        private void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }

        public static bool isSettingsFileExists()
        {
            return Resources.Load<JahroProjectSettings>(JahroConfig.ProjectSettingFile) != null;
        }

        public static JahroProjectSettings Load()
        {
#if UNITY_EDITOR
            string assetPath = $"{JahroConfig.ProjectResourcesFolder}/{JahroConfig.ProjectSettingFile}.asset";
            var assetDbSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<JahroProjectSettings>(assetPath);
            if (assetDbSettings != null)
            {
                return assetDbSettings;
            }
#endif

            return Resources.Load<JahroProjectSettings>(JahroConfig.ProjectSettingFile);
        }

        public static JahroProjectSettings CreateDefault()
        {
            var settings = ScriptableObject.CreateInstance<JahroProjectSettings>();
            settings._APIKey = "";
            settings._jahroEnabled = true;
            settings._useLaunchKeyboardShortcut = true;
            settings._useLaunchTapArea = true;
            settings._activeAssemblies = new List<string>();
            settings._launchKey = KeyCode.BackQuote;
            settings._autoDisableInRelease = false;
            settings._snapshotMode = IProjectSettings.SnapshotMode.StreamingExceptEditor;

#if UNITY_EDITOR
            var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player)
                .Where(assembly =>
                    !assembly.name.StartsWith("Unity") &&
                    !assembly.name.StartsWith("System") &&
                    !assembly.name.StartsWith("mscorlib") &&
                    !assembly.name.Contains("Editor"))
                .ToArray();

            foreach (var assembly in assemblies)
            {
                settings._activeAssemblies.Add(assembly.name);
            }
#endif
            return settings;
        }
    }
}