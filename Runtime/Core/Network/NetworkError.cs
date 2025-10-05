using System;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    [Serializable]
    internal class NetworkError
    {
        [SerializeField] internal string message;
        [SerializeField] internal string error;
        [SerializeField] internal int statusCode;
        [SerializeField] internal string details;
        internal UnityWebRequest.Result result;


        public override string ToString()
        {
            return $"Jahro Error: {message} {statusCode} {result} {details}";
        }

        public string ToLogString()
        {
            return $"Jahro Error {(statusCode == 0 ? "" : statusCode)}: {message}";
        }

        internal static NetworkError Build(long code, UnityWebRequest.Result result, string errorName, string errorDetails)
        {
            if (string.IsNullOrEmpty(errorDetails))
            {
                return new NetworkError()
                {
                    message = errorName,
                    statusCode = (int)code,
                    result = result
                };
            }
            try
            {
                var error = JsonUtility.FromJson<NetworkError>(errorDetails);
                return error;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parse: " + ex.Message);
                return new NetworkError()
                {
                    message = errorName,
                    statusCode = (int)code,
                    result = result
                };
            }
        }
    }
}
