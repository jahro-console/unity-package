using System;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    [Serializable]
    public struct LogRecord
    {
        [SerializeField] public string timestamp;
        [SerializeField] public string type;
        [SerializeField] public string message;
        [SerializeField] public string stacktrace;
        [SerializeField] public int count;

        public LogRecord(string type, string message, string stacktrace = null, int count = 1)
        {
            this.timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            this.type = type;
            this.message = message ?? string.Empty;
            this.stacktrace = stacktrace ?? string.Empty;
            this.count = count;
        }
    }
}
