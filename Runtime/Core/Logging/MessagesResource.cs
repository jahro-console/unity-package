namespace JahroConsole.Core.Logging
{
    internal static class MessagesResource
    {
        internal static readonly string LogWelcomeMessage = "<size=70><b><color=#0F0F11FF>Jahro\n</color></b></size>" +
                                                     "<color=#FF5C01><b>Visual Debug Console\n</b></color>" +
                                                     "Visit <color=#17E96B>jahro.io</color> for more info.\n" +
                                                     "Feel free to reach support - <i>support@jahro.io\n</i>";

        internal static readonly string LogCommandInvokeException = "Running the command <{0}> caused this exception: {1}";

        internal static readonly string LogCommandCastError = "The input for the command <{0}> couldn't be mapped to this parameters {1}";

        internal static readonly string LogCommandNotDefined = "The command <{0}> isn't defined.";

        internal static readonly string LogCommandMethodNotFound = "The method <{0}> couldn't be found for the command <{1}>";

        internal static readonly string LogCommandRegisterObjectNullReference = "The target object is null for command <{0}>.";

        internal static readonly string LogCommandNameHasDublicate = "The command <{0}> has a twin.";

        internal static readonly string LogCommandNameHasSpacing = "The command <{0}> has a space in its name, and that's a no-go in text mode.";

        internal static readonly string LogSavesParsingError = "There was an issue parsing the saves.";

        internal static readonly string LogCommandUnsupportedParamters = "Sorry, parameter {0} of type {1} isn't supported.";

        internal static readonly string LogCommandObjectNullReference = "The target object is null.";

        internal static readonly string LogCommandUnregisterCommandNotFound = "Can't find the command <{0}> in the group <{1}> to unregister it.";

        internal static readonly string LogWatcherNullObjectError = "Failed to watch over the object because it's null.";
    }
}