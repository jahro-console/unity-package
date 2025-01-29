using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Watcher
{
    internal class ConsoleWatcher
    {
        internal List<SimpleGroup<ConsoleWatcherEntry>> Groups { get { return _groups; } }

        private List<SimpleGroup<ConsoleWatcherEntry>> _groups;

        private FavoritesGroup<ConsoleWatcherEntry> _favoritesGroup;

        internal Action OnGroupsChanged = delegate { };

        internal ConsoleWatcher()
        {
            _groups = new List<SimpleGroup<ConsoleWatcherEntry>>();

            _favoritesGroup = new FavoritesGroup<ConsoleWatcherEntry>();
            _groups.Add(_favoritesGroup);
            _groups.Add(new DefaultGroup<ConsoleWatcherEntry>());
        }

        internal void InitEntryToWatch(ConsoleWatcherEntry entry, string groupName)
        {
            var targetGroup = GetGroup(groupName);
            if (targetGroup == null)
            {
                targetGroup = CreateNewGroup(groupName);
            }
            else if (targetGroup.HasDublicate(entry))
            {
                JahroLogger.LogWarning(string.Format(MessagesResource.LogCommandNameHasDublicate, entry.Name));
                return;
            }
            targetGroup.AddEntry(entry);

            entry.FavoritesStateChanged += delegate (bool state)
            {
                OnFavoritesChanged(entry, state);
            };

            if (entry.IsRuntime)
            {
                LoadEntryState(entry, targetGroup);
            }
        }

        internal void RemoveEntryFromWatch(JahroWatchAttribute attribute, MemberInfo memberInfo)
        {
            var targetGroup = GetGroup(attribute.GroupName);
            var entry = targetGroup.Entries.Where(e => e.MemberInfo == memberInfo && e.Name == attribute.Name).FirstOrDefault();
            if (entry != null)
            {
                targetGroup.RemoveEntry(entry);
                if (targetGroup.Entries.Count == 0)
                {
                    DestroyGroup(targetGroup);
                }
            }
        }

        internal void Initialize(ConsoleStorage storage, JahroContext context)
        {
            ConsoleStorageController.Instance.OnStorageSave += OnStateSave;

            LoadState(storage);
        }

        private SimpleGroup<ConsoleWatcherEntry> CreateNewGroup(string groupName)
        {
            var group = new SimpleGroup<ConsoleWatcherEntry>(groupName);
            _groups.Add(group);
            OnGroupsChanged();
            return group;
        }

        private void DestroyGroup(SimpleGroup<ConsoleWatcherEntry> group)
        {
            if (_groups.Contains(group))
            {
                _groups.Remove(group);
                OnGroupsChanged();
            }
        }

        private SimpleGroup<ConsoleWatcherEntry> GetGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _groups.Where(g => g is DefaultGroup<ConsoleWatcherEntry>).First();
            }
            return _groups.Where(g => g.Name == name).FirstOrDefault();
        }

        private void LoadState(ConsoleStorage storage)
        {
            if (storage.WatcherGroups == null)
            {
                return;
            }

            foreach (var groupData in storage.WatcherGroups)
            {
                var group = _groups.Where(g => g.Name == groupData.Name).FirstOrDefault();
                if (group != null)
                {
                    GroupWatcherData.ApplyData(groupData, group);
                }
            }
        }

        private void LoadEntryState(ConsoleWatcherEntry entry, SimpleGroup<ConsoleWatcherEntry> group)
        {
            var storageGroups = ConsoleStorageController.Instance.ConsoleStorage.WatcherGroups;
            var storageGroupData = storageGroups.Where(g => g.Name == group.Name).FirstOrDefault();
            if (storageGroupData.Entries != null)
            {
                GroupWatcherData.ApplyDataForEntry(storageGroupData, group, entry);
            }
        }

        private void OnFavoritesChanged(ConsoleWatcherEntry entry, bool favorite)
        {
            _favoritesGroup.CommandFavoriteChanged(entry, favorite);
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.WatcherGroups = new List<GroupWatcherData>();
            foreach (var group in _groups)
            {
                GroupWatcherData groupData;
                if (group is FavoritesGroup<ConsoleWatcherEntry>)
                {
                    groupData = GroupWatcherData.Extract(group);
                }
                else
                {
                    groupData = GroupWatcherData.Extract(group);
                }
                storage.WatcherGroups.Add(groupData);
            }
        }
    }
}