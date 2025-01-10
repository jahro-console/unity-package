using System;
using UnityEngine;

namespace Jahro.Core.Data
{
    [Serializable]
    internal class GeneralSettingsData
    {
        [SerializeField]
        internal Vector2 WindowAnchoredPosition;
        [SerializeField]
        internal Vector2 WindowSize;
        [SerializeField]
        internal bool Fullscreen = true;
        [SerializeField]
        internal Vector2 OpenButtonPosition;
        [SerializeField]
        internal string Mode;
        [SerializeField]
        internal bool filterDebug = true;
        [SerializeField]
        internal bool filterWarning = true;
        [SerializeField]
        internal bool filterError = true;
        [SerializeField]
        internal bool filterCommands = true;
        [SerializeField]
        internal int scaleMode = 1;
    }
}