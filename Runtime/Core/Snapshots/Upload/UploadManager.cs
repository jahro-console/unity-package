using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class UploadManager : IDisposable
    {
        internal const int BULK_BATCH_SIZE = 100;
        private readonly JahroContext _context;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HashSet<Task> _activeUploadTasks;
        private readonly Queue<SnapshotSession> _uploadQueue;
        private readonly object _queueLock = new object();
        internal event Action<SnapshotSession> OnSnapshotUploaded;
        internal event Action<SnapshotSession, string> OnSnapshotFailed;
        internal event Action<SnapshotSession, string> OnBulkFileReady;

        internal UploadManager(JahroContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cancellationTokenSource = new CancellationTokenSource();
            _activeUploadTasks = new HashSet<Task>();
            _uploadQueue = new Queue<SnapshotSession>();
        }

        internal async Task StopAsync()
        {
            _cancellationTokenSource.Cancel();

            if (_activeUploadTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(_activeUploadTasks);
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

        internal void QueueSnapshotForUpload(SnapshotSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            lock (_queueLock)
            {
                _uploadQueue.Enqueue(session);
            }

            ProcessUploadQueue();
        }

        private void ProcessUploadQueue()
        {
            lock (_queueLock)
            {
                if (_uploadQueue.Count == 0) return;

                var session = _uploadQueue.Dequeue();
                var uploadTask = UploadSnapshotAsync(session);
                _activeUploadTasks.Add(uploadTask);

                uploadTask.ContinueWith(task =>
                {
                    lock (_queueLock)
                    {
                        _activeUploadTasks.Remove(task);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private async Task UploadSnapshotAsync(SnapshotSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            try
            {
                var existingChunks = ChunkFileUtility.LoadExistingChunks(session.chunksMetaFolderPath, session.sessionId);

                var chunksToUpload = existingChunks.Where(chunk =>
                    chunk.status == ChunkMeta.ChunkStatus.Queued ||
                    chunk.status == ChunkMeta.ChunkStatus.Failed).ToList();

                if (chunksToUpload.Count != 0)
                {
                    var bulkFilePath = await CreateBulkFileInBackgroundAsync(session, chunksToUpload);

                    if (string.IsNullOrEmpty(bulkFilePath))
                    {
                        throw new Exception("Failed to create bulk file");
                    }

                    await UploadBulkFileAsync(session, bulkFilePath);
                    CleanupBulkFile(bulkFilePath);
                }

                await UploadAllVideoFramesAsync(session);
                await UploadAllScreenshotsAsync(session);

                session.SetStatus(SnapshotSession.Status.Uploaded);
                session.SetError(null);
                session.SaveInfo();
                OnSnapshotUploaded?.Invoke(session);
            }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Bulk upload failed — sessionId={session.sessionId}, error={ex.Message}");
                session.SetError(ex.Message);
                session.SaveInfo();
                OnSnapshotFailed?.Invoke(session, ex.Message);
            }
        }

        private async Task<string> CreateBulkFileInBackgroundAsync(SnapshotSession session, List<ChunkMeta> chunks)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var bulkFileName = $"bulk-{session.sessionId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json.gz";
                    var bulkFilePath = Path.Combine(session.chunksFolderPath, bulkFileName);

                    using (var fileStream = new FileStream(bulkFilePath, FileMode.Create, FileAccess.Write))
                    using (var gzipStream = new GZipStream(fileStream, System.IO.Compression.CompressionLevel.Optimal))
                    using (var writer = new StreamWriter(gzipStream, Encoding.UTF8))
                    {
                        writer.Write("[\n");

                        for (int i = 0; i < chunks.Count; i++)
                        {
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            var chunk = chunks[i];
                            BulkChunkContent(writer, chunk.chunkFilePath, i != chunks.Count - 1);
                            chunk.status = ChunkMeta.ChunkStatus.Uploading;
                            ChunkManager.UpdateChunkMeta(chunk);
                        }

                        writer.Write("\n]");
                        writer.Flush();
                    }

                    var fileInfo = new FileInfo(bulkFilePath);
                    OnBulkFileReady?.Invoke(session, bulkFilePath);
                    return bulkFilePath;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Failed to create bulk file — sessionId={session.sessionId}, error={ex.Message}");
                    return null;
                }
            }, _cancellationTokenSource.Token);
        }

        private void BulkChunkContent(StreamWriter writer, string chunkFilePath, bool addComma)
        {
            try
            {
                if (!File.Exists(chunkFilePath))
                {
                    JahroLogger.LogWarning($"[{JahroLogTag.Upload}] Chunk file missing — path={chunkFilePath}. Upload will skip this chunk.");
                    return;
                }

                using (var chunkFileStream = new FileStream(chunkFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var chunkReader = new StreamReader(chunkFileStream, Encoding.UTF8))
                {
                    string line;

                    var startBracket = chunkReader.ReadLine();
                    if (startBracket != "[")
                    {
                        JahroLogger.LogError($"[{JahroLogTag.Upload}] Corrupt chunk file — path={chunkFilePath}. File header failed validation.");
                        return;
                    }

                    while ((line = chunkReader.ReadLine()) != null)
                    {
                        if (line == "]" && chunkReader.EndOfStream)
                        {
                        }
                        else
                        {
                            writer.Write(line);
                        }
                    }

                    if (addComma)
                    {
                        writer.Write(",\n");
                    }
                }
            }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Chunk content stream failed — path={chunkFilePath}, error={ex.Message}");
            }
        }

        private async Task UploadBulkFileAsync(SnapshotSession session, string bulkFilePath)
        {
            var request = new BulkUploadRequest(_context, session, bulkFilePath);

            var tcs = new TaskCompletionSource<bool>();

            request.OnComplete += response =>
            {
                session.SetError(null);
                session.SaveInfo();
                tcs.SetResult(true);
            };

            request.OnFail += (error) =>
            {
                session.SetError(error.message);
                session.SaveInfo();
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Upload request failed — {error.ToLogString()}");
                tcs.SetException(new Exception($"Upload failed: {error.message}"));
            };

            try
            {
                await NetworkManager.Instance.SendRequestAsync(request);
                await tcs.Task;
            }
            finally
            {
                request.Dispose();
            }
        }

        private async Task UploadAllScreenshotsAsync(SnapshotSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var screenshotsMetas = ScreenshotFileUtility.GetScreenshotMetas(session);

            foreach (var screenshotMeta in screenshotsMetas)
            {
                if (screenshotMeta.status == ScreenshotMeta.ScreenshotState.Uploaded)
                {
                    continue;
                }

                await UploadScreenshotAsync(session, screenshotMeta);
            }
        }

        private async Task UploadScreenshotAsync(SnapshotSession session, ScreenshotMeta screenshotMeta)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (screenshotMeta == null) throw new ArgumentNullException(nameof(screenshotMeta));
            screenshotMeta.lastUploadAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            try
            {
                var request = new ScreenshotUploadRequest(_context, session, screenshotMeta);
                var tcs = new TaskCompletionSource<bool>();

                request.OnComplete += response =>
                {
                    screenshotMeta.status = ScreenshotMeta.ScreenshotState.Uploaded;
                    screenshotMeta.lastError = null;
                    ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
                    tcs.SetResult(true);
                };

                request.OnFail += (error) =>
                {
                    screenshotMeta.status = ScreenshotMeta.ScreenshotState.UploadFailed;
                    screenshotMeta.lastError = error.message;
                    ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
                    JahroLogger.LogError($"[{JahroLogTag.Upload}] Upload request failed — {error.ToLogString()}");
                    tcs.SetException(new Exception($"Upload failed: {error}"));
                };

                await NetworkManager.Instance.SendRequestAsync(request);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Screenshot upload failed — screenshotId={screenshotMeta.id}, sessionId={session.sessionId}, error={ex.Message}");
                screenshotMeta.status = ScreenshotMeta.ScreenshotState.UploadFailed;
                screenshotMeta.lastError = ex.Message;
                ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
            }
        }

        private async Task UploadAllVideoFramesAsync(SnapshotSession session)
        {
            string videoBasePath = Path.Combine(session.folderPath, "video");
            if (!Directory.Exists(videoBasePath)) return;

            var videoParts = Directory.GetDirectories(videoBasePath, "video-*");
            Array.Sort(videoParts, StringComparer.Ordinal);

            foreach (var videoPartPath in videoParts)
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                string videoPartFolder = Path.GetFileName(videoPartPath);
                if (!int.TryParse(videoPartFolder.Replace("video-", ""), out int videoIndex))
                    continue;
                string videoId = videoIndex.ToString("D3");

                var pendingFrames = VideoPackager.GetPendingFrames(videoPartPath, 0);
                if (pendingFrames.Count == 0) continue;

                int allFrameCount = pendingFrames.Count;

                for (int i = 0; i < pendingFrames.Count; i += BULK_BATCH_SIZE)
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    int batchSize = Math.Min(BULK_BATCH_SIZE, pendingFrames.Count - i);
                    var batch = pendingFrames.GetRange(i, batchSize);
                    bool isLastBatch = (i + batchSize >= pendingFrames.Count);

                    string tempZipPath = null;
                    try
                    {
                        tempZipPath = await VideoPackager.CreateFramePackageAsync(
                            batch, _cancellationTokenSource.Token);

                        int fromFrame = VideoPackager.ExtractFrameNumber(batch[0]);
                        int toFrame = VideoPackager.ExtractFrameNumber(batch[batch.Count - 1]);

                        await UploadVideoFrameBatchAsync(session, videoId, tempZipPath,
                            fromFrame, toFrame, batch.Count,
                            "recording",
                            isLastBatch,
                            isLastBatch ? allFrameCount : -1);

                        session.videoFramesUploaded += batch.Count;
                    }
                    finally
                    {
                        if (tempZipPath != null)
                            SharedFileUtility.DeleteFileSafe(tempZipPath);
                    }
                }
            }
        }

        private async Task UploadVideoFrameBatchAsync(
            SnapshotSession session, string videoId, string zipFilePath,
            int fromFrame, int toFrame, int frameCount,
            string uploadMode, bool isFinal, int totalFrames)
        {
            var request = new VideoFrameUploadRequest(_context, session, videoId,
                zipFilePath, fromFrame, toFrame, frameCount,
                uploadMode, isFinal, totalFrames);
            JahroLogger.LogDebug($"[{JahroLogTag.Upload}] Video frame batch uploading — videoId={videoId}, from={fromFrame}, to={toFrame}");

            var tcs = new TaskCompletionSource<bool>();

            request.OnComplete = response =>
            {
                JahroLogger.LogDebug($"[{JahroLogTag.Upload}] Video frame batch uploaded — videoId={videoId}, from={fromFrame}, to={toFrame}");
                tcs.SetResult(true);
            };

            request.OnFail = error =>
            {
                JahroLogger.LogError($"[{JahroLogTag.Upload}] Video frame batch upload failed — {error.ToLogString()}");
                tcs.SetException(new Exception($"Frame upload failed: {error.message}"));
            };

            try
            {
                await NetworkManager.Instance.SendRequestAsync(request);
                await tcs.Task;
                JahroLogger.LogDebug($"[{JahroLogTag.Upload}] Video frame batch confirmed — videoId={videoId}, from={fromFrame}, to={toFrame}");
            }
            finally
            {
                request.Dispose();
            }
        }

        private void CleanupBulkFile(string bulkFilePath)
        {
            SharedFileUtility.DeleteFileSafe(bulkFilePath);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}