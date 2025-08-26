using System;
using UnityEngine;


namespace JahroConsole.Core.Context
{
    [Serializable]
    public class VersionInfo
    {
        public string message;
        public string currentVersion;
        public bool isLatestVersion;
        public bool updateRequired;
        public bool updateRecommended;
        public string updateSeverity;
        public string latestVersion;
        public string changelogUrl;
        public string repositoryUrl;

        public override string ToString()
        {
            return $"VersionInfo currentVersion: {currentVersion} isLatestVersion: {isLatestVersion} updateRequired: {updateRequired} updateRecommended: {updateRecommended} updateSeverity: {updateSeverity} latestVersion: {latestVersion} changelogUrl: {changelogUrl} repositoryUrl: {repositoryUrl}";
        }
    }
}