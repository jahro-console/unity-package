using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Jahro.Core;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class ConsoleLoadingView : ConsoleBaseView
    {
        [SerializeField]
        private GameObject _errorMessage;

        [SerializeField]
        private GameObject _loadingLabel;

        public void Awake()
        {

        }

        public void Start()
        {
            RefreshSafeArea();

        }

        public void OnVisitJahroClick()
        {
            Application.OpenURL(JahroConfig.RegisterUrl);
        }

        public void EnterErrorState(bool error)
        {
            _loadingLabel.SetActive(!error);
            _errorMessage.SetActive(error);
        }

        protected override void OnActivate()
        {

        }

        protected override void OnDeactivate()
        {

        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);

        }

        protected override void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {
            base.OnSafeAreaChanged(safeArea, scaleFactor);

            RefreshSafeArea();
        }

        private void RefreshSafeArea()
        {
            // int leftPadding = (int)Mathf.Max(SafeArea.x/ScaleFactor, 0);
            // int rightPadding = (int)Mathf.Max((Screen.width - (SafeArea.x + SafeArea.width))/ScaleFactor, 0);
            // if (_contentLayoutGroup != null)
            // {
            //     _contentLayoutGroup.padding = new RectOffset(leftPadding, rightPadding, 0, 0);
            // }
        }
    }
}