using System;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class VideoFrameUploadRequest : RequestBase, IDisposable
    {
        [Serializable]
        internal class VideoFrameUploadResponse
        {
            [SerializeField] internal bool success;
            [SerializeField] internal string error;
        }

        internal Action<VideoFrameUploadResponse> OnComplete;
        internal Action<NetworkError> OnFail;

        private readonly JahroContext _context;
        private readonly SnapshotSession _session;
        private readonly string _videoId;
        private readonly string _zipFilePath;
        private readonly int _fromFrame;
        private readonly int _toFrame;
        private readonly int _frameCount;
        private readonly string _uploadMode;
        private readonly bool _isFinal;
        private readonly int _totalFrames;

        internal VideoFrameUploadRequest(
            JahroContext context, SnapshotSession session, string videoId,
            string zipFilePath, int fromFrame, int toFrame, int totalFramesInBatch,
            string uploadMode, bool isFinal, int totalFrames)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _videoId = videoId;
            _zipFilePath = zipFilePath;
            _fromFrame = fromFrame;
            _toFrame = toFrame;
            _frameCount = totalFramesInBatch;
            _uploadMode = uploadMode;
            _isFinal = isFinal;
            _totalFrames = totalFrames;
        }

        internal override RequestType GetRequestType() => RequestType.POST;

        internal override string GetURL()
        {
            return $"{JahroConfig.HostUrl}/snapshots/{_session.snapshotId}/videos";
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            base.FillHeaders(request);
            request.SetRequestHeader("x-api-key", _context.ApiKey);
            request.SetRequestHeader("x-user-id", _context.SelectedUserInfo?.Id ?? "");
            request.SetRequestHeader("Accept", "application/json");

            request.SetRequestHeader("X-Video-Id", _videoId);
            request.SetRequestHeader("X-Video-Fps", _context.VideoConfig.frameRate.ToString());
            request.SetRequestHeader("X-Frame-Range", $"{_fromFrame}-{_toFrame}");
            request.SetRequestHeader("X-Frame-Count", _frameCount.ToString());
            request.SetRequestHeader("X-Upload-Mode", _uploadMode);
            request.SetRequestHeader("X-Video-Status", _isFinal ? "final" : "open");
            request.SetRequestHeader("X-Video-Total-Frames", _totalFrames.ToString());
            request.SetRequestHeader("X-Batch-Timestamp",
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }

        internal override UploadHandler GetUploadHandler()
        {
            if (!File.Exists(_zipFilePath))
                throw new FileNotFoundException("ZIP package not found", _zipFilePath);

            return new UploadHandlerFile(_zipFilePath)
            {
                contentType = GetContentType()
            };
        }

        internal override string GetContentType() => "application/zip";

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            var response = JsonUtility.FromJson<VideoFrameUploadResponse>(result);
            OnComplete?.Invoke(response);
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }

        public void Dispose() { }
    }
}
