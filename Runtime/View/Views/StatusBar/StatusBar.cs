using JahroConsole.Core.Context;
using JahroConsole.Core.Network;
using JahroConsole.Core.Notifications;
using JahroConsole.Core.Snapshots;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class StatusBar : MonoBehaviour
    {
        [SerializeField]
        private Text ProjectNameLabel;

        [SerializeField]
        private Text SnapshotingStatusLabel;

        [SerializeField]
        private GameObject _statusSyncIcon;

        [SerializeField]
        private GameObject _statusNoSyncIcon;

        [SerializeField]
        private VersionStatusLabel VersionStatusLabel;

        private JahroContext _context;

        private ProjectInfo _projectInfo;

        void Start()
        {
            // NotificationService.Instance.NetworkSyncStateChanged += OnNetworkSyncStateChanged;
            // NotificationService.Instance.SnapshotingStateChanged += OnSnapshotingStateChanged;
            // OnSnapshotingStateChanged(SnapshotsManager.Instance.ActiveSession != null);
            // OnNetworkSyncStateChanged(NetworkManager.Instance.HasActiveRequests);
            _statusSyncIcon.SetActive(false);
            SnapshotingStatusLabel.text = "";
            if (_context != null)
            {
                OnContextInfoChanged(_context);
            }
        }

        void OnDestroy()
        {
            // NotificationService.Instance.NetworkSyncStateChanged -= OnNetworkSyncStateChanged;
            // NotificationService.Instance.SnapshotingStateChanged -= OnSnapshotingStateChanged;
        }

        public void InitContext(JahroContext context)
        {
            _context = context;
            context.OnContextInfoChanged += OnContextInfoChanged;
            OnContextInfoChanged(context);
        }

        private void OnSnapshotingStateChanged(bool obj)
        {
            if (obj)
            {
                SnapshotingStatusLabel.text = "snapshoting";
            }
            else
            {
                SnapshotingStatusLabel.text = "";
            }
        }

        private void OnNetworkSyncStateChanged(bool active)
        {
            // _statusSyncIcon.SetActive(active);
        }

        private void OnContextInfoChanged(JahroContext context)
        {
            _projectInfo = context.ProjectInfo;
            VersionStatusLabel.UpdateInfo(JahroConfig.CurrentVersion, context.VersionInfo);
            if (_projectInfo == null)
            {
                ProjectNameLabel.text = "Unknown project";
            }
            else
            {
                ProjectNameLabel.text = _projectInfo.Name;
            }
        }

        public void OnProjectNameClick()
        {
            if (_projectInfo == null)
            {
                Application.OpenURL(JahroConfig.RegisterUrl);
            }
            else
            {
                Application.OpenURL(_projectInfo.Url);
            }

        }
    }
}