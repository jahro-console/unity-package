namespace JahroConsole.Core.Logging
{
    internal static class JahroLogTag
    {
        // Tier 1 — Critical
        internal const string Network   = "Jahro.Network";
        internal const string Upload    = "Jahro.Upload";
        internal const string Snapshots = "Jahro.Snapshots";
        internal const string Storage   = "Jahro.Storage";
        internal const string Context   = "Jahro.Context";

        // Tier 2 — Core
        internal const string Commands  = "Jahro.Commands";
        internal const string Watcher   = "Jahro.Watcher";
        internal const string Chunks    = "Jahro.Chunks";

        // Tier 3 — Infrastructure
        internal const string Video     = "Jahro.Video";
        internal const string Core      = "Jahro.Core";
    }
}
