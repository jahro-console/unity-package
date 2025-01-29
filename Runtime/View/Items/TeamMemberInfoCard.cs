using System;
using JahroConsole.Core.Context;
using JahroConsole.View;
using UnityEngine;
using UnityEngine.UI;

internal class TeamMemberInfoCard : MonoBehaviour
{
    [SerializeField]
    private UserAvatar _avatar;

    [SerializeField]
    private Text _nameLabel;

    [SerializeField]
    private Button _selectButton;

    private UserInfo _userAccountInfo;

    private Action _onSelectClick;

    void Awake()
    {
        _selectButton?.onClick.AddListener(OnSelectClick);
    }

    internal void Init(UserInfo userAccountInfo, Action onSelectClick)
    {
        _userAccountInfo = userAccountInfo;
        _avatar.SetInitials(userAccountInfo);
        _nameLabel.text = userAccountInfo.Name;
        _onSelectClick = onSelectClick;
    }

    internal void OnSelectClick()
    {
        _onSelectClick?.Invoke();
    }
}