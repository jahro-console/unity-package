using System;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using JahroConsole.Core.Context;

namespace JahroConsole.Core.Snapshots
{
    /// <summary>
    /// Handles screen capture and JPEG encoding for video recording.
    /// Supports async GPU readback (default) and synchronous ReadPixels fallback
    /// (for environments like Android Emulator where async capture produces black frames).
    /// </summary>
    internal class FrameCaptureService : IDisposable
    {
        private RenderTexture _fullResRT;
        private RenderTexture _targetRT;
        private int _targetWidth;
        private int _targetHeight;
        private int _pendingReadbacks;

        private bool _useReadPixelsFallback;
        private Texture2D _readPixelsTex;
        private Texture2D _readPixelsScaledTex;

        private readonly VideoConfig _config;
        private int _consecutiveErrors;

        private const int MAX_PENDING_READBACKS = 3;
        private const int MAX_CONSECUTIVE_ERRORS = 10;

        internal int TargetWidth => _targetWidth;
        internal int TargetHeight => _targetHeight;
        internal bool UseReadPixelsFallback => _useReadPixelsFallback;

        /// <summary>
        /// Gate checked before encoding in the async readback callback.
        /// Return false to skip processing when the recorder is no longer in a valid capture state.
        /// Not used in the synchronous path (the caller guarantees valid state via CaptureLoop).
        /// </summary>
        internal Func<bool> ShouldProcessFrame;

        /// <summary>
        /// Invoked with JPEG-encoded frame bytes ready for writing.
        /// Fires from the GPU readback callback (async) or inline (sync).
        /// </summary>
        internal Action<byte[]> OnFrameEncoded;

        /// <summary>
        /// Fired when consecutive async readback errors reach the safety limit.
        /// </summary>
        internal Action OnCaptureErrorLimitReached;

        internal FrameCaptureService(VideoConfig config)
        {
            _config = config;
        }

        internal bool Initialize()
        {
            CalculateTargetResolution();
            if (!AllocateResources())
                return false;
            _useReadPixelsFallback = ShouldUseReadPixelsFallback();
            _consecutiveErrors = 0;
            _pendingReadbacks = 0;
            return true;
        }

        internal void CaptureFrame()
        {
            if (_useReadPixelsFallback)
                CaptureFrameSynchronous();
            else
                CaptureFrameAsync();
        }

        #region Async GPU Readback Path

        private void CaptureFrameAsync()
        {
            if (_pendingReadbacks >= MAX_PENDING_READBACKS) return;
            if (_targetRT == null) return;

            EnsureFullResRT();
            if (_fullResRT == null) return;

            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(_fullResRT);
                BlitFit(_fullResRT, _targetRT);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JahroVideo] Capture error: {ex.Message}");
                _consecutiveErrors++;
                return;
            }

            Interlocked.Increment(ref _pendingReadbacks);
            AsyncGPUReadback.Request(_targetRT, 0, TextureFormat.RGBA32, OnReadbackComplete);
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            Interlocked.Decrement(ref _pendingReadbacks);

            if (request.hasError)
            {
                _consecutiveErrors++;
                if (_consecutiveErrors >= MAX_CONSECUTIVE_ERRORS)
                {
                    Debug.LogError("[JahroVideo] Too many consecutive readback errors — stopping");
                    OnCaptureErrorLimitReached?.Invoke();
                }
                return;
            }

            if (ShouldProcessFrame != null && !ShouldProcessFrame())
                return;

            _consecutiveErrors = 0;

