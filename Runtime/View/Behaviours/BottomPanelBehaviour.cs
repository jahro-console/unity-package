using System;
using System.Collections;
using System.Collections.Generic;
using Jahro;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class BottomPanelBehaviour : MonoBehaviour
    {
        private SizeDragBehaviour _sizeDragBehaviour;

        private LayoutElement _layoutElement;

        private ConsoleMainWindow _mainWindow;

        private StatusBar _statusBar;

        private LayoutGroup _statusBarLayoutGroup;

        void Awake()
        {
            _sizeDragBehaviour = GetComponentInChildren<SizeDragBehaviour>();
            _sizeDragBehaviour.OnWindowRectChanged += OnWindowRectChanged;
            _layoutElement = GetComponent<LayoutElement>();
            _statusBar = GetComponentInChildren<StatusBar>();
            _statusBarLayoutGroup = _statusBar.GetComponent<LayoutGroup>();
        }

        internal void Init(ConsoleMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            if (mainWindow.IsMobileMode)
            {
                _sizeDragBehaviour.gameObject.SetActive(false);
                _statusBar.gameObject.SetActive(false);
                OnWindowRectChanged(mainWindow.WindowTransform.rect);
            }
            else
            {
                _layoutElement.preferredHeight = 30f;
                _sizeDragBehaviour.Init(mainWindow.JahroCanvas.GetComponent<RectTransform>(), mainWindow.WindowTransform);
            }
        }

        public void InitContext(JahroContext context)
        {
            _statusBar.InitContext(context);
        }

        private void OnWindowRectChanged(Rect obj)
        {
            _mainWindow?.WindowRectChanged(obj);
        }

        private void OnCloseButtonClick()
        {
            _mainWindow.Close();
        }
    }
}