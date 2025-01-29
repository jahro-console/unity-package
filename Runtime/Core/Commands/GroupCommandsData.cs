using System;
using System.Collections.Generic;
using System.Linq;
using JahroConsole.Core.Registry;
using UnityEngine;

namespace JahroConsole.Core.Commands
{
    [Serializable]
    internal struct GroupCommandsData
    {
        [SerializeField]
        internal string Name;
        [SerializeField]
        internal bool Foldout;
        [SerializeField]
        internal List<CommandEntryData> CommandEntries;

        internal static GroupCommandsData Extract(SimpleGroup<ConsoleCommandEntry> group)
        {
            var groupData = new GroupCommandsData();
            groupData.Name = group.Name;
            groupData.Foldout = group.Foldout;
            var entries = new List<CommandEntryData>();
            foreach (var entry in group.Entries)
            {
                var dataEntry = CommandEntryData.ExtractData(entry);
                entries.Add(dataEntry);
            }
            groupData.CommandEntries = entries;
            return groupData;
        }

        internal static GroupCommandsData ExtractFavoritesGroup(FavoritesGroup<ConsoleCommandEntry> group)
        {
            var groupData = new GroupCommandsData();
            groupData.Name = group.Name;
            groupData.Foldout = group.Foldout;
            return groupData;
        }

        internal static void ApplyData(GroupCommandsData data, SimpleGroup<ConsoleCommandEntry> group)
        {
            if (group == null)
            {
                return;
            }
            group.Foldout = data.Foldout;
            foreach (var entryData in data.CommandEntries)
            {
                var entry = group.Entries.Where(e => e.SimpleName == entryData.SimpleName).FirstOrDefault();
                if (entry != null)
                {
                    CommandEntryData.ApplyData(entryData, entry);
                }
            }
        }

        internal static void ApplyDataForEntry(GroupCommandsData data, SimpleGroup<ConsoleCommandEntry> group, ConsoleCommandEntry entry)
        {
            var entryData = data.CommandEntries.Where(e => e.SimpleName == entry.SimpleName).FirstOrDefault();
            CommandEntryData.ApplyData(entryData, entry);
        }
    }
}