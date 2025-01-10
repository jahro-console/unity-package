using System;

namespace Jahro.Core.Registry
{
    internal abstract class ConsoleEntry
    {
        internal string Name { get; private set; }

        internal string Description { get; private set; }

        internal bool Favorite { get; private set; } 

        internal Action<bool> FavoritesStateChanged;

        internal ConsoleEntry(string name, string description)
        {
            Name = name;
            Description = description;
        }

        internal void SetFavorite(bool favorite)
        {
            if (Favorite != favorite)
            {
                FavoritesStateChanged(favorite);
            }
            Favorite = favorite;
        }

        internal void SetFavorite(bool favorite, bool notify)
        {
            if (Favorite != favorite && notify)
            {
                FavoritesStateChanged(favorite);
            }
            Favorite = favorite;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is ConsoleEntry entry &&
                    Name == entry.Name &&
                    Description == entry.Description;
        }

        public override int GetHashCode()
        {
            return new {Name, Description}.GetHashCode();
        }
    }
}