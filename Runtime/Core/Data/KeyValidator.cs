using System;
using System.Threading.Tasks;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    public static class KeyValidator
    {
        [Serializable]
        public class ValidateKeyResponse
        {
            public string projectId;
            public string projectName;
            public string projectUrl;
            public string teamId;
            public string teamName;
            public string teamOverviewUrl;
            public string message;
            public bool success;
            public string apiKey;
            public string accountSettingsUrl;
        }

        public static async Task Send(string key, Action<ValidateKeyResponse> onComplete, Action<ValidateKeyResponse> onError)
        {
            ValidateKeyRequest request = new ValidateKeyRequest(key);
            request.OnComplete += (result) =>
            {
                onComplete?.Invoke(JsonUtility.FromJson<ValidateKeyResponse>(result));
            };
            request.OnFail += (error) =>
            {
                onError?.Invoke(new ValidateKeyResponse() { success = false, message = error });
            };
            await NetworkManager.Instance.SendRequestAsync(request);
        }
    }
}