
using System;

namespace JahroConsole.Core.Notifications
{
    internal class Notification
    {
        private readonly Action onClick;

        internal string Message { get; set; }

        internal Notification(string message, Action onClick = null)
        {
            Message = message;
            this.onClick = onClick;
        }
    }
}