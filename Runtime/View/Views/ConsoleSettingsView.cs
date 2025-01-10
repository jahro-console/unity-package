using System;
using Jahro.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
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

        public void Start()
        {
            RefreshSafeArea();

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

        private void OnKeepInWindowBoundsValueChanged(bool arg0)
        {

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