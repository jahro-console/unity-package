using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Snapshots;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    public static class FileManager
    {

        public const string ProjectSettingFile = "jahro-settings";

        public const string RootFolder = "Assets";

        public const string SettingsFolder = "Jahro";

        internal const string AssetsSettingPath = AssetsFolder + "/" + JahroConsoleAssetsFolder + "/" + "Resources";

        const string AssetsFolder = "Assets";

        const string PackageFolder = "Packages/" + JahroPackageName + "/" + JahroConsoleAssetsFolder;

        const string JahroPackageName = "io.jahro.console";

        const string JahroConsoleAssetsFolder = "JahroPackage";

        const string MainDirectoryName = "jahro";

        const string SnapshotFileName = "snapshotinfo.json";

        const string WEBGL_SAVES_KEY = "webglprefs";

        const string JAHRO_PREFS_KEY = "jahro_saves";

        const string STORAGE_METHOD_KEY = "jahro_storage_method";

        private static StorageMethod _activeStorageMethod = StorageMethod.Unknown;

        private enum StorageMethod
        {
            Unknown,
            File,
            PlayerPrefs
        }

#if UNITY_WEBGL

        internal static void SaveToLocalSavesFile(string data)
        {
            PlayerPrefs.SetString(WEBGL_SAVES_KEY, data);
            PlayerPrefs.Save();
        }

#else

        internal static bool SaveToLocalSavesFile(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            if (_activeStorageMethod == StorageMethod.PlayerPrefs)
            {
                return SaveToPlayerPrefs(data);
            }

            if (_activeStorageMethod != StorageMethod.PlayerPrefs && TrySaveToFile(data))
            {
                _activeStorageMethod = StorageMethod.File;
                SaveStorageMethodPreference(StorageMethod.File);
                return true;
            }

            if (SaveToPlayerPrefs(data))
            {
                _activeStorageMethod = StorageMethod.PlayerPrefs;
                SaveStorageMethodPreference(StorageMethod.PlayerPrefs);
                return true;
            }
            return false;
        }
#endif

#if UNITY_WEBGL

        internal static string ReadLocalSaveFromFile(out bool dataLoaded)
        {
            string result = PlayerPrefs.GetString(WEBGL_SAVES_KEY, "");
            dataLoaded = !string.IsNullOrEmpty(result);
            return result;
        }

#else
        internal static string ReadLocalSaveFromFile(out bool dataLoaded)
        {
            dataLoaded = false;

            LoadStorageMethodPreference();

            if (_activeStorageMethod == StorageMethod.PlayerPrefs)
            {
                string prefsData = ReadFromPlayerPrefs(out bool prefsLoaded);
                if (prefsLoaded)
                {
                    dataLoaded = true;
                    return prefsData;
                }
            }

            if (_activeStorageMethod != StorageMethod.PlayerPrefs)
            {
                string fileData = TryReadFromFile(out bool fileLoaded);
                if (fileLoaded)
                {
                    _activeStorageMethod = StorageMethod.File;
                    SaveStorageMethodPreference(StorageMethod.File);
                    dataLoaded = true;
                    return fileData;
                }
            }

            string fallbackData = ReadFromPlayerPrefs(out bool fallbackLoaded);
            if (fallbackLoaded)
            {
                _activeStorageMethod = StorageMethod.PlayerPrefs;
                SaveStorageMethodPreference(StorageMethod.PlayerPrefs);
                dataLoaded = true;
                return fallbackData;
            }
            return null;
        }
#endif

        internal static void SaveSnapshotInfo(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
            {
                return;
            }

            string filePath = Path.Combine(session.folderPath, SnapshotFileName);

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string tempPath = filePath + ".tmp";

                using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    string json = JsonUtility.ToJson(session);
                    sw.Write(json);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Access denied when saving snapshot info: {ex.Message}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"IO error when saving snapshot info: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save snapshot info: {ex.GetType().Name} - {ex.Message}");
            }
        }

        internal static List<SnapshotSession> ReadSnapshotsInfos()
        {
            List<SnapshotSession> sessions = new List<SnapshotSession>();

            try
            {
                string folderPath = GetMainDirectory();
                if (!Directory.Exists(folderPath))
                {
                    return sessions;
                }

                string[] snapshotsDirectories = Directory.GetDirectories(folderPath);

                foreach (var dirPath in snapshotsDirectories)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
                        {
                            continue;
                        }

                        string dirName = Path.GetFileName(dirPath);
                        if (!dirName.StartsWith("jahro-snapshot"))
                        {
                            continue;
                        }

                        string infoFilePath = Path.Combine(dirPath, SnapshotFileName);
                        if (!File.Exists(infoFilePath))
                        {
                            try
                            {
                                Directory.Delete(dirPath, true);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Could not delete invalid snapshot directory: {ex.Message}");
                            }
                            continue;
                        }

                        string result = "";
                        using (FileStream stream = File.OpenRead(infoFilePath))
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            result = sr.ReadToEnd();
                        }

                        if (string.IsNullOrEmpty(result))
                        {
                            continue;
                        }

                        try
                        {
                            var snapshotInfo = JsonUtility.FromJson<SnapshotSession>(result);
                            if (snapshotInfo != null)
                            {
                                sessions.Add(snapshotInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to parse snapshot info: {ex.Message}");
                        }
                    }
                    catch (Exception dirEx)
                    {
                        Debug.LogWarning($"Error processing snapshot directory: {dirPath}, {dirEx.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Access denied when reading snapshots: {ex.Message}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"IO error when reading snapshots: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read snapshots: {ex.GetType().Name} - {ex.Message}");
            }

            return sessions;
        }

        internal static string[] GetSnapshotScreenshotsPath(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
            {
                return new string[0];
            }

            try
            {
                string folderPath = session.folderPath;
                if (!Directory.Exists(folderPath))
                {
                    return new string[0];
                }

                return Directory.GetFiles(folderPath, "jahro-screenshot-*.jpg");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get screenshot paths: {ex.Message}");
                return new string[0];
            }
        }

        internal static string GetPathToScreenShot(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
            {
                return null;
            }

            try
            {
                string folderPath = session.folderPath;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string time = DateTime.Now.ToString("HH-mm-ss-fff");
                string filename = $"jahro-screenshot-{date}-at-{time}.jpg";

                return Path.Combine(folderPath, filename);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create screenshot path: {ex.Message}");
                return null;
            }
        }

        internal static string GetSnapshotLogFolder(DateTime dateTime)
        {
            try
            {
                string folderPath = GetMainDirectory();
                string date = dateTime.ToString("yyyy-MM-dd");
                string time = dateTime.ToString("HH-mm-ss-fff");
                string folderName = $"jahro-snapshot-{date}-at-{time}";
                string snapshotsFolderPath = Path.Combine(folderPath, folderName);

                if (!Directory.Exists(snapshotsFolderPath))
                {
                    Directory.CreateDirectory(snapshotsFolderPath);
                }
                return snapshotsFolderPath;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Access denied when creating snapshot folder: {ex.Message}");
                return null;
            }
            catch (IOException ex)
            {
                Debug.LogError($"IO error when creating snapshot folder: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create snapshot folder: {ex.Message}");
                return null;
            }
        }

        internal static string GetSnapshotLogFilePath(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
            {
                return null;
            }

            try
            {
                string folderPath = session.folderPath;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filename = "jahro-snapshot-log";
                return Path.Combine(folderPath, filename);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get snapshot log file path: {ex.Message}");
                return null;
            }
        }

        internal static void RemoveSnapshotFolder(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
            {
                return;
            }

            try
            {
                string folderPath = session.folderPath;
                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                    foreach (var fileInfo in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                    }

                    Directory.Delete(folderPath, true);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Access denied when removing snapshot folder: {ex.Message}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"IO error when removing snapshot folder: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to remove snapshot folder: {ex.Message}");
            }
        }

        private static bool TrySaveToFile(string data)
        {
            string filePath = GetLocalSavesFilePath();
            string tempPath = filePath + ".tmp";

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    sw.Write(data);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);

                return true;
            }
            catch
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch { }

                return false;
            }
        }

        private static string TryReadFromFile(out bool success)
        {
            success = false;
            string filePath = GetLocalSavesFilePath();

            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader sr = new StreamReader(stream))
                {
                    string result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        success = true;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"File read failed: {ex.GetType().Name} - {ex.Message}");
            }

            return null;
        }

        private static bool SaveToPlayerPrefs(string data)
        {
            try
            {
                PlayerPrefs.SetString(JAHRO_PREFS_KEY, data);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PlayerPrefs save failed: {ex.Message}");
                return false;
            }
        }

        // Read from PlayerPrefs with error handling
        private static string ReadFromPlayerPrefs(out bool success)
        {
            success = false;
            try
            {
                string data = PlayerPrefs.GetString(JAHRO_PREFS_KEY, null);
                success = !string.IsNullOrEmpty(data);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs read failed: {ex.Message}");
                return null;
            }
        }

        private static void SaveStorageMethodPreference(StorageMethod method)
        {
            try
            {
                PlayerPrefs.SetInt(STORAGE_METHOD_KEY, (int)method);
                PlayerPrefs.Save();
            }
            catch { /* Ignore errors here */ }
        }

        // Load preferred storage method
        private static void LoadStorageMethodPreference()
        {
            if (_activeStorageMethod != StorageMethod.Unknown)
            {
                return; // Already loaded
            }

            try
            {
                if (PlayerPrefs.HasKey(STORAGE_METHOD_KEY))
                {
                    _activeStorageMethod = (StorageMethod)PlayerPrefs.GetInt(STORAGE_METHOD_KEY);
                }
            }
            catch { /* Use default if there's an error */ }
        }

        private static string GetLocalSavesFilePath()
        {
            string folderPath = GetMainDirectory();
            string filename = "state-save.dat";
            return folderPath + Path.DirectorySeparatorChar + filename;
        }

        private static string GetMainDirectory()
        {
            string folderPath = Application.persistentDataPath + Path.DirectorySeparatorChar + MainDirectoryName;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }
    }
}