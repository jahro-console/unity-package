using System;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Jahro.Core.Network
{
    internal class ValidateKeyRequest : RequestBase
    {
        public Action<string> OnComplete;

        public Action<string> OnFail;

        private string _key;

        public ValidateKeyRequest(string key)
        {
            _key = key;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/client/verify-key";
        }

        internal override void FillHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("x-api-key", _key);
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            OnComplete?.Invoke(result);
        }

        protected override void OnRequestFail(string error, long responseCode)
        {
            base.OnRequestFail(error, responseCode);
            OnFail(error);
        }
    }
}