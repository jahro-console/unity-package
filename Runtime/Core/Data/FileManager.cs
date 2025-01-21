using System;
using System.Collections.Generic;
using System.IO;
using Jahro.Core.Snapshots;
using UnityEngine;

namespace Jahro.Core.Data
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

#if UNITY_WEBGL

        internal static void SaveToLocalSavesFile(string data)
        {
            PlayerPrefs.SetString(WEBGL_SAVES_KEY, data);
            PlayerPrefs.Save();
        }

#else

        internal static void SaveToLocalSavesFile(string data)
        {
            string filePath = GetLocalSavesFilePath();
            FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
#if JAHRO_DEBUG
            Debug.Log("Saves: " + filePath + " > exists: " + File.Exists(filePath));
#endif
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.Write(data);
            }
            stream.Close();

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
            string result = string.Empty;
            string path = GetLocalSavesFilePath();
            if (File.Exists(path) == false)
            {
                dataLoaded = false;
                return null;
            }
            FileStream stream = File.Open(path, FileMode.Open);
#if JAHRO_DEBUG
            Jahro.Log("Saves load: " + path);
#endif
            using (StreamReader sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }
            stream.Close();
            dataLoaded = !string.IsNullOrEmpty(result);
            return result;
        }
#endif

        internal static void SaveSnapshotInfo(SnapshotSession session)
        {
            string filePath = Path.Combine(session.folderPath, SnapshotFileName);
            FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(stream))
            {
                string json = JsonUtility.ToJson(session);
                sw.Write(json);
            }
        }

        internal static List<SnapshotSession> ReadSnapshotsInfos()
        {
            string folderPath = GetMainDirectory();
            string[] snapshotsDirectories = Directory.GetDirectories(folderPath);
            List<SnapshotSession> sessions = new List<SnapshotSession>();
            foreach (var dirPath in snapshotsDirectories)
            {
                if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath) && Path.GetFileName(dirPath).StartsWith("jahro-snapshot"))
                {
                    string infoFilePath = Path.Combine(dirPath, SnapshotFileName);
                    if (File.Exists(infoFilePath) == false)
                    {
                        Directory.Delete(dirPath, true);
                    }
                    else
                    {
                        string result = "";
                        FileStream stream = File.Open(infoFilePath, FileMode.Open);
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            result = sr.ReadToEnd();
                        }
                        var snapshopInfo = JsonUtility.FromJson<SnapshotSession>(result);
                        sessions.Add(snapshopInfo);
                    }
                }
            }
            return sessions;
        }

        internal static string[] GetSnapshotScreenshotsPath(SnapshotSession session)
        {
            string folderPath = session.folderPath;
            string[] screenshotFiles = Directory.GetFiles(folderPath, "jahro-screenshot-*.jpg");
            return screenshotFiles;
        }

        internal static string GetPathToScreenShot(SnapshotSession session)
        {
            string folderPath = session.folderPath;
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string time = DateTime.Now.ToString("HH-mm-ss-fff");
            string filename = $"jahro-screenshot-{date}-at-{time}.jpg";
            return Path.Combine(folderPath, filename);
        }

        internal static string GetSnapshotLogFolder(DateTime dateTime)
        {
            string folderPath = GetMainDirectory();
            string date = dateTime.ToString("yyyy-MM-dd");
            string time = dateTime.ToString("HH-mm-ss-fff");
            string folderName = $"jahro-snapshot-{date}-at-{time}";
            string snapshotsFolderPath = folderPath + Path.DirectorySeparatorChar + folderName;

            if (!Directory.Exists(snapshotsFolderPath))
            {
                Directory.CreateDirectory(snapshotsFolderPath);
            }
            return snapshotsFolderPath;
        }

        internal static string GetSnapshotLogFilePath(SnapshotSession session)
        {
            string folderPath = session.folderPath;
            string filename = "jahro-snapshot-log";
            return Path.Combine(folderPath, filename);
        }

        internal static void RemoveSnapshotFolder(SnapshotSession session)
        {
            string folderPath = session.folderPath;
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
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