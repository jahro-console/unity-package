using System;
using System.Collections.Generic;
using JahroConsole.Core.Logging;

namespace JahroConsole.Core.Notifications
{
    internal sealed class NotificationService
    {
        internal static NotificationService Instance
        {
            get
            {
                if (_instance == null) _instance = new NotificationService();
                return _instance;
            }
        }

        private static NotificationService _instance;

        private List<Notification> _notifications = new List<Notification>();

        internal event Action<Notification> OnNoficationAdded;

        internal event Action<bool> NetworkSyncStateChanged;

        internal event Action<bool> SnapshotingStateChanged;

        internal event Action<JahroLogGroup.EJahroLogGroup> OnLogAdded;

        internal event Action OnLogsClear;

        private NotificationService()
        {

        }

        internal void SendNotification(Notification notification)
        {
            _notifications.Add(notification);
            OnNoficationAdded?.Invoke(notification);
        }

        internal void ActiveNetwork(bool active)
        {
            NetworkSyncStateChanged?.Invoke(active);
        }

        internal void SnapshotingActive(bool active)
        {
            SnapshotingStateChanged?.Invoke(active);
        }

        internal void InvokeLogAdded(JahroLogGroup.EJahroLogGroup logGroup)
        {
            OnLogAdded?.Invoke(logGroup);
        }

        internal void InvokeLogsClear()
        {
            OnLogsClear?.Invoke();
        }
    }
}