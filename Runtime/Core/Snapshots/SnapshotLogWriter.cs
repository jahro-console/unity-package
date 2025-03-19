using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class SnapshotLogWriter
    {
        private const string LOG_START = "@JAHRO_LOG_START@";
        private const string MSG_START = "@JAHRO_MSG_START@";
        private const string TRACE_START = "@JAHRO_TRACE_START@";
        private const string LOG_END = "@JAHRO_LOG_END@";
        private const string LINE_END = "\n";
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim _logSignal = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _loggingTask;
        private long _totalBytesWritten = 0;
        private const long MaxLogSizeBytes = 1024L * 1024L * 500L; // 500 MB max log size
        private readonly object _fileLock = new object();
        private SnapshotSession _session;
        private FileStream _fileStream;
        private BufferedStream _bufferedStream;
        private GZipStream _gzipStream;
        private StreamWriter _streamWriter;
        private bool _isPaused;

        internal SnapshotLogWriter(SnapshotSession session)
        {
            this._session = session;
        }

        // Start logging asynchronously
        internal void StartLogging()
        {
            _loggingTask = Task.Run(async () => await ProcessLogQueue(_cts.Token));
        }

        internal void PauseWriting()
        {
            _isPaused = true;
        }

        internal void ResumeWriting()
        {
            _isPaused = false;
            _logSignal.Release();
        }

        // Stop logging and flush everything
        internal async Task StopLoggingAsync()
        {
            _logSignal.Release();
            _cts.Cancel();
            if (_loggingTask != null)
            {
                await _loggingTask;
            }
        }

        internal async Task ForceFlushBuffer()
        {
            if (_streamWriter != null)
            {
                await _streamWriter.FlushAsync();
            }

            if (_gzipStream != null)
            {
                await _gzipStream.FlushAsync();
            }

            if (_bufferedStream != null)
            {
                await _bufferedStream.FlushAsync();
            }

            if (_fileStream != null)
            {
                await _fileStream.FlushAsync();
            }
        }

        internal void Log(string message, string logType, string stackTrace)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var messageLength = message?.Length ?? 0;
            var stackTraceLength = stackTrace?.Length ?? 0;

            var header = $"{{\n" +
                $"    \"timestamp\": \"{timestamp}\",\n" +
                $"    \"type\": \"{logType}\",\n" +
                $"    \"message_length\": {messageLength},\n" +
                $"    \"stacktrace_length\": {stackTraceLength}\n" +
                $"}}";

            var logEntry = $"{LOG_START}{LINE_END}" +
                $"{header}{LINE_END}" +
                $"{MSG_START}{LINE_END}" +
                $"{message ?? ""}{LINE_END}" +
                $"{TRACE_START}{LINE_END}" +
                $"{stackTrace ?? ""}{LINE_END}" +
                $"{LOG_END}{LINE_END}";

            _logQueue.Enqueue(logEntry);
            _logSignal.Release();
        }

        private void InitializeStreams()
        {
            _fileStream = new FileStream(_session.logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _bufferedStream = new BufferedStream(_fileStream);
            _gzipStream = new GZipStream(_bufferedStream, CompressionMode.Compress);
            _streamWriter = new StreamWriter(_gzipStream, Encoding.UTF8, 1024, leaveOpen: true);
        }

        // This method processes the log queue and writes to file asynchronously 
        private async Task ProcessLogQueue(CancellationToken token)
        {
            InitializeStreams();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Wait for new logs or cancellation
                    await _logSignal.WaitAsync(token);

                    if (_isPaused || token.IsCancellationRequested)
                    {
                        continue;  // Skip processing if paused or cancellation requested
                    }

                    // Dequeue and write logs to file
                    while (_logQueue.TryDequeue(out var logEntry))
                    {
                        await _streamWriter.WriteLineAsync(logEntry);

                        _totalBytesWritten += Encoding.UTF8.GetByteCount(logEntry);

                        if (_totalBytesWritten >= MaxLogSizeBytes)
                        {
                            Console.WriteLine("Log file reached its max size limit.");
                            break;
                        }
                    }

                    await _streamWriter.FlushAsync();
                    await _gzipStream.FlushAsync();
                    await _bufferedStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                await ForceFlushBuffer();
                DisposeStreams();
            }
        }

        private void DisposeStreams()
        {
            _streamWriter?.Dispose();
            _gzipStream?.Dispose();
            _bufferedStream?.Dispose();
            _fileStream?.Dispose();
        }

        // Provide progress in terms of bytes written
        internal long GetTotalBytesWritten() => _totalBytesWritten;
    }
}