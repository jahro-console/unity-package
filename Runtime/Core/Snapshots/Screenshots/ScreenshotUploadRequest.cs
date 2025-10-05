using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class ScreenshotUploadRequest : RequestBase
    {
        [Serializable]
        internal class ScreenshotUploadResponse
        {
            [SerializeField] internal bool success;
            [SerializeField] internal string screenshotId;
            [SerializeField] internal string error;
        }

        internal Action<ScreenshotUploadResponse> OnComplete;
        internal Action<NetworkError> OnFail;

        private readonly string _snapshotId;
        private readonly ScreenshotMeta _screenshotMeta;
        private readonly string _screenshotFilePath;
        private readonly string _apiKey;
        private readonly string _userId;
        private readonly string _sessionId;

        internal ScreenshotUploadRequest(JahroContext context, SnapshotSession snapshotSession, ScreenshotMeta screenshotMeta)
        {
            _snapshotId = snapshotSession.snapshotId;
            _screenshotMeta = screenshotMeta;
            _screenshotFilePath = screenshotMeta.filepath;
            _apiKey = context.ApiKey;
            _userId = context.SelectedUserInfo?.Id ?? "";
            _sessionId = snapshotSession.sessionId;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST_MULTIPART;
        }

        internal override string GetURL()
        {
            return $"{JahroConfig.HostUrl}/snapshots/{_snapshotId}/screenshots";
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            base.FillHeaders(request);
            request.SetRequestHeader("X-Client-Session-Id", _sessionId);
            request.SetRequestHeader("x-api-key", _apiKey);
            request.SetRequestHeader("x-user-id", _userId);
        }

        internal override List<IMultipartFormSection> FormDataMultipart()
        {
            if (!File.Exists(_screenshotFilePath))
                throw new FileNotFoundException("Screenshot file not found", _screenshotFilePath);

            string filename = Path.GetFileName(_screenshotFilePath);
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("capturedAt", _screenshotMeta.createdAt),
                new MultipartFormDataSection("filename", filename),
                new MultipartFormFileSection("file", File.ReadAllBytes(_screenshotFilePath), filename, "image/jpeg")
            };
            return formData;
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            var response = JsonUtility.FromJson<ScreenshotUploadResponse>(result);
            OnComplete?.Invoke(response);
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }
    }
}
