using System;
using System.Collections.Generic;
using JahroConsole.Core.Commands;
using JahroConsole.Core.Context;
using JahroConsole.Core.Watcher;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    [Serializable]
    internal class ConsoleStorage
    {
        [SerializeField] internal GeneralSettingsData GeneralSettings;

        [SerializeField] internal List<GroupCommandsData> CommandGroups;

        [SerializeField] internal List<GroupWatcherData> WatcherGroups;

        [SerializeField] internal CommandsQueue CommandsQueue;

        [SerializeField] internal ProjectInfo ProjectInfo;

        [SerializeField] internal TeamInfo TeamInfo;

        [SerializeField] internal UserInfo[] TeamMembers;

        [SerializeField] internal VersionInfo VersionInfo;

        [SerializeField] internal UserInfo SelectedUserInfo;

        [NonSerialized] internal IProjectSettings ProjectSettings;

        internal ConsoleStorage()
        {
            GeneralSettings = new GeneralSettingsData();
            CommandsQueue = new CommandsQueue();
        }
    }
}