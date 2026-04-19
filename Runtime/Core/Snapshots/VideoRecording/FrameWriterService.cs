using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Snapshots;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal struct FrameData
    {
        public byte[] JpegBytes;
        public int VideoId;
        public int FrameNumber;
        public DateTime TimestampUtc;
    }

    internal class FrameWriterService : IDisposable
    {
        internal const string FrameFileExtension = ".jframe";

        private readonly ConcurrentQueue<FrameData> _queue = new ConcurrentQueue<FrameData>();
        private readonly ManualResetEventSlim _signal = new ManualResetEventSlim(false);
        private readonly string _framesDirectory;
        private readonly Action<Exception> _onError;

        private Thread _writerThread;
        private volatile bool _isRunning;
        private long _totalBytesWritten;
        private int _framesWritten;
        private int _errorCount;

        internal long TotalBytesWritten => Interlocked.Read(ref _totalBytesWritten);
        internal int FramesWritten => Interlocked.CompareExchange(ref _framesWritten, 0, 0);
        internal int QueueDepth => _queue.Count;
        internal int ErrorCount => _errorCount;

        internal FrameWriterService(string framesDirectory, Action<Exception> onError)
        {
            _framesDirectory = framesDirectory ?? throw new ArgumentNullException(nameof(framesDirectory));
            _onError = onError;
            SharedFileUtility.EnsureDirectoryExists(framesDirectory);
        }

        internal void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _writerThread = new Thread(WriterLoop)
            {
                Name = "JahroFrameWriter",
                IsBackground = true
            };
            _writerThread.Start();
        }

        internal void EnqueueFrame(byte[] jpegBytes, int videoId, int frameNumber, DateTime timestampUtc)
        {
            if (!_isRunning) return;

            _queue.Enqueue(new FrameData
            {
                JpegBytes = jpegBytes,
                VideoId = videoId,
                FrameNumber = frameNumber,
                TimestampUtc = timestampUtc
            });
            _signal.Set();
        }

        internal void StopAndFlush()
        {
            _isRunning = false;
            _signal.Set();

            if (_writerThread != null && _writerThread.IsAlive)
            {
                if (!_writerThread.Join(5000))
                {
                    JahroLogger.LogWarning($"[{JahroLogTag.Video}] Writer thread did not finish within timeout — frames may be lost");
                }
            }
            _writerThread = null;
        }

        public void Dispose()
        {
            StopAndFlush();
            _signal?.Dispose();
        }

        private void WriterLoop()
        {
            while (_isRunning || !_queue.IsEmpty)
            {
                if (_queue.TryDequeue(out var frame))
                {
                    try
                    {
                        WriteFrameToDisk(frame);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _errorCount);
                        JahroLogger.LogError($"[{JahroLogTag.Video}] Frame write failed — frame={frame.FrameNumber}, error={ex.Message}");
                        _onError?.Invoke(ex);
                    }
                }
                else if (_isRunning)
                {
                    _signal.Wait(200);
                    _signal.Reset();
                }
            }
        }

        private void WriteFrameToDisk(FrameData frame)
        {
            string timestamp = frame.TimestampUtc.ToString("yyyy-MM-ddTHH-mm-ss-fffZ");
            string fileName = $"frame_{frame.VideoId:D3}_{frame.FrameNumber:D6}_{timestamp}{FrameFileExtension}";
            string finalPath = Path.Combine(_framesDirectory, fileName);
            string tempPath = finalPath + ".tmp";

            try
            {
                File.WriteAllBytes(tempPath, frame.JpegBytes);
                File.Move(tempPath, finalPath);

                Interlocked.Add(ref _totalBytesWritten, frame.JpegBytes.Length);
                Interlocked.Increment(ref _framesWritten);
            }
            catch
            {
                SharedFileUtility.DeleteFileSafe(tempPath);
                throw;
            }
        }
    }
}
