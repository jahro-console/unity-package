using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jahro.View
{
    internal class FloatVisualParam : IntVisualParam
    {

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
        }

        public override object GetResult()
        {
            return float.Parse(_draggerComponent.Value);
        }

        protected override void OnApplyValue()
        {
            _currentValue = float.Parse(_draggerComponent.Value);
        }

        protected override void OnValueChange(float delta)
        {
            _draggerComponent.Value = (_currentValue + delta/51f).ToString("0.00");
        }
    }
}
