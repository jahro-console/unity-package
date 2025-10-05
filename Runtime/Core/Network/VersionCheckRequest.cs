using System;
using JahroConsole.Core.Context;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    internal class VersionCheckRequest : RequestBase
    {

        [Serializable]
        internal class VersionCheckPayload
        {
            [SerializeField]
            internal string currentVersion;
            [SerializeField]
            internal string unityVersion;
            [SerializeField]
            internal string deviceName;
            [SerializeField]
            internal string isFreshInstall;
            [SerializeField]
            internal string platform;
        }

        public Action<string> OnComplete;

        public Action<NetworkError> OnFail;

        private bool _isFreshInstall;

        public VersionCheckRequest(bool isFreshInstall)
        {
            _isFreshInstall = isFreshInstall;
        }

        internal override RequestType GetRequestType()
        {
            return RequestType.POST;
        }

        internal override string GetURL()
        {
            return JahroConfig.HostUrl + "/client/version-check";
        }

        internal override string GetBodyData()
        {
            var payload = new VersionCheckPayload
            {
                currentVersion = JahroConfig.CurrentVersion,
                unityVersion = Application.unityVersion,
                deviceName = SystemInfo.deviceName,
                isFreshInstall = _isFreshInstall.ToString(),
                platform = Application.platform.ToString(),
            };
            return JsonUtility.ToJson(payload);
        }

        protected override void OnRequestComplete(string result)
        {
            base.OnRequestComplete(result);
            OnComplete?.Invoke(result);
        }

        protected override void OnRequestFail(NetworkError error)
        {
            base.OnRequestFail(error);
            OnFail?.Invoke(error);
        }
    }
}