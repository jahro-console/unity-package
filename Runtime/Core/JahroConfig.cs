using System;
using System.IO;
using UnityEngine;

namespace JahroConsole
{
    public static class JahroConfig
    {
        #region Environment & Version

        public enum Environment
        {
            DEV,
            PROD
        }

        internal const Environment ENV = Environment.PROD;
        public static readonly string CurrentVersion = "1.0.0-beta1";

        #endregion

        #region Paths Configuration

        // Project Settings Structure
        public const string ProjectBaseFolder = "Assets/Jahro";
        public const string ProjectResourcesFolder = "Assets/Jahro/Resources";
        public const string ProjectSettingFile = "jahro-settings";


        // Runtime Directories
        public const string ApplicationMainDirectoryName = "jahro";
        public const string SnapshotFileName = "snapshotinfo.json";
        public const string StateSaveFileName = "state-save.dat";
        public const string LogDetailsFileName = "jahro-log-details.dat";

        // Computed Paths (initialized in Init method)
        public static string ApplicationPersistentDataPath { get; private set; }
        public static string ApplicationMainDirectory { get; private set; }
        public static string StateSaveFilePath { get; private set; }
        public static string LogDetailsFilePath { get; private set; }

        #endregion

        #region URLs Configuration

        // API Endpoints

        // Android emulator
        // public static string HostUrl => ENV == Environment.PROD
        //     ? "https://api.jahro.io/jahro-api"
        //     : "http://10.0.2.2:3000";

        // Unity Editor
        public static string HostUrl => ENV == Environment.PROD
            ? "https://api.jahro.io/jahro-api"
            : "http://localhost:3000";


        // Web URLs
        internal static readonly string RegisterUrl = "https://console.jahro.io";
        public static readonly string ChangelogUrl = "https://jahro.io/changelog";

        // Documentation URLs
        internal static readonly string DocumentationRoot = "https://jahro.io/docs/";
        internal static readonly string DocumentationWatcherOverview = "https://jahro.io/docs/watcher";
        internal static readonly string DocumentationCommandsOverview = "https://jahro.io/docs/unity-commands";
        internal static readonly string DocumentationSnapshots = "https://jahro.io/docs/snapshots";

        #endregion

        #region Network Configuration

        public const int DefaultTimeout = 10;
        public const int DefaultShortTimeout = 5;

        #endregion

        #region Logging Configuration

        public const string LogEntryDelimiter = "J#DE";
        public const int DefaultBufferSize = 4096;

        #endregion

        #region Initialization

        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize configuration - call this early in application lifecycle
        /// Handles Application.persistentDataPath safely
        /// </summary>
        public static void Init()
        {
            if (_isInitialized) return;

            ApplicationPersistentDataPath = Application.persistentDataPath;
            ApplicationMainDirectory = Path.Combine(ApplicationPersistentDataPath, ApplicationMainDirectoryName);
            StateSaveFilePath = Path.Combine(ApplicationMainDirectory, StateSaveFileName);
            LogDetailsFilePath = Path.Combine(ApplicationPersistentDataPath, LogDetailsFileName);

            if (!Directory.Exists(ApplicationMainDirectory))
            {
                Directory.CreateDirectory(ApplicationMainDirectory);
            }

            _isInitialized = true;
        }

        #endregion

        #region Utility Methods


        #endregion
    }
}