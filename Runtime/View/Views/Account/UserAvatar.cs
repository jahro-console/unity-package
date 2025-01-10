using System.Collections;
using System.Collections.Generic;
using Jahro;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class UserAvatar : MonoBehaviour
    {
        [SerializeField]
        private Text _initialsLabel;

        void Awake()
        {

        }

        internal void SetInitials(UserInfo userAccountInfo)
        {
            if (userAccountInfo == null)
            {
                _initialsLabel.text = "~";
                return;
            }
            _initialsLabel.text = userAccountInfo.GetInitialsName();
        }

    }
}