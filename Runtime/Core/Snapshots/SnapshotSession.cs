using System;
using Jahro.Core.Context;
using Jahro.Core.Data;
using UnityEngine;

namespace Jahro.Core.Snapshots
{
    [Serializable]
    internal class SnapshotSession
    {
        internal enum Status
        {
            Uploaded,
            Recording,
            Saved,
            Uploading,
            Error,
        }

        [SerializeField]
        internal long dateTimeTicks;

        [SerializeField]
        internal string folderPath;

        [SerializeField]
        internal string logFilePath;

        [SerializeField]
        internal int logsCountDebug;

        [SerializeField]
        internal int logsCountWarning;

        [SerializeField]
        internal int logsCountErrors;

        [SerializeField]
        internal int logsCountCommands;

        [SerializeField]
        internal int screenshotsCount;

        [SerializeField]
        private Status status = Status.Recording;

        [SerializeField]
        private string userKey;

        private string errorMessage;

        private float uploadProgress;

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

        internal int GetScreenshotCount()
        {
            return screenshotsCount;
        }

        internal string[] GetScreenshotsPaths()
        {
            return FileManager.GetSnapshotScreenshotsPath(this);
        }

        internal string GetErrorMessage()
        {
            return errorMessage;
        }

        internal float GetUploadProgress()
        {
            return uploadProgress;
        }

        internal void GetLogsCount(out int debug, out int warning, out int error, out int command)
        {
            debug = logsCountDebug;
            warning = logsCountWarning;
            error = logsCountErrors;
            command = logsCountCommands;
        }

        internal void StartUploading()
        {
            status = Status.Uploading;
            OnStatusUpdate(status);
        }

        internal void UpdateUploadProgress(float progress)
        {
            uploadProgress = progress;
            OnStatusUpdate(status);
        }

        internal void OnUploadSuccess()
        {
            status = Status.Uploaded;
            OnStatusUpdate(status);
            SaveInfo();
        }

        internal void OnUploadError(string errorMessage)
        {
            this.errorMessage = errorMessage;
            status = Status.Error;
            OnStatusUpdate(status);
        }

        internal void FlushLogsCount()
        {
            OnLogsCountUpdate(logsCountDebug, logsCountWarning, logsCountErrors, logsCountCommands);
        }

        internal void FlushScreenshotCount()
        {
            OnScreenshotsCountUpdate(screenshotsCount);
        }

        internal void SaveRecording()
        {
            status = Status.Saved;
            OnStatusUpdate(status);
            SaveInfo();
        }

        internal void SaveInfo()
        {
            FileManager.SaveSnapshotInfo(this);
        }

        internal void SetUser(UserInfo userAccountInfo)
        {
            userKey = userAccountInfo.Id;
        }

        internal static SnapshotSession CreateSession()
        {
            var session = new SnapshotSession();
            session.dateTimeTicks = DateTime.Now.Ticks;
            session.folderPath = FileManager.GetSnapshotLogFolder(new DateTime(session.dateTimeTicks));
            session.logFilePath = FileManager.GetSnapshotLogFilePath(session);
            return session;
        }
    }
}