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
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private JahroEditorController _controller;
        private WelcomeView _welcomeView;
        private SettingsView _settingsView;
        private Button _versionButton;
        private VisualElement _loadingContainer;
        private Label _errorLoadingLabel;
        private Label _versionLabel;
        public static bool isFreshInstall { get; set; }

        [MenuItem("Tools/Jahro Settings")]
        public static void ShowWindow()
        {
            JahroEditorView wnd = GetWindow<JahroEditorView>(false, "Jahro Settings", true);
            wnd.titleContent = new GUIContent("Jahro Settings");
            wnd.minSize = new Vector2(300, 300);
            Vector2 size = new Vector2(EditorGUIUtility.GetMainWindowPosition().width, EditorGUIUtility.GetMainWindowPosition().height);
            size = size * 0.5f;
            wnd.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - size * 0.5f, size);
        }

        public async void CreateGUI()
        {
            _controller = new JahroEditorController();
            SetupControllerEvents();

            VisualElement root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(rootVisualElement);

            InitializeUIElements(root);
            SetupViews(root);
            ConfigureFooterLinks(root.Q<VisualElement>("Footer"));

            await _controller.InitializeAsync();
        }

        private void OnDestroy()
        {
            _controller?.Cleanup();
            _welcomeView?.Cleanup();
            _settingsView?.Cleanup();
        }

        private void SetupControllerEvents()
        {
            _controller.OnStateChanged += OnStateChanged;
            _controller.OnLoadingError += OnError;
            _controller.OnVersionChecked += OnVersionChecked;
            _controller.OnApiKeyValidated += OnApiKeyValidated;
            _controller.OnApiKeyReset += OnApiKeyReset;
            _controller.OnValidationComplete += OnValidationComplete;
        }

        private void InitializeUIElements(VisualElement root)
        {
            _loadingContainer = root.Q<VisualElement>("LoadingBlock");
            _errorLoadingLabel = _loadingContainer.Q<Label>("ErrorLabel");
            _errorLoadingLabel.text = "";
            _versionLabel = root.Q<Label>("VersionMessage");
            _versionButton = root.Q<Button>("VersionButton");
            _versionButton.text = JahroConfig.CurrentVersion;
            _versionButton.clicked += () => Application.OpenURL(JahroConfig.ChangelogUrl);

            var projectPageLink = root.Q<Button>("ProjectPageLink");
            projectPageLink.clicked += () => Application.OpenURL("https://console.jahro.io");
        }

        private void SetupViews(VisualElement root)
        {
            _welcomeView = new WelcomeView(root);
            _welcomeView.OnValidationRequested += OnValidationRequested;

            _settingsView = new SettingsView(root);
            _settingsView.OnResetApiKey += OnResetApiKeyRequested;
        }

        private async void OnValidationRequested(string apiKey)
        {
            await _controller.CheckAndValidateExistingApiKeyAsync(apiKey);
        }

        private void OnResetApiKeyRequested()
        {
            _controller.ResetApiKey();
        }

        private void OnStateChanged(JahroEditorController.EditorState state)
        {
            switch (state)
            {
                case JahroEditorController.EditorState.Loading:
                    ShowLoading();
                    break;
                case JahroEditorController.EditorState.Welcome:
                    ShowWelcome();
                    break;
                case JahroEditorController.EditorState.Settings:
                    ShowSettings();
                    break;
                case JahroEditorController.EditorState.Error:
                    ShowError();
                    break;
            }
        }

        private void OnError(string errorMessage)
        {
            _errorLoadingLabel.text = errorMessage;
            _errorLoadingLabel.style.display = DisplayStyle.Flex;
        }

        private void OnVersionChecked(VersionInfo versionInfo)
        {
            UpdateVersionUI(versionInfo);
        }

        private void OnApiKeyValidated(KeyValidator.ValidateKeyResponse validation)
        {
            _settingsView.Initialize(validation, _controller.ProjectSettings);
        }

        private void OnApiKeyReset()
        {
            _welcomeView.ClearInput();
        }

        private void OnValidationComplete(bool success, string errorMessage)
        {
            _welcomeView.OnValidationComplete(success, errorMessage);
        }

        private void ShowLoading()
        {
            _loadingContainer.style.display = DisplayStyle.Flex;
            _welcomeView.Hide();
            _settingsView.Hide();
        }

        private void ShowWelcome()
        {
            _loadingContainer.style.display = DisplayStyle.None;
            _welcomeView.Show();
            _settingsView.Hide();
        }

        private void ShowSettings()
        {
            _loadingContainer.style.display = DisplayStyle.None;
            _welcomeView.Hide();
            _settingsView.Show();
        }

        private void ShowError()
        {
            _loadingContainer.style.display = DisplayStyle.Flex;
            _welcomeView.Hide();
            _settingsView.Hide();
        }

        private void UpdateVersionUI(VersionInfo response)
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
            SetupLinkButton(root, "HomeLink", JahroConfig.HomeUrl);
            SetupLinkButton(root, "DocsLink", JahroConfig.DocumentationRoot);
            SetupLinkButton(root, "DiscordLink", JahroConfig.DiscordUrl);
            SetupLinkButton(root, "GitHubLink", JahroConfig.GitHubUrl);
            SetupLinkButton(root, "ReportIssueLink", JahroConfig.ReportIssueUrl);
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