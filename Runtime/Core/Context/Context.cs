using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using JahroConsole.Core.Data;
using JahroConsole.Core.Network;
using UnityEngine;

namespace JahroConsole.Core.Context
{
    internal class JahroContext
    {

        internal Action<JahroContext> OnContextInfoChanged;

        internal Action<UserInfo> OnSelectedUserInfoChanged;

        private UserInfo _selectedUserInfo;

        private UserInfo[] _teamMembers;

        private ProjectInfo _projectInfo;

        private TeamInfo _teamInfo;

        private VersionInfo _versionInfo;

        private VideoConfig _videoConfig;

        private bool _apiKeyVerified;

        private IProjectSettings.SnapshotMode _snapshotMode;

        internal ProjectInfo ProjectInfo { get { return _projectInfo; } }

        internal TeamInfo TeamInfo { get { return _teamInfo; } }

        internal UserInfo SelectedUserInfo { get { return _selectedUserInfo; } }

        internal UserInfo[] TeamMembers { get { return _teamMembers; } }

        internal VersionInfo VersionInfo { get { return _versionInfo; } }

        internal VideoConfig VideoConfig { get { return _videoConfig; } }

        internal bool ApiKeyVerified { get { return _apiKeyVerified; } }

        internal string ApiKey { get; private set; }

        internal IProjectSettings.SnapshotMode SnapshotMode { get { return _snapshotMode; } }

        internal void SetSelectedMember(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return;
            }
            _selectedUserInfo = userInfo;
            ConsoleStorageController.Instance.ConsoleStorage.SelectedUserInfo = userInfo;
            OnSelectedUserInfoChanged?.Invoke(userInfo);
        }

        internal void Release()
        {
            OnContextInfoChanged = null;
        }

        private void UpdateInfo(ProjectInfo projectInfo, TeamInfo teamInfo, UserInfo[] users, VersionInfo versionInfo, ConsoleStorage storage, VideoConfig videoConfig = null)
        {
            storage.ProjectInfo = _projectInfo = projectInfo;
            storage.TeamInfo = _teamInfo = teamInfo;
            storage.TeamMembers = _teamMembers = users;
            storage.SelectedUserInfo = _teamMembers.FirstOrDefault(u => u.Id == storage.SelectedUserInfo?.Id);
            if (versionInfo != null)
            {
                storage.VersionInfo = _versionInfo = versionInfo;
            }
            _videoConfig = videoConfig ?? VideoConfig.Default;
            Debug.Log($"[JahroVideo] Video config: {_videoConfig.maxVideoSizeMB}MB, {_videoConfig.maxRecordingsDurationSec}s, {_videoConfig.frameRate}fps, {_videoConfig.jpegQuality}quality, {_videoConfig.maxResolutionSize}px");
            OnContextInfoChanged?.Invoke(this);
        }

        private void RestoreInfo(ConsoleStorage storage)
        {
            _projectInfo = storage.ProjectInfo;
            _teamInfo = storage.TeamInfo;
            _teamMembers = storage.TeamMembers;
            _versionInfo = storage.VersionInfo;
            _videoConfig = VideoConfig.Default;
            OnContextInfoChanged?.Invoke(this);
        }

        internal static async void Init(ConsoleStorage storage, string sessionId, Action<JahroContext> onProcessed)
        {
            var context = new JahroContext();
            context.ApiKey = storage.ProjectSettings.APIKey;
            context._snapshotMode = storage.ProjectSettings.SnapshotingMode;
            context._selectedUserInfo = ConsoleStorageController.Instance.ConsoleStorage.SelectedUserInfo;

            var initContextRequest = new InitContextRequest(sessionId, context.ApiKey, JahroConfig.CurrentVersion, context._selectedUserInfo?.Id ?? string.Empty);
            initContextRequest.OnComplete = (result) =>
            {
                context._apiKeyVerified = true;
                context.UpdateInfo(result.projectInfo, result.tenantInfo, result.users, result.versionInfo, storage, result.videoConfig);
                if (context._selectedUserInfo == null || string.IsNullOrEmpty(context._selectedUserInfo.Id))
                {
                    context._selectedUserInfo = context._teamMembers[0];
                }
                else
                {
                    var selectedUser = context._teamMembers.FirstOrDefault(m => m.Id == context._selectedUserInfo.Id);
                    context._selectedUserInfo = selectedUser ?? context._teamMembers[0];
                }
                storage.SelectedUserInfo = context._selectedUserInfo;
                onProcessed(context);
            };
            initContextRequest.OnFail = (error) =>
            {
                Debug.LogError(error.ToLogString());
                if (error.statusCode == 401)
                {
                    OnApiError(storage, context);
                    onProcessed(context);
                }
                else
                {
                    context._apiKeyVerified = true;
                    context.RestoreInfo(storage);
                    onProcessed(context);
                }
            };
            await NetworkManager.Instance.SendRequestAsync(initContextRequest);
        }

        internal static async void Refresh(ConsoleStorage storage, string sessionId, JahroContext context)
        {
            if (context == null) return;

            var refreshRequest = new RefreshContextRequest(sessionId, context.ApiKey, context._projectInfo.Id, context._selectedUserInfo.Id);
            refreshRequest.OnComplete = (result) =>
            {
                context.UpdateInfo(result.projectInfo, result.tenantInfo, result.users, null, storage);
                var selectedUser = result.users.FirstOrDefault(u => u.Id == context._selectedUserInfo.Id);
                context.SetSelectedMember(selectedUser);
            };
            refreshRequest.OnFail = (error) =>
            {
                Debug.LogError(error.ToLogString());
                if (error.statusCode == 401)
                {
                    OnApiError(storage, context);
                }
                else
                {
                    context._apiKeyVerified = true;
                    context.RestoreInfo(storage);
                }
            };
            await NetworkManager.Instance.SendRequestAsync(refreshRequest);
        }

        private static void OnApiError(ConsoleStorage storage, JahroContext context)
        {
            context._apiKeyVerified = false;
            context._projectInfo = storage.ProjectInfo = null;
            context._teamInfo = storage.TeamInfo = null;
            context._teamMembers = storage.TeamMembers = null;
            context._selectedUserInfo = storage.SelectedUserInfo = null;
            context._videoConfig = VideoConfig.Default;
        }
    }
}