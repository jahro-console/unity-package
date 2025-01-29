using System;
using System.Collections.Generic;
using System.Linq;
using JahroConsole.Core.Registry;
using UnityEngine;

namespace JahroConsole.Core.Watcher
{
    [Serializable]
    internal struct GroupWatcherData
    {
        [SerializeField]
        internal string Name;
        [SerializeField]
        internal bool Foldout;
        [SerializeField]
        internal List<WatcherEntryData> Entries;

        internal static GroupWatcherData Extract(SimpleGroup<ConsoleWatcherEntry> group)
        {
            var groupData = new GroupWatcherData();
            groupData.Name = group.Name;
            groupData.Foldout = group.Foldout;
            if (group.Entries != null)
            {
                groupData.Entries = new List<WatcherEntryData>();
                foreach (var entry in group.Entries)
                {
                    groupData.Entries.Add(WatcherEntryData.ExtractData(entry));
                }
            }
            return groupData;
        }

        internal static void ApplyData(GroupWatcherData data, SimpleGroup<ConsoleWatcherEntry> group)
        {
            if (group == null)
            {
                return;
            }
            group.Foldout = data.Foldout;
            if (data.Entries != null)
            {
                foreach (var entryData in data.Entries)
                {
                    var entry = group.Entries.Where(e => e.Name == entryData.Name).FirstOrDefault();
                    if (entry != null)
                    {
                        WatcherEntryData.ApplyData(entryData, entry);
                    }
                }
            }
        }

        internal static void ApplyDataForEntry(GroupWatcherData data, SimpleGroup<ConsoleWatcherEntry> group, ConsoleWatcherEntry entry)
        {
            var entryData = data.Entries.Where(e => e.Name == entry.Name).FirstOrDefault();
            WatcherEntryData.ApplyData(entryData, entry);
        }
    }
}