using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    internal abstract class RequestBase
    {
        internal enum RequestType
        {
            POST,
            GET,
            DELETE,
            PUT,
            POST_MULTIPART
        }

        internal Action<float> OnUploadProgress;

        internal virtual string GetContentType() => null;

        internal virtual bool UseDownloadFileHandler => false;

        internal virtual string GetDownloadFilePath() => null;

        internal string BuildRequestURL()
        {
            string query = GetQuery();
            if (string.IsNullOrEmpty(query) == false)
            {
                return GetURL() + "?" + query;
            }
            return GetURL();
        }

        internal virtual WWWForm FormDataWWW()
        {
            return null;
        }

        internal virtual List<IMultipartFormSection> FormDataMultipart()
        {
            return null;
        }

        internal abstract string GetURL();

        internal abstract RequestType GetRequestType();

        internal virtual int GetTimeout()
        {
            return JahroConfig.DefaultTimeout;
        }

        internal virtual string GetBodyData()
        {
            return "";
        }

        internal virtual string GetQuery()
        {
            return "";
        }

        internal virtual UploadHandler GetUploadHandler()
        {
            return null;
        }

        internal virtual void FillHeaders(UnityWebRequest request)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.HasKey("usessionId"))
            {
                request.SetRequestHeader("usessionId", UnityEditor.EditorPrefs.GetString("usessionId"));
            }
            else
            {
                string usessionId = Guid.NewGuid().ToString();
                request.SetRequestHeader("usessionId", usessionId);
                UnityEditor.EditorPrefs.SetString("usessionId", usessionId);
            }
#endif
        }

        internal virtual void UpdateUploadProgress(float uploadProgress)
        {
            OnUploadProgress?.Invoke(uploadProgress);
        }

        protected virtual void OnRequestComplete(string result)
        {

        }

        protected virtual void OnRequestFail(NetworkError error)
        {

        }

        internal void HandleResponse(UnityWebRequest request, NetworkError error = null)
        {
            if (request != null && request.result == UnityWebRequest.Result.Success)
            {
                OnRequestComplete(request.downloadHandler.text);
            }
            else
            {
                OnRequestFail(error);
            }
        }
    }
}