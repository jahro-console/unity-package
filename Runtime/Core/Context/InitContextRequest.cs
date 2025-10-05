using System;
using System.Collections.Concurrent;
using JahroConsole.Core.Data;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Context
{
    internal class InitContextRequest : RequestBase
    {

        [Serializable]
        internal class InitContextPayload
        {
            [SerializeField]
            internal string sessionId;
            [SerializeField]
            internal string platform;
            [SerializeField]
            internal string unityVersion;
            [SerializeField]
            internal string deviceName;
            [SerializeField]
            internal string jahroVersion;
        }

        [Serializable]
        internal class InitContextResponse
        {
            [SerializeField]
            internal ProjectInfo projectInfo;
            [SerializeField]
            internal TeamInfo tenantInfo;
            [SerializeField]
            internal UserInfo[] users;
            [SerializeField]
            internal VersionInfo versionInfo;
        }

        internal Action<InitContextResponse> OnComplete;

        internal Action<NetworkError> OnFail;

        private string _sessionId;

        private string _key;

        private string _version;

        internal InitContextRequest(string sessionId, string apiKey, string version)
        {
            _sessionId = sessionId;
            _key = apiKey;
            _version = version;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST;
        }

        internal override int GetTimeout()
        {
            return JahroConfig.DefaultShortTimeout;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/client/session";
        }

        internal override string GetBodyData()
        {
            var payload = new InitContextPayload
            {
                sessionId = _sessionId,
                platform = Application.platform.ToString(),
                unityVersion = Application.unityVersion,
                deviceName = SystemInfo.deviceName,
                jahroVersion = _version,
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
            OnComplete?.Invoke(JsonUtility.FromJson<InitContextResponse>(result));
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }
    }
}