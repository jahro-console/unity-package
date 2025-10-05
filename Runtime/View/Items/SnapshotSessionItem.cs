using JahroConsole.Core.Snapshots;
using UnityEngine;
using UnityEngine.UI;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Notifications;
using JahroConsole.Core.Utilities;

namespace JahroConsole.View
{
    internal class SnapshotSessionItem : MonoBehaviour
    {
        [SerializeField]
        private InputField TitleInputField;

        [SerializeField]
        private Toggle RenameToggle;

        [SerializeField]
        private Button UploadButton;

        [SerializeField]
        private Button StopButton;

        [SerializeField]
        private Button RetryButton;

        [SerializeField]
        private Button CopyURLButton;

        [SerializeField]
        private Button OpenButton;

        [SerializeField]
        private Text StatusText;

        [SerializeField]
        private Text StatusDetailsText;

        [SerializeField]
        private BlinkIndicator IndicatorDebug;

        [SerializeField]
        private BlinkIndicator IndicatorWarning;

        [SerializeField]
        private BlinkIndicator IndicatorError;

        [SerializeField]
        private BlinkIndicator IndicatorCommands;

        [SerializeField]
        private Text ScreenshotsInfo;

        [SerializeField]
        private RectTransform LogsGroupHolder;

        [SerializeField]
        private RectTransform LogsGroupHolderTight;

        private GameObject _indicatorDebugGroup;
        private GameObject _indicatorWarningGroup;
        private GameObject _indicatorErrorGroup;
        private GameObject _indicatorCommandsGroup;

        private ConsoleSnapshotsView _consoleView;
        private SnapshotSession _session;
        private string _originalName;

        internal void Init(ConsoleSnapshotsView view, SnapshotSession session)
        {
            _consoleView = view;
            _session = session;
            if (_session.GetStatus() == SnapshotSession.Status.Recording || _session.GetStatus() == SnapshotSession.Status.Streaming)
            {
                _session.OnLogsCountUpdate += OnLogsCountUpdate;
                _session.OnScreenshotsCountUpdate += OnScreenshotsCountUpdate;
            }
            _session.OnStatusUpdate += OnStatusUpdate;

            _indicatorDebugGroup = IndicatorDebug.gameObject.transform.parent.gameObject;
            _indicatorWarningGroup = IndicatorWarning.gameObject.transform.parent.gameObject;
            _indicatorErrorGroup = IndicatorError.gameObject.transform.parent.gameObject;
            _indicatorCommandsGroup = IndicatorCommands.gameObject.transform.parent.gameObject;

            SetScreenshotsCount(_session.GetScreenshotCount());
            _session.GetLogsCount(out int debug, out int warning, out int error, out int command);
            SetLogsCount(debug, warning, error, command);
            SetUITightMode(view.MainWindow.IsTightMode);
            OnStatusUpdate(_session.GetStatus());

            if (_session.GetStatus() == SnapshotSession.Status.Recorded || _session.GetStatus() == SnapshotSession.Status.Streamed || _session.GetStatus() == SnapshotSession.Status.Uploaded)
            {
                ShowTimeAgo();
            }

            _originalName = _session.name;
            TitleInputField.text = _originalName;
            TitleInputField.onSubmit.AddListener(OnTitleSubmit);
            TitleInputField.onEndEdit.AddListener(OnTitleEndEdit);

            UploadButton.onClick.AddListener(OnUploadClick);
            RetryButton.onClick.AddListener(OnRetryClick);
            StopButton.onClick.AddListener(OnStopClick);
            RenameToggle.onValueChanged.AddListener(OnRenameToggleValueChanged);
            CopyURLButton.onClick.AddListener(OnCopyURLClick);
            OpenButton.onClick.AddListener(OnOpenClick);
        }

        internal void ShowTimeAgo()
        {
            StatusDetailsText.gameObject.SetActive(true);
            StatusDetailsText.text = TimeAgo.GetTimeAgo(_session.GetRecordDate());
            StatusDetailsText.color = Color.white;
        }

        internal void SetUITightMode(bool tight)
        {
            if (tight)
            {
                _indicatorDebugGroup.transform.SetParent(LogsGroupHolderTight);
                _indicatorCommandsGroup.transform.SetParent(LogsGroupHolderTight);
            }
            else
            {
                _indicatorDebugGroup.transform.SetParent(LogsGroupHolder);
                _indicatorCommandsGroup.transform.SetParent(LogsGroupHolder);
            }
        }

        private void OnLogsCountUpdate(int arg1, int arg2, int arg3, int arg4)
        {
            SetLogsCount(arg1, arg2, arg3, arg4);
        }

        private void OnScreenshotsCountUpdate(int obj)
        {
            SetScreenshotsCount(obj);
        }

        private void OnUploadClick()
        {
            SnapshotsManager.Instance.UploadSnapshot(_session, null, (error) => OnUploadError(error));
        }

        private void OnStopClick()
        {
            if (_session.GetStatus() == SnapshotSession.Status.Recording)
            {
                SnapshotsManager.Instance.StopRecordingSnapshot(_session);
            }
            else if (_session.GetStatus() == SnapshotSession.Status.Streaming)
            {
                SnapshotsManager.Instance.StopStreamingSnapshot(_session);
            }
        }

