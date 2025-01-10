using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class SearchInputBehaviour : MonoBehaviour
    {

        [SerializeField]
        private GameObject _cancelButton;

        private InputField _inputField;

        void Awake()
        {
            _inputField = GetComponentInChildren<InputField>();
            _cancelButton.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _inputField.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _inputField.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _cancelButton.gameObject.SetActive(false);
            }
            else
            {
                _cancelButton.gameObject.SetActive(true);
            }
        }

        public void CancelInput()
        {
            _inputField.text = "";
        }
    }
}