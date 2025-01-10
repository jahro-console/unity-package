using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Jahro.View
{
    internal class IntVisualParam : BaseVisualParam
    {
        protected ValueDraggerComponent _draggerComponent;

        protected float _currentValue = 0;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
            _draggerComponent = GetComponent<ValueDraggerComponent>();
            _draggerComponent.OnDragValueChanged += OnValueChange;
            _draggerComponent.OnDragValueApply += OnApplyValue;
            if (defaultParam == null)
            {
                _draggerComponent.Value = "0";
            }
            else
            {
                _draggerComponent.Value = defaultParam.ToString();
            }
            // _draggerComponent.InputField.textComponent.UpdateMeshPadding();
            _draggerComponent.OnValueSubmitted += (value) => OnParamSubmitted?.Invoke();
        }

        protected override void OnSelect()
        {
            base.OnSelect();
            _draggerComponent.Select();
        }

        public override object GetResult()//convert to object vv
        {
            return int.Parse(_draggerComponent.Value);
        }

        protected virtual float DragValueToNumber(float deltaDrag)
        {
            return deltaDrag / 10f;
        }

        protected virtual void OnApplyValue()
        {
            _currentValue = int.Parse(_draggerComponent.Value);
        }

        protected virtual void OnValueChange(float delta)
        {
            _draggerComponent.Value = Mathf.RoundToInt(_currentValue + delta / 2f).ToString();
        }
    }
}