        private void OnRetryClick()
        {
            SnapshotsManager.Instance.UploadSnapshot(_session, null, (error) => OnUploadError(error));
        }

        private void OnRenameToggleValueChanged(bool value)
        {
            if (value)
            {
                TitleInputField.readOnly = false;
                TitleInputField.Select();
                TitleInputField.ActivateInputField();
                _originalName = TitleInputField.text;
                RenameToggle.targetGraphic.color = RenameToggle.colors.highlightedColor;
            }
            else
            {
                TitleInputField.readOnly = true;
                RenameToggle.targetGraphic.color = RenameToggle.colors.normalColor;
            }
        }

        private void OnTitleSubmit(string value)
        {
            SaveNameIfChanged(value);
        }

        private void OnTitleEndEdit(string value)
        {
            if (TitleInputField.readOnly)
            {
                return;
            }

            SaveNameIfChanged(value);
        }


        private void OnCopyURLClick()
        {
            JahroEntriesCopyHelper.CopyToClipboard(_session.frontendUrl);
            NotificationService.Instance.SendNotification(new Notification("Copied to clipboard"));
        }

        private void OnOpenClick()
        {
            Application.OpenURL(_session.frontendUrl);
        }

        private void OnStatusUpdate(SnapshotSession.Status status)
        {
            _originalName = _session.name;
            TitleInputField.text = _originalName;
            SetErrorMessage(_session.GetErrorMessage());

            switch (status)
            {
                case SnapshotSession.Status.Streaming:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    StopButton.gameObject.SetActive(true);
                    CopyURLButton.gameObject.SetActive(true);
                    OpenButton.gameObject.SetActive(true);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Streaming...";
                    StatusText.color = new Color(0.035f, 0.753f, 0.153f);
                    break;
                case SnapshotSession.Status.Streamed:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    StopButton.gameObject.SetActive(false);
                    CopyURLButton.gameObject.SetActive(true);
                    OpenButton.gameObject.SetActive(true);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Streamed";
                    StatusText.color = Color.white;
                    break;
                case SnapshotSession.Status.Recording:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    StopButton.gameObject.SetActive(true);
                    CopyURLButton.gameObject.SetActive(false);
                    OpenButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Recording...";
                    StatusText.color = new Color(0.035f, 0.753f, 0.153f);
                    break;
                case SnapshotSession.Status.Uploading:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    StopButton.gameObject.SetActive(false);
                    CopyURLButton.gameObject.SetActive(false);
                    OpenButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Uploading...";
                    StatusText.color = Color.white;
                    break;
                case SnapshotSession.Status.Uploaded:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    StopButton.gameObject.SetActive(false);
                    CopyURLButton.gameObject.SetActive(true);
                    OpenButton.gameObject.SetActive(true);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Uploaded";
                    StatusText.color = Color.white;
                    break;
                case SnapshotSession.Status.Recorded:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(true);
                    StopButton.gameObject.SetActive(false);
                    CopyURLButton.gameObject.SetActive(false);
                    OpenButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Ready to upload";
                    StatusText.color = Color.white;
                    break;
            }
        }

        private void OnUploadError(string error)
        {
            NotificationService.Instance.SendNotification(new Notification($"Upload failed: {error}"));
        }

        private void SetErrorMessage(string errorMessage = null)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                StatusDetailsText.text = "";
                StatusDetailsText.gameObject.SetActive(false);
                return;
            }

            StatusDetailsText.gameObject.SetActive(true);
            StatusDetailsText.text = errorMessage;
            StatusDetailsText.color = Color.red;
        }

        private void SetScreenshotsCount(int count)
        {
            ScreenshotsInfo.text = count.ToString() + " Screenshots";
        }

        private void SetLogsCount(int debug, int warning, int error, int command)
        {
            _indicatorDebugGroup.SetActive(debug > 0);
            IndicatorDebug.SetCount(debug);

            _indicatorWarningGroup.SetActive(warning > 0);
            IndicatorWarning.SetCount(warning);

            _indicatorErrorGroup.SetActive(error > 0);
            IndicatorError.SetCount(error);

            _indicatorCommandsGroup.SetActive(command > 0);
            IndicatorCommands.SetCount(command);
        }

        private void SaveNameIfChanged(string newName)
        {
            RenameToggle.isOn = false;
            newName = newName.Trim();
            if (newName == _originalName)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                OnRenameError("Name cannot be empty");
                return;
            }

            if (newName.Length > 100)
            {
                OnRenameError("Name is too long (max 100 characters)");
                return;
            }

            SnapshotsManager.Instance.RenameSnapshot(_session, newName,
                () => OnRenameSuccess(),
                (error) => OnRenameError(error));
        }

        private void OnRenameSuccess()
        {
            _originalName = _session.name;
            NotificationService.Instance.SendNotification(new Notification("Snapshot renamed successfully"));
        }

        private void OnRenameError(string errorMessage)
        {
            TitleInputField.text = _originalName;
            NotificationService.Instance.SendNotification(new Notification($"Rename failed: {errorMessage}"));
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_session != null)
            {
                _session.OnLogsCountUpdate -= OnLogsCountUpdate;
                _session.OnScreenshotsCountUpdate -= OnScreenshotsCountUpdate;
                _session.OnStatusUpdate -= OnStatusUpdate;
            }
        }
    }
}