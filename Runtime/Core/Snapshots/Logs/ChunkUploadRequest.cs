using System;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class ChunkUploadRequest : RequestBase, IDisposable
    {

        [Serializable]
        internal class ChunkUploadResponse
        {
            [SerializeField] internal bool success;
            [SerializeField] internal string error;
            [SerializeField] internal string serverChunkId;
        }

        private readonly static int TIMEOUT = 4;

        internal Action<ChunkUploadResponse> OnComplete;
        internal Action<NetworkError> OnFail;

        private readonly ChunkMeta _chunkMeta;
        private readonly JahroContext _context;
        private readonly SnapshotSession _snapshotSession;
        private readonly string _filePath;
        private const bool _gzip = true;
        private string _tempGzipPath;

        internal ChunkUploadRequest(JahroContext context, SnapshotSession snapshotSession, ChunkMeta chunkMeta)
        {
            _context = context;
            _snapshotSession = snapshotSession;
            _chunkMeta = chunkMeta;
            _filePath = _chunkMeta.chunkFilePath;
        }

        internal override RequestType GetRequestType() => RequestType.POST;

        internal override string GetURL()
        {
            return $"{JahroConfig.HostUrl}/snapshots/{_snapshotSession.snapshotId}/logs/_bulk";
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            base.FillHeaders(request);
            request.SetRequestHeader("x-api-key", _context.ApiKey);
            request.SetRequestHeader("x-user-id", _context.SelectedUserInfo?.Id ?? "");
            if (_gzip)
            {
                request.SetRequestHeader("Content-Encoding", "gzip");
            }
            request.SetRequestHeader("Accept", "application/json");
        }

        internal override UploadHandler GetUploadHandler()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Chunk file not found", _filePath);

            string fileToUpload = _filePath;
            if (_gzip)
            {
                _tempGzipPath = FileUploadHelpers.CreateGzipTempFile(_filePath);
                fileToUpload = _tempGzipPath;
            }

            return new UploadHandlerFile(fileToUpload)
            {
                contentType = GetContentType()
            };
        }

        internal override string GetContentType()
        {
            return "application/json";
        }


        internal override int GetTimeout()
        {
            return TIMEOUT;
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            var response = JsonUtility.FromJson<ChunkUploadResponse>(result);
            OnComplete?.Invoke(response);
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_tempGzipPath))
            {
                SharedFileUtility.DeleteFileSafe(_tempGzipPath);
            }
        }
    }
}
