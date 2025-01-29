namespace JahroConsole.View
{
    internal class DoubleVisualParam : IntVisualParam
    {

        private double _currentDoubleValue;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
        }

        public override object GetResult()
        {
            return double.Parse(_draggerComponent.Value);
        }

        protected override void OnApplyValue()
        {
            _currentDoubleValue = double.Parse(_draggerComponent.Value);
        }

        protected override void OnValueChange(float delta)
        {
            _draggerComponent.Value = (_currentDoubleValue + delta / 51f).ToString("0.0000");
        }
    }
}
