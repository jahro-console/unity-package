using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class SnapshotUpdateRequest : RequestBase
    {

        [Serializable]
        internal class SnapshotUpdatePayload
        {
            [SerializeField] internal string name;
        }

        [Serializable]
        internal class SnapshotUpdateResponse
        {
            [SerializeField] internal string id;
            [SerializeField] internal string name;
            [SerializeField] internal string projectId;
            [SerializeField] internal string tenantId;
            [SerializeField] internal string slug;
            [SerializeField] internal string createdAt;
            [SerializeField] internal string updatedAt;
            [SerializeField] internal string createdBy;
            [SerializeField] internal string platform;
            [SerializeField] internal string deviceName;
            [SerializeField] internal string unityVersion;
            [SerializeField] internal string version;
            [SerializeField] internal int screenshotsCount;
            [SerializeField] internal string frontendUrl;
        }

        internal Action<SnapshotUpdateResponse> OnComplete;
        internal Action<NetworkError> OnFail;
        private SnapshotSession _session;
        private JahroContext _context;

        internal SnapshotUpdateRequest(JahroContext context, SnapshotSession session) : base()
        {
            _session = session;
            _context = context;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.PUT;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/snapshots/" + _session.snapshotId;
        }

        internal override string GetBodyData()
        {
            var payload = new SnapshotUpdatePayload
            {
                name = _session.name,
            };
            return JsonUtility.ToJson(payload);
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("x-api-key", _context.ApiKey);
            request.SetRequestHeader("x-user-id", _context.SelectedUserInfo.Id);
            request.SetRequestHeader("x-log-version", "1.2");
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            OnComplete?.Invoke(JsonUtility.FromJson<SnapshotUpdateResponse>(result));
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }
    }
}