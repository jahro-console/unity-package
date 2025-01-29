using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JahroConsole.View
{
    internal class BaseOptionsView : MonoBehaviour, IPointerClickHandler
    {

        public RectTransform PanelTransform;

        public event Action OnOptionsShow;

        public event Action OnOptionsClose;

        private void Awake()
        {
            Init();
            gameObject.SetActive(false);
        }

        protected virtual void Init()
        {

        }

        protected virtual void OnShow()
        {
            OnOptionsShow?.Invoke();
        }

        protected virtual void OnClose()
        {
            OnOptionsClose?.Invoke();
        }

        internal virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        internal virtual void Close()
        {
            gameObject.SetActive(false);
            OnClose();
        }

        internal bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var click = eventData.pointerPressRaycast.gameObject == this.gameObject;
            if (click)
            {
                Close();
            }
        }

        public void OnCloseButtonClick()
        {
            Close();
        }

        public void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {
            float preferedWidth = (safeArea.width - 12) / scaleFactor;
            PanelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(preferedWidth, 350));

        }
    }
}