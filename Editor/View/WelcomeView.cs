using System;
using System.Threading.Tasks;
using JahroConsole.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace JahroConsole.Editor
{
    public class WelcomeView
    {
        private VisualElement _container;
        private TextField _keyInput;
        private Label _errorLabel;
        private Button _validateButton;
        private string _originalButtonText;
        private int _animationFrame;
        private IVisualElementScheduledItem _loadingAnimation;

        public event Action<KeyValidator.ValidateKeyResponse> OnValidationSuccess;

        public WelcomeView(VisualElement root)
        {
            _container = root.Q<VisualElement>("WelcomeBlock");
            _keyInput = root.Q<TextField>("key-input");
            _errorLabel = root.Q<Label>("KeyErrorLabel");
            _validateButton = root.Q<Button>("ValidateKey");
            _originalButtonText = _validateButton.text;

            _errorLabel.style.display = DisplayStyle.None;

            _validateButton.clicked += OnValidateKeyClicked;

            _keyInput.RegisterValueChangedCallback(evt =>
            {
                _errorLabel.style.display = DisplayStyle.None;
            });

            _keyInput.RegisterCallback<NavigationSubmitEvent>(evt =>
            {
                OnValidateKeyClicked();
            });
        }

        private async void OnValidateKeyClicked()
        {
            StartLoadingAnimation();

            string key = _keyInput.value;
            if (string.IsNullOrEmpty(key))
            {
                ShowError("API Key is required");
                StopLoadingAnimation();
                return;
            }

            KeyValidator.ValidateKeyResponse validation = await ValidateAPIKey(key);
            StopLoadingAnimation();

            if (validation != null && validation.success)
            {
                OnValidationSuccess?.Invoke(validation);
            }
            else if (validation != null && !validation.success)
            {
                ShowError(validation.message);
            }
        }

        private void StartLoadingAnimation()
        {
            _validateButton.SetEnabled(false);
            _animationFrame = 0;

            if (_loadingAnimation != null)
                _loadingAnimation.Pause();

            _loadingAnimation = _validateButton.schedule.Execute(() =>
            {
                _animationFrame = (_animationFrame + 1) % 4;
                string dots = new string('.', _animationFrame);
                _validateButton.text = $"Validating{dots.PadRight(3)}";
            }).Every(300);
        }

        private void StopLoadingAnimation()
        {
            _validateButton.SetEnabled(true);
            _validateButton.text = _originalButtonText;

            if (_loadingAnimation != null)
            {
                _loadingAnimation.Pause();
                _loadingAnimation = null;
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

        public void ShowError(string message)
        {
            _errorLabel.style.display = DisplayStyle.Flex;
            _errorLabel.text = message;
        }

        private async Task<KeyValidator.ValidateKeyResponse> ValidateAPIKey(string key)
        {
            try
            {
                KeyValidator.ValidateKeyResponse result = null;
                await KeyValidator.Send(key,
                    (response) => { result = response; },
                    (response) => { result = response; }
                );
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ShowError(ex.Message);
                return null;
            }
        }
    }
}