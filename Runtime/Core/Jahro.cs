using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
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
        /// Event triggered when the Jahro Console is displayed.
        /// </summary>
        public static UnityAction OnConsoleShow;

        /// <summary>
        /// Event triggered when the Jahro Console is hidden.
        /// </summary>
        public static UnityAction OnConsoleHide;

        private static JahroViewManager _viewManager;
        private static bool _isInitialized;

        /// <summary>
        /// Use this method to manually initialize Jahro, if necessary.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void InitializeIfNeeded()
        {
            if (_isInitialized)
            {
                return;
            }
            ///
            LogFileManager.Clear();

            var projectSettings = JahroProjectSettings.LoadOrCreate();
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
            JahroLogger.DuplicateToUnityConsole = projectSettings.DuplicateToUnityConsole;

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

            var projectSettings = JahroProjectSettings.LoadOrCreate();
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
        /// Reveals the Jahro Console's Launch button.
        /// </summary>
        public static void ShowLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't show Status Button");
                return;
            }
            _viewManager.ShowLaunchButton();
        }

        /// <summary>
        /// Conceals the Jahro Console's Launch button.
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
            JahroLogger.Instance.Dispose();
            JahroViewManager.DestroyInstance();
            ConsoleStorageController.Release();
            JahroSession.EndSession();
        }

        private static void OnAssemblyReload()
        {
#if UNITY_EDITOR            
            if (UnityEditor.EditorApplication.isPlaying)
            {
                ConsoleStorageController.SaveState();
                Release();
            }
#endif
        }
    }
}