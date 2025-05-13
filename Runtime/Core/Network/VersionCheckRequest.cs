using System;
using JahroConsole.Core.Context;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    internal class VersionCheckRequest : RequestBase
    {
        public Action<string> OnComplete;

        public Action<string> OnFail;

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

        internal override WWWForm FormDataWWW()
        {
            var form = new WWWForm();
            form.AddField("currentVersion", JahroConfig.CurrentVersion);
            form.AddField("unityVersion", Application.unityVersion);
            form.AddField("deviceName", SystemInfo.deviceName);
            form.AddField("isFreshInstall", _isFreshInstall.ToString());
            form.AddField("platform", Application.platform.ToString());
            return form;
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