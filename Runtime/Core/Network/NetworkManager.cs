using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using JahroConsole.Core.Notifications;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    internal class NetworkManager
    {
        private static readonly Lazy<NetworkManager> _instance = new Lazy<NetworkManager>(() => new NetworkManager());

        internal static NetworkManager Instance => _instance.Value;

        internal Action<NetworkError> OnError;

        private int _activeRequests = 0;

        internal bool HasActiveRequests
        {
            get
            {
                return _activeRequests > 0;
            }
        }

        internal IEnumerator SendRequestCoroutine(RequestBase request)
        {
            UnityWebRequest webRequest = CreateWebRequest(request);
            _activeRequests++;
            if (_activeRequests == 1) NotificationService.Instance.ActiveNetwork(true);
            request.FillHeaders(webRequest);

            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                request.UpdateUploadProgress(webRequest.uploadProgress);
                yield return null;
            }

            _activeRequests--;
            if (_activeRequests == 0) NotificationService.Instance.ActiveNetwork(false);
            HandleResponse(webRequest, request);
            webRequest.Dispose();
        }

        internal async Task SendRequestAsync(RequestBase request)
        {
            UnityWebRequest webRequest = CreateWebRequest(request);
            request.FillHeaders(webRequest);

            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                request.UpdateUploadProgress(webRequest.uploadProgress);
                await Task.Yield();
            }

            HandleResponse(webRequest, request);
            webRequest.Dispose();
        }

        private UnityWebRequest CreateWebRequest(RequestBase request)
        {
            var url = request.BuildRequestURL();

            UnityWebRequest webRequest;
            switch (request.GetRequestType())
            {
                case RequestBase.RequestType.POST:
                    webRequest = UnityWebRequest.Post(url, request.FormDataWWW());
                    break;
                case RequestBase.RequestType.POST_MULTIPART:
                    var boundary = UnityWebRequest.GenerateBoundary();
                    var multipartSections = request.FormDataMultipart();
                    var formData = UnityWebRequest.SerializeFormSections(multipartSections, boundary);

                    byte[] terminate = Encoding.UTF8.GetBytes(string.Concat("–", Encoding.UTF8.GetString(boundary), "–"));
                    byte[] body = new byte[formData.Length + terminate.Length];
                    Buffer.BlockCopy(formData, 0, body, 0, formData.Length);
                    Buffer.BlockCopy(terminate, 0, body, formData.Length, terminate.Length);
                    string contentType = string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));

                    webRequest = new UnityWebRequest(url);
                    var uploadHandler = new UploadHandlerRaw(body);
                    uploadHandler.contentType = contentType;
                    webRequest.uploadHandler = uploadHandler;
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.method = "POST";
                    break;
                case RequestBase.RequestType.GET:
                    webRequest = UnityWebRequest.Get(url);
                    break;
                case RequestBase.RequestType.DELETE:
                    webRequest = UnityWebRequest.Delete(url);
                    break;
                case RequestBase.RequestType.PUT:
                    webRequest = UnityWebRequest.Put(url, request.GetBodyData());
                    break;
                default:
                    webRequest = UnityWebRequest.Get(url);
                    break;
            }
            return webRequest;
        }

        private async Task SendWebRequestAsync(UnityWebRequest webRequest)
        {
            var tcs = new TaskCompletionSource<bool>();
            webRequest.SendWebRequest().completed += operation => tcs.SetResult(true);
            await tcs.Task;
        }

        private void HandleResponse(UnityWebRequest webRequest, RequestBase request)
        {

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                OnError?.Invoke(NetworkError.Build(webRequest.responseCode, webRequest.result, webRequest.error, webRequest.downloadHandler.text));
            }

            request.HandleResponse(webRequest);
        }
    }
}

