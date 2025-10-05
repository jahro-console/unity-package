using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Data
{

    public interface IProjectSettings
    {
        public enum SnapshotMode
        {
            Recording,
            StreamingExceptEditor,
            StreamingAll,
        }
        public bool JahroEnabled { get; set; }

        public bool UseLaunchKeyboardShortcut { get; set; }

        public bool UseLaunchTapArea { get; set; }

        public KeyCode LaunchKey { get; set; }

        public List<string> ActiveAssemblies { get; set; }

        public string APIKey { get; set; }

        public SnapshotMode SnapshotingMode { get; set; }
    }
}