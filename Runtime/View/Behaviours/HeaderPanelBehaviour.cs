using JahroConsole.Core.Context;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
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

        private MobileMenu _mobileMenu;

        private UserAvatar _userAvatar;

        private JahroContext _context;

        public void Init(ConsoleMainWindow mainWindow, MobileMenu mobileMenu)
        {
            _mainWindow = mainWindow;
            _mobileMenu = mobileMenu;
            _windowTransform = mainWindow.GetComponent<RectTransform>();
            _canvasTransform = mainWindow.JahroCanvas.GetComponent<RectTransform>();
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _userAvatar = GetComponentInChildren<UserAvatar>();
            if (_context != null)
            {
                _userAvatar.SetInitials(_context.SelectedUserInfo);
                _mobileMenu.InitContext(_context);
            }

            if (mainWindow.IsMobileMode)
            {
                FullscreenButton.SetActive(false);
                CloseButton.SetActive(false);
                SettingsViewToggle.gameObject.SetActive(false);
                MobileCloseButton.gameObject.SetActive(true);
                MenuButton.SetActive(true);
            }
            else
            {
                FullscreenButton.SetActive(true);
                SettingsViewToggle.gameObject.SetActive(true);
                MobileCloseButton.gameObject.SetActive(false);
                MenuButton.SetActive(mainWindow.IsTightMode);
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

        private void OnTightModeChanged(bool tightMode)
        {
            if (_mainWindow.IsMobileMode)
            {
                MenuButton.SetActive(true);
            }
            else
            {
                MenuButton.SetActive(tightMode);
            }
            SettingsViewToggle.gameObject.SetActive(!tightMode);
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
    }
}