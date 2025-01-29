using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class SnapshotUploadRequest : RequestBase
    {

        [Serializable]
        internal class RefreshContextResponse
        {
            internal ProjectInfo projectInfo;
            internal TeamInfo tenantInfo;
            internal UserInfo[] users;
        }

        internal Action<RefreshContextResponse> OnComplete;

        internal Action<string, long> OnFail;

        private SnapshotSession _session;

        private string _key;

        private string _projectId;

        private string _userId;

        private string _teamId;

        internal SnapshotUploadRequest(SnapshotSession session, string apiKey, string teamId, string projectId, string userId) : base()
        {
            _session = session;
            _key = apiKey;
            _teamId = teamId;
            _projectId = projectId;
            _userId = userId;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST_MULTIPART;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/client/upload-snapshot";
        }

        internal override List<IMultipartFormSection> FormDataMultipart()
        {
            var formSections = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("tenantId", _teamId),
                new MultipartFormDataSection("projectId", _projectId),
                new MultipartFormDataSection("createdBy", _userId),
                new MultipartFormDataSection("platform", Application.platform.ToString()),
                new MultipartFormDataSection("version", Application.version),
                new MultipartFormDataSection("unityVersion", Application.unityVersion),
                new MultipartFormDataSection("deviceName", SystemInfo.deviceName),
                new MultipartFormFileSection("logFile", File.ReadAllBytes(_session.logFilePath), Path.GetFileName(_session.logFilePath), "application/gzip")
            };

            foreach (var filePath in _session.GetScreenshotsPaths())
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                formSections.Add(new MultipartFormFileSection("screenshot", fileData, Path.GetFileName(filePath), "image/jpeg"));
            }
            return formSections;
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("x-api-key", _key);
            request.SetRequestHeader("x-log-version", "1.1");
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            OnComplete?.Invoke(JsonUtility.FromJson<RefreshContextResponse>(result));
        }

        protected override void OnRequestFail(string error, long responseCode)
        {
            base.OnRequestFail(error, responseCode);
            OnFail?.Invoke(error, responseCode);
        }
    }
}