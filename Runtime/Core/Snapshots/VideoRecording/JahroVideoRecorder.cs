using System;
using System.Collections;
using System.IO;
using UnityEngine;
using JahroConsole.Core.Context;
using JahroConsole.Core.Notifications;

namespace JahroConsole.Core.Snapshots
{
    internal class JahroVideoRecorder : IDisposable
    {
        private string _videoBasePath;
        private int _videoPartIndex;
        private float _cumulativeDurationSec;
        private long _cumulativeVideoBytesWritten;
        private readonly VideoConfig _config;
        private JahroVideoState _state = JahroVideoState.Idle;
        private string _currentPartPath;
        private string _framesPath;
        private DateTime _recordingStartUtc;
        private DateTime _recordingEndUtc;
        private float _finalDurationSec;
        private int _frameCounter;
        private float _nextCaptureTime;
        private long _minDiskSpaceMB;

        private FrameWriterService _writerService;
        private FrameCaptureService _captureService;
        private readonly ShadowRecordingController _shadowController = new ShadowRecordingController();

        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        private const int DISK_CHECK_INTERVAL_FRAMES = 50;

        internal JahroVideoState CurrentState => _state;
        internal string CurrentPartPath => _currentPartPath;
        internal string CurrentVideoPartId => _videoPartIndex > 0 ? _videoPartIndex.ToString("D3") : null;

        internal float RecordingDurationSec
        {
            get
            {
                if (_state == JahroVideoState.Idle || _state == JahroVideoState.Error || _state == JahroVideoState.Committing)
                    return 0f;

                float totalElapsed = (float)(DateTime.UtcNow - _recordingStartUtc).TotalSeconds;
                return Mathf.Max(0f, totalElapsed);
            }
        }

        internal float TotalRecordingDurationSec => _cumulativeDurationSec + RecordingDurationSec;

        internal int FramesCaptured => _frameCounter;
        internal long StorageUsedBytes => _writerService?.TotalBytesWritten ?? 0;
        internal int WriteQueueDepth => _writerService?.QueueDepth ?? 0;
        internal int VideoPartIndex => _videoPartIndex;

        internal event Action<JahroVideoState> OnStateChanged;
        internal event Action<JahroVideoError> OnError;

        internal JahroVideoRecorder(VideoConfig config)
        {
            _config = config;
            _minDiskSpaceMB = _config.maxVideoSizeMB;
        }

        internal void SetVideoBasePath(string videoBasePath)
        {
            if (_state != JahroVideoState.Idle && _state != JahroVideoState.Error)
            {
                Debug.LogWarning("[JahroVideo] Cannot change video path while recording");
                return;
            }
            _videoBasePath = videoBasePath;
            _videoPartIndex = 0;
            _cumulativeDurationSec = 0;
            _cumulativeVideoBytesWritten = 0;
        }

        #region Standard Recording

        internal void StartRecording()
        {
            if (string.IsNullOrEmpty(_videoBasePath))
            {
                Debug.LogWarning("[JahroVideo] Cannot start recording: video base path not set");
                return;
            }

            if (_state != JahroVideoState.Idle && _state != JahroVideoState.Error)
            {
                Debug.LogWarning("[JahroVideo] Cannot start recording: current state is " + _state);
                return;
            }

            JahroConfig.Init();

            if (_cumulativeDurationSec >= _config.maxRecordingsDurationSec)
            {
                Debug.LogWarning("[JahroVideo] Cannot start recording: global duration limit already reached");
                FireError(JahroVideoError.MaxDurationReached);
                return;
            }

            long maxVideoBytes = (long)_config.maxVideoSizeMB * 1024L * 1024L;
            if (maxVideoBytes > 0 && _cumulativeVideoBytesWritten >= maxVideoBytes)
            {
                Debug.LogWarning("[JahroVideo] Cannot start recording: session video size limit already reached");
                FireError(JahroVideoError.StorageLimitReached);
                return;
            }

            if (!CheckDiskSpaceForStart())
                return;

            _videoPartIndex++;
            string videoPartName = $"video-{_videoPartIndex}";

            SharedFileUtility.EnsureDirectoryExists(_videoBasePath);

            _currentPartPath = Path.Combine(_videoBasePath, videoPartName);
            SharedFileUtility.EnsureDirectoryExists(_currentPartPath);

            _framesPath = _currentPartPath;

            if (!InitializeCaptureService())
            {
                SetState(JahroVideoState.Error);
                FireError(JahroVideoError.FrameCaptureFailure);
                return;
            }

            _writerService = new FrameWriterService(_framesPath, OnWriterError);
            _writerService.Start();

            _frameCounter = 0;
            _recordingStartUtc = DateTime.UtcNow;
            _nextCaptureTime = Time.realtimeSinceStartup;

            SetState(JahroVideoState.Recording);
            NotificationService.Instance.SendNotification(new Notification("Replay recording"));
            Debug.Log($"[JahroVideo] Recording started — part: {videoPartName}, resolution: {_captureService.TargetWidth}x{_captureService.TargetHeight}, fps: {_config.frameRate}, captureMode: {(_captureService.UseReadPixelsFallback ? "ReadPixels" : "AsyncGPU")}");
            Debug.Log($"[JahroVideo] Frames directory — path={_framesPath}");
#if UNITY_ANDROID
            Debug.Log($"[JahroVideo] Android frame access — run: adb pull \"{_framesPath}\" .");
#endif
        }

