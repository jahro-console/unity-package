using System;
using System.IO;
using Jahro.Core.Logging;
using UnityEngine;

namespace Jahro.Core.Data
{
    internal class ConsoleStorageController
    {

        private static ConsoleStorageController _instance;

        private ConsoleStorage _storage;

        private IProjectSettings _settings;

        internal ConsoleStorage ConsoleStorage { get { return _storage; } }

        internal static ConsoleStorageController Instance
        {
            private set
            {
                _instance = value;
            }
            get
            {
                if (_instance == null)
                {
                    _instance = new ConsoleStorageController();
                }
                return _instance;
            }
        }

        internal Action<ConsoleStorage> OnStorageLoad = delegate { };

        internal Action<ConsoleStorage> OnStorageSave = delegate { };

        private ConsoleStorageController() { }

        internal static void InitSettings(IProjectSettings settings)
        {
            Instance._settings = settings;
        }

        private void ReadLocalSaves()
        {
#if JAHRO_DEBUG
            Debug.Log("Start reading local saves");
#endif
            bool dataLoaded;
            var json = FileManager.ReadLocalSaveFromFile(out dataLoaded);
            if (dataLoaded)
            {
                try
                {
                    _storage = JsonUtility.FromJson<ConsoleStorage>(json);
                }
                catch
                {
                    JahroLogger.LogError(MessagesResource.LogSavesParsingError);
                    _storage = new ConsoleStorage();
                }
            }
            else
            {
                JahroLogger.LogDebug(MessagesResource.LogSavesLoadingError);
                _storage = new ConsoleStorage();
            }

            _storage.ProjectSettings = _settings;
            OnStorageLoad(_storage);
        }

        private void WriteLocalSaves()
        {
            OnStorageSave(_storage);

            string json = JsonUtility.ToJson(_storage, true);
            FileManager.SaveToLocalSavesFile(json);
        }

        internal static void LoadState()
        {
            Instance.ReadLocalSaves();
#if UNITY_EDITOR
            string version = "";
            var asm = Instance.GetType().Assembly;
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(asm);
            if (packageInfo == null)
            {
                var assembly = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName("Jahro.Console");
                var parentDirectory = Directory.GetParent(Path.GetDirectoryName(assembly));
                var filePath = Path.Combine(parentDirectory.FullName, "package.json");
                version = FileManager.ReadPackageJsonVersion(filePath);
            }
            else
            {
                version = packageInfo.version;
            }
            Instance._storage.CurrentJahroVersion = version;
#endif
        }

        internal static void SaveState()
        {
            Instance.WriteLocalSaves();
        }

        internal static void Release()
        {
            Instance = null;
        }

    }
}