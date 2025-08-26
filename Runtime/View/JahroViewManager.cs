using System.Collections;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Snapshots;
using JahroConsole.View;
using UnityEngine;
using UnityEngine.Events;

namespace JahroConsole
{

    public sealed class JahroViewManager : MonoBehaviour
    {

        private const float WEBGL_SAVES_INTERVAL = 2f;

        public enum ConsoleViewStates
        {
            MainWindowShow,
            MainWindowHide
        }

        public UnityAction<ConsoleViewStates> OnStateChanged;

        private ConsoleOpenButton _launchButton;

        private ConsoleMainWindow _mainWindow;

        private Canvas _canvas;

        private bool _keyboardShortcutEnabled;
        private bool _tapAreaEnabled;
        private KeyCode _launchKeyCode;

        private void Awake()
        {
            _launchButton = GetComponentInChildren<ConsoleOpenButton>(true);
            _launchButton.OnConsoleOpenClick += OnOpenButtonClick;
            _launchButton.OnSnapshotTakeClick += OnScreenshotButtonClick;
            _launchButton.Hide();
            _mainWindow = GetComponentInChildren<ConsoleMainWindow>(true);
            _mainWindow.OnMainWindowOpen += OnMainWindowOpen;
            _mainWindow.OnMainWindowClose += OnMainWindowClose;
            _canvas = GetComponentInChildren<Canvas>(true);

            if (this.transform.parent == null)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }

        private void Start()
        {
            LoadState(ConsoleStorageController.Instance.ConsoleStorage);
            if (_tapAreaEnabled)
            {
                InitTouch();
            }

            InitKeyboard();

            ShowLaunchButton();
#if UNITY_WEBGL

            StartCoroutine(ContinuesSave());
#endif

        }

        internal void InitContext(JahroContext context)
        {
            _mainWindow.InitContext(context);
        }

        internal void SetCanvasSortingOrder(int sortingOrder)
        {
            _canvas.sortingOrder = sortingOrder;
        }

        public void HideWindow()
        {
            _mainWindow.Close();
        }

        public void ShowWindow()
        {
            _mainWindow.Show();
        }

        public bool IsWindowOpen()
        {
            return _mainWindow.IsOpen();
        }

        public void ShowLaunchButton()
        {
            if (!Jahro.IsLaunchButtonEnabled)
            {
                return;
            }
            _launchButton.Show();
        }

        public void HideLaunchButton()
        {
            _launchButton.Hide();
        }

        public bool IsOpenButtonVisible()
        {
            return _launchButton.gameObject.activeSelf;
        }

        public void Release()
        {

        }

        private void OnMainWindowOpen()
        {
            HideLaunchButton();
            JahroSession.RefreshSession();
            if (OnStateChanged != null)
            {
                OnStateChanged(ConsoleViewStates.MainWindowShow);
            }
        }

        private void OnMainWindowClose()
        {
            ShowLaunchButton();
            if (OnStateChanged != null)
            {
                OnStateChanged(ConsoleViewStates.MainWindowHide);
            }
        }

        private void OnOpenButtonClick()
        {
            ShowWindow();
        }

        private void OnScreenshotButtonClick()
        {
            StartCoroutine(SnapshotsManager.Instance.CaptureScreenshot());
        }

        private void InitTouch()
        {
            var tapTracker = gameObject.AddComponent<GestureTrackerTap>();
            tapTracker.OnTapsTracked += ShowWindow;
        }

        private void InitKeyboard()
        {
            var keyboardTracker = gameObject.AddComponent<KeyboardTracker>();
            keyboardTracker.Init(_launchKeyCode);
            keyboardTracker.OnTildaPressed += delegate ()
            {
                if (IsWindowOpen())
                {
                    HideWindow();
                }
                else
                {
                    ShowWindow();
                }
            };
            keyboardTracker.OnEscPressed += delegate ()
            {
                if (IsWindowOpen())
                {
                    HideWindow();
                }
            };

            if (_keyboardShortcutEnabled)
            {
                keyboardTracker.SwitchToTextMode += delegate ()
                {
                    if (IsWindowOpen())
                    {
                        _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Text);
                    }
                };
                keyboardTracker.SwitchToVisualMode += delegate ()
                {
                    if (IsWindowOpen())
                    {
                        _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Visual);
                    }
                };
                keyboardTracker.SwitchToWatcherMode += delegate ()
                {
                    if (IsWindowOpen())
                    {
                        _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Watcher);
                    }
                };
            }
        }

        private IEnumerator ContinuesSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(WEBGL_SAVES_INTERVAL);
                ConsoleStorageController.SaveState();
            }
        }

        private void OnApplicationQuit()
        {
            ConsoleStorageController.SaveState();
            JahroSession.EndSession();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                ConsoleStorageController.SaveState();
            }
            JahroSession.PauseSession(pause);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ConsoleStorageController.SaveState();
            }
            JahroSession.PauseSession(!hasFocus);
        }

        private void LoadState(ConsoleStorage storage)
        {
            _keyboardShortcutEnabled = storage.ProjectSettings.UseLaunchKeyboardShortcut;
            _tapAreaEnabled = storage.ProjectSettings.UseLaunchTapArea;
            _launchKeyCode = storage.ProjectSettings.LaunchKey;
        }

        public static void DestroyInstance()
        {
            if (IsViewInstantiated() == false)
            {
                return;
            }

            var viewManager = GetInstance();
            viewManager.Release();
            GameObject.DestroyImmediate(viewManager.gameObject);
        }

        public static bool IsViewInstantiated()
        {
            return GetInstance() != null;
        }

        public static JahroViewManager GetInstance()
        {
            return GameObject.FindFirstObjectByType<JahroViewManager>();
        }

        internal static JahroViewManager InstantiateView()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/JahroConsole");
            var consoleObject = GameObject.Instantiate(prefab);
            var viewManager = consoleObject.GetComponent<JahroViewManager>();

            var sceneCanvases = Canvas.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int maxSortingOrder = 0;
            foreach (var canvas in sceneCanvases)
            {
                if (canvas.sortingOrder > maxSortingOrder)
                {
                    maxSortingOrder = canvas.sortingOrder;
                }
            }

            viewManager.SetCanvasSortingOrder(maxSortingOrder + 1);
            viewManager.HideWindow();
            return viewManager;
        }
    }
}