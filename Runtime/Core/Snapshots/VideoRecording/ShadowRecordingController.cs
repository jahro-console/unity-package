using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using JahroConsole.Core.Logging;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    /// <summary>
    /// Manages shadow recording state: pause/resume tracking, frame window
    /// management, commit operations, and background cleanup of expired frames.
    /// </summary>
    internal class ShadowRecordingController
    {
        private string _shadowPath;
        private float _shadowWindowSec;
        private float _shadowStartRealtimeSec;
        private DateTime _shadowStartUtc;
        private bool _isPaused;
        private float _pauseStartRealtimeSec;
        private float _accumulatedPausedSec;
        private volatile bool _commitInProgress;

        private const int SHADOW_CLEANUP_INTERVAL_FRAMES = 30;

        internal bool IsActive { get; private set; }
        internal bool IsPaused => _isPaused;
        internal bool IsCommitting => _commitInProgress;
        internal string ShadowPath => _shadowPath;

        internal void Activate(string shadowPath, float windowSec)
        {
            _shadowPath = shadowPath;
            _shadowWindowSec = windowSec;
            _shadowStartRealtimeSec = Time.realtimeSinceStartup;
            _shadowStartUtc = DateTime.UtcNow;
            _accumulatedPausedSec = 0f;
            _isPaused = false;
            _commitInProgress = false;
            IsActive = true;
        }

        internal void Deactivate()
        {
            IsActive = false;
            _commitInProgress = false;
        }

        internal void OnApplicationPause(bool pausing)
        {
            if (!IsActive) return;

            if (pausing)
            {
                _isPaused = true;
                _pauseStartRealtimeSec = Time.realtimeSinceStartup;
            }
            else
            {
                if (_isPaused)
                    _accumulatedPausedSec += Time.realtimeSinceStartup - _pauseStartRealtimeSec;
                _isPaused = false;
            }
        }

        internal void BeginCommit()
        {
            _commitInProgress = true;
        }

        internal void EndCommit()
        {
            _commitInProgress = false;
        }

        /// <summary>
        /// Moves frames within the shadow window to a new video part directory.
        /// Returns the part directory path on success, null on failure.
        /// Resets shadow timing on success so the next window starts fresh.
        /// </summary>
        internal string CommitFrames(string videoBasePath, ref int videoPartIndex)
        {
            float effectiveElapsedSec = Time.realtimeSinceStartup - _shadowStartRealtimeSec - _accumulatedPausedSec;
            DateTime cutoffUtc = _shadowStartUtc + TimeSpan.FromSeconds(effectiveElapsedSec - _shadowWindowSec);

            string[] allFiles;
            try
            {
                allFiles = Directory.GetFiles(_shadowPath, "*" + FrameWriterService.FrameFileExtension);
            }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Video}] CommitShadow failed to enumerate shadow files — path={_shadowPath}, error={ex.Message}");
                return null;
            }

            var framesInWindow = new List<(string path, DateTime timestamp)>(allFiles.Length);
            foreach (var file in allFiles)
            {
                if (TryParseShadowTimestamp(file, out DateTime ts) && ts >= cutoffUtc)
                    framesInWindow.Add((file, ts));
            }
            framesInWindow.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

            videoPartIndex++;
            string partDirName = $"video-{videoPartIndex}";
            string partDirPath = Path.Combine(videoBasePath, partDirName);
            SharedFileUtility.EnsureDirectoryExists(partDirPath);

            int newFrameNumber = 0;
            foreach (var (filePath, timestamp) in framesInWindow)
            {
                newFrameNumber++;
                string tsStr = timestamp.ToString("yyyy-MM-ddTHH-mm-ss-fffZ");
                string destName = $"frame_{videoPartIndex:D3}_{newFrameNumber:D6}_{tsStr}{FrameWriterService.FrameFileExtension}";
                try
                {
                    File.Move(filePath, Path.Combine(partDirPath, destName));
                }
                catch (Exception ex)
                {
                    JahroLogger.LogError($"[{JahroLogTag.Video}] CommitShadow failed to move frame — error={ex.Message}");
                }
            }

            foreach (var file in allFiles)
            {
                if (File.Exists(file))
                    SharedFileUtility.DeleteFileSafe(file);
            }

            ResetTimers();

            JahroLogger.LogDebug($"[{JahroLogTag.Video}] Shadow committed — part={partDirName}, frames={newFrameNumber}");
            return partDirPath;
        }

        internal bool ShouldRunCleanup(int frameCounter)
        {
            return IsActive && frameCounter % SHADOW_CLEANUP_INTERVAL_FRAMES == 0;
        }

        internal void RunCleanupAsync()
        {
            float elapsed = Time.realtimeSinceStartup - _shadowStartRealtimeSec - _accumulatedPausedSec;
            DateTime cutoff = _shadowStartUtc + TimeSpan.FromSeconds(elapsed - _shadowWindowSec);
            _ = Task.Run(() => CleanupExpiredFrames(cutoff));
        }

        internal void DeleteShadowDirectory()
        {
            if (!string.IsNullOrEmpty(_shadowPath))
            {
                try
                {
                    if (Directory.Exists(_shadowPath))
                        Directory.Delete(_shadowPath, true);
                }
                catch (Exception ex)
                {
                    JahroLogger.LogWarning($"[{JahroLogTag.Video}] Failed to delete shadow directory — path={_shadowPath}, error={ex.Message}");
                }
                _shadowPath = null;
            }
        }

        private void ResetTimers()
        {
            _shadowStartRealtimeSec = Time.realtimeSinceStartup;
            _shadowStartUtc = DateTime.UtcNow;
            _accumulatedPausedSec = 0f;
        }

        private void CleanupExpiredFrames(DateTime cutoffUtc)
        {
            string path = _shadowPath;
            if (_commitInProgress || string.IsNullOrEmpty(path)) return;

            try
            {
                string[] files = Directory.GetFiles(path, "*" + FrameWriterService.FrameFileExtension);
                foreach (var file in files)
                {
                    if (_commitInProgress) return;
                    if (TryParseShadowTimestamp(file, out DateTime ts) && ts < cutoffUtc)
                        SharedFileUtility.DeleteFileSafe(file);
                }
            }
            catch (Exception ex)
            {
                JahroLogger.LogWarning($"[{JahroLogTag.Video}] Shadow cleanup failed — error={ex.Message}");
            }
        }

        internal static bool TryParseShadowTimestamp(string filePath, out DateTime timestamp)
        {
            timestamp = default;
            string name = Path.GetFileNameWithoutExtension(filePath);
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore < 0) return false;
            string tsStr = name.Substring(lastUnderscore + 1);
            return DateTime.TryParseExact(
                tsStr,
                "yyyy-MM-ddTHH-mm-ss-fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out timestamp);
        }
    }
}
