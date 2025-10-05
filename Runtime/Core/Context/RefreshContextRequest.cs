using System;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Context
{
    internal class RefreshContextRequest : RequestBase
    {
        [Serializable]
        internal class RefreshContextPayload
        {
            [SerializeField]
            internal string sessionId;
            [SerializeField]
            internal string projectId;
            [SerializeField]
            internal string userId;
        }

        [Serializable]
        internal class RefreshContextResponse
        {
            [SerializeField]
            internal ProjectInfo projectInfo;
            [SerializeField]
            internal TeamInfo tenantInfo;
            [SerializeField]
            internal UserInfo[] users;
        }

        internal Action<RefreshContextResponse> OnComplete;

        internal Action<NetworkError> OnFail;

        private string _sessionId;

        private string _key;

        private string _projectId;

        private string _userId;

        internal RefreshContextRequest(string sessionId, string apiKey, string projectId, string userId)
        {
            _sessionId = sessionId;
            _key = apiKey;
            _projectId = projectId;
            _userId = userId;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/client/refresh";
        }

        internal override string GetBodyData()
        {
            var payload = new RefreshContextPayload
            {
                sessionId = _sessionId,
                projectId = _projectId,
                userId = _userId,
            };
            return JsonUtility.ToJson(payload);
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("x-api-key", _key);
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            var response = JsonUtility.FromJson<RefreshContextResponse>(result);
            OnComplete?.Invoke(response);
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }
    }
}