            try
            {
                NativeArray<byte> rawData = request.GetData<byte>();
                byte[] jpegBytes = EncodeToJpeg(rawData);
                if (jpegBytes != null)
                    OnFrameEncoded?.Invoke(jpegBytes);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JahroVideo] Encoding error: {ex.Message}");
                _consecutiveErrors++;
            }
        }

        private void EnsureFullResRT()
        {
            int screenW = Screen.width;
            int screenH = Screen.height;

            if (screenW <= 0 || screenH <= 0) return;
            if (_fullResRT != null && _fullResRT.width == screenW && _fullResRT.height == screenH) return;

            if (_fullResRT != null)
            {
                _fullResRT.Release();
                UnityEngine.Object.Destroy(_fullResRT);
            }

            try
            {
                _fullResRT = new RenderTexture(screenW, screenH, 0, RenderTextureFormat.ARGB32);
                _fullResRT.Create();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JahroVideo] Failed to reallocate capture RT: {ex.Message}");
                _fullResRT = null;
            }
        }

        #endregion

        #region Synchronous ReadPixels Fallback

        /// <summary>
        /// Synchronous fallback capture used when ScreenCapture.CaptureScreenshotIntoRenderTexture
        /// produces black frames (e.g. Android Emulator with OpenGL ES Translator).
        /// Reads directly from the framebuffer via Texture2D.ReadPixels, then scales via Blit.
        /// Must be called after WaitForEndOfFrame (i.e. from CaptureLoop coroutine).
        /// </summary>
        private void CaptureFrameSynchronous()
        {
            if (_targetRT == null) return;

            try
            {
                int screenW = Screen.width;
                int screenH = Screen.height;
                if (screenW <= 0 || screenH <= 0) return;

                if (_readPixelsTex == null || _readPixelsTex.width != screenW || _readPixelsTex.height != screenH)
                {
                    if (_readPixelsTex != null) UnityEngine.Object.Destroy(_readPixelsTex);
                    _readPixelsTex = new Texture2D(screenW, screenH, TextureFormat.RGBA32, false);
                }

                if (_readPixelsScaledTex == null)
                    _readPixelsScaledTex = new Texture2D(_targetWidth, _targetHeight, TextureFormat.RGBA32, false);

                var prevActive = RenderTexture.active;
                RenderTexture.active = null;
                _readPixelsTex.ReadPixels(new Rect(0, 0, screenW, screenH), 0, 0);
                _readPixelsTex.Apply(false);

                // Unlike the async path (ScreenCapture → AsyncGPUReadback), Texture2D.ReadPixels
                // from the default framebuffer already returns pixel data in the orientation that
                // the subsequent pipeline expects on most platforms — no Y-flip is needed.
                //
                // Whether a flip is required depends on what convention ReadPixels uses:
                //   • Android Emulator (OpenGL ES Translator / SwiftShader over Metal or D3D host):
                //     top-down convention → no flip.
                //   • Direct3D / Metal (iOS, macOS, Windows): top-down → no flip.
                //   • OpenGL / Vulkan (Linux, other non-Android): bottom-up → flip needed,
                //     but ShouldUseReadPixelsFallback() returns false there (async path used instead).
                bool needsYFlip = NeedsReadPixelsYFlip();
                if (needsYFlip)
                    Graphics.Blit(_readPixelsTex, _targetRT, new Vector2(1f, -1f), new Vector2(0f, 1f));
                else
                    Graphics.Blit(_readPixelsTex, _targetRT);

                RenderTexture.active = _targetRT;
                _readPixelsScaledTex.ReadPixels(new Rect(0, 0, _targetWidth, _targetHeight), 0, 0);
                _readPixelsScaledTex.Apply(false);
                RenderTexture.active = prevActive;

                NativeArray<byte> rawData = _readPixelsScaledTex.GetRawTextureData<byte>();
                byte[] jpegBytes = EncodeToJpeg(rawData);
                if (jpegBytes != null)
                    OnFrameEncoded?.Invoke(jpegBytes);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JahroVideo] Fallback capture error: {ex.Message}");
                _consecutiveErrors++;
            }
        }

        /// <summary>
        /// Returns true when the device cannot produce valid frames via
        /// ScreenCapture.CaptureScreenshotIntoRenderTexture + AsyncGPUReadback,
        /// requiring the synchronous Texture2D.ReadPixels fallback instead.
        ///
        /// Currently this is limited to the Android Emulator, which runs an
        /// OpenGL ES software translator (OpenGL ES Translator, SwiftShader)
        /// that silently writes nothing to the capture RT.
        ///
        /// All other platforms (real Android devices, iOS, iOS Simulator,
        /// Windows, macOS, Linux) use the async path and are explicitly excluded.
        /// </summary>
        private static bool ShouldUseReadPixelsFallback()
        {
#if UNITY_ANDROID
            string gpu = SystemInfo.graphicsDeviceName ?? string.Empty;
            return gpu.IndexOf("Emulator", StringComparison.OrdinalIgnoreCase) >= 0
                || gpu.IndexOf("Translator", StringComparison.OrdinalIgnoreCase) >= 0
                || gpu.IndexOf("SwiftShader", StringComparison.OrdinalIgnoreCase) >= 0;
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns true when the synchronous ReadPixels path needs a Y-flip blit
        /// before encoding, because Texture2D.ReadPixels from the default
        /// framebuffer returns bottom-up pixel data on this platform.
        ///
        /// Platform breakdown:
        ///   Android Emulator (OpenGL ES Translator / SwiftShader):
        ///     The emulated default framebuffer is presented in the host
        ///     platform's top-down convention (Metal on macOS, D3D on Windows).
        ///     ReadPixels returns row 0 = top of screen → no flip needed.
        ///
        ///   Real Android OpenGL ES hardware:
        ///     ReadPixels returns row 0 = bottom of screen → flip would be needed.
        ///     However, this synchronous path is never taken on real hardware
        ///     (AsyncGPU works there), so the value here is not exercised.
        ///
        ///   Direct3D / Metal (iOS, iOS Simulator, macOS, Windows):
        ///     Framebuffer is top-down → ReadPixels returns top-down → no flip.
        ///
        ///   OpenGL / Vulkan (Linux, other non-Android):
        ///     graphicsUVStartsAtTop == false → flip needed.
        ///     In practice ShouldUseReadPixelsFallback() returns false here,
        ///     so this path is never taken.
        /// </summary>
        private static bool NeedsReadPixelsYFlip()
        {
#if UNITY_ANDROID
            return false;
#else
            return !SystemInfo.graphicsUVStartsAtTop;
#endif
        }

        #endregion

        #region JPEG Encoding

        private byte[] EncodeToJpeg(NativeArray<byte> rawData)
        {
            NativeArray<byte> jpegNative = ImageConversion.EncodeNativeArrayToJPG(
                rawData,
                GraphicsFormat.R8G8B8A8_UNorm,
                (uint)_targetWidth,
                (uint)_targetHeight,
                0,
                _config.jpegQuality);

            byte[] jpegBytes = jpegNative.ToArray();
            jpegNative.Dispose();

            if (jpegBytes == null || jpegBytes.Length == 0)
            {
                Debug.LogWarning("[JahroVideo] JPEG encoding produced empty result");
                return null;
            }

            return jpegBytes;
        }

        #endregion

        #region Blit

        /// <summary>
        /// Fits the entire source inside target, preserving aspect ratio.
        /// Black bars fill any remaining space (letterbox/pillarbox).
        /// When aspects match, does a single direct blit with no temp RT.
        ///
        /// Y-flip (scale.y = -1) is always applied here because
        /// ScreenCapture.CaptureScreenshotIntoRenderTexture writes the screen
        /// into the RT using the raw GPU render-target convention, which is
        /// vertically inverted relative to what AsyncGPUReadback returns as
        /// "row 0". Flipping during the blit ensures that after readback,
        /// byte[0] corresponds to the top row of the visible image on every
        /// platform (OpenGL, OpenGL ES, Metal, Direct3D, Vulkan).
        /// </summary>
        private static void BlitFit(RenderTexture source, RenderTexture target)
        {
            float srcAspect = (float)source.width / source.height;
            float tgtAspect = (float)target.width / target.height;

            int fitW, fitH;
            if (srcAspect > tgtAspect)
            {
                fitW = target.width;
                fitH = Mathf.Max(2, Mathf.RoundToInt(target.width / srcAspect));
            }
            else
            {
                fitH = target.height;
                fitW = Mathf.Max(2, Mathf.RoundToInt(target.height * srcAspect));
            }

            fitW = (fitW / 2) * 2;
            fitH = (fitH / 2) * 2;

            if (fitW >= target.width && fitH >= target.height)
            {
                Graphics.Blit(source, target, new Vector2(1f, -1f), new Vector2(0f, 1f));
                return;
            }

            var prev = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = prev;

            var temp = RenderTexture.GetTemporary(fitW, fitH, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, temp, new Vector2(1f, -1f), new Vector2(0f, 1f));

            int offsetX = (target.width - fitW) / 2;
            int offsetY = (target.height - fitH) / 2;
            Graphics.CopyTexture(temp, 0, 0, 0, 0, fitW, fitH, target, 0, 0, offsetX, offsetY);

            RenderTexture.ReleaseTemporary(temp);
        }

        #endregion

        #region Resolution & Resources

        private void CalculateTargetResolution()
        {
            int screenW = Screen.width;
            int screenH = Screen.height;

            if (screenW <= 0 || screenH <= 0)
            {
                screenW = 1280;
                screenH = 720;
            }

            int maxDim = Mathf.Max(screenW, screenH);
            float scale = Mathf.Min((float)_config.maxResolutionSize / maxDim, 1f);

            _targetWidth = Mathf.Max(2, Mathf.RoundToInt(screenW * scale));
            _targetHeight = Mathf.Max(2, Mathf.RoundToInt(screenH * scale));

            _targetWidth = (_targetWidth / 2) * 2;
            _targetHeight = (_targetHeight / 2) * 2;
        }

        private bool AllocateResources()
        {
            try
            {
                int screenW = Screen.width;
                int screenH = Screen.height;
                if (screenW <= 0 || screenH <= 0)
                {
                    Debug.LogError("[JahroVideo] Cannot allocate resources: invalid screen dimensions");
                    return false;
                }

                Debug.Log($"[JahroVideo] GPU info — device={SystemInfo.graphicsDeviceName}, type={SystemInfo.graphicsDeviceType}, asyncReadback={SystemInfo.supportsAsyncGPUReadback}, screen={screenW}x{screenH}");

                _fullResRT = new RenderTexture(screenW, screenH, 0, RenderTextureFormat.ARGB32);
                _fullResRT.Create();
                Debug.Log($"[JahroVideo] Full-res RT allocated — size={screenW}x{screenH}, created={_fullResRT.IsCreated()}");

                _targetRT = new RenderTexture(_targetWidth, _targetHeight, 0, RenderTextureFormat.ARGB32);
                _targetRT.Create();
                Debug.Log($"[JahroVideo] Target RT allocated — size={_targetWidth}x{_targetHeight}, created={_targetRT.IsCreated()}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JahroVideo] Failed to allocate resources: {ex.Message}");
                ReleaseResources();
                return false;
            }
        }

        private void ReleaseResources()
        {
            if (_fullResRT != null)
            {
                _fullResRT.Release();
                UnityEngine.Object.Destroy(_fullResRT);
                _fullResRT = null;
            }
            if (_targetRT != null)
            {
                _targetRT.Release();
                UnityEngine.Object.Destroy(_targetRT);
                _targetRT = null;
            }
            if (_readPixelsTex != null)
            {
                UnityEngine.Object.Destroy(_readPixelsTex);
                _readPixelsTex = null;
            }
            if (_readPixelsScaledTex != null)
            {
                UnityEngine.Object.Destroy(_readPixelsScaledTex);
                _readPixelsScaledTex = null;
            }
        }

        #endregion

        public void Dispose()
        {
            ReleaseResources();
        }
    }
}
