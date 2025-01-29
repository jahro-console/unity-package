using System;
using JahroConsole.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Context
{
    internal class InitContextRequest : RequestBase
    {

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

        internal Action<string, long> OnFail;

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

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/client/session";
        }

        internal override WWWForm FormDataWWW()
        {
            var form = new WWWForm();
            form.AddField("sessionId", _sessionId);
            form.AddField("platform", Application.platform.ToString());
            form.AddField("unityVersion", Application.unityVersion);
            form.AddField("jahro-version", _version);
            return form;
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

        protected override void OnRequestFail(string error, long responseCode)
        {
            base.OnRequestFail(error, responseCode);
            OnFail?.Invoke(error, responseCode);
        }
    }
}