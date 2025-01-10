using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jahro.View
{
    internal class Vector2VisualParam : BaseVisualParam
    {
        public ValueDraggerComponent ValueDraggerX;

        public ValueDraggerComponent ValueDraggerY;

        private float _currentValueX;

        private float _currentValueY;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
            
            if (defaultParam == null)
            {
                ValueDraggerX.Value = "0";
                ValueDraggerY.Value = "0";
            }
            else
            {
                var defaultVector = (Vector2)defaultParam;
                ValueDraggerX.Value = defaultVector.x.ToString();
                ValueDraggerY.Value = defaultVector.y.ToString();
            }

            ValueDraggerX.OnDragValueChanged += OnValueChangedX;
            ValueDraggerY.OnDragValueChanged += OnValueChangedY;

            ValueDraggerX.OnDragValueApply += OnApplyValue;
            ValueDraggerY.OnDragValueApply += OnApplyValue;
        }

        public override object GetResult()
        {
            return new Vector2(float.Parse(ValueDraggerX.Value), float.Parse(ValueDraggerY.Value));
        }

        private void OnApplyValue()
        {
            _currentValueX = float.Parse(ValueDraggerX.Value);
            _currentValueY = float.Parse(ValueDraggerY.Value);
        }

        private void OnValueChangedX(float delta)
        {
            ValueDraggerX.Value = (_currentValueX + delta/51f).ToString("0.00");
        }

        private void OnValueChangedY(float delta)
        {
            ValueDraggerY.Value = (_currentValueY + delta/51f).ToString("0.00");
        }
    }
}