        internal void StopRecording()
        {
            if (_state == JahroVideoState.Shadow)
            {
                StopShadowRecording();
                return;
            }

            if (_state != JahroVideoState.Recording)
                return;

            _recordingEndUtc = DateTime.UtcNow;
            _finalDurationSec = RecordingDurationSec;

            SetState(JahroVideoState.Stopping);
            FinalizeRecording();
            NotificationService.Instance.SendNotification(new Notification("Replay recording stopped"));
        }

        private void FinalizeRecording()
        {
            if (_writerService != null)
            {
                _writerService.StopAndFlush();
                _cumulativeVideoBytesWritten += _writerService.TotalBytesWritten;
                _writerService.Dispose();
                _writerService = null;
            }

            _captureService?.Dispose();
            _captureService = null;

            _cumulativeDurationSec += _finalDurationSec;

            int totalFrames = _frameCounter;
            _currentPartPath = null;
            _framesPath = null;

            SetState(JahroVideoState.Idle);
            Debug.Log($"[JahroVideo] Recording stopped — {totalFrames} frames, cumulative duration: {_cumulativeDurationSec:F1}s");
        }

        #endregion

        #region Shadow Recording

        internal void StartShadowRecording(string shadowPath, float windowSec)
        {
            if (string.IsNullOrEmpty(_videoBasePath))
            {
                Debug.LogWarning("[JahroVideo] Cannot start shadow recording: video base path not set");
                return;
            }

            if (_state != JahroVideoState.Idle && _state != JahroVideoState.Error)
            {
                Debug.LogWarning("[JahroVideo] Cannot start shadow recording: current state is " + _state);
                return;
            }

            JahroConfig.Init();

            if (!CheckDiskSpaceForStart())
                return;

            _shadowController.Activate(shadowPath, windowSec);

            SharedFileUtility.EnsureDirectoryExists(_videoBasePath);
            SharedFileUtility.EnsureDirectoryExists(shadowPath);

            _framesPath = shadowPath;
            _currentPartPath = shadowPath;

            if (!InitializeCaptureService())
            {
                _shadowController.Deactivate();
                SetState(JahroVideoState.Error);
                FireError(JahroVideoError.FrameCaptureFailure);
                return;
            }

            _writerService = new FrameWriterService(shadowPath, OnWriterError);
            _writerService.Start();

            _frameCounter = 0;
            _recordingStartUtc = DateTime.UtcNow;
            _nextCaptureTime = Time.realtimeSinceStartup;

            SetState(JahroVideoState.Shadow);
            NotificationService.Instance.SendNotification(new Notification("Shadow recording active"));
            Debug.Log($"[JahroVideo] Shadow recording started — window: {windowSec}s, resolution: {_captureService.TargetWidth}x{_captureService.TargetHeight}, captureMode: {(_captureService.UseReadPixelsFallback ? "ReadPixels" : "AsyncGPU")}");
            Debug.Log($"[JahroVideo] Shadow frames directory — path={_framesPath}");
#if UNITY_ANDROID
            Debug.Log($"[JahroVideo] Android frame access — run: adb pull \"{_framesPath}\" .");
#endif
        }

        internal string CommitShadow()
        {
            if (_state != JahroVideoState.Shadow)
            {
                Debug.LogWarning("[JahroVideo] CommitShadow called but not in Shadow state");
                return null;
            }

            _shadowController.BeginCommit();
            _writerService?.StopAndFlush();

            string partPath = _shadowController.CommitFrames(_videoBasePath, ref _videoPartIndex);

            RestartShadowWriter();
            _shadowController.EndCommit();

            if (partPath != null)
                NotificationService.Instance.SendNotification(new Notification("Shadow replay saved"));

            return partPath;
        }

        internal void OnApplicationPause(bool pausing)
        {
            _shadowController.OnApplicationPause(pausing);
        }

        private void StopShadowRecording()
        {
            _shadowController.Deactivate();

            if (_writerService != null)
            {
                _writerService.StopAndFlush();
                _writerService.Dispose();
                _writerService = null;
            }

            _captureService?.Dispose();
            _captureService = null;

            _shadowController.DeleteShadowDirectory();

            _currentPartPath = null;
            _framesPath = null;
            SetState(JahroVideoState.Idle);
            Debug.Log("[JahroVideo] Shadow recording stopped without commit");
        }

