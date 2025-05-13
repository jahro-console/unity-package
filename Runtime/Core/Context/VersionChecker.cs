using System;
using System.Threading.Tasks;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    public static class VersionChecker
    {
        [Serializable]
        public class VersionResponse
        {
            public bool success;
            public string message;
            public string currentVersion;
            public bool isLatestVersion;
            public bool updateRequired;
            public bool updateRecommended;
            public string updateSeverity;
            public string latestVersion;
            public string changelogUrl;
            public string repositoryUrl;
        }

        public static async Task Send(bool isFreshInstall, Action<VersionResponse> onComplete, Action<VersionResponse> onError)
        {
            VersionCheckRequest request = new VersionCheckRequest(isFreshInstall);
            request.OnComplete += (result) =>
            {
                onComplete?.Invoke(JsonUtility.FromJson<VersionResponse>(result));
            };
            request.OnFail += (error) =>
            {
                onError?.Invoke(new VersionResponse() { success = false, message = error });
            };
            await NetworkManager.Instance.SendRequestAsync(request);
        }
    }
}