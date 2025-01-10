using System;
using System.Collections;
using System.Collections.Generic;
using Jahro;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class HeaderPanelBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject FullscreenButton;

        [SerializeField]
        private GameObject CloseButton;

        [SerializeField]
        private Toggle ConsoleViewToggle;

        [SerializeField]
        private Toggle VisualViewToggle;

        [SerializeField]
        private Toggle WatcherViewToggle;

        [SerializeField]
        private Toggle SnapshotsViewToggle;

        [SerializeField]
        private Toggle AccountViewToggle;

        [SerializeField]
        private Toggle SettingsViewToggle;

        [SerializeField]
        internal LogsIndicatorBehaviour LogsIndicator;

        [SerializeField]
        private GameObject MenuButton;

        [SerializeField]
        private GameObject MobileCloseButton;

        private ConsoleMainWindow _mainWindow;

        private RectTransform _windowTransform;

        private RectTransform _canvasTransform;

        private HorizontalLayoutGroup _layoutGroup;

        private LayoutElement _layoutElement;

        private MobileMenu _mobileMenu;

        private UserAvatar _userAvatar;

        private Vector2 _dragOffset;

        private JahroContext _context;

        private bool _dragging;

        public void Init(ConsoleMainWindow mainWindow, MobileMenu mobileMenu)
        {
            _mainWindow = mainWindow;
            _mobileMenu = mobileMenu;
            _windowTransform = mainWindow.GetComponent<RectTransform>();
            _canvasTransform = mainWindow.JahroCanvas.GetComponent<RectTransform>();
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _layoutElement = GetComponent<LayoutElement>();
            _userAvatar = GetComponentInChildren<UserAvatar>();
            if (_context != null)
            {
                _userAvatar.SetInitials(_context.SelectedUserInfo);
                _mobileMenu.InitContext(_context);
            }

            mainWindow.OnSafeAreaChanged += OnSafeAreaChanged;
            if (mainWindow.IsMobileMode)
            {
                FullscreenButton.SetActive(false);
                CloseButton.SetActive(false);
                SettingsViewToggle.gameObject.SetActive(false);
                MobileCloseButton.gameObject.SetActive(true);
            }
            else
            {
                InitDrag();
                FullscreenButton.SetActive(true);
                SettingsViewToggle.gameObject.SetActive(true);
                MobileCloseButton.gameObject.SetActive(false);
            }

            _mainWindow.OnTightModeChanged += OnTightModeChanged;

            ConsoleViewToggle.onValueChanged.AddListener(OnConsoleTabValueChange);
            VisualViewToggle.onValueChanged.AddListener(OnVisualTabValueChange);
            WatcherViewToggle.onValueChanged.AddListener(OnWactherTabValueChange);
            SnapshotsViewToggle.onValueChanged.AddListener(OnSnapshotsTabValueChange);
            AccountViewToggle.onValueChanged.AddListener(OnAccountTabValueChange);
            SettingsViewToggle.onValueChanged.AddListener(OnSettingsTabValueChange);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.GetComponent<RectTransform>());
        }

        public void InitContext(JahroContext context)
        {
            _context = context;
            _userAvatar?.SetInitials(context.SelectedUserInfo);
            _mobileMenu?.InitContext(context);
            context.OnSelectedUserInfoChanged += OnSelectedUserInfoChanged;
        }

        private void OnSelectedUserInfoChanged(UserInfo info)
        {
            _userAvatar?.SetInitials(info);
        }

        private void OnTightModeChanged(bool obj)
        {
            MenuButton.SetActive(obj);
        }

        public void OnCloseButtonClick()
        {
            _mainWindow.Close();
        }

        public void OnFullscreenClick()
        {
            _mainWindow.SetFullscreenMode();
        }

        public void OnMobileMenuClick()
        {
            _mobileMenu.Open();
        }

        public void UpdateToggleStates()
        {
            switch (_mainWindow.CurrentMode)
            {
                case ConsoleMainWindow.Mode.Text:
                    ConsoleViewToggle.isOn = true;
                    break;
                case ConsoleMainWindow.Mode.Visual:
                    VisualViewToggle.isOn = true;
                    break;
                case ConsoleMainWindow.Mode.Watcher:
                    WatcherViewToggle.isOn = true;
                    break;
                case ConsoleMainWindow.Mode.Snapshots:
                    SnapshotsViewToggle.isOn = true;
                    break;
                case ConsoleMainWindow.Mode.Account:
                    AccountViewToggle.isOn = true;
                    break;
                case ConsoleMainWindow.Mode.Settings:
                    SettingsViewToggle.isOn = true;
                    break;
            }
        }

        public void OnOptionsClick()
        {
            _mainWindow.OpenOptionsMenu();
        }

        private void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {

            var currentOffset = _layoutGroup.padding;
            int leftPadding = (int)Mathf.Max(safeArea.x / scaleFactor, 14);
            int topPadding = (int)Mathf.Max((Screen.height - (safeArea.y + safeArea.height)) / scaleFactor, 0);
            float heightOffset = 52 + topPadding;

            _layoutGroup.padding = new RectOffset(leftPadding, currentOffset.right, topPadding, currentOffset.bottom);
            _layoutElement.preferredHeight = heightOffset;
        }

        private void InitDrag()
        {
            var headerDragEventTrigger = gameObject.GetComponent<EventTrigger>();
            headerDragEventTrigger.triggers[0].callback.AddListener(OnHeaderPointerDown);
            headerDragEventTrigger.triggers[1].callback.AddListener(OnHeaderPointerDrag);
            headerDragEventTrigger.triggers[2].callback.AddListener(OnHeaderPointerUp);
        }

        private void OnConsoleTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Text);
            }
        }

        private void OnVisualTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Visual);
            }
        }

        private void OnWactherTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Watcher);
            }
        }

        private void OnSnapshotsTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Snapshots);
            }
        }

        private void OnAccountTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Account);
            }
        }

        private void OnSettingsTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Settings);
            }
        }

        private void OnHeaderPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 clickLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_windowTransform, pointerEventData.position, pointerEventData.pressEventCamera, out clickLocalPoint);

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
            _windowTransform.anchoredPosition = dragLocalPoint;
            _mainWindow.WindowPositionChanged(_windowTransform.anchoredPosition);
        }

        private void OnHeaderPointerUp(BaseEventData eventData)
        {
            _dragging = false;
        }

    }
}