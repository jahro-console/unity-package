namespace JahroConsole.Core.Context
{
    internal static class JahroConfig
    {
        public enum Environment
        {
            DEV,
            PROD
        }

        internal const Environment ENV = Environment.PROD;

        internal static readonly string CurrentVersion = "0.1.0-alpha.3";

        internal static readonly string RegisterUrl = "https://console.jahro.io";

        internal static readonly string DocumentationRoot = "https://docs.jahro.io/";

        internal static readonly string DocumentationWatcherOverview = "https://docs.jahro.io/watcher";

        internal static readonly string DocumentationCommandsOverview = "https://docs.jahro.io/commands";

        internal static readonly string HostUrl = ENV == Environment.PROD ? "https://octopus-app-p98wl.ondigitalocean.app/jahro-api" : "http://localhost:3000";
    }
}