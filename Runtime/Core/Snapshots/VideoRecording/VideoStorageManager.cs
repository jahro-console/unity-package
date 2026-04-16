using System;
using System.IO;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal static class VideoStorageManager
    {
        internal static long GetAvailableDiskSpaceMB()
        {
            try
            {
                string path = JahroConfig.ApplicationPersistentDataPath;
                if (string.IsNullOrEmpty(path)) return long.MaxValue;

#if UNITY_ANDROID && !UNITY_EDITOR
                return GetAndroidDiskSpaceMB();
#else
                string root = Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(root)) root = "/";
                var driveInfo = new DriveInfo(root);
                return driveInfo.AvailableFreeSpace / (1024 * 1024);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JahroVideo] Cannot determine disk space: {ex.Message}");
                return long.MaxValue;
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static long GetAndroidDiskSpaceMB()
        {
            using (var statFs = new AndroidJavaObject("android.os.StatFs",
                JahroConfig.ApplicationPersistentDataPath))
            {
                long blockSize = statFs.Call<long>("getBlockSizeLong");
                long availBlocks = statFs.Call<long>("getAvailableBlocksLong");
                return (blockSize * availBlocks) / (1024 * 1024);
            }
        }
#endif
    }
}
