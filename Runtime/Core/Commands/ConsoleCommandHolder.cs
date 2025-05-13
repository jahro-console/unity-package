using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Commands
{
    internal class ConsoleCommandHolder
    {
        internal List<SimpleGroup<ConsoleCommandEntry>> Groups { get { return _groups; } }

        private List<SimpleGroup<ConsoleCommandEntry>> _groups;

        private RecentGroup _recentGroup;

        private FavoritesGroup<ConsoleCommandEntry> _favoritesGroup;

        internal Action OnGroupsChanged = delegate { };

        internal ConsoleCommandHolder()
        {
            _groups = new List<SimpleGroup<ConsoleCommandEntry>>();
            _recentGroup = new RecentGroup();
            _groups.Add(_recentGroup);
            _favoritesGroup = new FavoritesGroup<ConsoleCommandEntry>();
            _groups.Add(_favoritesGroup);
            _groups.Add(new DefaultGroup<ConsoleCommandEntry>());
        }

        internal void InitCommandMethod(ConsoleCommandEntry commandEntry, string groupName)
        {
            var targetGroup = GetGroup(groupName);
            if (targetGroup == null)
            {
                targetGroup = CreateNewGroup(groupName);
            }

            commandEntry.FavoritesStateChanged += delegate (bool state)
            {
                OnCommandFavoritesChanged(commandEntry, state);
            };
            commandEntry.OnExecuted += delegate
            {
                OnCommandExecuted(commandEntry);
            };

            if (targetGroup.HasDublicate(commandEntry))
            {
                JahroLogger.Log(string.Format(MessagesResource.LogCommandNameHasDublicate, commandEntry.Name), string.Empty, EJahroLogType.JahroWarning);
            }
            targetGroup.AddEntry(commandEntry);

            if (commandEntry.Runtime)
            {
                LoadEntryState(commandEntry, targetGroup);
            }

            if (commandEntry.Name.Contains(' '))
            {
                JahroLogger.Log(string.Format(MessagesResource.LogCommandNameHasSpacing, commandEntry.Name), string.Empty, EJahroLogType.JahroWarning);
            }
        }

        internal void RemoveEntry(JahroCommandAttribute attribute, MethodInfo methodInfo)
        {
            var targetGroup = GetGroup(attribute.GroupName);
            var entry = targetGroup.Entries.Where(e => e.MethodInfo == methodInfo && e.Name == attribute.MethodName).FirstOrDefault();
            if (entry != null)
            {
                targetGroup.RemoveEntry(entry);
                if (targetGroup.Entries.Count == 0)
                {
                    DestroyGroup(targetGroup);
                }
            }
        }

        internal void RemoveRuntimeEntry(string name, string groupName)
        {
            var targetGroup = GetGroup(groupName);
            if (targetGroup == null)
            {
                JahroLogger.LogError(string.Format(MessagesResource.LogCommandUnregisterCommandNotFound, name, groupName));
                return;
            }

            var entries = targetGroup.Entries.Where(e => e.Name == name && e.Runtime == true).ToList();
            foreach (var entry in entries)
            {
                targetGroup.RemoveEntry(entry);
            }
        }

        internal List<ConsoleCommandEntry> GetCommandEntries(string name, string[] args)
        {
            List<ConsoleCommandEntry> resultEntries = new List<ConsoleCommandEntry>();
            foreach (var group in _groups)
            {
                if (group is RecentGroup || group is FavoritesGroup<ConsoleCommandEntry>)
                {
                    continue;
                }

                var entries = group.GetCommandEntries(name, args);
                if (entries != null)
                {
                    resultEntries.AddRange(entries);
                }
            }
            return resultEntries;
        }

        internal List<ConsoleCommandEntry> GetPossibleCommandsNames(string name)
        {
            string nameToFind = name.ToLower();
            List<ConsoleCommandEntry> resultEntries = new List<ConsoleCommandEntry>();
            foreach (var group in _groups)
            {
                if (group is RecentGroup || group is FavoritesGroup<ConsoleCommandEntry>)
                {
                    continue;
                }
                var entries = group.Entries;
                if (entries != null)
                {
                    foreach (var entryName in entries)
                    {
                        if (entryName.Name.ToLower().IndexOf(nameToFind) != -1)
                        {
                            resultEntries.Add(entryName);
                        }
                    }
                }
            }
            return resultEntries;
        }

        internal List<string> GetCommandsNames()
        {
            List<string> resultEntries = new List<string>();
            foreach (var group in _groups)
            {
                if (group is RecentGroup || group is FavoritesGroup<ConsoleCommandEntry>)
                {
                    continue;
                }
                var entries = group.Entries;
                if (entries != null)
                {
                    foreach (var entryName in entries)
                    {
                        resultEntries.Add(entryName.Name);
                    }
                }
            }
            return resultEntries;
        }

        internal void Initialize(ConsoleStorage storage, JahroContext context)
        {
            ConsoleStorageController.Instance.OnStorageSave += OnStateSave;

            LoadState(storage);
        }

        private SimpleGroup<ConsoleCommandEntry> CreateNewGroup(string groupName)
        {
            var group = new SimpleGroup<ConsoleCommandEntry>(groupName);
            _groups.Add(group);
            OnGroupsChanged();
            return group;
        }

        private void DestroyGroup(SimpleGroup<ConsoleCommandEntry> group)
        {
            if (_groups.Contains(group))
            {
                _groups.Remove(group);
                OnGroupsChanged();
            }
        }

        private SimpleGroup<ConsoleCommandEntry> GetGroup(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "Default")
            {
                return _groups.Where(g => g is DefaultGroup<ConsoleCommandEntry>).First();
            }
            return _groups.Where(g => g.Name == name).FirstOrDefault();
        }

        private void LoadState(ConsoleStorage storage)
        {
            if (storage.CommandGroups == null)
            {
                return;
            }

            foreach (var groupData in storage.CommandGroups)
            {
                var group = _groups.Where(g => g.Name == groupData.Name).FirstOrDefault();
                if (group is RecentGroup)
                {
                    foreach (var entryData in groupData.CommandEntries)
                    {
                        var r = _groups.SelectMany(g => g.Entries).Where(e => e.SimpleName == entryData.SimpleName).FirstOrDefault();
                        if (r != null)
                            group.Entries.Add(r);
                    }
                    GroupCommandsData.ApplyData(groupData, group);
                }
                else if (group is FavoritesGroup<ConsoleCommandEntry>)
                {
                    group.Foldout = groupData.Foldout;
                }
                else
                {
                    GroupCommandsData.ApplyData(groupData, group);
                }
            }
        }

        private void LoadEntryState(ConsoleCommandEntry entry, SimpleGroup<ConsoleCommandEntry> group)
        {
            if (ConsoleStorageController.Instance.ConsoleStorage.CommandGroups == null)
            {
                return;
            }
            var storageGroups = ConsoleStorageController.Instance.ConsoleStorage.CommandGroups;
            var storageGroupData = storageGroups.Where(g => g.Name == group.Name).FirstOrDefault();
            if (storageGroupData.CommandEntries != null)
            {
                GroupCommandsData.ApplyDataForEntry(storageGroupData, group, entry);
            }

            var recentGroupData = storageGroups.Where(g => g.Name == "Recent").FirstOrDefault();
            if (recentGroupData.CommandEntries != null)
            {
                int count = recentGroupData.CommandEntries.Where(e => e.SimpleName == entry.SimpleName).Count();
                if (count > 0 && _recentGroup.Entries.Contains(entry) == false)
                {
                    _recentGroup.AddEntry(entry);
                }
            }
        }

        private void OnCommandFavoritesChanged(ConsoleCommandEntry entry, bool favorite)
        {
            _favoritesGroup.CommandFavoriteChanged(entry, favorite);
        }

        private void OnCommandExecuted(ConsoleCommandEntry entry)
        {
            _recentGroup.CommandExecuted(entry);
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.CommandGroups = new List<GroupCommandsData>();
            foreach (var group in _groups)
            {
                GroupCommandsData groupData;
                if (group is FavoritesGroup<ConsoleCommandEntry>)
                {
                    groupData = GroupCommandsData.ExtractFavoritesGroup(group as FavoritesGroup<ConsoleCommandEntry>);
                }
                else
                {
                    groupData = GroupCommandsData.Extract(group);
                }
                storage.CommandGroups.Add(groupData);
            }
        }
    }
}