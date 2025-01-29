using System;
using JahroConsole.Core.Logging;
using UnityEngine;

namespace JahroConsole.Core.Data
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