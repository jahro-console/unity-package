using UnityEngine.UIElements;

namespace JahroConsole.Editor
{
    public class SettingsTabView
    {
        private Button _accountButton;
        private Button _settingsButton;
        private Button _snapshotsSettingsButton;
        private VisualElement _accountContainer;
        private VisualElement _settingsContainer;
        private VisualElement _snapshotsContainer;

        public SettingsTabView(VisualElement root)
        {
            var tabsContainer = root.Q<VisualElement>("TabsControl");

            _accountButton = tabsContainer.Q<Button>("AccountTab");
            _accountButton.clicked += () => SwitchToTab("account");

            _settingsButton = tabsContainer.Q<Button>("SettingTab");
            _settingsButton.clicked += () => SwitchToTab("settings");

            _snapshotsSettingsButton = tabsContainer.Q<Button>("SnapshotsTab");
            _snapshotsSettingsButton.clicked += () => SwitchToTab("snapshots");

            _accountContainer = root.Q<VisualElement>("AccountContainer");
            _settingsContainer = root.Q<VisualElement>("SettingsContainer");
            _snapshotsContainer = root.Q<VisualElement>("SnapshotsContainer");

            SwitchToTab("account");
        }

        public void SwitchToTab(string tabName)
        {

            if (_accountContainer != null && _settingsContainer != null)
            {
                if (tabName == "account")
                {
                    _accountContainer.style.display = DisplayStyle.Flex;
                    _settingsContainer.style.display = DisplayStyle.None;
                    _snapshotsContainer.style.display = DisplayStyle.None;

                    _accountButton.AddToClassList("tab-button-active");
                    _settingsButton.RemoveFromClassList("tab-button-active");
                    _snapshotsSettingsButton.RemoveFromClassList("tab-button-active");
                }
                else if (tabName == "snapshots")
                {
                    _accountContainer.style.display = DisplayStyle.None;
                    _settingsContainer.style.display = DisplayStyle.None;
                    _snapshotsContainer.style.display = DisplayStyle.Flex;

                    _accountButton.RemoveFromClassList("tab-button-active");
                    _settingsButton.RemoveFromClassList("tab-button-active");
                    _snapshotsSettingsButton.AddToClassList("tab-button-active");
                }
                else
                {
                    _accountContainer.style.display = DisplayStyle.None;
                    _settingsContainer.style.display = DisplayStyle.Flex;
                    _snapshotsContainer.style.display = DisplayStyle.None;

                    _accountButton.RemoveFromClassList("tab-button-active");
                    _settingsButton.AddToClassList("tab-button-active");
                    _snapshotsSettingsButton.RemoveFromClassList("tab-button-active");
                }
            }
        }
    }
}