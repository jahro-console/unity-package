using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JahroConsole.Core.Notifications;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Core.Network
{
    internal class NetworkManager
    {
        private static readonly Lazy<NetworkManager> _instance = new Lazy<NetworkManager>(() => new NetworkManager());
        internal static NetworkManager Instance => _instance.Value;
        private int _activeRequests = 0;
        internal bool HasActiveRequests => _activeRequests > 0;

        private readonly HashSet<UnityWebRequest> _activeWebRequests = new HashSet<UnityWebRequest>();
        private readonly object _requestsLock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isShuttingDown = false;

        internal async Task SendRequestAsync(RequestBase request)
        {
            if (_isShuttingDown)
            {
                return;
            }

            UnityWebRequest webRequest = null;
            try
            {
                Interlocked.Increment(ref _activeRequests);

                webRequest = CreateWebRequest(request);
                request.FillHeaders(webRequest);
                ConfigureDownloadHandler(webRequest, request);
                webRequest.timeout = request.GetTimeout();

                lock (_requestsLock)
                {
                    _activeWebRequests.Add(webRequest);
                    if (_activeWebRequests.Count > 0)
                    {
                        NotificationService.Instance.ActiveNetwork(true);
                    }
                }

                var operation = webRequest.SendWebRequest();
                await WaitForRequestCompletion(operation, request);
            }
            finally
            {
                HandleResponse(webRequest, request);

                lock (_requestsLock)
                {
                    _activeWebRequests.Remove(webRequest);
                    if (_activeWebRequests.Count == 0)
                    {
                        NotificationService.Instance.ActiveNetwork(false);
                    }
                }

                webRequest?.Dispose();
                Interlocked.Decrement(ref _activeRequests);
            }
        }

        private void ConfigureDownloadHandler(UnityWebRequest webRequest, RequestBase request)
        {
            if (request.UseDownloadFileHandler && !string.IsNullOrEmpty(request.GetDownloadFilePath()))
            {
                webRequest.downloadHandler = new DownloadHandlerFile(request.GetDownloadFilePath());
            }
            else if (webRequest.downloadHandler == null)
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
            }
        }

        private async Task WaitForRequestCompletion(UnityWebRequestAsyncOperation operation, RequestBase request)
        {
            while (!operation.isDone)
            {
                try
                {
                    request.UpdateUploadProgress(operation.webRequest.uploadProgress);
                }
                catch
                {
                }
                await Task.Yield();
            }

            try
            {
                request.UpdateUploadProgress(operation.webRequest.uploadProgress);
            }
            catch
            {
            }
        }

        private UnityWebRequest CreateWebRequest(RequestBase request)
        {
            var url = request.BuildRequestURL();

            var customUploadHandler = request.GetUploadHandler();
            if (customUploadHandler != null)
            {
                return CreateRequestWithCustomUploadHandler(url, request, customUploadHandler);
            }

            return CreateRequestByType(url, request);
        }

        private UnityWebRequest CreateRequestWithCustomUploadHandler(string url, RequestBase request, UploadHandler uploadHandler)
        {
            var webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = uploadHandler,
                downloadHandler = new DownloadHandlerBuffer()
            };

            SetContentTypeIfAvailable(uploadHandler, request.GetContentType());
            return webRequest;
        }

        private UnityWebRequest CreateRequestByType(string url, RequestBase request)
        {
            return request.GetRequestType() switch
            {
                RequestBase.RequestType.POST => CreatePostRequest(url, request),
                RequestBase.RequestType.GET => UnityWebRequest.Get(url),
                RequestBase.RequestType.DELETE => UnityWebRequest.Delete(url),
                RequestBase.RequestType.PUT => CreatePutRequest(url, request),
                RequestBase.RequestType.POST_MULTIPART => CreatePostMultipartRequest(url, request),
                _ => UnityWebRequest.Get(url)
            };
        }

        private UnityWebRequest CreatePostRequest(string url, RequestBase request)
        {
            var body = request.GetBodyData();

            if (!string.IsNullOrEmpty(body))
            {
                return CreateRequestWithBody(url, request, body, UnityWebRequest.kHttpVerbPOST);
            }

            var formData = request.FormDataWWW();
            return UnityWebRequest.Post(url, formData ?? new WWWForm());
        }

        private UnityWebRequest CreatePutRequest(string url, RequestBase request)
        {
            var body = request.GetBodyData();

            if (!string.IsNullOrEmpty(body))
            {
                return CreateRequestWithBody(url, request, body, UnityWebRequest.kHttpVerbPUT);
            }

            return UnityWebRequest.Put(url, "");
        }

        private UnityWebRequest CreatePostMultipartRequest(string url, RequestBase request)
        {
            UnityWebRequest webRequest;
            try
            {
                var boundary = UnityWebRequest.GenerateBoundary();
                var multipartSections = request.FormDataMultipart();
                var formData = UnityWebRequest.SerializeFormSections(multipartSections, boundary);

                byte[] terminate = Encoding.UTF8.GetBytes(string.Concat("-", Encoding.UTF8.GetString(boundary), "-"));
                byte[] body = new byte[formData.Length + terminate.Length];
                Buffer.BlockCopy(formData, 0, body, 0, formData.Length);
                Buffer.BlockCopy(terminate, 0, body, formData.Length, terminate.Length);
                string contentType = string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
                webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                var uploadHandler = new UploadHandlerRaw(body);
                uploadHandler.contentType = contentType;
                webRequest.uploadHandler = uploadHandler;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating multipart request: {ex.Message}");
                throw;
            }
            return webRequest;
        }

        private UnityWebRequest CreateRequestWithBody(string url, RequestBase request, string body, string httpMethod)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            var contentType = request.GetContentType() ?? "application/json";
            var uploadHandler = new UploadHandlerRaw(bytes) { contentType = contentType };

            return new UnityWebRequest(url, httpMethod)
            {
                uploadHandler = uploadHandler,
                downloadHandler = new DownloadHandlerBuffer()
            };
        }

        private static void SetContentTypeIfAvailable(UploadHandler uploadHandler, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return;

            try
            {
                if (uploadHandler is UploadHandlerRaw rawHandler)
                {
                    rawHandler.contentType = contentType;
                }
                else
                {
                    uploadHandler.contentType = contentType;
                }
            }
            catch
            {
            }
        }

        private void HandleResponse(UnityWebRequest webRequest, RequestBase request)
        {
            NetworkError error = null;
            if (webRequest != null && webRequest.error == "Request aborted")
            {
                return;
            }

            if (webRequest == null || webRequest.result != UnityWebRequest.Result.Success)
            {
                error = NetworkError.Build(
                    webRequest?.responseCode ?? 0,
                    webRequest?.result ?? UnityWebRequest.Result.ConnectionError,
                    webRequest?.error ?? "Network Manager Error",
                    webRequest?.downloadHandler?.text ?? "Network Manager Error");
            }

            request.HandleResponse(webRequest, error);
        }

        internal async Task ShutdownAsync()
        {
            _isShuttingDown = true;

            while (HasActiveRequests)
            {
                try
                {
                    await Task.Yield();
                }
                catch
                {
                    // Silently handle any errors
                }
            }

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        internal void Shutdown()
        {
            _isShuttingDown = true;

            lock (_requestsLock)
            {
                foreach (var webRequest in _activeWebRequests)
                {
                    try
                    {
                        if (webRequest != null && !webRequest.isDone)
                        {
                            webRequest.Abort();
                        }
                    }
                    catch
                    {
                    }
                }
                _activeWebRequests.Clear();
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}

