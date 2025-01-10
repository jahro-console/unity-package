using UnityEngine;
using System.Collections;
using System;
using Jahro.Core.Notifications;
using UnityEngine.UI;

namespace Jahro.View
{
    public class GlobalNotifications : MonoBehaviour
    {
        public const float NOTIFICATION_LIFETIME_SECONDS = 5;

        [SerializeField]
        private GameObject _notificationsRoot;

        [SerializeField]
        private GameObject _notificationObjectPrefab;

        void Awake()
        {
        }

        void Start()
        {

        }

        private void OnEnable()
        {
            NotificationService.Instance.OnNoficationAdded += OnNoficationAdded;
        }

        private void OnDisable()
        {
            NotificationService.Instance.OnNoficationAdded -= OnNoficationAdded;
            ClearNotifications();
        }

        private void OnNoficationAdded(Notification notification)
        {
            StartCoroutine(CreateNotification(notification.Message));
        }

        private IEnumerator CreateNotification(string message)
        {

            var obj = GameObject.Instantiate(_notificationObjectPrefab);
            obj.transform.SetParent(_notificationsRoot.transform, false);
            obj.transform.SetAsFirstSibling();
            var text = obj.GetComponentInChildren<Text>(true);
            text.text = message;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_notificationsRoot.transform);
            yield return new WaitForSeconds(NOTIFICATION_LIFETIME_SECONDS);

            GameObject.Destroy(obj);
        }

        private void ClearNotifications()
        {
            StopAllCoroutines();
            foreach (Transform child in _notificationsRoot.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}