using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Network;
using JahroConsole.Core.Notifications;
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

        internal enum ReplayRecordingMode
        {
            Manual,
            Autostart,
            Shadow,
        }

        private const int MAX_SNAPSHOTS = 10;

        private JahroContext _context;

        private ChunkManager _chunkManager;

        private UploadManager _uploadManager;

        private StreamManager _streamManager;

        private SnapshotSession _activeSession;

        private bool _isRecording = false;

        private JahroVideoRecorder _videoRecorder;

        private JahroSnapshotVideoCoroutineHost _videoCoroutineHost;

        internal JahroVideoRecorder VideoRecorder { get { return _videoRecorder; } }

        internal List<SnapshotSession> SnapshotSessions { get; private set; }

        internal UploadMode Mode { get; private set; }

        internal ReplayRecordingMode ReplayMode { get; set; }

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
                JahroLogger.LogWarning($"[{JahroLogTag.Snapshots}] StartSnapshot called while already recording — existing sessionId={_activeSession?.sessionId}. Call StopSnapshot first.");
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

            ConsoleStorageController.Instance.OnStorageLoad += OnStorageLoad;

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
                    _streamManager.OnLogLimitExceeded += HandleStreamLogLimitExceeded;
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

            SetupVideoRecorder();
            if (ReplayMode == ReplayRecordingMode.Autostart)
            {
                StartVideoRecording();
            }
            else if (ReplayMode == ReplayRecordingMode.Shadow)
            {
                StartShadowRecording();
            }
        }

        internal void PauseSession(bool pause)
        {
            if (_videoRecorder != null)
            {
                _videoRecorder.OnApplicationPause(pause);
            }
        }

        private void OnStorageLoad(ConsoleStorage storage)
        {
            ReplayMode = (ReplayRecordingMode)storage.GeneralSettings.replayRecordingMode;
        }

        internal async Task FlushAsync(string logLimitErrorMessage = null)
        {
            if (!_isRecording) return;

            if (_activeSession == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(logLimitErrorMessage))
            {
                _activeSession.SetError(logLimitErrorMessage);
                _activeSession.SaveInfo();
            }

            if (_videoRecorder != null)
            {
                if (_videoRecorder.CurrentState == JahroVideoState.Recording ||
                    _videoRecorder.CurrentState == JahroVideoState.Shadow)
                {
                    _videoRecorder.StopRecording();
                }
                _activeSession.videoPartsCount = _videoRecorder.VideoPartIndex;
                _videoRecorder.OnStateChanged -= OnVideoRecorderStateChanged;
                _videoRecorder.Dispose();
                _videoRecorder = null;
            }

            DestroyVideoCoroutineHost();

            JahroLogger.OnLogEvent -= LogEvent;

            ValidateSession(_activeSession);

            if (_chunkManager != null)
            {
                if (!string.IsNullOrEmpty(logLimitErrorMessage))
                    _chunkManager.Abort();
                else
                    await _chunkManager.FlushAndStopAsync();
                _chunkManager = null;
            }

            if (_streamManager != null)
            {
                _streamManager.OnLogLimitExceeded -= HandleStreamLogLimitExceeded;
                await _streamManager.StopAsync();
                _streamManager.Dispose();
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

        private void HandleStreamLogLimitExceeded(string errorMessage)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await FlushAsync(errorMessage);
                }
                catch (Exception ex)
                {
                    JahroLogger.LogError($"[{JahroLogTag.Snapshots}] Log flush failed after limit — error={ex.Message}");
                }
            });
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

            if (_videoRecorder != null &&
                (_videoRecorder.CurrentState == JahroVideoState.Recording ||
                 _videoRecorder.CurrentState == JahroVideoState.Shadow))
            {
                _videoRecorder.StopRecording();
                _activeSession.videoPartsCount = _videoRecorder.VideoPartIndex;
            }

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
                int totalFrames = 0;
                if (_videoRecorder != null &&
                    (_videoRecorder.CurrentState == JahroVideoState.Recording ||
                     _videoRecorder.CurrentState == JahroVideoState.Shadow))
                {
                    totalFrames = _videoRecorder.FramesCaptured;
                    _videoRecorder.StopRecording();
                    session.videoPartsCount = _videoRecorder.VideoPartIndex;
                }

                if (_chunkManager != null)
                {
                    await _chunkManager.FlushAndStopAsync();
                    _chunkManager = null;
                }

                if (_streamManager != null)
                {
                    if (!string.IsNullOrEmpty(_streamManager.VideoFramesPath) && totalFrames > 0)
                    {
                        _streamManager.EnqueueFinalVideoUpload(totalFrames);
                    }

                    await _streamManager.FlushAndStopAsync();
                    _streamManager.OnLogLimitExceeded -= HandleStreamLogLimitExceeded;
                    _streamManager.Dispose();
                    _streamManager = null;
                }

                ValidateSession(session);
            }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Snapshots}] Stop streaming snapshot failed — error={ex.Message}");
                throw;
            }
        }

        internal IEnumerator CaptureScreenshot()
        {
            if (_activeSession == null)
            {
                NotificationService.Instance.SendNotification(new Notification("Session stopped. Can't capture screenshot"));
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
                JahroLogger.LogError($"[{JahroLogTag.Snapshots}] Snapshot API request failed — {error.ToLogString()}");
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
                    JahroLogger.LogError($"[{JahroLogTag.Snapshots}] Unexpected error — error={ex.GetType().Name}: {ex.Message}");
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
                JahroLogger.LogError($"[{JahroLogTag.Snapshots}] Snapshot API request failed — {error.ToLogString()}");
                onError?.Invoke(error.message);
            };

            await NetworkManager.Instance.SendRequestAsync(createRequest);
        }

        private void ChechExistingSnapshots()
        {
            foreach (var snapshot in SnapshotSessions)
            {
                if (_activeSession.sessionId == snapshot.sessionId) continue;

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

                if (snapshot.GetStatus() == SnapshotSession.Status.Recording)
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
                var logTypeStr = JahroLogGroup.GroupToCommonString(logType);
                _chunkManager.EnqueueLog(message, logTypeStr, context);

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
                JahroLogger.LogError($"[{JahroLogTag.Snapshots}] Log event processing failed — error={ex.Message}");
            }
        }

        private void SetupVideoRecorder()
        {
            if (_videoRecorder != null)
            {
                _videoRecorder.OnStateChanged -= OnVideoRecorderStateChanged;
                _videoRecorder.Dispose();
            }

            _videoRecorder = new JahroVideoRecorder(_context.VideoConfig);
            _videoRecorder.OnStateChanged += OnVideoRecorderStateChanged;

            string videoBasePath = Path.Combine(_activeSession.folderPath, "video");
            _videoRecorder.SetVideoBasePath(videoBasePath);

            EnsureVideoCoroutineHost();
        }

        private void OnVideoRecorderStateChanged(JahroVideoState state)
        {
            if (state == JahroVideoState.Idle && _activeSession != null && _videoRecorder != null)
            {
                _activeSession.videoPartsCount = _videoRecorder.VideoPartIndex;
                _activeSession.SaveInfo();
            }
            Jahro.FireVideoStateChanged(state);
        }

        private void EnsureVideoCoroutineHost()
        {
            if (_videoCoroutineHost != null)
                return;

            var viewManager = JahroViewManager.GetInstance();
            if (viewManager == null)
            {
                JahroLogger.LogWarning($"[{JahroLogTag.Snapshots}] Cannot create coroutine host — Jahro console root not found in scene. Ensure Jahro is initialized before starting video.");
                return;
            }

            var go = new GameObject("JahroSnapshotVideoCoroutineHost");
            go.transform.SetParent(viewManager.transform, false);
            _videoCoroutineHost = go.AddComponent<JahroSnapshotVideoCoroutineHost>();
        }

        private void DestroyVideoCoroutineHost()
        {
            if (_videoCoroutineHost == null)
                return;

            UnityEngine.Object.Destroy(_videoCoroutineHost.gameObject);
            _videoCoroutineHost = null;
        }

        internal void StartVideoRecording()
        {
            EnsureVideoCoroutineHost();
            if (_videoCoroutineHost == null)
                return;

            _videoCoroutineHost.StartCoroutine(VideoRecordingRoutine());
        }

        private IEnumerator VideoRecordingRoutine()
        {
            if (_activeSession == null)
            {
                JahroLogger.LogWarning($"[{JahroLogTag.Snapshots}] StartVideo called with no active snapshot session. Start a snapshot session first.");
                yield break;
            }
            if (_videoRecorder == null) yield break;

            _videoRecorder.StartRecording();
            if (_videoRecorder.CurrentState != JahroVideoState.Recording)
                yield break;

            _activeSession.videoPartsCount++;
            _activeSession.FlushVideoFragmentsCount();

            if (Mode == UploadMode.Streaming && _streamManager != null)
            {
                _streamManager.SetVideoSource(_videoRecorder.CurrentPartPath,
                    _videoRecorder.VideoPartIndex, () => _videoRecorder.FramesCaptured);
            }

            yield return _videoRecorder.CaptureLoop();
        }

        internal void StopVideoRecording()
        {
            if (_videoRecorder == null || _videoRecorder.CurrentState != JahroVideoState.Recording)
                return;

            int totalFrames = _videoRecorder.FramesCaptured;
            _videoRecorder.StopRecording();

            if (_activeSession != null)
                _activeSession.videoPartsCount = _videoRecorder.VideoPartIndex;

            if (Mode == UploadMode.Streaming && _streamManager != null)
            {
                _streamManager.EnqueueFinalVideoUpload(totalFrames);
            }
        }

        internal void StartShadowRecording()
        {
            EnsureVideoCoroutineHost();
            if (_videoCoroutineHost == null) return;
            _videoCoroutineHost.StartCoroutine(ShadowRecordingRoutine());
        }

        private IEnumerator ShadowRecordingRoutine()
        {
            if (_activeSession == null || _videoRecorder == null) yield break;

            string shadowPath = Path.Combine(_activeSession.folderPath, "video", "shadow");
            float windowSec = _context.VideoConfig.shadowWindowSec;

            _videoRecorder.StartShadowRecording(shadowPath, windowSec);
            if (_videoRecorder.CurrentState != JahroVideoState.Shadow) yield break;

            yield return _videoRecorder.CaptureLoop();
        }

        internal void CommitShadowRecording()
        {
            if (_videoRecorder == null) return;
            if (_videoRecorder.CurrentState != JahroVideoState.Shadow) return;

            string partPath = _videoRecorder.CommitShadow();

            if (partPath != null && _activeSession != null)
            {
                _activeSession.videoPartsCount = _videoRecorder.VideoPartIndex;
                _activeSession.FlushVideoFragmentsCount();
                _activeSession.SaveInfo();
                Jahro.FireVideoStateChanged(JahroVideoState.Shadow);
            }
        }

    }
}