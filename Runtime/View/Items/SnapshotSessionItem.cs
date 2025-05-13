using JahroConsole.Core.Snapshots;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class SnapshotSessionItem : MonoBehaviour
    {
        [SerializeField]
        private Text TitleText;

        [SerializeField]
        private RectTransform InfoHolder;

        [SerializeField]
        private Button UploadButton;

        [SerializeField]
        private Button DeleteButton;

        [SerializeField]
        private Button StopAndUploadButton;

        [SerializeField]
        private Button RetryButton;

        [SerializeField]
        private Text StatusText;

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
        private RectTransform ErrorHolder;

        [SerializeField]
        private RectTransform HintHolder;

        [SerializeField]
        private Text ErrorText;

        [SerializeField]
        private GridLayoutGroup _gridLayoutGroup;

        private LayoutElement _infoHolderLayoutElement;

        private ConsoleSnapshotsView _consoleView;

        private SnapshotSession _session;

        internal void Init(ConsoleSnapshotsView view, SnapshotSession session)
        {
            _consoleView = view;
            _session = session;
            if (_session.GetStatus() == SnapshotSession.Status.Recording)
            {
                _session.OnLogsCountUpdate += OnLogsCountUpdate;
                _session.OnScreenshotsCountUpdate += OnScreenshotsCountUpdate;
            }
            _session.OnStatusUpdate += OnStatusUpdate;

            _infoHolderLayoutElement = InfoHolder.GetComponent<LayoutElement>();
            HintHolder.gameObject.SetActive(false);
            ErrorHolder.gameObject.SetActive(false);

            SetTitle();
            SetScreenshotsCount(_session.GetScreenshotCount());
            _session.GetLogsCount(out int debug, out int warning, out int error, out int command);
            SetLogsCount(debug, warning, error, command);
            SetUITightMode(view.MainWindow.IsTightMode);
            OnStatusUpdate(_session.GetStatus());

            UploadButton.onClick.AddListener(OnUploadClick);
            DeleteButton.onClick.AddListener(OnDeleteClick);
            RetryButton.onClick.AddListener(OnRetryClick);
            StopAndUploadButton.onClick.AddListener(OnStopAndUploadClick);
        }

        internal void ShowHint()
        {
            HintHolder.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        internal void SetError(string errorMessage)
        {
            ErrorHolder.gameObject.SetActive(true);
            ErrorText.text = errorMessage;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        internal void SetUITightMode(bool tight)
        {
            _gridLayoutGroup.constraintCount = tight ? 1 : 2;
            _infoHolderLayoutElement.preferredHeight = tight ? 50 : 40;
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
            SnapshotsManager.Instance.UploadSnapshotCoroutine(_session);
        }

        private void OnDeleteClick()
        {
            _consoleView.OpenDialog("Delete snapshot?", "Are you sure you want to delete this snapshot?", () =>
            {
                SnapshotsManager.Instance.DeleteSnapshot(_session);
            });
        }

        private void OnStopAndUploadClick()
        {
            SnapshotsManager.Instance.StopRecording();
            SnapshotsManager.Instance.UploadSnapshotCoroutine(_session);
        }

        private void OnRetryClick()
        {
            ErrorHolder.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            SnapshotsManager.Instance.UploadSnapshotCoroutine(_session);
        }

        private void OnStatusUpdate(SnapshotSession.Status status)
        {
            switch (status)
            {
                case SnapshotSession.Status.Recording:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    DeleteButton.gameObject.SetActive(false);
                    StopAndUploadButton.gameObject.SetActive(true);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Recording...";
                    break;
                case SnapshotSession.Status.Uploading:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    DeleteButton.gameObject.SetActive(false);
                    StopAndUploadButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = $"Uploading... {Mathf.RoundToInt(_session.GetUploadProgress() * 100f)}%";
                    break;
                case SnapshotSession.Status.Error:
                    RetryButton.gameObject.SetActive(true);
                    UploadButton.gameObject.SetActive(false);
                    DeleteButton.gameObject.SetActive(false);
                    StopAndUploadButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(false);
                    SetError(_session.GetErrorMessage());
                    break;
                case SnapshotSession.Status.Uploaded:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(false);
                    DeleteButton.gameObject.SetActive(false);
                    StopAndUploadButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(true);
                    StatusText.text = "Uploaded";
                    break;
                case SnapshotSession.Status.Saved:
                    RetryButton.gameObject.SetActive(false);
                    UploadButton.gameObject.SetActive(true);
                    DeleteButton.gameObject.SetActive(true);
                    StopAndUploadButton.gameObject.SetActive(false);
                    StatusText.gameObject.SetActive(false);
                    break;
            }
        }

        private void SetScreenshotsCount(int Count)
        {
            ScreenshotsInfo.text = "Screenshots: " + Count;
        }

        private void SetLogsCount(int debug, int warning, int error, int command)
        {
            IndicatorDebug.SetCount(debug, false);
            IndicatorWarning.SetCount(warning, false);
            IndicatorError.SetCount(error, false);
            IndicatorCommands.SetCount(command, false);
        }

        private void SetTitle()
        {
            string time = _session.GetRecordDate().ToString("HH:mm:ss");
            string date = _session.GetRecordDate().ToString("MMM dd");
            TitleText.text = $"{time}\n{date}";
        }
    }
}