using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class StreamManager : IDisposable
    {
        private readonly JahroContext _context;
        private ChunkManager _chunkManager;
        private SnapshotSession _snapshotSession;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentQueue<Func<Task>> _uploadQueue;
        private readonly SemaphoreSlim _queueSemaphore;
        private Task _queueProcessorTask;
        private volatile bool _isProcessing;
        private int _logLimitFlushScheduled;
        internal event Action<ChunkMeta> OnChunkUploaded;
        internal event Action<ChunkMeta, string> OnChunkFailed;

        /// <summary>
        /// Raised when the backend rejects a chunk for exceeding the per-snapshot log cap, after stopping the upload pump.
        /// Handlers must schedule any work that awaits shutdown of this manager off the current upload path (deadlock risk otherwise).
        /// </summary>
        internal event Action<string> OnLogLimitExceeded;

        private string _videoFramesPath;
        private int _videoPartIndex;
        private string _videoId;
        private int _lastUploadedFrameNumber;
        private Func<int> _getFramesCaptured;

        internal string VideoFramesPath => _videoFramesPath;
        internal int LastUploadedFrameNumber => _lastUploadedFrameNumber;

        internal StreamManager(JahroContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cancellationTokenSource = new CancellationTokenSource();
            _uploadQueue = new ConcurrentQueue<Func<Task>>();
            _queueSemaphore = new SemaphoreSlim(0);
            _isProcessing = false;
        }

        internal async Task StopAsync()
        {
            _isProcessing = false;
            _cancellationTokenSource.Cancel();

            if (_queueProcessorTask != null)
            {
                try
                {
                    await _queueProcessorTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown
                }
                catch (Exception ex)
                {
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Shutdown failed — error={ex.Message}");
                }
            }
        }

        internal async Task FlushAndStopAsync()
        {
            _isProcessing = false;

            var remainingTasks = new List<Task>();
            while (_uploadQueue.TryDequeue(out var uploadTask))
            {
                remainingTasks.Add(uploadTask());
            }

            if (remainingTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(remainingTasks);
                }
                catch (Exception ex)
                {
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Final upload flush failed — error={ex.Message}");
                }
            }
        }

        internal void StreamSnapshot(SnapshotSession snapshotSession, ChunkManager chunkManager)
        {
            if (_snapshotSession != null)
            {
                throw new Exception("Snapshot session already set");
            }

            _snapshotSession = snapshotSession ?? throw new ArgumentNullException(nameof(snapshotSession));
            _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
            _chunkManager.OnChunkCreated += OnChunkCreated;

            _isProcessing = true;
            _queueProcessorTask = ProcessUploadQueueAsync();
        }

        internal void SetVideoSource(string framesPath, int videoPartIndex, Func<int> getFramesCaptured)
        {
            _videoFramesPath = framesPath;
            _videoPartIndex = videoPartIndex;
            _videoId = videoPartIndex.ToString("D3");
            _lastUploadedFrameNumber = 0;
            _getFramesCaptured = getFramesCaptured;
        }

        internal void ClearVideoSource()
        {
            _videoFramesPath = null;
            _videoPartIndex = 0;
            _videoId = null;
            _getFramesCaptured = null;
        }

        internal void EnqueueFinalVideoUpload(int totalFramesCaptured)
        {
            if (string.IsNullOrEmpty(_videoFramesPath)) return;

            var framesPath = _videoFramesPath;
            int videoPartIndex = _videoPartIndex;
            string videoId = _videoId;
            int lastUploaded = _lastUploadedFrameNumber;

            _uploadQueue.Enqueue(async () =>
                await UploadVideoFramesBatchAsync(
                    framesPath, videoPartIndex, videoId, lastUploaded, totalFramesCaptured,
                    isFinal: true, totalFrames: totalFramesCaptured));
            _queueSemaphore.Release();
        }

        internal void StreamScreenshot(ScreenshotMeta screenshotMeta)
        {
            if (screenshotMeta == null) throw new Exception("Screenshot meta is null");

            _uploadQueue.Enqueue(async () => await UploadScreenshotAsync(screenshotMeta));
            _queueSemaphore.Release();
        }

        private void OnChunkCreated(ChunkMeta chunkInfo)
        {
            _uploadQueue.Enqueue(async () => await UploadChunkAsync(chunkInfo, _cancellationTokenSource.Token));
            _queueSemaphore.Release();

            if (!string.IsNullOrEmpty(_videoFramesPath))
            {
                _uploadQueue.Enqueue(async () => await UploadPendingVideoFramesAsync());
                _queueSemaphore.Release();
            }
        }

        private async Task ProcessUploadQueueAsync()
        {
            while (_isProcessing)
            {
                try
                {
                    await _queueSemaphore.WaitAsync(_cancellationTokenSource.Token);

                    if (_uploadQueue.TryDequeue(out var uploadTask))
                    {
                        await uploadTask();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Upload queue processing failed — error={ex.Message}");
                }
            }
        }

        private async Task UploadScreenshotAsync(ScreenshotMeta screenshotMeta)
        {
            screenshotMeta.lastUploadAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            try
            {
                var request = new ScreenshotUploadRequest(_context, _snapshotSession, screenshotMeta);
                request.OnComplete = (response) =>
                {
                    screenshotMeta.status = ScreenshotMeta.ScreenshotState.Uploaded;
                    screenshotMeta.lastError = null;
                    ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
                };

                request.OnFail = (error) =>
                {
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Screenshot upload failed — screenshotId={screenshotMeta.id}, {error.ToLogString()}");
                    screenshotMeta.status = ScreenshotMeta.ScreenshotState.UploadFailed;
                    screenshotMeta.lastError = error.message;
                    ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
                };

                await NetworkManager.Instance.SendRequestAsync(request);
            }
            catch (Exception ex)
            {
                screenshotMeta.status = ScreenshotMeta.ScreenshotState.UploadFailed;
                screenshotMeta.lastError = ex.Message;
                ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Screenshot upload exception — screenshotId={screenshotMeta.id}, error={ex.Message}");
            }
        }

        private async Task UploadChunkAsync(ChunkMeta chunkInfo, CancellationToken cancellationToken)
        {
            chunkInfo.status = ChunkMeta.ChunkStatus.Uploading;
            chunkInfo.lastAttemptAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            ChunkManager.UpdateChunkMeta(chunkInfo);

            try
            {
                using (var request = new ChunkUploadRequest(_context, _snapshotSession, chunkInfo))
                {
                    request.OnComplete = (response) =>
                    {
                        chunkInfo.status = ChunkMeta.ChunkStatus.Uploaded;
                        chunkInfo.lastError = null;
                        ChunkManager.UpdateChunkMeta(chunkInfo);

                        _snapshotSession.chunksUploaded++;
                        _snapshotSession.SaveInfo();

                        OnChunkUploaded?.Invoke(chunkInfo);
                    };

                    request.OnFail = (error) =>
                    {
                        chunkInfo.status = ChunkMeta.ChunkStatus.Failed;
                        chunkInfo.lastError = error.message;
                        ChunkManager.UpdateChunkMeta(chunkInfo);

                        JahroLogger.LogError($"[{JahroLogTag.Upload}] Chunk upload failed — chunkId={chunkInfo.chunkId}, {error.ToLogString()}");
                        var flushMsg = LogLimitFlushMessage(error, null);
                        if (flushMsg != null)
                        {
                            ScheduleLogLimitShutdown(flushMsg);
                            return;
                        }

                        OnChunkFailed?.Invoke(chunkInfo, error.message);
                    };

                    await NetworkManager.Instance.SendRequestAsync(request);
                }
            }
            catch (Exception ex)
            {
                chunkInfo.status = ChunkMeta.ChunkStatus.Failed;
                chunkInfo.lastError = ex.Message;
                chunkInfo.lastAttemptAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                ChunkManager.UpdateChunkMeta(chunkInfo);

                JahroLogger.LogError($"[{JahroLogTag.Upload}] Chunk upload exception — chunkId={chunkInfo.chunkId}, error={ex.Message}");
                var flushMsgEx = LogLimitFlushMessage(null, ex.Message);
                if (flushMsgEx != null)
                {
                    ScheduleLogLimitShutdown(flushMsgEx);
                    return;
                }

                OnChunkFailed?.Invoke(chunkInfo, ex.Message);
            }
        }

        /// <summary>Returns the error text when the backend rejected a bulk for exceeding the per-snapshot log cap; otherwise null (caller keeps streaming).</summary>
        private static string LogLimitFlushMessage(NetworkError error, string exceptionMessage)
        {
            var text = error?.message ?? error?.details ?? exceptionMessage;
            if (string.IsNullOrEmpty(text) || text.IndexOf("Too many logs", StringComparison.OrdinalIgnoreCase) < 0)
                return null;
            return text;
        }

        /// <summary>Stops the upload pump and notifies listeners; flush must run outside this queue (see <see cref="OnLogLimitExceeded"/>).</summary>
        private void ScheduleLogLimitShutdown(string errorMessage)
        {
            if (Interlocked.Exchange(ref _logLimitFlushScheduled, 1) != 0)
                return;

            _isProcessing = false;
            try { _cancellationTokenSource.Cancel(); }
            catch (Exception ex) { JahroLogger.LogWarning($"[{JahroLogTag.Upload}] Cancellation failed — error={ex.Message}"); }

            OnLogLimitExceeded?.Invoke(errorMessage);
        }

        private Task UploadPendingVideoFramesAsync()
        {
            int totalCaptured = _getFramesCaptured?.Invoke() ?? 0;
            return UploadVideoFramesBatchAsync(
                _videoFramesPath, _videoPartIndex, _videoId,
                _lastUploadedFrameNumber, totalCaptured,
                isFinal: false, totalFrames: -1);
        }

        private async Task UploadVideoFramesBatchAsync(
            string framesPath, int videoPartIndex, string videoId,
            int lastUploaded, int totalCaptured,
            bool isFinal, int totalFrames)
        {
            if (string.IsNullOrEmpty(framesPath) || !Directory.Exists(framesPath))
                return;

            if (totalCaptured <= lastUploaded)
                return;

            var pendingFrames = VideoPackager.GetPendingFramesByCounter(
                framesPath, videoPartIndex, lastUploaded, totalCaptured);
            if (pendingFrames.Count == 0) return;

            string tempZipPath = null;
            try
            {
                tempZipPath = await VideoPackager.CreateFramePackageAsync(
                    pendingFrames, _cancellationTokenSource.Token);

                int fromFrame = VideoPackager.ExtractFrameNumber(pendingFrames[0]);
                int toFrame = VideoPackager.ExtractFrameNumber(pendingFrames[pendingFrames.Count - 1]);

                using var request = new VideoFrameUploadRequest(
                    _context, _snapshotSession, videoId, tempZipPath,
                    fromFrame, toFrame, pendingFrames.Count,
                    "streaming", isFinal, totalFrames);

                request.OnComplete = response =>
                {
                    _lastUploadedFrameNumber = toFrame;
                    _snapshotSession.videoFramesUploaded += pendingFrames.Count;
                };

                request.OnFail = error =>
                {
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Video frame upload failed — videoId={videoId}, {error.ToLogString()}");
                };

                await NetworkManager.Instance.SendRequestAsync(request);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Video frame batch upload exception — videoId={videoId}, error={ex.Message}");
            }
            finally
            {
                if (tempZipPath != null)
                    SharedFileUtility.DeleteFileSafe(tempZipPath);
            }
        }

        public void Dispose()
        {
            if (_chunkManager != null)
            {
                _chunkManager.OnChunkCreated -= OnChunkCreated;
            }
            _cancellationTokenSource?.Dispose();
            _queueSemaphore?.Dispose();
        }
    }
}
