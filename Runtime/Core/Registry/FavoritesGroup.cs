namespace JahroConsole.Core.Registry
{
    internal class FavoritesGroup<T> : SimpleGroup<T> where T : ConsoleEntry
    {

        internal FavoritesGroup() : base("Favorites")
        {
            Foldout = true;
            LIFO = true;
        }

        internal void CommandFavoriteChanged(T entry, bool favorite)
        {
            if (favorite)
            {
                AddEntry(entry);
            }
            else
            {
                RemoveEntry(entry);
            }
        }
    }
}