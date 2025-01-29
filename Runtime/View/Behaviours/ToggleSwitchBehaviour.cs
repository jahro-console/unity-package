using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitchBehaviour : MonoBehaviour
    {
        private Toggle _toggle;

        [SerializeField]
        private Graphic _graphic;

        [SerializeField]
        private GameObject _whenOn;

        [SerializeField]
        private GameObject _whenOff;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void Start()
        {
            UpdateSelectableColor();
        }

        private void OnValueChanged(bool value)
        {
            UpdateSelectableColor();
        }

        private void UpdateSelectableColor()
        {
            if (_graphic != null)
            {
                _graphic.color = _toggle.isOn ? _toggle.colors.selectedColor : _toggle.colors.normalColor;
            }
            if (_whenOn != null)
            {
                _whenOn.SetActive(_toggle.isOn);
            }
            if (_whenOff != null)
            {
                _whenOff.SetActive(!_toggle.isOn);
            }
        }
    }
}