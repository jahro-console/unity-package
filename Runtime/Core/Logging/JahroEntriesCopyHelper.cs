using UnityEngine;

namespace JahroConsole.Core.Logging
{
    internal static class JahroEntriesCopyHelper
    {
#if UNITY_WEBGL	 
        [DllImport("__Internal")]
        private static extern void CopyToClipboardWeb(string textToCopy);
#endif
        internal static void CopyToClipboard(string data)
        {
#if UNITY_WEBGL	 
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                CopyToClipboardWeb(data);
            else
                GUIUtility.systemCopyBuffer = data;
#else
            GUIUtility.systemCopyBuffer = data;
#endif
        }
    }
}