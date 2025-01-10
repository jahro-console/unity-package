using System;
using System.Collections;
using Jahro.Core.Data;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Jahro;
using Jahro.Core.Context;

namespace Jahro.View
{
    public class ConsoleMainWindow : MonoBehaviour
    {
        public const float MIN_WIDTH = 420;

        public const float MIN_HEIGHT = 250;

        private readonly float TIGHT_MODE_WIDTH = 600;

        private readonly float DELTA_TO_FULLSCREEN = Screen.width * 0.01f;

        public RectTransform HeaderPanelTransform;

        public Canvas RootCanvas;// { get; private set; }

        public Canvas JahroCanvas;

        public float FrameRightOffset;

        public RectTransform WindowTransform { get; private set; }

        [SerializeField]
        internal HeaderPanelBehaviour HeaderPanelBehaviour { get; private set; }

        [SerializeField]
        internal BottomPanelBehaviour BottomPanelBehaviour { get; private set; }

        internal CanvasScalingBehaviour ScalingBehaviour { get; private set; }

        internal bool Fullscreen { get; private set; }

        public bool IsMobileMode { get { return Application.isMobilePlatform; } }

        // public bool IsMobileMode { get { return true;}}

        private RectTransform _canvasTransform;

        private RectTransform _rootCanvasTransform;

        private ConsoleBaseView[] _views;

        private ConsoleLoadingView _loadingView;

        private ConsoleBaseView _activeView;

        private int _lastScreenWidth;

        private int _lastScreenHeight;

        private Vector3[] _cornetsArray = new Vector3[4];

        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);

        public Mode CurrentMode { get; private set; }

        public bool IsTightMode = false;

        internal Action OnMainWindowOpen;

        internal Action OnMainWindowClose;

        public Action OnWindowSizeChanged;

        public Action<bool> OnTightModeChanged;

        public Action<Rect, float> OnSafeAreaChanged;

        public enum Mode
        {
            Text,
            Visual,
            Watcher,
            Snapshots,
            Account,
            Settings
        }

        private void Awake()
        {
            WindowTransform = GetComponent<RectTransform>();
            // Canvas = GetComponentInParent<Canvas>();
            _canvasTransform = JahroCanvas.GetComponent<RectTransform>();
            _rootCanvasTransform = RootCanvas.GetComponent<RectTransform>();
            ScalingBehaviour = GetComponent<CanvasScalingBehaviour>();
            ScalingBehaviour.OnScaleChanged += OnScaleChanged;

            HeaderPanelBehaviour = GetComponentInChildren<HeaderPanelBehaviour>();
            BottomPanelBehaviour = GetComponentInChildren<BottomPanelBehaviour>();

            ConsoleMainWindow.Mode defaultMode = Mode.Text;

            _views = GetComponentsInChildren<ConsoleBaseView>(true);
            foreach (var view in _views)
            {
                view.InitView(this);
                if (view is ConsoleLoadingView)
                {
                    _loadingView = view as ConsoleLoadingView;
                }
                view.Deactivate();
            }
            _activeView = PickView(defaultMode);
            ShowLoading();
            CurrentMode = defaultMode;
        }

        private void Start()
        {
            var mobileMenu = GetComponentInChildren<MobileMenu>(true);
            mobileMenu.Init(this);
            HeaderPanelBehaviour.Init(this, mobileMenu);
            BottomPanelBehaviour.Init(this);

            ConsoleStorageController.Instance.OnStorageSave += OnStateSave;

            LoadState(ConsoleStorageController.Instance.ConsoleStorage);

            if (IsMobileMode) SetFullscreenMode();
        }

        internal void InitContext(JahroContext context)
        {
            HeaderPanelBehaviour.InitContext(context);
            BottomPanelBehaviour.InitContext(context);
            foreach (var view in _views)
            {
                view.InitContext(context);
            }
            if (context.ApiKeyVerified)
                HideLoading();
            else
                _loadingView.EnterErrorState(true);
        }

        private void Update()
        {
            if (WindowTransform.rect.width < TIGHT_MODE_WIDTH)
            {
                if (!IsTightMode) SetTightMode(true);
            }
            else
            {
                if (IsTightMode) SetTightMode(false);
            }

            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
            {
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;

                if (Fullscreen)
                {
                    SetFullscreenMode();
                }
                OnWindowSizeChanged?.Invoke();
            }

            var safeArea = Screen.safeArea;
            if (safeArea != _lastSafeArea)
            {
                _lastSafeArea = safeArea;
                OnSafeAreaChanged?.Invoke(safeArea, RootCanvas.scaleFactor);
            }
        }

        internal void SetFullscreenMode()
        {
            Fullscreen = true;

            ResizeWindowToCanvas();
            WindowRectChanged(WindowTransform.rect);
        }

        private void OnScaleChanged(float scale)
        {
            AdaptToParentCanvas();
            if (Fullscreen)
            {
                ResizeWindowToCanvas();
            }
        }

        private void SetTightMode(bool enabled)
        {
            if (IsTightMode == enabled) return;
            IsTightMode = enabled;
            foreach (var view in _views)
            {
                view.SetTightMode(enabled);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)HeaderPanelBehaviour.transform);

