using JahroConsole.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class ConsoleSettingsView : ConsoleBaseView
    {
        [SerializeField]
        private Toggle _uiScaleSmallToggle;

        [SerializeField]
        private Toggle _uiScaleDefaultToggle;

        [SerializeField]
        private Toggle _uiScaleLargeToggle;

        [SerializeField]
        private Toggle _keepInWindowBounds;

        public void Awake()
        {
            _uiScaleSmallToggle.onValueChanged.AddListener(OnUiScaleSmallToggleValueChanged);
            _uiScaleDefaultToggle.onValueChanged.AddListener(OnUiScaleDefaultToggleValueChanged);
            _uiScaleLargeToggle.onValueChanged.AddListener(OnUiScaleLargeToggleValueChanged);
            _keepInWindowBounds.onValueChanged.AddListener(OnKeepInWindowBoundsValueChanged);
        }

        public override void OnStateLoad(ConsoleStorage storage)
        {
            base.OnStateLoad(storage);
            var mode = (CanvasScalingBehaviour.ScaleMode)storage.GeneralSettings.scaleMode;
            switch (mode)
            {
                case CanvasScalingBehaviour.ScaleMode.Small:
                    _uiScaleSmallToggle.SetIsOnWithoutNotify(true);
                    break;
                case CanvasScalingBehaviour.ScaleMode.Default:
                    _uiScaleDefaultToggle.SetIsOnWithoutNotify(true);
                    break;
                case CanvasScalingBehaviour.ScaleMode.Large:
                    _uiScaleLargeToggle.SetIsOnWithoutNotify(true);
                    break;
            }

            _keepInWindowBounds.SetIsOnWithoutNotify(storage.GeneralSettings.keepInWindowBounds);
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

        private void OnKeepInWindowBoundsValueChanged(bool isOn)
        {
            MainWindow.KeepInWindowBounds = isOn;
        }

        private void OnUiScaleLargeToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                MainWindow.ScalingBehaviour.SwitchToMode(CanvasScalingBehaviour.ScaleMode.Large);
            }
        }

        private void OnUiScaleDefaultToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                MainWindow.ScalingBehaviour.SwitchToMode(CanvasScalingBehaviour.ScaleMode.Default);
            }
        }

        private void OnUiScaleSmallToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                MainWindow.ScalingBehaviour.SwitchToMode(CanvasScalingBehaviour.ScaleMode.Small);
            }
        }
    }
}