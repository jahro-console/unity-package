using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal static class ScreenshotFileUtility
    {
        internal static ScreenshotMeta[] GetScreenshotMetas(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.screenshotsFolderPath))
                return new ScreenshotMeta[0];

            try
            {
                var metaFiles = SharedFileUtility.GetFilesWithPattern(session.screenshotsFolderPath, "jahro-screenshot-*.jpg.meta");
                var metas = new List<ScreenshotMeta>();

                foreach (var metaFile in metaFiles)
                {
                    var meta = SharedFileUtility.LoadJsonFromFile<ScreenshotMeta>(metaFile);
                    if (meta != null)
                    {
                        metas.Add(meta);
                    }
                }
                return metas.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get screenshot metas: {ex.Message}");
                return new ScreenshotMeta[0];
            }
        }

        internal static void SaveScreenshotMeta(ScreenshotMeta screenshotMeta)
        {
            if (screenshotMeta == null || string.IsNullOrEmpty(screenshotMeta.metaFilePath)) return;
            SharedFileUtility.SaveJsonToFile(screenshotMeta, screenshotMeta.metaFilePath);
        }

        internal static string[] GetSnapshotScreenshotFilesPaths(SnapshotSession session)
        {
            if (session == null || string.IsNullOrEmpty(session.screenshotsFolderPath))
                return new string[0];

            return SharedFileUtility.GetFilesWithPattern(session.screenshotsFolderPath, "jahro-screenshot-*.jpg");
        }

    }
}
