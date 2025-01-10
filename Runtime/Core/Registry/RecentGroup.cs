using Jahro.Core.Commands;

namespace Jahro.Core.Registry
{
    internal class RecentGroup : SimpleGroup<ConsoleCommandEntry>
    {

        internal const int RECENT_LIMIT = 10;

        internal RecentGroup() : base("Recent")
        {
            Foldout = true;
            LIFO = true;
        }

        internal void CommandExecuted(ConsoleCommandEntry entry)
        {
            if (Entries.Contains(entry))
            {
                // Entries.Remove(entry);

            }
            else
            {
                AddEntry(entry);
            }

            if (_entries.Count > RECENT_LIMIT)
            {
                _entries.RemoveAt(0);
            }
            OnEntriesChanged();
        }
    }
}