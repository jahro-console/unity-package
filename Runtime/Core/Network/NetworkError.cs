using System;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    [Serializable]
    public class NetworkError
    {
        [SerializeField] public string message;
        [SerializeField] public string error;
        [SerializeField] public int statusCode;
        [SerializeField] public string details;
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
            catch
            {
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
