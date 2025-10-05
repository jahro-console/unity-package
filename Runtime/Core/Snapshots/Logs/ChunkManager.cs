using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class ChunkManager
    {
        private const int LOGS_PER_CHUNK = 1000;
        private const int FLUSH_INTERVAL_MS = 5000; // 5 seconds

        private readonly Dictionary<string, LogRecord> _uniqueBuffer = new Dictionary<string, LogRecord>();
        private readonly object _bufferLock = new object();
        private readonly Timer _flushTimer;

        private readonly SnapshotSession _session;

        private volatile bool _isRunning = false;
        private int _chunkSequence = 0;
        private static SynchronizationContext _mainThreadContext;
        internal event Action<ChunkMeta> OnChunkCreated;

        internal ChunkManager(SnapshotSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));

            _flushTimer = new Timer(OnTimerFlush, null, Timeout.Infinite, Timeout.Infinite);

            if (_mainThreadContext == null)
            {
                _mainThreadContext = SynchronizationContext.Current;
            }
        }

        internal void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _flushTimer.Change(FLUSH_INTERVAL_MS, FLUSH_INTERVAL_MS);
        }

        internal async Task FlushAndStopAsync()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _flushTimer.Dispose();

            Task writeTask = null;
            lock (_bufferLock)
            {
                if (_uniqueBuffer.Count > 0)
                {
                    writeTask = WriteChunkAsync();
                }
            }

            if (writeTask != null)
            {
                try
                {
                    await writeTask;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error writing final chunk: {ex.Message}");
                }
            }
        }

        internal void EnqueueLog(string message, string logType, string stackTrace)
        {
            if (!_isRunning) return;

            var logRecord = new LogRecord(logType, message, stackTrace);
            var logKey = GenerateLogKey(logRecord);

            lock (_bufferLock)
            {
                if (_uniqueBuffer.TryGetValue(logKey, out var existingRecord))
                {
                    existingRecord.count++;
                    _uniqueBuffer[logKey] = existingRecord;
                }
                else
                {
                    _uniqueBuffer[logKey] = logRecord;
                }

                if (_uniqueBuffer.Count >= LOGS_PER_CHUNK)
                {
                    WriteChunk();
                }
            }
        }

        private void OnTimerFlush(object state)
        {
            if (!_isRunning) return;

            lock (_bufferLock)
            {
                if (_uniqueBuffer.Count > 0)
                {
                    WriteChunk();
                }
            }
        }

        private async void WriteChunk()
        {
            await WriteChunkAsync();
        }

        private async Task WriteChunkAsync()
        {
            if (_uniqueBuffer.Count == 0) return;

            var chunkId = Guid.NewGuid().ToString();
            var sequence = Interlocked.Increment(ref _chunkSequence);
            var filename = $"chunk-{sequence:D6}-{chunkId}.json";
            var filePath = Path.Combine(_session.chunksFolderPath, filename);
            var metaFilePath = Path.Combine(_session.chunksMetaFolderPath, filename + ".meta");
            var tempPath = filePath + ".tmp";

            var logsToWrite = new List<LogRecord>(_uniqueBuffer.Values);
            _uniqueBuffer.Clear();

            try
            {
                await SharedFileUtility.WriteAtomicFileAsync(tempPath, filePath, async stream =>
                {
                    await WriteChunkContent(stream, logsToWrite);
                }).ConfigureAwait(false);

                var chunkInfo = new ChunkMeta
                {
                    chunkId = chunkId,
                    sequence = sequence,
                    chunkFilename = filename,
                    chunkFilePath = filePath,
                    metaFilePath = metaFilePath,
                    sessionId = _session.sessionId,
                    logsCount = logsToWrite.Count,
                    sizeBytes = new FileInfo(filePath).Length,
                    createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    status = ChunkMeta.ChunkStatus.Queued
                };

                UpdateChunkMeta(chunkInfo);

                _session.chunksCreated++;
                _session.SaveInfo();

                _mainThreadContext?.Post(_ => OnChunkCreated?.Invoke(chunkInfo), null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write chunk: {ex.Message}");
            }
        }

        private string GenerateLogKey(LogRecord logRecord)
        {
            return $"{logRecord.type}|{logRecord.message}|{logRecord.stacktrace}";
        }

        private async Task WriteChunkContent(Stream stream, List<LogRecord> logs)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                await writer.WriteAsync("[\n");
                int logsWritten = 0;
                foreach (var log in logs)
                {
                    var logEntry = JsonUtility.ToJson(log);
                    if (logsWritten > 0)
                    {
                        await writer.WriteAsync(",");
                    }
                    logsWritten++;
                    await writer.WriteLineAsync(logEntry);
                }
                await writer.WriteAsync("\n]");

                await writer.FlushAsync();
            }
        }

        internal static void UpdateChunkMeta(ChunkMeta chunkInfo)
        {
            if (chunkInfo == null || string.IsNullOrEmpty(chunkInfo.metaFilePath)) return;
            SharedFileUtility.SaveJsonToFile(chunkInfo, chunkInfo.metaFilePath);
        }
    }

}
