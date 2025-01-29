using UnityEngine;

namespace JahroConsole.View
{
    internal class Vector3VisualParam : BaseVisualParam
    {
        public ValueDraggerComponent ValueDraggerX;

        public ValueDraggerComponent ValueDraggerY;

        public ValueDraggerComponent ValueDraggerZ;

        private float _currentValueX;

        private float _currentValueY;

        private float _currentValueZ;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);

            if (defaultParam == null)
            {
                ValueDraggerX.Value = "0";
                ValueDraggerY.Value = "0";
                ValueDraggerZ.Value = "0";
            }
            else
            {
                var defaultVector = (Vector3)defaultParam;
                ValueDraggerX.Value = defaultVector.x.ToString();
                ValueDraggerY.Value = defaultVector.y.ToString();
                ValueDraggerZ.Value = defaultVector.z.ToString();
            }

            ValueDraggerX.OnDragValueChanged += OnValueChangedX;
            ValueDraggerY.OnDragValueChanged += OnValueChangedY;
            ValueDraggerZ.OnDragValueChanged += OnValueChangedZ;

            ValueDraggerX.OnDragValueApply += OnApplyValue;
            ValueDraggerY.OnDragValueApply += OnApplyValue;
            ValueDraggerZ.OnDragValueApply += OnApplyValue;
        }

        public override object GetResult()
        {
            return new Vector3(float.Parse(ValueDraggerX.Value), float.Parse(ValueDraggerY.Value), float.Parse(ValueDraggerZ.Value));
        }

        private void OnApplyValue()
        {
            _currentValueX = float.Parse(ValueDraggerX.Value);
            _currentValueY = float.Parse(ValueDraggerY.Value);
            _currentValueZ = float.Parse(ValueDraggerZ.Value);
        }

        private void OnValueChangedX(float delta)
        {
            ValueDraggerX.Value = (_currentValueX + delta / 51f).ToString("0.00");
        }

        private void OnValueChangedY(float delta)
        {
            ValueDraggerY.Value = (_currentValueY + delta / 51f).ToString("0.00");
        }

        private void OnValueChangedZ(float delta)
        {
            ValueDraggerZ.Value = (_currentValueZ + delta / 51f).ToString("0.00");
        }
    }
}