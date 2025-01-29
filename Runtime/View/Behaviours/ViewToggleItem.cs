using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class ViewToggleItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Image;

        public Text Text;

        public GameObject[] WithTightModeDisable;

        private Color ActiveColor = new Color(0.949f, 0.294f, 0.086f);

        private Color DeactiveColor = new Color(0.616f, 0.616f, 0.616f);

        private Toggle _toggle;

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(onValueChanged);
            onValueChanged(_toggle.isOn);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Text != null) Text.color = _toggle.colors.highlightedColor;
            if (Image != null) Image.color = _toggle.colors.highlightedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Text != null) Text.color = _toggle.isOn ? _toggle.colors.selectedColor : _toggle.colors.normalColor;
            if (Image != null) Image.color = _toggle.isOn ? _toggle.colors.selectedColor : _toggle.colors.normalColor;
        }

        internal void SetTightMode(bool enabled)
        {
            if (WithTightModeDisable != null)
            {
                foreach (var item in WithTightModeDisable)
                {
                    item.SetActive(!enabled);
                }
            }
        }

        private void onValueChanged(bool active)
        {
            if (active)
            {
                if (Text != null) Text.color = ActiveColor;
                if (Image != null) Image.color = ActiveColor;
            }
            else
            {
                if (Text != null) Text.color = DeactiveColor;
                if (Image != null) Image.color = DeactiveColor;
            }
        }


    }
}