using System;
using UnityEngine;

namespace JahroConsole.Core.Context
{
    [Serializable]
    internal class VideoConfig
    {
        internal static VideoConfig Default { get; } = new VideoConfig()
        {
            maxVideoSizeMB = 200,
            maxRecordingsDurationSec = 900,
            frameRate = 5,
            jpegQuality = 50,
            maxResolutionSize = 860,
            shadowWindowSec = 120
        };

        [SerializeField] internal int maxVideoSizeMB;
        [SerializeField] internal int maxRecordingsDurationSec;
        [SerializeField] internal int frameRate;
        [SerializeField] internal int jpegQuality;
        [SerializeField] internal int maxResolutionSize;
        [SerializeField] internal int shadowWindowSec;
    }

}
