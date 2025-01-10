using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Jahro.Core.Network
{
    internal abstract class RequestBase
    {
        internal enum RequestType
        {
            POST,
            POST_MULTIPART,
            GET,
            DELETE,
            PUT
        }

        internal Action<float> OnUploadProgress;


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

        }

        internal virtual void UpdateUploadProgress(float uploadProgress)
        {
            OnUploadProgress?.Invoke(uploadProgress);
        }

        protected virtual void OnRequestComplete(string result)
        {

        }

        protected virtual void OnRequestFail(string error, long responseCode)
        {

        }

        internal void HandleResponse(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                OnRequestComplete(request.downloadHandler.text);
            }
            else
            {
                OnRequestFail(request.error, request.responseCode);
            }
        }
    }
}