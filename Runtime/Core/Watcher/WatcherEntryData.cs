using System;
using UnityEngine;

namespace JahroConsole.Core.Watcher
{
    [Serializable]
    internal struct WatcherEntryData
    {
        [SerializeField]
        internal bool Favorite;

        [SerializeField]
        internal string Name;

        internal static WatcherEntryData ExtractData(ConsoleWatcherEntry entry)
        {
            var dataEntry = new WatcherEntryData();
            dataEntry.Name = entry.Name;
            dataEntry.Favorite = entry.Favorite;
            return dataEntry;
        }

        internal static void ApplyData(WatcherEntryData entryData, ConsoleWatcherEntry entry)
        {
            entry.SetFavorite(entryData.Favorite, true);
        }
    }
}