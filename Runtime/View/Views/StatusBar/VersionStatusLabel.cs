using System.Collections;
using System.Collections.Generic;
using Jahro;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class VersionStatusLabel : MonoBehaviour
    {
        [SerializeField]
        private Image Indicator;

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
            string url = _versionInfo?.ChangelogUrl;
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
            switch (versionInfo.UpdateSeverity)
            {
                case "critical":
                    Indicator.color = new Color(0.953f, 0f, 0.173f);
                    break;
                case "major":
                    Indicator.color = new Color(0.102f, 0.231f, 0.945f);
                    break;
                case "none":
                    Indicator.color = new Color(0f, 0f, 0f, 0f);
                    break;
                default:
                    Indicator.color = new Color(0f, 0f, 0f, 0f);
                    break;
            }
        }

    }
}