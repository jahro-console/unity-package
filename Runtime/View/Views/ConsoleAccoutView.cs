using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Jahro;
using System.IO.IsolatedStorage;
using Jahro.Core;
using Jahro.Core.Context;

namespace Jahro.View
{
    internal class ConsoleAccoutView : ConsoleBaseView
    {
        [SerializeField]
        private Text _activeUserNameLabel;

        [SerializeField]
        private Text _activeTeamNameLabel;

        [SerializeField]
        private UserAvatar _activeUserAvatar;

        [SerializeField]
        private Text _currentPlanLabel;

        [SerializeField]
        private RectTransform _teamMembersContainer;

        [SerializeField]
        private GameObject _teamMemberPrefab;

        void Start()
        {
            RefreshSafeArea();
        }

        public void ManageAccountClick()
        {
            Application.OpenURL(Context.TeamInfo.Url);
        }

        public void UpgradeAccountClick()
        {
            Application.OpenURL(Context.TeamInfo.Url);
        }

        public override void InitContext(JahroContext context)
        {
            base.InitContext(context);
            Context.OnSelectedUserInfoChanged += OnSelectedUserInfoChanged;
            Context.OnContextInfoChanged += OnContextInfoChanged;
            OnContextInfoChanged(context);
        }

        private void OnContextInfoChanged(JahroContext context)
        {
            FillActiveUser(Context.SelectedUserInfo);
            var teamMembers = Context.TeamMembers;
            if (teamMembers != null && Context.SelectedUserInfo != null)
            {
                if (teamMembers.Length > 1)
                {
                    foreach (Transform child in _teamMembersContainer)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var teamMember in teamMembers)
                {
                    if (teamMember.Id != Context.SelectedUserInfo.Id)
                    {
                        var teamMemberCard = Instantiate(_teamMemberPrefab, _teamMembersContainer).GetComponent<TeamMemberInfoCard>();
                        teamMemberCard.Init(teamMember, () =>
                        {
                            Context.SetSelectedMember(teamMember);
                        });
                    }
                }
            }

            _activeTeamNameLabel.text = Context.TeamInfo == null ? "" : "Team: " + Context.TeamInfo.Name;
        }

        private void OnSelectedUserInfoChanged(UserInfo selectedUserInfo)
        {
            FillActiveUser(selectedUserInfo);
        }

        private void FillActiveUser(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return;
            }
            _activeUserNameLabel.text = userInfo.Name;
            _activeUserAvatar.SetInitials(userInfo);
        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);

        }

        protected override void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {
            base.OnSafeAreaChanged(safeArea, scaleFactor);

            RefreshSafeArea();
        }

        private void RefreshSafeArea()
        {
            // int leftPadding = (int)Mathf.Max(SafeArea.x/ScaleFactor, 0);
            // int rightPadding = (int)Mathf.Max((Screen.width - (SafeArea.x + SafeArea.width))/ScaleFactor, 0);
            // if (_contentLayoutGroup != null)
            // {
            //     _contentLayoutGroup.padding = new RectOffset(leftPadding, rightPadding, 0, 0);
            // }
        }
    }
}