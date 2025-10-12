using System;

namespace JahroConsole.Core.Network
{
    internal class EditorNetworkManager : NetworkManager
    {
        private static readonly Lazy<EditorNetworkManager> _instance = new Lazy<EditorNetworkManager>(() => new EditorNetworkManager());
        internal static new EditorNetworkManager Instance => _instance.Value;
    }
}