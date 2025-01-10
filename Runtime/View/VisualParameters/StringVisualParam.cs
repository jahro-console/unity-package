using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class StringVisualParam : BaseVisualParam
    {

        private InputField _inputField;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
            _inputField = GetComponentInChildren<InputField>();

            if (defaultParam == null)
            {
                _inputField.text = string.Empty;
            }
            else
            {
                _inputField.text = (string)defaultParam;
            }
            // _inputField.textComponent.UpdateMeshPadding();
            _inputField.onSubmit.AddListener(OnSubmit);
        }

        public override object GetResult()
        {
            return _inputField.text;
        }

        protected override void OnSelect()
        {
            base.OnSelect();
            _inputField.Select();
        }

        private void OnSubmit(string value)
        {
            OnParamSubmitted?.Invoke();
        }
    }
}