using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using JahroConsole.Core.Notifications;
using JahroConsole.Core.Data;

namespace JahroConsole.View
{
    public class ConsoleOpenButton : MonoBehaviour
    {
        private const float NOTIFICATION_LIFETIME_SECONDS = 5f;

        [SerializeField]
        private ConsoleMainWindow _mainWindow;

        [SerializeField]
        private GameObject _notificationPrefab;

        [SerializeField]
        private GameObject _notificationHolder;

        private List<Notification> _notifications = new List<Notification>();

        private Vector2 _dragOffset;

        private RectTransform _holderTransform;

        private RectTransform _canvasTransform;

        internal Action OnConsoleOpenClick;

        internal Action OnSnapshotTakeClick;

        void Awake()
        {
            _holderTransform = GetComponent<RectTransform>();
            _canvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }

        void Start()
        {
            ConsoleStorageController.Instance.OnStorageSave += OnStateSave;
            LoadState(ConsoleStorageController.Instance.ConsoleStorage);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnOpenClick()
        {
            OnConsoleOpenClick?.Invoke();
        }

        public void OnSnapshotClick()
        {
            OnSnapshotTakeClick?.Invoke();
        }

        private void LateUpdate()
        {
            CheckScreenBounds();
        }

        private void OnEnable()
        {
            NotificationService.Instance.OnNoficationAdded += OnNoficationAdded;
            _notificationHolder.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            NotificationService.Instance.OnNoficationAdded -= OnNoficationAdded;
            ClearNotifications();
        }

        private void OnNoficationAdded(Notification notification)
        {
            _notificationHolder.gameObject.SetActive(true);
            _notifications.Add(notification);

            StartCoroutine(CreateNotificationText(notification));
        }

        private IEnumerator CreateNotificationText(Notification notification)
        {
            var obj = GameObject.Instantiate(_notificationPrefab);
            obj.transform.SetParent(_notificationHolder.transform, false);
            obj.transform.SetAsFirstSibling();
            var text = obj.GetComponentInChildren<Text>(true);
            text.text = notification.Message;
            // LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_notificationsRoot.transform);

            yield return new WaitForSeconds(NOTIFICATION_LIFETIME_SECONDS);
            _notifications.Remove(notification);
            GameObject.Destroy(obj);

            if (_notifications.Count == 0)
            {
                _notificationHolder.gameObject.SetActive(false);
            }
        }

        private void ClearNotifications()
        {
            StopAllCoroutines();
            _notifications.Clear();
            _notificationHolder.gameObject.SetActive(false);
            foreach (Transform child in _notificationHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.GeneralSettings.OpenButtonPosition = _holderTransform.anchoredPosition;
        }

        private void LoadState(ConsoleStorage storage)
        {
            var loadedPosition = storage.GeneralSettings.OpenButtonPosition;
            if (loadedPosition == Vector2.zero)
            {
                loadedPosition.x = Screen.width / 2f;
                loadedPosition.y = Screen.height / 2f;
            }
            _holderTransform.anchoredPosition = loadedPosition;
            CheckScreenBounds();
        }

        public void OnButtonPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 clickLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_holderTransform, pointerEventData.position, pointerEventData.pressEventCamera, out clickLocalPoint);

            _dragOffset.x = clickLocalPoint.x;
            _dragOffset.y = clickLocalPoint.y;
        }

        public void OnButtonPointerDrag(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;

            if ((pointerEventData.pressPosition - pointerEventData.position).magnitude < EventSystem.current.pixelDragThreshold)
            {
                return;
            }

            Vector2 dragLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, pointerEventData.position, pointerEventData.pressEventCamera, out dragLocalPoint);

            dragLocalPoint.x -= _dragOffset.x;
            dragLocalPoint.y -= _dragOffset.y;

            _holderTransform.anchoredPosition = dragLocalPoint;
        }

        private void CheckScreenBounds()
        {
            Vector3[] corners = new Vector3[4];
            _holderTransform.GetWorldCorners(corners);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Vector3 clampedPosition = _holderTransform.position;

            // Check left boundary
            if (corners[0].x < 0)
            {
                clampedPosition.x -= corners[0].x;
            }

            // Check right boundary
            if (corners[2].x > screenWidth)
            {
                clampedPosition.x -= corners[2].x - screenWidth;
            }

            // Check bottom boundary
            if (corners[0].y < 0)
            {
                clampedPosition.y -= corners[0].y;
            }

            // Check top boundary
            if (corners[1].y > screenHeight)
            {
                clampedPosition.y -= corners[1].y - screenHeight;
            }

            _holderTransform.position = clampedPosition;
        }
    }
}