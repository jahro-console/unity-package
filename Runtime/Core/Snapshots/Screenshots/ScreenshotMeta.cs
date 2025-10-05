using System;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    [Serializable]
    internal class ScreenshotMeta
    {
        [Serializable]
        public enum ScreenshotState
        {
            Saved,
            Uploaded,
            UploadFailed
        }

        [SerializeField] internal string id;
        [SerializeField] internal string filepath;
        [SerializeField] internal string metaFilePath;
        [SerializeField] internal string createdAt;
        [SerializeField] internal ScreenshotState status;
        [SerializeField] internal long sizeBytes;
        [SerializeField] internal string lastError;
        [SerializeField] internal string lastUploadAt;
    }
}
