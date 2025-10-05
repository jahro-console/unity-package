using System;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Snapshots
{
    internal class SnapshotFetchRequest : RequestBase
    {
        internal Action<SnapshotSession.RemoteSnapshotInfo> OnComplete;
        internal Action<NetworkError> OnFail;
        private JahroContext _context;
        private SnapshotSession _session;

        internal SnapshotFetchRequest(JahroContext context, SnapshotSession session) : base()
        {
            _context = context;
            _session = session;
        }
        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/snapshots/" + _session.snapshotId;
        }

        internal override string GetContentType()
        {
            return "application/json";
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.GET;
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