using System;
using System.Collections.Generic;
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
        internal class VideoFrameUploadPayload
        {
            [SerializeField] internal string videoId;
            [SerializeField] internal string videoFps;
            [SerializeField] internal string frameRange;
            [SerializeField] internal string frameCount;
            [SerializeField] internal string uploadMode;
            [SerializeField] internal string videoStatus;
            [SerializeField] internal string totalFrames;
            [SerializeField] internal string batchTimestamp;
        }

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

        internal override RequestType GetRequestType() => RequestType.POST_MULTIPART;

        internal override string GetURL()
        {
            return $"{JahroConfig.HostUrl}/snapshots/{_session.snapshotId}/videos";
        }

        internal override List<IMultipartFormSection> FormDataMultipart()
        {
            if (!File.Exists(_zipFilePath))
                throw new FileNotFoundException("ZIP package not found", _zipFilePath);

            string filename = Path.GetFileName(_zipFilePath);
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("videoId", _videoId),
                new MultipartFormDataSection("videoFps", _context.VideoConfig.frameRate.ToString()),
                new MultipartFormDataSection("frameRange", $"{_fromFrame}-{_toFrame}"),
                new MultipartFormDataSection("frameCount", _frameCount.ToString()),
                new MultipartFormDataSection("uploadMode", _uploadMode),
                new MultipartFormDataSection("videoStatus", _isFinal ? "final" : "open"),
                new MultipartFormDataSection("totalFrames", _totalFrames.ToString()),
                new MultipartFormDataSection("batchTimestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                new MultipartFormFileSection("file", File.ReadAllBytes(_zipFilePath), filename, "application/zip")
            };
            return formData;
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            base.FillHeaders(request);
            request.SetRequestHeader("x-api-key", _context.ApiKey);
            request.SetRequestHeader("x-user-id", _context.SelectedUserInfo?.Id ?? "");
            request.SetRequestHeader("Content-Encoding", "gzip");
        }

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
