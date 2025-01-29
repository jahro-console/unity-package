using System;
using UnityEngine;
using JahroPackage.Runtime.View.InputHandling;

namespace JahroConsole.View
{
    internal class KeyboardTracker : MonoBehaviour
    {

        public Action OnTildaPressed = delegate { };

        public Action OnEscPressed = delegate { };

        public Action SwitchToTextMode = delegate { };

        public Action SwitchToVisualMode = delegate { };

        public Action SwitchToWatcherMode = delegate { };

        private KeyCode _launchKeyCode;

        internal void Init(KeyCode launchKeyCode)
        {
            _launchKeyCode = launchKeyCode;
        }

        void Update()
        {
            if (JahroInput.GetKeyDown(_launchKeyCode))
            {
                OnTildaPressed();
            }
            else if (JahroInput.GetKeyDown(KeyCode.Escape) && Application.isMobilePlatform)
            {
                OnEscPressed();
            }
            else if (JahroInput.GetKey(KeyCode.LeftAlt) && JahroInput.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchToTextMode();
            }
            else if (JahroInput.GetKey(KeyCode.LeftAlt) && JahroInput.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchToVisualMode();
            }
            else if (JahroInput.GetKey(KeyCode.LeftAlt) && JahroInput.GetKeyDown(KeyCode.Alpha3))
            {
                SwitchToWatcherMode();
            }
        }

        public static float GetSoftKeyboardHeight()
        {

            if (Application.isEditor)
            {
                return 0;//0.565f; //Default value for editor
            }


#if UNITY_ANDROID
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    View.Call("getWindowVisibleDisplayFrame", rect);
                    return (float)(Screen.height - rect.Call<int>("height")) / Screen.height;
                }
            }
#else
        return (float)TouchScreenKeyboard.area.height / Screen.height;
#endif
        }
    }
}