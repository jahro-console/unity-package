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

        private bool _apiKeyVerified;

        internal ProjectInfo ProjectInfo { get { return _projectInfo; } }

        internal TeamInfo TeamInfo { get { return _teamInfo; } }

        internal UserInfo SelectedUserInfo { get { return _selectedUserInfo; } }

        internal UserInfo[] TeamMembers { get { return _teamMembers; } }

        internal VersionInfo VersionInfo { get { return _versionInfo; } }

        internal bool ApiKeyVerified { get { return _apiKeyVerified; } }

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

        private void UpdateInfo(ProjectInfo projectInfo, TeamInfo teamInfo, UserInfo[] users, VersionInfo versionInfo, ConsoleStorage storage)
        {
            storage.ProjectInfo = _projectInfo = projectInfo;
            storage.TeamInfo = _teamInfo = teamInfo;
            storage.TeamMembers = _teamMembers = users;
            storage.SelectedUserInfo = _teamMembers.FirstOrDefault(u => u.Id == storage.SelectedUserInfo?.Id);
            if (versionInfo != null)
            {
                storage.VersionInfo = _versionInfo = versionInfo;
            }
            OnContextInfoChanged?.Invoke(this);
        }

        private void RestoreInfo(ConsoleStorage storage)
        {
            _projectInfo = storage.ProjectInfo;
            _teamInfo = storage.TeamInfo;
            _teamMembers = storage.TeamMembers;
            _versionInfo = storage.VersionInfo;
            OnContextInfoChanged?.Invoke(this);
        }

        internal static async void Init(ConsoleStorage storage, string sessionId, Action<JahroContext> onProcessed)
        {
            var context = new JahroContext();
            var initContextRequest = new InitContextRequest(sessionId, storage.ProjectSettings.APIKey, JahroConfig.CurrentVersion);
            context._selectedUserInfo = ConsoleStorageController.Instance.ConsoleStorage.SelectedUserInfo;
            initContextRequest.OnComplete = (result) =>
            {
                context._apiKeyVerified = true;
                context.UpdateInfo(result.projectInfo, result.tenantInfo, result.users, result.versionInfo, storage);
                if (context._selectedUserInfo == null || string.IsNullOrEmpty(context._selectedUserInfo.Id))
                {
                    context._selectedUserInfo = context._teamMembers[0];
                }
                else
                {
                    var selectedUser = context._teamMembers.FirstOrDefault(m => m.Id == context._selectedUserInfo.Id);
                    context._selectedUserInfo = selectedUser;
                }
                onProcessed(context);
            };
            initContextRequest.OnFail = (error, responseCode) =>
            {
                Debug.LogError($"Jahro: {responseCode} - {error}");
                if (responseCode == 401)
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
            var refreshRequest = new RefreshContextRequest(sessionId, storage.ProjectSettings.APIKey, context._projectInfo.Id, context._selectedUserInfo.Id);
            refreshRequest.OnComplete = (result) =>
            {
                context.UpdateInfo(result.projectInfo, result.tenantInfo, result.users, null, storage);
                var selectedUser = result.users.FirstOrDefault(u => u.Id == context._selectedUserInfo.Id);
                context.SetSelectedMember(selectedUser);
            };
            refreshRequest.OnFail = (error, responseCode) =>
            {
                Debug.LogError($"Jahro: {responseCode} - {error}");
                if (responseCode == 401)
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
        }
    }
}