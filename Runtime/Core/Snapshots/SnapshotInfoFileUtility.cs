using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Context;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal static class SnapshotInfoFileUtility
    {
        internal static void CreateSnapshotFolder(DateTime dateTime, out string folderPath, out string chunksFolderPath, out string chunksMetaFolderPath, out string screenshotsFolderPath)
        {
            folderPath = null;
            chunksFolderPath = null;
            chunksMetaFolderPath = null;
            screenshotsFolderPath = null;

            try
            {
                string jahroMainFolderPath = JahroConfig.ApplicationMainDirectory;
                string date = dateTime.ToString("yyyy-MM-dd");
                string time = dateTime.ToString("HH-mm-ss-fff");
                string folderName = $"jahro-snapshot-{date}-at-{time}";

                folderPath = Path.Combine(jahroMainFolderPath, folderName);
                SharedFileUtility.EnsureDirectoryExists(folderPath);

                chunksFolderPath = Path.Combine(folderPath, "chunks");
                SharedFileUtility.EnsureDirectoryExists(chunksFolderPath);

                chunksMetaFolderPath = Path.Combine(chunksFolderPath, "meta");
                SharedFileUtility.EnsureDirectoryExists(chunksMetaFolderPath);

                screenshotsFolderPath = Path.Combine(folderPath, "screenshots");
                SharedFileUtility.EnsureDirectoryExists(screenshotsFolderPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create snapshot folder: {ex.Message}");
            }
        }

        internal static void RemoveSnapshotFolder(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
                return;

            SharedFileUtility.RemoveDirectorySafe(session.folderPath);
        }

        internal static void SaveSnapshotInfo(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
                return;

            string filePath = Path.Combine(session.folderPath, JahroConfig.SnapshotFileName);
            SharedFileUtility.SaveJsonToFile(session, filePath);
        }

        internal static List<SnapshotSession> ReadSnapshotsInfos()
        {
            var sessions = new List<SnapshotSession>();

            try
            {
                string folderPath = JahroConfig.ApplicationMainDirectory;
                if (!Directory.Exists(folderPath))
                    return sessions;

                string[] snapshotsDirectories = Directory.GetDirectories(folderPath);

                foreach (var dirPath in snapshotsDirectories)
                {
                    try
                    {
                        string dirName = Path.GetFileName(dirPath);
                        if (!dirName.StartsWith("jahro-snapshot"))
                            continue;

                        string infoFilePath = Path.Combine(dirPath, JahroConfig.SnapshotFileName);
                        if (!File.Exists(infoFilePath))
                        {
                            SharedFileUtility.RemoveDirectorySafe(dirPath);
                            continue;
                        }

                        var snapshotInfo = SharedFileUtility.LoadJsonFromFile<SnapshotSession>(infoFilePath);
                        if (snapshotInfo == null)
                        {
                            SharedFileUtility.RemoveDirectorySafe(dirPath);
                            continue;
                        }

                        if (snapshotInfo.snapshotVersion != SnapshotSession.SNAPSHOT_VERSION)
                        {
                            SharedFileUtility.RemoveDirectorySafe(dirPath);
                            continue;
                        }

                        sessions.Add(snapshotInfo);
                    }
                    catch (Exception dirEx)
                    {
                        Debug.LogWarning($"Error processing snapshot directory: {dirPath}, {dirEx.Message}");
                        SharedFileUtility.RemoveDirectorySafe(dirPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read snapshots: {ex.GetType().Name} - {ex.Message}");
            }

            return sessions;
        }

        internal static string GetSnapshotLogFilePath(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.folderPath))
                return null;

            try
            {
                SharedFileUtility.EnsureDirectoryExists(session.folderPath);
                return Path.Combine(session.folderPath, "jahro-snapshot-log");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get snapshot log file path: {ex.Message}");
                return null;
            }
        }
    }
}
