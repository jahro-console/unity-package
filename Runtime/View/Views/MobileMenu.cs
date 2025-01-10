using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Jahro.Core.Context;
using Jahro.Core.Data;
using System;

namespace Jahro.View
{
    internal class MobileMenu : MonoBehaviour, IPointerClickHandler
    {

        [SerializeField]
        private VersionStatusLabel VersionStatusLabel;

        [SerializeField]
        private Text ProjectNameLabel;

        [SerializeField]
        private Text ProjectURLLabel;

        private ConsoleMainWindow _mainWindow;

        private ProjectInfo _projectInfo;

        public void Init(ConsoleMainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            Close();
        }

        public void InitContext(JahroContext context)
        {
            context.OnContextInfoChanged += OnContextInfoChanged;
            OnContextInfoChanged(context);
        }

        private void OnContextInfoChanged(JahroContext context)
        {
            _projectInfo = context.ProjectInfo;
            VersionStatusLabel.UpdateInfo(ConsoleStorageController.Instance.ConsoleStorage.CurrentJahroVersion, context.VersionInfo);
            if (_projectInfo == null)
            {
                ProjectNameLabel.text = "Unknown project";
            }
            else
            {
                ProjectNameLabel.text = _projectInfo.Name;
            }
        }

        internal void Open()
        {
            gameObject.SetActive(true);
        }

        internal void Close()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var clickOnModalView = eventData.pointerPressRaycast.gameObject == gameObject;
            if (clickOnModalView)
            {
                Close();
            }
        }

        public void OnConsoleMenuClick()
        {
            _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Text);
            Close();
        }

        public void OnVisualMenuClick()
        {
            _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Visual);
            Close();
        }

        public void OnWatcherMenuClick()
        {
            _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Watcher);
            Close();
        }

        public void OnSnapshotsMenuClick()
        {
            _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Snapshots);
            Close();
        }

        public void OnSettingsMenuClick()
        {
            _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Settings);
            Close();
        }

        public void OnDocsMenuClick()
        {
            Application.OpenURL(JahroConfig.DocumentationRoot);
        }

        public void OnProjectURLMenuClick()
        {
            Application.OpenURL(_projectInfo.Url);
        }

    }
}