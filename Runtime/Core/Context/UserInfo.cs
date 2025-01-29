using System;
using UnityEngine;

namespace JahroConsole.Core.Context
{
    [Serializable]
    internal class UserInfo
    {

        [SerializeField]
        private string id;

        [SerializeField]
        private string name;

        internal string Id => id;

        internal string Name => name;

        internal string GetInitialsName()
        {
            var names = name.Split(' ');
            if (names.Length == 0 || string.IsNullOrEmpty(names[0]))
            {
                return "~";
            }
            if (names.Length == 1)
            {
                return names[0].Substring(0, 1);
            }
            return names[0].Substring(0, 1) + names[1].Substring(0, 1);
        }
    }
}