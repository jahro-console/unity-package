using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Data
{

    public interface IProjectSettings
    {
        public bool JahroEnabled { get; set; }

        public bool UseLaunchKeyboardShortcut { get; set; }

        public bool UseLaunchTapArea { get; set; }

        public KeyCode LaunchKey { get; set; }

        public List<string> ActiveAssemblies { get; set; }

        public bool DuplicateToUnityConsole { get; set; }

        public string APIKey { get; set; }
    }
}