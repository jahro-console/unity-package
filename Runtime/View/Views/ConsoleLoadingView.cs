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
    }
}