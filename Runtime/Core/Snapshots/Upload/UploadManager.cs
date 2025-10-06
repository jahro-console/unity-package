using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class UploadManager : IDisposable
    {
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
                    Debug.LogError($"Error during UploadManager shutdown: {ex.Message}");
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

                if (chunksToUpload.Count == 0)
                {
                    OnSnapshotUploaded?.Invoke(session);
                    return;
                }

                var bulkFilePath = await CreateBulkFileInBackgroundAsync(session, chunksToUpload);

                if (string.IsNullOrEmpty(bulkFilePath))
                {
                    throw new Exception("Failed to create bulk file");
                }

                await UploadBulkFileAsync(session, bulkFilePath);
                await UploadAllScreenshotsAsync(session);
                CleanupBulkFile(bulkFilePath);
                session.SetStatus(SnapshotSession.Status.Uploaded);
                session.SetError(null);
                session.SaveInfo();
                OnSnapshotUploaded?.Invoke(session);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during bulk upload for snapshot {session.sessionId}: {ex.Message}");
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
                    Debug.LogError($"Failed to create bulk file for session {session.sessionId}: {ex.Message}");
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
                    Debug.LogWarning($"Chunk file not found: {chunkFilePath}");
                    return;
                }

                using (var chunkFileStream = new FileStream(chunkFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var chunkReader = new StreamReader(chunkFileStream, Encoding.UTF8))
                {
                    string line;

                    var startBracket = chunkReader.ReadLine();
                    if (startBracket != "[")
                    {
                        Debug.LogError($"Corrupted chunk file: {chunkFilePath}");
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
                Debug.LogError($"Failed to stream chunk content from {chunkFilePath}: {ex.Message}");
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
                Debug.LogError(error.ToLogString());
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
                    Debug.LogError(error.ToLogString());
                    tcs.SetException(new Exception($"Upload failed: {error}"));
                };

                await NetworkManager.Instance.SendRequestAsync(request);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error uploading screenshot {screenshotMeta.id} for session {session.sessionId}: {ex.Message}");
                screenshotMeta.status = ScreenshotMeta.ScreenshotState.UploadFailed;
                screenshotMeta.lastError = ex.Message;
                ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
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