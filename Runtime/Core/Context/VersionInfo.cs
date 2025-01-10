using System;
using UnityEngine;

namespace Jahro.Core.Context
{
    [Serializable]
    internal class VersionInfo
    {
        [SerializeField]
        private string updateSeverity;

        [SerializeField]
        private string changelogUrl;

        [SerializeField]
        private string newVersionName;

        internal string UpdateSeverity => updateSeverity;

        internal string ChangelogUrl => changelogUrl;

        internal string NewVersionName => newVersionName;

        public override string ToString()
        {
            return "VersionInfo: importance" + updateSeverity + " -> to version:" + newVersionName + "\n ";
        }
    }
}
