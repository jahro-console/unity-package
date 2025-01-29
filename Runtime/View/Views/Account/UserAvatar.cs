using JahroConsole.Core.Context;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
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
            _initialsLabel.text = userAccountInfo.GetInitialsName().ToUpper();
        }

    }
}