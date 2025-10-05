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
        // SnapshotsManager class
        internal enum UploadMode
        {
            Recording,
            Streaming,
        }

        private const int MAX_SNAPSHOTS = 10;

        private JahroContext _context;

        private ChunkManager _chunkManager;

        private UploadManager _uploadManager;

        private StreamManager _streamManager;

        private SnapshotSession _activeSession;

        private bool _isRecording = false;

        internal List<SnapshotSession> SnapshotSessions { get; private set; }

        internal UploadMode Mode { get; private set; }

        internal SnapshotSession ActiveSession { get { return _activeSession; } }

        internal Action<SnapshotSession> OnSessionAdded = delegate { };

        internal Action<SnapshotSession> OnSessionRemoved = delegate { };

        private static SnapshotsManager instance;

        internal static SnapshotsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SnapshotsManager();
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        private SnapshotsManager()
        {
            SnapshotSessions = SnapshotInfoFileUtility.ReadSnapshotsInfos();
        }

        internal void Init(string jahroSessionId, UploadMode mode)
        {
            if (_isRecording)
            {
                Debug.LogWarning("Already recording a snapshot");
                return;
            }

            _isRecording = true;
            Mode = mode;

            _activeSession = SnapshotSession.Create(jahroSessionId);
            _activeSession.SetStatus(SnapshotSession.Status.Recording);
            _activeSession.SaveInfo();
            SnapshotSessions.Add(_activeSession);
            OnSessionAdded(_activeSession);

            _chunkManager = new ChunkManager(_activeSession);
            _chunkManager.Start();

            JahroLogger.OnLogEvent += LogEvent;
            NotificationService.Instance.SnapshotingActive(true);
        }

        internal void InitContext(JahroContext context)
        {
            if (!_isRecording) return;
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;

            _uploadManager = new UploadManager(_context);
            _streamManager = new StreamManager(_context);

            if (Mode == UploadMode.Streaming)
            {
                CreateRemoteSnapshot(_context, _activeSession, (session) =>
                {
                    _streamManager.StreamSnapshot(_activeSession, _chunkManager);
                    _activeSession.SetStatus(SnapshotSession.Status.Streaming);
                    _activeSession.SaveInfo();

                    RefreshSnapshots();
                },
                (error) =>
                {
                    if (_activeSession == null) return;
                    _activeSession.SetError(error);
                    _activeSession.SaveInfo();
                });
            }

            ChechExistingSnapshots();
        }

        internal async Task FlushAsync()
        {
            if (!_isRecording) return;

            if (_activeSession == null)
            {
                return;
            }

            JahroLogger.OnLogEvent -= LogEvent;

            ValidateSession(_activeSession);

            if (_chunkManager != null)
            {
                await _chunkManager.FlushAndStopAsync();
                _chunkManager = null;
            }

            if (_streamManager != null)
            {
                await _streamManager?.StopAsync();
                _streamManager?.Dispose();
                _streamManager = null;
            }

            _uploadManager?.StopAsync();
            _uploadManager?.Dispose();
            _uploadManager = null;

            CheckAndRemoveOldest();
            NotificationService.Instance.SnapshotingActive(false);

            _isRecording = false;
            _activeSession = null;
        }

        internal void UploadSnapshot(SnapshotSession session, Action onComplete, Action<string> onError)
        {
            session.SetStatus(SnapshotSession.Status.Uploading);
            session.SaveInfo();

            CreateRemoteSnapshot(_context, session, (remoteSession) =>
            {
                _uploadManager.QueueSnapshotForUpload(remoteSession);
                onComplete?.Invoke();
            },
            (error) =>
            {
                session.SetStatus(SnapshotSession.Status.Recorded);
                session.SetError(error);
                session.SaveInfo();
                onError?.Invoke(error);
            });
        }

        internal async void StopRecordingSnapshot(SnapshotSession session)
        {
            if (_activeSession == null) return;

            if (session == null) throw new Exception("Session is not set");
            if (session.sessionId != _activeSession.sessionId) throw new Exception("Session is not the active session");

            if (_chunkManager == null) return;

            await _chunkManager.FlushAndStopAsync();
            _chunkManager = null;

            _activeSession = null;

            ValidateSession(session);
        }

        internal async void StopStreamingSnapshot(SnapshotSession session)
        {
            await StopStreamingSnapshotAsync(session);
        }

        internal async Task StopStreamingSnapshotAsync(SnapshotSession session)
        {
            if (_activeSession == null) return;

            if (session.GetStatus() != SnapshotSession.Status.Streaming)
                throw new Exception("Session is not streaming");

            _activeSession = null;

            try
            {
                // Step 1: Stop accepting new logs and flush all pending logs to chunks
                if (_chunkManager != null)
                {
                    await _chunkManager.FlushAndStopAsync();
                    _chunkManager = null;
                }

                // Step 2: Wait for all pending uploads to complete
                if (_streamManager != null)
                {
                    await _streamManager.FlushAndStopAsync();
                    _streamManager = null;
                }

                // Step 3: Stop recording and validate session
                ValidateSession(session);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping streaming snapshot: {ex.Message}");
                throw;
            }
        }

        internal IEnumerator CaptureScreenshot()
        {
            if (_activeSession == null)
            {
                NotificationService.Instance.SendNotification(new Notification("Session stopped, can't capture screenshot"));
                yield break;
            }

            ScreenshotMeta screenshotMeta = new ScreenshotMeta();
            yield return ScreenShooter.Capture(_activeSession, screenshotMeta);
            _activeSession.screenshotsCount++;
            _activeSession.FlushScreenshotCount();

            NotificationService.Instance.SendNotification(new Notification("Screenshot capruted"));

            if (Mode == UploadMode.Streaming)
            {
                _streamManager.StreamScreenshot(screenshotMeta);
            }
        }

        internal void RenameSnapshot(SnapshotSession session, string title)
        {
            RenameSnapshot(session, title, null, null);
        }

        internal async void RenameSnapshot(SnapshotSession session, string title, Action onSuccess, Action<string> onError)
        {
            session.name = title;
            session.SaveInfo();

            if (!session.IsRemote())
            {
                onSuccess?.Invoke();
                return;
            }

            var updateRequest = new SnapshotUpdateRequest(_context, session);
            updateRequest.OnComplete = (response) =>
            {
                onSuccess?.Invoke();
            };
            updateRequest.OnFail = (error) =>
            {
                Debug.LogError(error.ToLogString());
                onError?.Invoke(error.message);
            };
            await NetworkManager.Instance.SendRequestAsync(updateRequest);
        }

        internal async void RefreshSnapshots(Action<string> onError = null)
        {
            foreach (var session in SnapshotSessions)
            {
                if (!session.IsRemote()) continue;
                try
                {
                    var fetchRequest = new SnapshotFetchRequest(_context, session);
                    fetchRequest.OnComplete = (response) =>
                    {
                        session.SetRemoteInfo(response);
                        session.SaveInfo();
                    };
                    fetchRequest.OnFail = (error) =>
                    {
                        onError?.Invoke(error.message);
                    };
                    await NetworkManager.Instance.SendRequestAsync(fetchRequest);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    onError?.Invoke(ex.Message);
                }
            }
        }

        private async void CreateRemoteSnapshot(JahroContext context, SnapshotSession session, Action<SnapshotSession> onComplete, Action<string> onError = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var createRequest = new SnapshotCreateRequest(_context, session);
            createRequest.OnComplete = (response) =>
            {
                session.SetRemoteInfo(response);
                session.SaveInfo();
                onComplete?.Invoke(session);
            };
            createRequest.OnFail = (error) =>
            {
                Debug.LogError(error.ToLogString());
                onError?.Invoke(error.message);
            };

            await NetworkManager.Instance.SendRequestAsync(createRequest);
        }

        private void ChechExistingSnapshots()
        {
            foreach (var snapshot in SnapshotSessions)
            {
                snapshot.SetError(null);
                if (snapshot.GetStatus() == SnapshotSession.Status.Recorded)
                {
                    continue;
                }

                if (snapshot.GetStatus() == SnapshotSession.Status.Streaming)
                {
                    snapshot.SetStatus(SnapshotSession.Status.Streamed);
                    snapshot.SaveInfo();
                    continue;
                }

                if (snapshot.GetStatus() == SnapshotSession.Status.Uploading)
                {
                    snapshot.SetStatus(SnapshotSession.Status.Recorded);
                    snapshot.SaveInfo();
                    continue;
                }

                int chunksToUpload = snapshot.chunksCreated - snapshot.chunksUploaded;

                if (snapshot.GetStatus() == SnapshotSession.Status.Streamed && chunksToUpload > 0)
                {
                    //TODO: recheck if chunks are uploaded
                }

            }
        }

        private void ValidateSession(SnapshotSession session)
        {
            if (session == null) return;
            bool isStreamed = session.GetStatus() == SnapshotSession.Status.Streaming && session.IsRemote();
            bool isRecorded = session.GetStatus() == SnapshotSession.Status.Recording && !session.isEmpty();
            bool isEmpty = session.isEmpty();

            if (isStreamed)
            {
                session.SetStatus(SnapshotSession.Status.Streamed);
            }
            else if (isRecorded)
            {
                session.SetStatus(SnapshotSession.Status.Recorded);
            }
            else if (isEmpty)
            {
                SnapshotInfoFileUtility.RemoveSnapshotFolder(session);
                SnapshotSessions.Remove(session);
                OnSessionRemoved(session);
                return;
            }

            session.SaveInfo();
        }

        private void CheckAndRemoveOldest()
        {
            if (SnapshotSessions.Count <= MAX_SNAPSHOTS)
            {
                return;
            }

            var sessionsToRemove = SnapshotSessions
                .OrderBy(s => s.dateTimeTicks)
                .Take(SnapshotSessions.Count - MAX_SNAPSHOTS)
                .ToList();

            foreach (var session in sessionsToRemove)
            {
                SnapshotInfoFileUtility.RemoveSnapshotFolder(session);
                SnapshotSessions.Remove(session);
                OnSessionRemoved(session);
            }
        }

        private void LogEvent(string message, string context, EJahroLogType logType)
        {
            if (!_isRecording || _activeSession == null || _chunkManager == null) return;

            try
            {
                // Convert log type to string
                var logTypeStr = JahroLogGroup.GroupToCommonString(logType);

                // Enqueue to chunk manager
                _chunkManager.EnqueueLog(message, logTypeStr, context);

                // Update session counts
                var logGroup = JahroLogGroup.MatchGroup(logType);
                switch (logGroup)
                {
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
            catch (Exception ex)
            {
                Debug.LogError($"Error processing log event: {ex.Message}");
            }
        }
    }
}