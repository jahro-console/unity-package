using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class SnapshotCreateRequest : RequestBase
    {

        [Serializable]
        internal class SnapshotCreatePayload
        {
            [SerializeField] internal string projectId;
            [SerializeField] internal string tenantId;
            [SerializeField] internal string name;
            [SerializeField] internal string timezone;
            [SerializeField] internal string createdBy;
            [SerializeField] internal string createdAt;
            [SerializeField] internal string platform;
            [SerializeField] internal string deviceName;
            [SerializeField] internal string unityVersion;
            [SerializeField] internal string version;
        }

        internal Action<SnapshotSession.RemoteSnapshotInfo> OnComplete;
        internal Action<NetworkError> OnFail;
        private SnapshotSession _session;
        private JahroContext _context;

        internal SnapshotCreateRequest(JahroContext context, SnapshotSession session) : base()
        {
            _session = session;
            _context = context;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/snapshots";
        }

        internal override string GetBodyData()
        {
            var payload = new SnapshotCreatePayload
            {
                projectId = _context.ProjectInfo.Id,
                tenantId = _context.TeamInfo.Id,
                name = _session.name,
                timezone = _session.timezone,
                createdBy = _context.SelectedUserInfo.Id,
                createdAt = new DateTime(_session.dateTimeTicks).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                platform = Application.platform.ToString(),
                deviceName = SystemInfo.deviceName,
                unityVersion = Application.unityVersion,
                version = Application.version,
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
            OnComplete?.Invoke(JsonUtility.FromJson<SnapshotSession.RemoteSnapshotInfo>(result));
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }
    }
}