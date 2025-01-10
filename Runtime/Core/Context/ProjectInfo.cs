using System;
using UnityEngine;

namespace Jahro.Core.Context
{
    [Serializable]
    internal class ProjectInfo
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string id;

        [SerializeField]
        private string url;

        internal string Name => name;

        internal string Id => id;

        internal string Url => url;

        public override string ToString()
        {
            return "ProjectInfo: " + name + " -> ID:" + id + " URL:" + url + "\n ";
        }
    }
}