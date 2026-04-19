using System;
using System.Collections;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Snapshots;
using UnityEngine;

namespace JahroConsole
{
    /// <summary>
    /// Video recording API for Jahro Console.
    /// Records game frames as JPEG sequences to disk as part of the active snapshot session.
    /// Note: Screen Space - Overlay canvases are NOT captured (Unity engine limitation).
    /// </summary>
    public static partial class Jahro
    {
        // =====================================================================
        // Video Recording — Events
        // =====================================================================

        /// <summary>
        /// Fired when video recording state changes (Idle, Recording, Paused, Stopping, Error).
        /// </summary>
        public static event Action<JahroVideoState> OnVideoStateChanged;

        // =====================================================================
        // Video Recording — Control
        // =====================================================================

        /// <summary>
        /// Starts video recording for the current snapshot session.
        /// Each call creates a new video part (video-1, video-2, …).
        /// Recording runs on a hidden child of the Jahro console; you can <c>yield return</c> this enumerator to wait until capture stops.
        /// </summary>
        public static void StartVideoRecording()
        {
            if (!Enabled)
            {
                JahroLogger.LogWarning($"[{JahroLogTag.Video}] StartVideoRecording called while Jahro is disabled");
                return;
            }
            SnapshotsManager.Instance.StartVideoRecording();
        }

        /// <summary>
        /// Stops the current video recording and finalizes the video part.
        /// </summary>
        public static void StopVideoRecording()
        {
            if (!Enabled) return;
            SnapshotsManager.Instance.StopVideoRecording();
        }

        // =====================================================================
        // Video Recording — State
        // =====================================================================

        /// <summary>
        /// Current state of the video recorder.
        /// </summary>
        public static JahroVideoState VideoState
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.CurrentState ?? JahroVideoState.Idle;
            }
        }

        /// <summary>
        /// True when video is actively recording (manual mode).
        /// </summary>
        public static bool IsVideoRecording
        {
            get
            {
                return VideoState == JahroVideoState.Recording;
            }
        }

        /// <summary>
        /// True when shadow recording is active (rolling buffer mode).
        /// </summary>
        public static bool IsShadowRecording
        {
            get
            {
                return VideoState == JahroVideoState.Shadow || VideoState == JahroVideoState.Committing;
            }
        }

        /// <summary>
        /// Commits the current shadow recording buffer as a new video part.
        /// Only valid when <see cref="IsShadowRecording"/> is true.
        /// </summary>
        public static void CommitShadowRecording()
        {
            if (!Enabled) return;
            SnapshotsManager.Instance.CommitShadowRecording();
        }

        // =====================================================================
        // Video Recording — Diagnostics
        // =====================================================================

        /// <summary>Duration of the current video part in seconds.</summary>
        public static float VideoRecordingDuration
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.RecordingDurationSec ?? 0f;
            }
        }

        /// <summary>Cumulative duration across all video parts in the current session.</summary>
        public static float VideoTotalDuration
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.TotalRecordingDurationSec ?? 0f;
            }
        }

        /// <summary>Number of frames captured in the current video part.</summary>
        public static int VideoFramesCaptured
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.FramesCaptured ?? 0;
            }
        }

        /// <summary>Total bytes written to disk for the current video part.</summary>
        public static long VideoStorageUsedBytes
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.StorageUsedBytes ?? 0;
            }
        }

        /// <summary>Current video part index (1-based, increments each start).</summary>
        public static int VideoPartIndex
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.VideoPartIndex ?? 0;
            }
        }

        /// <summary>Number of frames waiting to be written to disk.</summary>
        public static int VideoWriteQueueDepth
        {
            get
            {
                var recorder = SnapshotsManager.Instance.VideoRecorder;
                return recorder?.WriteQueueDepth ?? 0;
            }
        }

        // =====================================================================
        // Video Recording — Internal event forwarding
        // =====================================================================

        internal static void FireVideoStateChanged(JahroVideoState state) =>
            OnVideoStateChanged?.Invoke(state);

    }
}
