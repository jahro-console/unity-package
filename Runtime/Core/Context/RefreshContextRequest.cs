using System;
using Jahro.Core.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Jahro.Core.Context
{
    internal class RefreshContextRequest : RequestBase
    {

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

        internal Action<string, long> OnFail;

        private string _key;

        private string _projectId;

        private string _userId;

        internal RefreshContextRequest(string apiKey, string projectId, string userId)
        {
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

        internal override WWWForm FormDataWWW()
        {
            var form = new WWWForm();
            form.AddField("projectId", _projectId);
            form.AddField("userId", _userId);
            return form;
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

        protected override void OnRequestFail(string error, long responseCode)
        {
            base.OnRequestFail(error, responseCode);
            OnFail?.Invoke(error, responseCode);
        }
    }
}