        private void RestartShadowWriter()
        {
            if (_writerService != null)
            {
                _writerService.Dispose();
                _writerService = null;
            }
            _writerService = new FrameWriterService(_shadowController.ShadowPath, OnWriterError);
            _writerService.Start();
            _nextCaptureTime = Time.realtimeSinceStartup;
        }

        #endregion

        #region Capture Loop

        internal IEnumerator CaptureLoop()
        {
            float captureInterval = 1f / Mathf.Max(1, _config.frameRate);

            while (_state == JahroVideoState.Recording || _state == JahroVideoState.Shadow)
            {
                yield return _waitForEndOfFrame;

                if (_state != JahroVideoState.Recording && _state != JahroVideoState.Shadow) continue;
                if (_shadowController.IsCommitting || _shadowController.IsPaused) continue;
                if (Time.realtimeSinceStartup < _nextCaptureTime) continue;

                _captureService?.CaptureFrame();

                _nextCaptureTime = Time.realtimeSinceStartup + captureInterval;

                if (!_shadowController.IsActive)
                    CheckRecordingLimits();
            }
        }

        #endregion

        #region Frame Processing

        private bool InitializeCaptureService()
        {
            _captureService?.Dispose();
            _captureService = new FrameCaptureService(_config);
            _captureService.OnFrameEncoded = ProcessCapturedFrame;
            _captureService.OnCaptureErrorLimitReached = OnCaptureErrorLimitReached;
            _captureService.ShouldProcessFrame = () =>
                (_state == JahroVideoState.Recording || _state == JahroVideoState.Shadow)
                && _writerService != null
                && !_shadowController.IsCommitting;
            return _captureService.Initialize();
        }

        private void ProcessCapturedFrame(byte[] jpegBytes)
        {
            if (_writerService == null) return;

            _frameCounter++;
            int videoId = _shadowController.IsActive ? 0 : _videoPartIndex;
            _writerService.EnqueueFrame(jpegBytes, videoId, _frameCounter, DateTime.UtcNow);

            if (_frameCounter % DISK_CHECK_INTERVAL_FRAMES == 0)
                CheckDiskSpace();

            if (_shadowController.ShouldRunCleanup(_frameCounter))
                _shadowController.RunCleanupAsync();
        }

        private void OnCaptureErrorLimitReached()
        {
            StopRecording();
            FireError(JahroVideoError.FrameCaptureFailure);
        }

        #endregion

        #region Limits & Monitoring

        private bool CheckDiskSpaceForStart()
        {
            long diskSpaceMB = VideoStorageManager.GetAvailableDiskSpaceMB();
            if (diskSpaceMB < _minDiskSpaceMB)
            {
                Debug.LogWarning($"[JahroVideo] Insufficient disk space: {diskSpaceMB}MB available, {_minDiskSpaceMB}MB required");
                SetState(JahroVideoState.Error);
                FireError(JahroVideoError.InsufficientDiskSpace);
                return false;
            }
            return true;
        }

        private void CheckRecordingLimits()
        {
            float totalDuration = _cumulativeDurationSec + RecordingDurationSec;
            if (totalDuration >= _config.maxRecordingsDurationSec)
            {
                Debug.Log("[JahroVideo] Max recording duration reached (global limit)");
                StopRecording();
                FireError(JahroVideoError.MaxDurationReached);
                return;
            }

            long currentPartBytes = _writerService?.TotalBytesWritten ?? 0;
            long totalBytes = _cumulativeVideoBytesWritten + currentPartBytes;
            long maxBytes = (long)_config.maxVideoSizeMB * 1024L * 1024L;
            if (maxBytes > 0 && totalBytes >= maxBytes)
            {
                Debug.Log("[JahroVideo] Max video size reached (session limit)");
                StopRecording();
                FireError(JahroVideoError.StorageLimitReached);
            }
        }

        private void CheckDiskSpace()
        {
            long diskSpaceMB = VideoStorageManager.GetAvailableDiskSpaceMB();
            if (diskSpaceMB < _minDiskSpaceMB)
            {
                Debug.LogWarning("[JahroVideo] Disk space low — stopping recording");
                StopRecording();
                FireError(JahroVideoError.InsufficientDiskSpace);
            }
        }

        #endregion

        #region State & Events

        private void SetState(JahroVideoState newState)
        {
            if (_state == newState) return;
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void FireError(JahroVideoError error)
        {
            OnError?.Invoke(error);
        }

        private void OnWriterError(Exception ex)
        {
            if (ex is IOException)
            {
                Debug.LogError($"[JahroVideo] Disk write error — may indicate full disk: {ex.Message}");
            }
        }

        #endregion

        public void Dispose()
        {
            if (_state == JahroVideoState.Recording || _state == JahroVideoState.Shadow)
            {
                StopRecording();
            }

            if (_writerService != null)
            {
                _writerService.Dispose();
                _writerService = null;
            }

            _captureService?.Dispose();
            _captureService = null;
        }
    }
}