            OnTightModeChanged?.Invoke(IsTightMode);
        }

        internal void WindowRectChanged(Rect rect)
        {
            Fullscreen = IsCloseToFullscreen();
            foreach (var view in _views)
            {
                view.OnWindowRectChanged(rect);
            }
        }

        internal void WindowPositionChanged(Vector2 anchoredPosition)
        {
            Fullscreen = IsCloseToFullscreen();
        }

        internal void Show()
        {
            gameObject.SetActive(true);
            OnMainWindowOpen?.Invoke();
            ConsoleEvents.Instance.ConsoleOpen(CurrentMode);
        }

        internal void Close()
        {
            gameObject.SetActive(false);
            OnMainWindowClose?.Invoke();
            ConsoleEvents.Instance.ConsoleClose();
        }

        internal bool IsOpen()
        {
            return gameObject.activeInHierarchy;
        }

        internal void ShowLoading()
        {
            _activeView.Deactivate();
            _loadingView.Activate();
        }
        internal void HideLoading()
        {
            _loadingView.Deactivate();
            _activeView.Activate();
        }

        internal void SwitchToMode(Mode mode)
        {
            CurrentMode = mode;
            HeaderPanelBehaviour.UpdateToggleStates();
            OnActiveViewChanged();
        }

        internal void OpenOptionsMenu()
        {
            if (_activeView.OptionsView == null)
            {
                return;
            }

            if (_activeView.OptionsView.IsOpen())
                _activeView.CloseOptions();
            else
                _activeView.ShowOptions();
        }

        internal void ForceCloseOptions()
        {
            _activeView.CloseOptions();
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.GeneralSettings.WindowAnchoredPosition = WindowTransform.anchoredPosition;
            storage.GeneralSettings.WindowSize = WindowTransform.rect.size;
            storage.GeneralSettings.Fullscreen = Fullscreen;
            storage.GeneralSettings.Mode = CurrentMode.ToString();
            storage.GeneralSettings.scaleMode = ScalingBehaviour.GetCurrentMode();
            foreach (var view in _views)
            {
                view.OnStateSave(storage);
            }
        }

        private void LoadState(ConsoleStorage storage)
        {
            Fullscreen = storage.GeneralSettings.Fullscreen;
            if (Fullscreen)
            {
                ResizeWindowToCanvas();
            }
            else
            {
                var size = storage.GeneralSettings.WindowSize;
                WindowTransform.anchoredPosition = storage.GeneralSettings.WindowAnchoredPosition;
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, WindowTransform.anchoredPosition.x, size.x);
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -WindowTransform.anchoredPosition.y, size.y);
            }

            if (string.IsNullOrEmpty(storage.GeneralSettings.Mode) == false)
            {
                SwitchToMode((Mode)Enum.Parse(typeof(Mode), storage.GeneralSettings.Mode));
            }
            else
            {
                SwitchToMode(Mode.Text);
            }

            ScalingBehaviour.SwitchToMode((CanvasScalingBehaviour.ScaleMode)storage.GeneralSettings.scaleMode);

            foreach (var view in _views)
            {
                view.OnStateLoad(storage);
            }
        }

        private void OnActiveViewChanged()
        {
            var previoutActiveView = _activeView;
            _activeView = PickView(CurrentMode);
            if (_loadingView.isActiveAndEnabled)
            {
                return;
            }
            if (previoutActiveView != _activeView)
            {
                if (previoutActiveView != null)
                    previoutActiveView.Deactivate();
                _activeView.Activate();
                ConsoleEvents.Instance.ModeChanged(CurrentMode);
            }
        }

        private bool IsCloseToFullscreen()
        {
            Vector3 left, top, right, bottom;
            _canvasTransform.GetWorldCorners(_cornetsArray);
            left = _cornetsArray[0];
            top = _cornetsArray[1];
            right = _cornetsArray[2];
            bottom = _cornetsArray[3];

            WindowTransform.GetWorldCorners(_cornetsArray);
            left -= _cornetsArray[0];
            top -= _cornetsArray[1];
            right -= _cornetsArray[2];
            bottom -= _cornetsArray[3];

            return (left.magnitude < DELTA_TO_FULLSCREEN && top.magnitude < DELTA_TO_FULLSCREEN
                && right.magnitude < DELTA_TO_FULLSCREEN && bottom.magnitude < DELTA_TO_FULLSCREEN);
        }

        private void ResizeWindowToCanvas()
        {
            WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, _canvasTransform.rect.width);
            WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _canvasTransform.rect.height);
        }

        private void AdaptToParentCanvas()
        {
            if (RootCanvas != JahroCanvas)
            {
                _canvasTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, _rootCanvasTransform.rect.width * FrameRightOffset);
                _canvasTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _rootCanvasTransform.rect.height);
            }
        }

        private ConsoleBaseView PickView(Mode mode)
        {
            ConsoleBaseView pickedView = _views[0];
            foreach (var view in _views)
            {
                if (mode == Mode.Text && view is ConsoleTextView)
                {
                    pickedView = view;
                }
                else if (mode == Mode.Visual && view is ConsoleVisualView)
                {
                    pickedView = view;
                }
                else if (mode == Mode.Watcher && view is ConsoleWatcherView)
                {
                    pickedView = view;
                }
                else if (mode == Mode.Snapshots && view is ConsoleSnapshotsView)
                {
                    pickedView = view;
                }
                else if (mode == Mode.Account && view is ConsoleAccoutView)
                {
                    pickedView = view;
                }
                else if (mode == Mode.Settings && view is ConsoleSettingsView)
                {
                    pickedView = view;
                }
            }
            return pickedView;
        }
    }
}