using System;
using JahroConsole.Core.Data;
using JahroConsole.Core.Context;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JahroConsole.Editor
{
    public class JahroEditorView : EditorWindow
    {
        private enum WindowMode
        {
            LOADING,
            WELCOME,
            SETTINGS
        }

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField]
        private JahroProjectSettings _projectSettings;

        private WelcomeView _welcomeView;
        private SettingsView _settingsView;
        private Button _versionButton;
        private VisualElement _loadingContainer;
        private Label _errorLoadingLabel;
        private KeyValidator.ValidateKeyResponse _apiKeyValidation;
        private Label _versionLabel;
        public static bool isFreshInstall { get; set; }

        [MenuItem("Tools/Jahro Settings")]
        public static void ShowWindow()
        {
            JahroEditorView wnd = GetWindow<JahroEditorView>(false, "Jahro Settings", true);
            wnd.titleContent = new GUIContent("Jahro Settings");
            wnd.minSize = new Vector2(300, 300);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(rootVisualElement);

            _loadingContainer = root.Q<VisualElement>("LoadingBlock");
            _errorLoadingLabel = _loadingContainer.Q<Label>("ErrorLabel");
            _errorLoadingLabel.text = "";
            _welcomeView = new WelcomeView(root);
            _settingsView = new SettingsView(root);
            _versionLabel = root.Q<Label>("VersionMessage");
            _versionButton = root.Q<Button>("VersionButton");
            _versionButton.text = JahroConfig.CurrentVersion;
            _versionButton.clicked += () =>
            {
                Application.OpenURL(JahroConfig.ChangelogUrl);
            };
            var projectPageLink = root.Q<Button>("ProjectPageLink");
            projectPageLink.clicked += () =>
            {
                Application.OpenURL("https://console.jahro.io");
            };

            ConfigureFooterLinks(root.Q<VisualElement>("Footer"));

            _welcomeView.OnValidationSuccess += OnAPIKeyValidated;
            _settingsView.OnResetApiKey += ResetApiKey;

            _projectSettings = JahroProjectSettings.LoadOrCreate();

            SetWindowMode(WindowMode.LOADING);

            CheckVersion();
        }

        private async void CheckVersion()
        {
            try
            {
                await VersionChecker.Send(isFreshInstall,
                    (response) =>
                    {
                        UpdateUI(response);
                        _errorLoadingLabel.text = "";
                    },
                    (error) =>
                    {
                        _errorLoadingLabel.text = "Error checking version: " + error.message;
                    }
                );
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                CheckAndValidateAPIKey();
            }
        }

        private async void CheckAndValidateAPIKey()
        {
            SetWindowMode(WindowMode.LOADING);

            string savedApiKey = _projectSettings.APIKey;

            if (string.IsNullOrEmpty(savedApiKey))
            {
                SetWindowMode(WindowMode.WELCOME);
                return;
            }

            try
            {
                await KeyValidator.Send(savedApiKey, (response) =>
                {
                    _apiKeyValidation = response;
                }, (response) =>
                {
                    _apiKeyValidation = response;
                    _welcomeView.ShowError(response.message);
                });

                if (_apiKeyValidation != null && _apiKeyValidation.success)
                {
                    OnAPIKeyValidated(_apiKeyValidation);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _welcomeView.ShowError("Error connecting to server. Please try again later.");
                SetWindowMode(WindowMode.WELCOME);
            }
        }

        private void OnAPIKeyValidated(KeyValidator.ValidateKeyResponse validation)
        {
            _apiKeyValidation = validation;
            _projectSettings.APIKey = validation.apiKey;
            _settingsView.Initialize(validation, _projectSettings);
            SetWindowMode(WindowMode.SETTINGS);
        }

        private void ResetApiKey()
        {
            _projectSettings.APIKey = string.Empty;
            SetWindowMode(WindowMode.WELCOME);
        }

        private void SetWindowMode(WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.LOADING:
                    _loadingContainer.style.display = DisplayStyle.Flex;
                    _welcomeView.Hide();
                    _settingsView.Hide();
                    break;
                case WindowMode.WELCOME:
                    _loadingContainer.style.display = DisplayStyle.None;
                    _welcomeView.Show();
                    _settingsView.Hide();
                    break;
                case WindowMode.SETTINGS:
                    _loadingContainer.style.display = DisplayStyle.None;
                    _welcomeView.Hide();
                    _settingsView.Show();
                    break;
            }
        }

        private void UpdateUI(VersionChecker.VersionResponse response)
        {
            if (response.updateRequired)
            {
                _versionLabel.style.display = DisplayStyle.Flex;
                _versionLabel.text = "Please update Jahro to the latest version.";
                _versionButton.style.backgroundColor = new Color(1, 0.41f, 0.41f);
            }
            else if (response.updateRecommended)
            {
                _versionLabel.style.display = DisplayStyle.Flex;
                _versionLabel.text = "There is a new version available.";
                _versionButton.style.backgroundColor = new Color(1, 1, 0.41f);
            }
            else
            {
                _versionLabel.style.display = DisplayStyle.None;
                _versionButton.style.backgroundColor = new Color(0.02f, 0.59f, 0.41f);
            }
        }

        private void ConfigureFooterLinks(VisualElement root)
        {
            SetupLinkButton(root, "HomeLink", "https://jahro.io?utm_source=unity-client&utm_medium=settings&utm_content=footer");
            SetupLinkButton(root, "DocsLink", "https://docs.jahro.io/?utm_source=unity-client&utm_medium=settings&utm_content=footer");
            SetupLinkButton(root, "DiscordLink", "https://discord.gg/txcHFRDeV4");
            SetupLinkButton(root, "GitHubLink", "https://github.com/jahro-console/unity-package");
            SetupLinkButton(root, "ReportIssueLink", "https://github.com/jahro-console/unity-package/issues");
        }

        private void SetupLinkButton(VisualElement root, string buttonName, string url)
        {
            var button = root.Q<Button>(buttonName);
            if (button != null)
            {
                button.clicked += () => Application.OpenURL(url);
            }
        }
    }
}