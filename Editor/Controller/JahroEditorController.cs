using System;
using System.Threading.Tasks;
using JahroConsole.Core.Data;
using JahroConsole.Core.Context;
using UnityEditor;
using UnityEngine;
using JahroConsole.Core.Network;

namespace JahroConsole.Editor
{
    public class JahroEditorController
    {
        public enum EditorState
        {
            Loading,
            Welcome,
            Settings,
            Error
        }

        public event Action<EditorState> OnStateChanged;
        public event Action<string> OnLoadingError;
        public event Action<VersionInfo> OnVersionChecked;
        public event Action<KeyValidator.ValidateKeyResponse> OnApiKeyValidated;
        public event Action OnApiKeyReset;
        public event Action<bool, string> OnValidationComplete;

        private EditorState _currentState = EditorState.Loading;
        private JahroProjectSettings _projectSettings;
        private KeyValidator.ValidateKeyResponse _apiKeyValidation;
        private VersionInfo _versionInfo;

        public EditorState CurrentState => _currentState;
        public JahroProjectSettings ProjectSettings => _projectSettings;
        public KeyValidator.ValidateKeyResponse ApiKeyValidation => _apiKeyValidation;
        public VersionInfo VersionInfo => _versionInfo;

        public async Task InitializeAsync()
        {
            _projectSettings = JahroProjectSettings.Load();
            _projectSettings.OnSettingsChanged += SaveProjectSettings;

            SetState(EditorState.Loading);
            await PerformInitializationAsync();
        }

        public void Cleanup()
        {
            if (_projectSettings != null)
            {
                _projectSettings.OnSettingsChanged -= SaveProjectSettings;
            }
        }

        public void ResetApiKey()
        {
            if (_projectSettings != null)
            {
                _projectSettings.APIKey = string.Empty;
                _apiKeyValidation = null;
                OnApiKeyReset?.Invoke();
                SetState(EditorState.Welcome);
            }
        }

        public async Task CheckAndValidateExistingApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                SetState(EditorState.Welcome);
                OnValidationComplete?.Invoke(false, $"API key is required");
                return;
            }

            try
            {
                await KeyValidator.EditorSend(apiKey,
                    (response) => HandleApiKeyValidation(response),
                    (error) =>
                    {
                        SetState(EditorState.Welcome);
                        OnValidationComplete?.Invoke(false, $"API key error: {error.message}");
                    }
                );
            }
            catch (Exception ex)
            {
                SetState(EditorState.Welcome);
                OnLoadingError?.Invoke(ex.Message);
            }
        }

        private async Task PerformInitializationAsync()
        {
            try
            {
                await CheckVersionAsync();
                await CheckAndValidateExistingApiKeyAsync(_projectSettings?.APIKey);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                OnLoadingError?.Invoke(ex.Message);
            }
        }

        private async Task CheckVersionAsync()
        {
            try
            {
                await VersionChecker.EditorSend(JahroEditorView.isFreshInstall,
                    (response) =>
                    {
                        _versionInfo = response;
                        OnVersionChecked?.Invoke(response);
                    },
                    (error) =>
                    {
                        _versionInfo = null;
                        OnLoadingError?.Invoke(error.message);
                    }
                );
            }
            catch (Exception ex)
            {
                OnLoadingError?.Invoke(ex.Message);
            }
        }

        private void HandleApiKeyValidation(KeyValidator.ValidateKeyResponse response)
        {
            if (response != null && response.success)
            {
                _apiKeyValidation = response;
                _projectSettings.APIKey = response.apiKey;
                SetState(EditorState.Settings);
                OnApiKeyValidated?.Invoke(response);
                OnValidationComplete?.Invoke(true, null);
            }
            else
            {
                string errorMessage = response?.message ?? "Invalid API key";
                OnLoadingError?.Invoke(errorMessage);
                OnValidationComplete?.Invoke(false, errorMessage);
            }
        }

        private void SetState(EditorState newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                OnStateChanged?.Invoke(newState);
            }
        }

        private void SaveProjectSettings()
        {
            if (_projectSettings != null)
            {
                EditorUtility.SetDirty(_projectSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
