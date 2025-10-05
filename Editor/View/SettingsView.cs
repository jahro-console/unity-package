using UnityEngine;
using UnityEngine.UIElements;
using JahroConsole.Core.Data;
using System;
using UnityEditor.UIElements;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Compilation;
using System.Linq;

namespace JahroConsole.Editor
{
    public class SettingsView
    {
        private VisualElement _container;
        private KeyValidator.ValidateKeyResponse _validationData;
        private JahroProjectSettings _projectSettings;

        private SettingsTabView _tabView;
        private Label _projectNameLabel;
        private Label _teamNameLabel;
        private Label _apiKeyLabel;
        private Button _resetApiKeyButton;
        private Button _projectOverviewButton;
        private Button _teamOverviewButton;
        private Button _accountSettingsButton;
        private Toggle _jahroEnableToggle;
        private Toggle _autoDisableToggle;
        private EnumField _launchKeyField;
        private Toggle _keyboardShortcutsToggle;
        private Toggle _mobileTapAreaToggle;
        private MaskField _assembliesField;
        private List<string> _assembliesNames = new List<string>();
        private int _assembliesFlag;
        private EnumField _snapshotsModeField;

        public event Action OnResetApiKey;

        public SettingsView(VisualElement root)
        {
            _container = root.Q<VisualElement>("SettingsBlock");
            _tabView = new SettingsTabView(_container);
            var tabViewContainer = _container.Q<VisualElement>("TabView");

            var accountTab = tabViewContainer.Q<VisualElement>("AccountContainer");
            _projectNameLabel = accountTab.Q<Label>("ProjectName");
            _teamNameLabel = accountTab.Q<Label>("TeamName");
            _apiKeyLabel = accountTab.Q<VisualElement>("APIKeyContainer").Q<Label>("TeamName");
            _resetApiKeyButton = accountTab.Q<Button>("ResetApiKey");

            var linksContainer = accountTab.Q<VisualElement>("LinksContainer");
            _projectOverviewButton = linksContainer.Q<Button>("ProjectOverviewButton");
            _teamOverviewButton = linksContainer.Q<Button>("TeamOverviewButton");
            _accountSettingsButton = linksContainer.Q<Button>("AccountSettingsButton");

            var settingsTab = tabViewContainer.Q<VisualElement>("SettingsContainer");
            var scrollView = settingsTab.Q<ScrollView>();
            _jahroEnableToggle = scrollView.Q<Toggle>("JahroEnableToggle");
            _autoDisableToggle = scrollView.Q<Toggle>("AutoDisableToggle");
            _keyboardShortcutsToggle = scrollView.Q<Toggle>("KeyboardShortcutsToggle");
            _mobileTapAreaToggle = scrollView.Q<Toggle>("MobileTapAreaToggle");
            _launchKeyField = scrollView.Q<EnumField>("LaunchKeyPicker");

            _assembliesField = tabViewContainer.Q<MaskField>("AssembliesPicker");

            var snapshotsSettingsTab = tabViewContainer.Q<VisualElement>("SnapshotsContainer");
            _snapshotsModeField = snapshotsSettingsTab.Q<EnumField>("SnapshotsModePicker");

            InitializeEnumFields();

            _resetApiKeyButton.clicked += OnResetApiKeyClicked;
        }

        private void LoadAssemblies()
        {
            try
            {
                var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player)
                    .Where(assembly =>
                        !assembly.name.StartsWith("Unity") &&
                        !assembly.name.StartsWith("System") &&
                        !assembly.name.StartsWith("mscorlib") &&
                        !assembly.name.Contains("Editor"))
                    .ToArray();

                _assembliesNames = assemblies
                    .Select(a => a.name)
                    .OrderBy(name => name)
                    .ToList();

                if (_assembliesNames.Count > 31)
                {
                    Debug.LogWarning("More than 31 user assemblies detected. MaskField can only handle 31 options. Some assemblies may not be selectable.");
                    _assembliesNames = _assembliesNames.Take(31).ToList();
                }

                if (_assembliesField != null && _assembliesNames.Count > 0)
                {
                    _assembliesField.choices = _assembliesNames;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading assemblies: {ex.Message}");
            }
        }

        private void UpdateAssemblySelection()
        {
            if (_projectSettings == null || _assembliesNames == null || _assembliesNames.Count == 0 || _assembliesField == null)
                return;

            _assembliesFlag = 0;
            for (int i = 0; i < _assembliesNames.Count; i++)
            {
                if (_projectSettings.ActiveAssemblies.Contains(_assembliesNames[i]))
                {
                    _assembliesFlag |= 1 << i;
                }
            }

            _assembliesField.value = _assembliesFlag;
        }

        private void InitializeEnumFields()
        {
            if (_launchKeyField != null)
            {
                _launchKeyField.Init(KeyCode.Tilde);
                _launchKeyField.tooltip = "Key to press to open Jahro window during play mode";
            }

            if (_snapshotsModeField != null)
            {
                _snapshotsModeField.Init(IProjectSettings.SnapshotMode.StreamingExceptEditor);
                _snapshotsModeField.tooltip = "Mode of snapshots uploading";
            }

            LoadAssemblies();
        }

