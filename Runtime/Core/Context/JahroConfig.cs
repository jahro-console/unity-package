namespace JahroConsole.Core.Context
{
    public static class JahroConfig
    {
        public enum Environment
        {
            DEV,
            PROD
        }

        internal const Environment ENV = Environment.PROD;

        public static readonly string CurrentVersion = "0.1.0-alpha.10";

        internal static readonly string RegisterUrl = "https://console.jahro.io";

        public static readonly string ChangelogUrl = "https://jahro.io/changelog";

        internal static readonly string DocumentationRoot = "https://docs.jahro.io/";

        internal static readonly string DocumentationWatcherOverview = "https://jahro.io/docs/watcher";

        internal static readonly string DocumentationCommandsOverview = "https://jahro.io/docs/unity-commands";

        internal static readonly string HostUrl = ENV == Environment.PROD ? "https://api.jahro.io/jahro-api" : "http://localhost:3000";
    }
}