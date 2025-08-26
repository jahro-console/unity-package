using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class VersionStatusLabel : MonoBehaviour
    {
        [SerializeField]
        private Image Indicator;

        [SerializeField]
        private GameObject VersionWarningContainer;

        [SerializeField]
        private Text VersionText;

        private VersionInfo _versionInfo;

        void Start()
        {
            Indicator.color = new Color(0f, 0f, 0f, 0f);
            VersionText.text = "";
        }

        public void OnLabelClick()
        {
            string url = _versionInfo?.changelogUrl;
            Application.OpenURL(url != null ? url : "https://jahro.io/changelog");
        }

        public void UpdateInfo(string currentVersion, VersionInfo versionInfo)
        {
            _versionInfo = versionInfo;
            VersionText.text = "v" + currentVersion;
            if (_versionInfo == null)
            {
                Indicator.color = new Color(0f, 0f, 0f, 0f);
                return;
            }

            if (_versionInfo.updateRequired)
            {
                Indicator.color = new Color(0.953f, 0f, 0.173f);
                if (VersionWarningContainer != null) VersionWarningContainer.SetActive(true);
            }
            else if (_versionInfo.updateRecommended)
            {
                Indicator.color = new Color(0.102f, 0.231f, 0.945f);
            }
            else
            {
                Indicator.color = new Color(0f, 0f, 0f, 0f);
            }

        }

    }
}