        public void Initialize(KeyValidator.ValidateKeyResponse validation, JahroProjectSettings projectSettings)
        {
            _validationData = validation;
            _projectSettings = projectSettings;

            if (validation == null) return;

            string apiKey = _projectSettings.APIKey;
            _projectNameLabel.text = validation.projectName;
            _teamNameLabel.text = validation.teamName;

            if (_apiKeyLabel != null && !string.IsNullOrEmpty(apiKey) && apiKey.Length > 6)
            {
                string start = apiKey.Substring(0, 2);
                string end = apiKey.Substring(apiKey.Length - 4);
                _apiKeyLabel.text = $"{start}...{end}";
            }

            SetupExternalLinkButton(_projectOverviewButton, validation.projectUrl);
            SetupExternalLinkButton(_teamOverviewButton, validation.teamOverviewUrl);
            SetupExternalLinkButton(_accountSettingsButton, validation.accountSettingsUrl);

            if (_launchKeyField != null && _projectSettings.LaunchKey != default)
            {
                _launchKeyField.value = _projectSettings.LaunchKey;
            }

            if (_projectSettings.ActiveAssemblies == null)
            {
                _projectSettings.ActiveAssemblies = new List<string>();
                if (_assembliesNames != null && _assembliesNames.Count > 0)
                {
                    _projectSettings.ActiveAssemblies = new List<string>(_assembliesNames);
                }
            }

            UpdateAssemblySelection();

            SetupManualBinding();
        }

        private void SetupManualBinding()
        {
            if (_projectSettings == null) return;

            if (_jahroEnableToggle != null)
            {
                _jahroEnableToggle.value = _projectSettings.JahroEnabled;
                _jahroEnableToggle.RegisterValueChangedCallback(evt =>
                {
                    _projectSettings.JahroEnabled = evt.newValue;
                });
            }

            if (_autoDisableToggle != null)
            {
                _autoDisableToggle.value = _projectSettings.AutoDisableInRelease;
                _autoDisableToggle.RegisterValueChangedCallback(evt =>
                {
                    _projectSettings.AutoDisableInRelease = evt.newValue;
                });
            }

            if (_keyboardShortcutsToggle != null)
            {
                _keyboardShortcutsToggle.value = _projectSettings.UseLaunchKeyboardShortcut;
                _keyboardShortcutsToggle.RegisterValueChangedCallback(evt =>
                {
                    _projectSettings.UseLaunchKeyboardShortcut = evt.newValue;
                });
            }

            if (_mobileTapAreaToggle != null)
            {
                _mobileTapAreaToggle.value = _projectSettings.UseLaunchTapArea;
                _mobileTapAreaToggle.RegisterValueChangedCallback(evt =>
                {
                    _projectSettings.UseLaunchTapArea = evt.newValue;
                });
            }

            if (_launchKeyField != null)
            {
                _launchKeyField.value = _projectSettings.LaunchKey;
                _launchKeyField.RegisterValueChangedCallback(evt =>
                {
                    _projectSettings.LaunchKey = (KeyCode)evt.newValue;
                });
            }

            if (_assembliesField != null)
            {
                _assembliesField.RegisterValueChangedCallback(evt =>
                {
                    if (_projectSettings != null && _assembliesNames != null && _assembliesNames.Count > 0)
                    {
                        _assembliesFlag = (int)evt.newValue;

                        var newActiveAssemblies = new List<string>(_projectSettings.ActiveAssemblies);

                        for (int i = 0; i < _assembliesNames.Count; i++)
                        {
                            string name = _assembliesNames[i];
                            bool active = newActiveAssemblies.Contains(name);
                            int layer = 1 << i;
                            bool selected = (_assembliesFlag & layer) != 0;

                            if (!active && selected)
                            {
                                newActiveAssemblies.Add(name);
                            }
                            else if (active && !selected)
                            {
                                newActiveAssemblies.Remove(name);
                            }
                        }

                        _projectSettings.ActiveAssemblies = newActiveAssemblies;
                    }
                });
            }

            if (_snapshotsModeField != null)
            {
                _snapshotsModeField.value = _projectSettings.SnapshotingMode;
                _snapshotsModeField.RegisterValueChangedCallback(evt =>
                {
                    _projectSettings.SnapshotingMode = (IProjectSettings.SnapshotMode)evt.newValue;
                });
            }
        }

        private void OnResetApiKeyClicked()
        {
            if (UnityEditor.EditorUtility.DisplayDialog(
                "Reset API Key",
                "Are you sure you want to reset your API key? You will need to enter it again.",
                "Reset",
                "Cancel"))
            {
                OnResetApiKey?.Invoke();
            }
        }

        private void SetupExternalLinkButton(Button button, string url)
        {
            if (button != null && !string.IsNullOrEmpty(url))
            {
                button.clicked += () =>
                {
                    Application.OpenURL(url);
                };
            }
        }

        public void Show()
        {
            _container.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            _container.style.display = DisplayStyle.None;
        }


    }
}