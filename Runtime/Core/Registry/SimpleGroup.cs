using System;
using System.Collections.Generic;
using System.Linq;

namespace JahroConsole.Core.Registry
{
    internal class SimpleGroup<T> where T : ConsoleEntry
    {
        internal string Name { get; private set; }

        internal bool Foldout { get; set; }

        internal bool LIFO { get; set; }

        internal List<T> Entries { get { return _entries; } }

        protected List<T> _entries;

        internal Action OnEntriesChanged = delegate { };

        internal SimpleGroup(string name)
        {
            Name = name;
            _entries = new List<T>(); //TODO serialize

            Foldout = true;
        }

        internal IEnumerable<T> GetCommandEntries(string name, string[] args)
        {
            return _entries.Where(e => e.Name == name);
        }

        internal string[] GetCommandsNames()
        {
            return _entries.Select(e => e.Name).ToArray();
        }

        internal void AddEntry(T entry)
        {
            _entries.Add(entry);
            OnEntriesChanged();
        }

        internal void RemoveEntry(T entry)
        {
            if (_entries.Contains(entry))
            {
                _entries.Remove(entry);
            }
            OnEntriesChanged();
        }

        internal bool HasDublicate(T entry)
        {
            return _entries
                .Where(e => e.Equals(entry))
                .Count() > 0;
        }
    }
}