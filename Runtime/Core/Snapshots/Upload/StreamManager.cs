using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
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
        internal event Action<ChunkMeta> OnChunkUploaded;
        internal event Action<ChunkMeta, string> OnChunkFailed;

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
                    Debug.LogError($"Error during StreamManager shutdown: {ex.Message}");
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
                    Debug.LogError($"Error during final uploads: {ex.Message}");
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
                    Debug.LogError($"Error processing upload queue: {ex.Message}");
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
                    Debug.LogError(error.ToLogString());
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
                Debug.LogError($"Screenshot {screenshotMeta.id} failed: {ex.Message}");
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

                        Debug.LogError(error.ToLogString());
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

                Debug.LogError($"Chunk {chunkInfo.chunkId} failed: {ex.Message}");
                OnChunkFailed?.Invoke(chunkInfo, ex.Message);
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
