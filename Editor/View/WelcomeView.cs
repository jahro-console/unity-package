using System;
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

        public event Action<string> OnValidationRequested;

        public WelcomeView(VisualElement root)
        {
            _container = root.Q<VisualElement>("WelcomeBlock");
            _keyInput = root.Q<TextField>("key-input");
            _errorLabel = root.Q<Label>("KeyErrorLabel");
            _validateButton = root.Q<Button>("ValidateKey");
            _validateButton.text = "Submit";

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

        private void OnValidateKeyClicked()
        {
            StartLoadingAnimation();
            OnValidationRequested?.Invoke(_keyInput.value.Trim());
        }

        public void OnValidationComplete(bool success, string errorMessage = null)
        {
            StopLoadingAnimation();

            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                ShowError(errorMessage);
            }
        }

        public void ClearInput()
        {
            _keyInput.value = "";
            _errorLabel.style.display = DisplayStyle.None;
        }

        private void StartLoadingAnimation()
        {
            _validateButton.SetEnabled(false);

            _validateButton.text = "Validating...";
        }

        private void StopLoadingAnimation()
        {
            _validateButton.SetEnabled(true);
            _validateButton.text = "Submit";
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

        public void Cleanup()
        {
        }
    }
}