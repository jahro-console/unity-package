using System;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Network;
using JahroConsole.View;
using UnityEngine;
using UnityEngine.Events;

namespace JahroConsole
{
    /// <summary>
    /// Main API File
    /// </summary>
    public static partial class Jahro
    {
        /// <summary>
        /// Verifies if the Jahro Console window is currently open.
        /// </summary>
        /// <returns></returns>
        public static bool IsOpen { get { return _viewManager.IsWindowOpen(); } }

        /// <summary>
        /// Confirms if the Jahro Console is currently enabled in ProjectSettings.
        /// </summary>
        /// <value></value>
        public static bool Enabled { get; private set; }

        /// <summary>
        /// Confirms if the Jahro Console's Launch button is currently enabled.
        /// </summary>
        /// <value></value>
        public static bool IsLaunchButtonEnabled { get; private set; } = true;

        /// <summary>
        /// Indicates if Jahro has been released and cleaned up.
        /// </summary>
        public static bool IsReleased { get { return _isReleased; } }

        /// <summary>
        /// Event triggered when the Jahro Console is displayed.
        /// </summary>
        public static UnityAction OnConsoleShow;

        /// <summary>
        /// Event triggered when the Jahro Console is hidden.
        /// </summary>
        public static UnityAction OnConsoleHide;

        private static JahroViewManager _viewManager;
        private static bool _isInitialized;
        private static bool _isReleased = false;
        private static readonly object _releaseLock = new object();
        private static GameObject _lifecycleManager;

        /// <summary>
        /// Use this method to manually initialize Jahro, if necessary. The most cases you don't need to call this method.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void InitializeIfNeeded()
        {
            if (_isInitialized)
            {
                return;
            }

            JahroConfig.Init();

            LogFileManager.Clear();

            var projectSettings = JahroProjectSettings.Load();
            if (projectSettings == null) throw new Exception("Jahro Project Settings not found InitializeIfNeeded");

            ConsoleStorageController.InitSettings(projectSettings);
            if (projectSettings.AutoDisableInRelease && !Debug.isDebugBuild)
            {
                Enabled = false;
            }
            else
            {
                Enabled = projectSettings.JahroEnabled;
            }

            if (!Enabled)
            {
                Debug.Log("Jahro Console: Disabled in this build");
                return;
            }

            JahroLogger.Instance.StartCatching();

#if UNITY_EDITOR

            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
#endif
            _isInitialized = true;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void OnRuntimeMethodLoad()
        {
            if (!Enabled)
            {
                return;
            }

            var projectSettings = JahroProjectSettings.Load();
            if (projectSettings == null) throw new Exception("Jahro Project Settings not found Runtime");

            ConsoleStorageController.InitSettings(projectSettings);

            InitView();
            JahroSession.StartNewSession(OnContextLoaded, projectSettings);
        }

        private static void OnContextLoaded(JahroContext context)
        {
            _viewManager.InitContext(context);
        }

        /// <summary>
        /// Initializes the Jahro Console's user interface.
        /// </summary>
        private static void InitView()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled");
                return;
            }
            if (!JahroViewManager.IsViewInstantiated())
            {
                _viewManager = JahroViewManager.InstantiateView();
            }
            _viewManager = JahroViewManager.GetInstance();
            _viewManager.OnStateChanged += (state) =>
            {
                if (state == JahroViewManager.ConsoleViewStates.MainWindowShow)
                {
                    if (OnConsoleShow != null) OnConsoleShow();
                }
                else if (state == JahroViewManager.ConsoleViewStates.MainWindowHide)
                {
                    if (OnConsoleHide != null) OnConsoleHide();
                }
            };

        }

        /// <summary>
        /// Displays the main Jahro Console interface.
        /// </summary>
        public static void ShowConsoleView()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't show console view");
                return;
            }
            _viewManager.ShowWindow();
        }

        /// <summary>
        /// Closes the main Jahro Console interface.
        /// </summary>
        public static void CloseConsoleView()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't hide view");
                return;
            }
            _viewManager.HideWindow();
        }

        /// <summary>
        /// Enables the Jahro Console's Launch button.
        /// </summary>
        public static void EnableLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't enable launch button");
                return;
            }
            IsLaunchButtonEnabled = true;
        }

        /// <summary>
        /// Disables the Jahro Console's Launch button.
        /// </summary>
        public static void DisableLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't disable launch button");
                return;
            }
            IsLaunchButtonEnabled = false;
            if (_viewManager != null && _viewManager.IsOpenButtonVisible())
            {
                _viewManager.HideLaunchButton();
            }
        }

        /// <summary>
        /// Shows the Jahro Console's Launch button in view.
        /// </summary>
        public static void ShowLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't show Status Button");
                return;
            }
            if (!IsLaunchButtonEnabled)
            {
                Debug.LogWarning("Jahro Console Launch button disabled. Can't show Launch button");
                return;
            }
            _viewManager.ShowLaunchButton();
        }

        /// <summary>
        /// Hides the Jahro Console's Launch button in view.
        /// </summary>
        public static void HideLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't hide Status Button");
                return;
            }
            _viewManager.HideLaunchButton();
        }

        /// <summary>
        /// Checks if the Jahro Console's Launch button is visible.
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("This method is deprecated. Use IsLaunchButtonEnabled property instead.")]
        public static bool IsStatusButtonEnabled()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Status button disabled");
                return false;
            }
            return _viewManager.IsOpenButtonVisible();
        }

        /// <summary>
        /// Frees resources and dismantles the Jahro Console instance.
        /// </summary>
        public static void Release()
        {
            lock (_releaseLock)
            {
                if (_isReleased)
                {
                    return;
                }
                _isReleased = true;
            }

            try
            {
                ConsoleStorageController.SaveState();
                JahroLogger.Instance.Dispose();
                JahroViewManager.DestroyInstance();
                ConsoleStorageController.Release();
                JahroSession.EndSession();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void OnAssemblyReload()
        {
#if UNITY_EDITOR            
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Release();
            }
#endif
        }
    }
}