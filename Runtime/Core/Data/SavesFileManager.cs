using System;
using System.IO;
using JahroConsole.Core.Context;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    public static class SavesFileManager
    {
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

        private static void LoadStorageMethodPreference()
        {
            if (_activeStorageMethod != StorageMethod.Unknown)
            {
                return;
            }

            try
            {
                if (PlayerPrefs.HasKey(STORAGE_METHOD_KEY))
                {
                    _activeStorageMethod = (StorageMethod)PlayerPrefs.GetInt(STORAGE_METHOD_KEY);
                }
            }
            catch { }
        }

        private static string GetLocalSavesFilePath()
        {
            string folderPath = JahroConfig.ApplicationMainDirectory;
            string filename = "state-save.dat";
            return folderPath + Path.DirectorySeparatorChar + filename;
        }
    }
}