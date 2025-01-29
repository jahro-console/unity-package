namespace JahroConsole.Core.Logging
{
    internal static class MessagesResource
    {
        internal static readonly string LogWelcomeMessage = "<size=70><b><color=#0F0F11FF>Jahro\n</color></b></size>" +
                                                     "<color=#FF5C01><b>Visual Debug Console\n</b></color>" +
                                                     "Visit <color=#17E96B>jahro.io</color> for more info.\n" +
                                                     "Feel free to reach support - <i>support@jahro.io\n</i>";

        internal static readonly string LogCommandInvokeException = "Whoops! Running the command <{0}> caused this exception: {1}";

        internal static readonly string LogCommandCastError = "Oops! The input for the command <{0}> couldn't be mapped to this parameters {1}";

        internal static readonly string LogCommandNotDefined = "Hmmm, the command <{0}> isn't defined.";

        internal static readonly string LogCommandMethodNotFound = "Wait a minute, the method <{0}> couldn't be found for the command <{1}>";

        internal static readonly string LogCommandRegisterObjectNullReference = "Hold up, the target object is null for command <{0}>.";

        internal static readonly string LogCommandNameHasDublicate = "Wait a minute, the command <{0}> has a twin.";

        internal static readonly string LogCommandNameHasSpacing = "FYI, the command <{0}> has a space in its name, and that's a no-go in text mode.";

        internal static readonly string LogSavesLoadingError = "Bummer, couldn't load the saves.";

        internal static readonly string LogSavesParsingError = "Yikes, there was an issue parsing the saves.";

        internal static readonly string MailSubjectForSharing = "Jahro: Logs -> ";

        internal static readonly string LogCommandUnsupportedParamters = "Sorry, parameter {0} of type {1} isn't supported.";

        internal static readonly string LogCommandMonoObjectsNotFound = "Uh-oh, the command <{0}> isn't found in the object's implementation.";

        internal static readonly string LogCommandObjectNullReference = "Hold up, the target object is null.";

        internal static readonly string LogCommandUnregisterCommandNotFound = "Can't find the command <{0}> in the group <{1}> to unregister it.";

        internal static readonly string LogWatcherNullObjectError = "Oops, failed to watch over the object because it's null.";

        internal static readonly string LogDisWatcherNullObjectError = "Uh-oh, failed to unwatch over the object because it's null.";

        internal static readonly string LogWatcherExceptionForProperty = "Uh-oh, failed to get value from Watch property, caused by: {0}.";

        internal static readonly string LowOnInitialized = "Jahro's up and running! Happy coding!";

        internal static readonly string LogNewVersionAvailable = "Guess who's got a shiny upgrade? Jahro {0}! Navigate to Package Manager, hit Re-Download and Import. Before doing that, you might want to remove the old version from the project, just in case, just to be sure.";
    }
}