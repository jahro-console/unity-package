using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace JahroConsole.View
{
    internal class DialogModalView : MonoBehaviour, IPointerClickHandler
    {

        public RectTransform ModalViewTransform;

        public Text TitleText;

        public Text DescriptionText;

        public Button CloseButton;

        public Button ActionButton;

        private Action _onAction;

        private RectTransform _parentRect;

        private void Awake()
        {
            CloseButton?.onClick.AddListener(OnCloseClick);
            ActionButton?.onClick.AddListener(OnActionClick);
        }

        private void Start()
        {
            // gameObject.SetActive(false);
        }

        public void Open(ConsoleMainWindow mainWindow, RectTransform parentRect, string title, string description, Action onAction)
        {
            _parentRect = parentRect;
            _onAction = onAction;

            TitleText.text = title;
            DescriptionText.text = description;

            gameObject.SetActive(true);
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var clickOnModalView = eventData.pointerPressRaycast.gameObject == this.gameObject;
            if (clickOnModalView)
            {
                Close();
            }
        }

        private void OnCloseClick()
        {
            Close();
        }

        private void OnActionClick()
        {
            _onAction?.Invoke();
            Close();
        }
    }
}