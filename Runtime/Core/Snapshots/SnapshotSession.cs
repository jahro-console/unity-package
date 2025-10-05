using System;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    [Serializable]
    internal class SnapshotSession
    {
        internal enum Status
        {
            Recording,
            Streaming,
            Uploading,
            Recorded,
            Streamed,
            Uploaded,
        }

        [Serializable]
        public class RemoteSnapshotInfo
        {
            [SerializeField] public string id;
            [SerializeField] public string projectId;
            [SerializeField] public string tenantId;
            [SerializeField] public string slug;
            [SerializeField] public string name;
            [SerializeField] public string createdAt;
            [SerializeField] public string updatedAt;
            [SerializeField] public string createdBy;
            [SerializeField] public string platform;
            [SerializeField] public string deviceName;
            [SerializeField] public string unityVersion;
            [SerializeField] public string version;
            [SerializeField] public int screenshotsCount;
            [SerializeField] public int logsTotalCount;
            [SerializeField] public int logsDebugCount;
            [SerializeField] public int logsWarningCount;
            [SerializeField] public int logsErrorCount;
            [SerializeField] public int logsExceptionCount;
            [SerializeField] public int logsCommandCount;
            [SerializeField] public string frontendUrl;
        }

        internal const int SNAPSHOT_VERSION = 2;

        [SerializeField] internal string sessionId;
        [SerializeField] internal long dateTimeTicks;
        [SerializeField] internal int snapshotVersion;
        [SerializeField] internal string folderPath;
        [SerializeField] internal string logFilePath;
        [SerializeField] internal string chunksFolderPath;
        [SerializeField] internal string chunksMetaFolderPath;
        [SerializeField] internal string bulkChunksFilePath;
        [SerializeField] internal string screenshotsFolderPath;
        [SerializeField] internal int logsCountDebug;
        [SerializeField] internal int logsCountWarning;
        [SerializeField] internal int logsCountErrors;
        [SerializeField] internal int logsCountCommands;
        [SerializeField] internal int screenshotsCount;
        [SerializeField] private Status status = Status.Recording;
        [SerializeField] internal string timezone;
        [SerializeField] private string lastError;

        // Remote snapshot fields
        [SerializeField] internal string snapshotId;
        [SerializeField] internal string name;
        [SerializeField] internal string projectId;
        [SerializeField] internal string teamId;
        [SerializeField] internal string createdBy;
        [SerializeField] internal string slug;
        [SerializeField] internal string platform;
        [SerializeField] internal string deviceName;
        [SerializeField] internal string unityVersion;
        [SerializeField] internal string version;
        [SerializeField] internal string frontendUrl;

        //System
        [SerializeField] internal int chunksCreated;
        [SerializeField] internal int chunksUploaded;


        internal Action<Status> OnStatusUpdate = delegate { };

        internal Action<int> OnScreenshotsCountUpdate = delegate { };

        internal Action<int, int, int, int> OnLogsCountUpdate = delegate { };

        internal DateTime GetRecordDate()
        {
            return new DateTime(dateTimeTicks);
        }

        internal Status GetStatus()
        {
            return status;
        }

        internal void SetStatus(Status newStatus)
        {
            status = newStatus;
            OnStatusUpdate(status);
        }

        internal int GetScreenshotCount()
        {
            return screenshotsCount;
        }

        internal string[] GetScreenshotFilesPaths()
        {
            return ScreenshotFileUtility.GetSnapshotScreenshotFilesPaths(this);
        }

        internal void GetLogsCount(out int debug, out int warning, out int error, out int command)
        {
            debug = logsCountDebug;
            warning = logsCountWarning;
            error = logsCountErrors;
            command = logsCountCommands;
        }

        internal string GetErrorMessage()
        {
            return lastError;
        }

        internal void SetError(string errorMessage)
        {
            lastError = errorMessage;
            OnStatusUpdate(status);
        }

        internal void SetRemoteInfo(RemoteSnapshotInfo remoteSnapshotInfo)
        {
            this.snapshotId = string.IsNullOrEmpty(remoteSnapshotInfo.id) ? this.snapshotId : remoteSnapshotInfo.id;
            this.name = string.IsNullOrEmpty(remoteSnapshotInfo.name) ? this.name : remoteSnapshotInfo.name;
            this.frontendUrl = string.IsNullOrEmpty(remoteSnapshotInfo.frontendUrl) ? this.frontendUrl : remoteSnapshotInfo.frontendUrl;
            this.createdBy = string.IsNullOrEmpty(remoteSnapshotInfo.createdBy) ? this.createdBy : remoteSnapshotInfo.createdBy;
            this.slug = string.IsNullOrEmpty(remoteSnapshotInfo.slug) ? this.slug : remoteSnapshotInfo.slug;
            this.projectId = string.IsNullOrEmpty(remoteSnapshotInfo.projectId) ? this.projectId : remoteSnapshotInfo.projectId;
            this.teamId = string.IsNullOrEmpty(remoteSnapshotInfo.tenantId) ? this.teamId : remoteSnapshotInfo.tenantId;
            this.platform = string.IsNullOrEmpty(remoteSnapshotInfo.platform) ? this.platform : remoteSnapshotInfo.platform;
            this.deviceName = string.IsNullOrEmpty(remoteSnapshotInfo.deviceName) ? this.deviceName : remoteSnapshotInfo.deviceName;
            this.unityVersion = string.IsNullOrEmpty(remoteSnapshotInfo.unityVersion) ? this.unityVersion : remoteSnapshotInfo.unityVersion;
            this.version = string.IsNullOrEmpty(remoteSnapshotInfo.version) ? this.version : remoteSnapshotInfo.version;
            OnStatusUpdate(status);
        }

        internal bool isEmpty()
        {
            return logsCountDebug == 0 && logsCountWarning == 0 && logsCountErrors == 0 && logsCountCommands == 0 && screenshotsCount == 0;
        }

        internal bool IsRemote()
        {
            return !string.IsNullOrEmpty(snapshotId);
        }

        internal void FlushLogsCount()
        {
            OnLogsCountUpdate(logsCountDebug, logsCountWarning, logsCountErrors, logsCountCommands);
        }

        internal void FlushScreenshotCount()
        {
            OnScreenshotsCountUpdate(screenshotsCount);
        }

        internal void SaveInfo()
        {
            SnapshotInfoFileUtility.SaveSnapshotInfo(this);
        }

        internal static SnapshotSession Create(string sessionId)
        {
            DateTime now = DateTime.Now;
            var session = new SnapshotSession();
            session.sessionId = sessionId;
            session.dateTimeTicks = now.Ticks;
            session.name = now.ToString("HH:mm:ss MMM dd");
            session.timezone = TimeZoneInfo.Local.StandardName;
            session.snapshotVersion = SNAPSHOT_VERSION;
            SnapshotInfoFileUtility.CreateSnapshotFolder(
                new DateTime(session.dateTimeTicks),
                out session.folderPath,
                out session.chunksFolderPath,
                out session.chunksMetaFolderPath,
                out session.screenshotsFolderPath);
            session.logFilePath = SnapshotInfoFileUtility.GetSnapshotLogFilePath(session);
            return session;
        }
    }
}