using JahroConsole.Core.Data;
using JahroConsole.Core.Snapshots;
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

        [SerializeField]
        private Toggle _replayManualModeToggle;

        [SerializeField]
        private Toggle _replayAutostartModeToggle;

        [SerializeField]
        private Toggle _replayShadowModeToggle;

        private int _replayRecordingMode;

        public void Awake()
        {
            _uiScaleSmallToggle.onValueChanged.AddListener(OnUiScaleSmallToggleValueChanged);
            _uiScaleDefaultToggle.onValueChanged.AddListener(OnUiScaleDefaultToggleValueChanged);
            _uiScaleLargeToggle.onValueChanged.AddListener(OnUiScaleLargeToggleValueChanged);
            _keepInWindowBounds.onValueChanged.AddListener(OnKeepInWindowBoundsValueChanged);
            _replayManualModeToggle.onValueChanged.AddListener(OnReplayManualModeToggleValueChanged);
            _replayAutostartModeToggle.onValueChanged.AddListener(OnReplayAutostartModeToggleValueChanged);
            _replayShadowModeToggle.onValueChanged.AddListener(OnReplayShadowModeToggleValueChanged);
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

            _replayRecordingMode = storage.GeneralSettings.replayRecordingMode;
            switch ((SnapshotsManager.ReplayRecordingMode)_replayRecordingMode)
            {
                case SnapshotsManager.ReplayRecordingMode.Manual:
                    _replayManualModeToggle.SetIsOnWithoutNotify(true);
                    break;
                case (SnapshotsManager.ReplayRecordingMode)1:
                    _replayAutostartModeToggle.SetIsOnWithoutNotify(true);
                    break;
                case (SnapshotsManager.ReplayRecordingMode)2:
                    _replayShadowModeToggle.SetIsOnWithoutNotify(true);
                    break;
            }
        }

        public override void OnStateSave(ConsoleStorage storage)
        {
            base.OnStateSave(storage);
            storage.GeneralSettings.replayRecordingMode = _replayRecordingMode;
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

        private void OnReplayManualModeToggleValueChanged(bool isOn)
        {
            _replayRecordingMode = (int)SnapshotsManager.ReplayRecordingMode.Manual;
        }

        private void OnReplayAutostartModeToggleValueChanged(bool isOn)
        {
            _replayRecordingMode = (int)SnapshotsManager.ReplayRecordingMode.Autostart;
        }
        private void OnReplayShadowModeToggleValueChanged(bool isOn)
        {
            _replayRecordingMode = (int)SnapshotsManager.ReplayRecordingMode.Shadow;
        }
    }
}