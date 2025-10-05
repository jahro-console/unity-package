using System;
using System.Collections;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JahroConsole.View
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

        public bool KeepInWindowBounds { get; set; }

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

        private LayoutGroup _layoutGroup;

        public Mode CurrentMode { get; private set; }

        public bool IsTightMode = false;

        internal Action OnMainWindowOpen;

        internal Action OnMainWindowClose;

        public Action OnWindowSizeChanged;

        public Action<bool> OnTightModeChanged;

        private Vector2 _dragOffset;

        private bool _dragging;

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
            _layoutGroup = GetComponent<LayoutGroup>();

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

            InitDrag();
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

        private void OnDestroy()
        {
            ConsoleStorageController.Instance.OnStorageSave -= OnStateSave;
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

        private void InitDrag()
        {
            var headerDragEventTrigger = gameObject.GetComponent<EventTrigger>();
            headerDragEventTrigger.triggers[0].callback.AddListener(OnHeaderPointerDown);
            headerDragEventTrigger.triggers[1].callback.AddListener(OnHeaderPointerDrag);
            headerDragEventTrigger.triggers[2].callback.AddListener(OnHeaderPointerUp);
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
                StartCoroutine(SafeAreaChanged(safeArea));
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
            ResizeWindowToCanvas();
            CheckScreenBounds();
            WindowRectChanged(WindowTransform.rect);
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

            ForceHeightRecalculationForAllViews();
        }

        private void ForceHeightRecalculationForAllViews()
        {
            if (_activeView != null)
            {
                var scrollView = _activeView.GetComponentInChildren<JahroScrollView>();
                scrollView?.ForceHeightRecalculation();
            }
        }

        internal void TriggerHeightRecalculation()
        {
            // Trigger height recalculation for all views
            ForceHeightRecalculationForAllViews();
        }

        internal void WindowPositionChanged(Vector2 anchoredPosition)
        {
            Fullscreen = IsCloseToFullscreen();
        }

        internal IEnumerator SafeAreaChanged(Rect safeArea)
        {
            yield return new WaitForSeconds(0.1f);
            float scaleFactor = RootCanvas.scaleFactor;
            int leftPadding = (int)Mathf.Max(safeArea.x / scaleFactor, 0);
            int topPadding = (int)Mathf.Max((Screen.height - (safeArea.y + safeArea.height)) / scaleFactor, 0);
            int rightPadding = (int)Mathf.Max((Screen.width - (safeArea.x + safeArea.width)) / scaleFactor, 0);
            int bottomPadding = (int)Mathf.Max(safeArea.y / scaleFactor, 0);
            if (_layoutGroup != null)
            {
                _layoutGroup.padding = new RectOffset(leftPadding, rightPadding, topPadding, bottomPadding);
            }

            if (Fullscreen)
            {
                SetFullscreenMode();
            }
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
            storage.GeneralSettings.keepInWindowBounds = KeepInWindowBounds;
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
            KeepInWindowBounds = storage.GeneralSettings.keepInWindowBounds;

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


        private void OnHeaderPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 clickLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(WindowTransform, pointerEventData.position, pointerEventData.pressEventCamera, out clickLocalPoint);

            _dragOffset.x = clickLocalPoint.x;
            _dragOffset.y = clickLocalPoint.y;

            _dragging = true;
        }

        private void OnHeaderPointerDrag(BaseEventData eventData)
        {
            if (_dragging == false)
            {
                return;
            }

            PointerEventData pointerEventData = (PointerEventData)eventData;

            if ((pointerEventData.pressPosition - pointerEventData.position).magnitude < EventSystem.current.pixelDragThreshold)
            {
                return;
            }

            Vector2 dragLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, pointerEventData.position, pointerEventData.pressEventCamera, out dragLocalPoint);
            dragLocalPoint.x += _canvasTransform.rect.width / 2f - _dragOffset.x;
            dragLocalPoint.y -= _canvasTransform.rect.height / 2f + _dragOffset.y;
            WindowTransform.anchoredPosition = dragLocalPoint;
            WindowPositionChanged(WindowTransform.anchoredPosition);
            CheckScreenBounds();
        }

        private void OnHeaderPointerUp(BaseEventData eventData)
        {
            _dragging = false;
        }

        private void CheckScreenBounds()
        {
            if (KeepInWindowBounds == false)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            WindowTransform.GetWorldCorners(corners);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Vector3 clampedPosition = WindowTransform.position;

            // Check left boundary
            if (corners[0].x < 0)
            {
                clampedPosition.x -= corners[0].x;
            }

            // Check right boundary
            if (corners[2].x > screenWidth)
            {
                clampedPosition.x -= corners[2].x - screenWidth;
            }

            // Check bottom boundary
            if (corners[0].y < 0)
            {
                clampedPosition.y -= corners[0].y;
            }

            // Check top boundary
            if (corners[1].y > screenHeight)
            {
                clampedPosition.y -= corners[1].y - screenHeight;
            }

            WindowTransform.position = clampedPosition;
        }
    }
}