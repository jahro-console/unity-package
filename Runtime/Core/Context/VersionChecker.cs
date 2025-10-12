using System;
using System.Threading.Tasks;
using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    public static class VersionChecker
    {
        public static async Task EditorSend(bool isFreshInstall, Action<VersionInfo> onComplete, Action<VersionInfo> onError = null)
        {
            VersionCheckRequest request = new VersionCheckRequest(isFreshInstall);
            request.OnComplete += (result) =>
            {
                onComplete?.Invoke(JsonUtility.FromJson<VersionInfo>(result));
            };
            request.OnFail += (error) =>
            {
                onError?.Invoke(new VersionInfo() { message = error.message });
            };
            await EditorNetworkManager.Instance.SendRequestAsync(request);
        }
    }
}