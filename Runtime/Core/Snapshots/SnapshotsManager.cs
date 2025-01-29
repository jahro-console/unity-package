using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Network;
using JahroConsole.Core.Notifications;
using JahroConsole.Core.Utilities;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class SnapshotsManager
    {
        private const int MAX_SNAPSHOTS = 10;

        private JahroContext _context;

        private SnapshotLogWriter _logWriter;

        private SnapshotSession _activeSession;

        internal List<SnapshotSession> SnapshotSessions { get; private set; }

        internal SnapshotSession ActiveSession { get { return _activeSession; } }

        internal Action<SnapshotSession> OnSessionAdded = delegate { };

        internal Action<SnapshotSession> OnSessionRemoved = delegate { };

        private static SnapshotsManager instance;

        internal static SnapshotsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SnapshotsManager();
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        private SnapshotsManager()
        {
            SnapshotSessions = FileManager.ReadSnapshotsInfos();
        }

        internal void InitContext(JahroContext context)
        {
            _context = context;
        }

        internal void StartRecording()
        {
            _activeSession = SnapshotSession.CreateSession();
            SnapshotSessions.Add(_activeSession);
            OnSessionAdded(_activeSession);

            _logWriter = new SnapshotLogWriter(_activeSession);
            _logWriter.StartLogging();
            JahroLogger.OnLogEvent += LogEvent;
            NotificationService.Instance.SnapshotingActive(true);
        }

        internal void StopRecording()
        {
            if (_activeSession == null)
            {
                return;
            }

            if (_context.SelectedUserInfo != null) _activeSession.SetUser(_context.SelectedUserInfo);
            Task.Run(async () => await _logWriter.StopLoggingAsync());
            _activeSession.SaveRecording();
            CheckAndRemoveOldest();
            NotificationService.Instance.SnapshotingActive(false);
            _activeSession = null;
        }

        internal IEnumerator CaptureScreenshot()
        {
            if (_activeSession != null)
            {
                _activeSession.screenshotsCount++;
                yield return ScreenShooter.Capture(_activeSession);
                _activeSession.FlushScreenshotCount();
            }
        }

        internal IEnumerator UploadSnapshotCoroutine(SnapshotSession snapshotSession)
        {
            bool uploadSuccess = true;
            string errorMessage = "";

            snapshotSession.StartUploading();

            var snapshotUploadRequest = new SnapshotUploadRequest(snapshotSession,
                    ConsoleStorageController.Instance.ConsoleStorage.ProjectSettings.APIKey,
                    _context.TeamInfo.Id,
                    _context.ProjectInfo.Id,
                    _context.SelectedUserInfo.Id);
            snapshotUploadRequest.OnUploadProgress = (progress) =>
            {
                snapshotSession.UpdateUploadProgress(progress);
            };
            snapshotUploadRequest.OnComplete = (response) =>
            {
                snapshotSession.UpdateUploadProgress(1f);
                uploadSuccess = true;
            };
            snapshotUploadRequest.OnFail = (error, responseCode) =>
            {
                uploadSuccess = false;
                errorMessage = error;
            };
            yield return NetworkManager.Instance.SendRequestCoroutine(snapshotUploadRequest);

            if (uploadSuccess)
            {
                yield return new WaitForSeconds(1f);
                snapshotSession.OnUploadSuccess();
            }
            else
            {
                snapshotSession.OnUploadError(errorMessage);
            }
        }

        internal void DeleteSnapshot(SnapshotSession snapshotSession)
        {
            FileManager.RemoveSnapshotFolder(snapshotSession);
            SnapshotSessions.Remove(snapshotSession);
            OnSessionRemoved(snapshotSession);
        }

        private void CheckAndRemoveOldest()
        {
            if (SnapshotSessions.Count < MAX_SNAPSHOTS)
            {
                return;
            }

            var oldest = SnapshotSessions.Select(s => s).OrderBy(s => s.dateTimeTicks).First();
            FileManager.RemoveSnapshotFolder(oldest);
            SnapshotSessions.Remove(oldest);
            OnSessionRemoved(oldest);
        }

        private void LogEvent(string message, string context, EJahroLogType logType)
        {
            if (_activeSession == null)
            {
                return;
            }

            _logWriter.Log(message, logType.ToString(), context);
            var logGroup = JahroLogGroup.MatchGroup(logType);
            switch (logGroup)
            {
                case JahroLogGroup.EJahroLogGroup.Internal:
                    break;
                case JahroLogGroup.EJahroLogGroup.Debug:
                    _activeSession.logsCountDebug++;
                    break;
                case JahroLogGroup.EJahroLogGroup.Warning:
                    _activeSession.logsCountWarning++;
                    break;
                case JahroLogGroup.EJahroLogGroup.Error:
                    _activeSession.logsCountErrors++;
                    break;
                case JahroLogGroup.EJahroLogGroup.Command:
                    _activeSession.logsCountCommands++;
                    break;
            }
            _activeSession.FlushLogsCount();
        }
    }
}