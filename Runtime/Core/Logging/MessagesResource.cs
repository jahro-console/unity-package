namespace JahroConsole.Core.Logging
{
    internal static class MessagesResource
    {
        internal static readonly string LogWelcomeMessage = "<size=70><b><color=#0F0F11FF>Jahro\n</color></b></size>" +
                                                     "<color=#FF5C01><b>Visual Debug Console\n</b></color>" +
                                                     "Visit <color=#17E96B>jahro.io</color> for more info.\n" +
                                                     "Feel free to reach support - <i>support@jahro.io\n</i>";

        internal static readonly string LogCommandInvokeException = "[Jahro.Commands] Command threw an unhandled exception — command={0}, error={1}. Check the method implementation for unguarded exceptions.";

        internal static readonly string LogCommandCastError = "[Jahro.Commands] Parameter mapping failed — command={0}, expectedParams={1}. Verify input types match the method signature.";

        internal static readonly string LogCommandNotDefined = "[Jahro.Commands] Command not found — command={0}. Confirm the method has [JahroCommand] and its assembly is listed in Active Assemblies.";

        internal static readonly string LogCommandMethodNotFound = "[Jahro.Commands] Method resolution failed — method={0}, command={1}. The method may have been renamed, removed, or is not accessible.";

        internal static readonly string LogCommandRegisterObjectNullReference = "[Jahro.Commands] RegisterObject called with null — command={0}. Pass a valid instance; null registrations are skipped.";

        internal static readonly string LogCommandNameHasDublicate = "[Jahro.Commands] Duplicate command name — command={0}. Command names must be unique across all registered assemblies. One registration will be ignored.";

        internal static readonly string LogCommandNameHasSpacing = "[Jahro.Commands] Invalid command name — command={0}. Command names cannot contain spaces (text-mode input splits on whitespace). Use underscore or camelCase.";

        internal static readonly string LogSavesParsingError = "[Jahro.Storage] State file parse failed. The local state file may be corrupt or written by an incompatible version. File will be reset.";

        internal static readonly string LogCommandUnsupportedParamters = "[Jahro.Commands] Unsupported parameter type — param={0}, type={1}. Supported types: bool, int, float, string, enum, Vector2, Vector3, and their arrays.";

        internal static readonly string LogCommandObjectNullReference = "[Jahro.Commands] Command target is null — the registered object was destroyed or garbage collected before the command executed.";

        internal static readonly string LogCommandUnregisterCommandNotFound = "[Jahro.Commands] Unregister failed — command={0}, group={1}. Command was not found; it may have already been unregistered.";

        internal static readonly string LogWatcherNullObjectError = "[Jahro.Watcher] Watch target is null — the registered object was destroyed or is not yet initialized. Call RegisterObject after the object is ready.";
    }
}