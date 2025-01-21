using UnityEngine;
using UnityEngine.UI;
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
        private RectTransform _teamMembersContainer;

        [SerializeField]
        private GameObject _teamMemberPrefab;

        public void ManageAccountClick()
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
    }
}