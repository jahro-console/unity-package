using System;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    [Serializable]
    public class ChunkMeta
    {
        [Serializable]
        public enum ChunkStatus
        {
            Queued,
            Uploading,
            Uploaded,
            Failed
        }

        [SerializeField] public string chunkId;
        [SerializeField] public int sequence;
        [SerializeField] public string chunkFilename;
        [SerializeField] public string chunkFilePath;
        [SerializeField] public string metaFilePath;
        [SerializeField] public string createdAt;
        [SerializeField] public string sessionId;
        [SerializeField] public int logsCount;
        [SerializeField] public ChunkStatus status;
        [SerializeField] public int attempts;
        [SerializeField] public string lastAttemptAt;
        [SerializeField] public string lastError;
        [SerializeField] public long sizeBytes;
    }
}
