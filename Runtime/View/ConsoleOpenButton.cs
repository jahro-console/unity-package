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
        private const float NOTIFICATION_LIFETIME_SECONDS = 3f;

        [SerializeField] private ConsoleMainWindow _mainWindow;
        [SerializeField] private GameObject _notificationPrefab;
        [SerializeField] private GameObject _notificationHolder;
        private List<Notification> _notifications = new List<Notification>();
        private Vector2 _dragOffset;
        private RectTransform _holderTransform;
        private RectTransform _canvasTransform;
        private CanvasGroup _canvasGroup;
        private Vector2 _positionPercentage;
        private bool _isInitialized = false;
        internal Action OnConsoleOpenClick;
        internal Action OnSnapshotTakeClick;

        void Awake()
        {
            InitializeComponents();
        }

        void Start()
        {
            SetupEventHandlers();
            LoadPosition();
        }

        void LateUpdate()
        {
            CheckScreenBounds();
        }

        void OnEnable()
        {
            NotificationService.Instance.OnNoficationAdded += OnNoficationAdded;
            _notificationHolder.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            NotificationService.Instance.OnNoficationAdded -= OnNoficationAdded;
            ClearNotifications();
        }

        void OnDestroy()
        {
            CleanupEventHandlers();
        }

        void OnRectTransformDimensionsChange()
        {
            if (gameObject.activeInHierarchy == false || !_isInitialized)
            {
                return;
            }
            StartCoroutine(UpdatePositionNextFrame());
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
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
            StartCoroutine(RestoreVisibility());
        }

        public void OnButtonPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 currentScreenPosition = new Vector2(
                _holderTransform.position.x,
                _holderTransform.position.y
            );
            _dragOffset = pointerEventData.position - currentScreenPosition;
        }

        public void OnButtonPointerDrag(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;

            if ((pointerEventData.pressPosition - pointerEventData.position).magnitude < EventSystem.current.pixelDragThreshold)
            {
                return;
            }

            Vector2 newScreenPosition = pointerEventData.position;
            newScreenPosition.x -= _dragOffset.x;
            newScreenPosition.y -= _dragOffset.y;

            _positionPercentage = new Vector2(
                newScreenPosition.x / Screen.width,
                newScreenPosition.y / Screen.height
            );

            UpdatePositionFromPercentage();
        }

        private void InitializeComponents()
        {
            _holderTransform = GetComponent<RectTransform>();
            _canvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void SetupEventHandlers()
        {
            ConsoleStorageController.Instance.OnStorageSave += SavePosition;
            ConsoleStorageController.Instance.OnStorageLoad += LoadPosition;
            if (_mainWindow != null && _mainWindow.ScalingBehaviour != null)
            {
                _mainWindow.ScalingBehaviour.OnScaleChanged += OnScaleChanged;
            }
        }

        private void CleanupEventHandlers()
        {
            ConsoleStorageController.Instance.OnStorageSave -= SavePosition;
            ConsoleStorageController.Instance.OnStorageLoad -= LoadPosition;
            if (_mainWindow != null && _mainWindow.ScalingBehaviour != null)
            {
                _mainWindow.ScalingBehaviour.OnScaleChanged -= OnScaleChanged;
            }
        }

        private void UpdatePositionFromPercentage()
        {
            if (_canvasTransform == null) return;

            Vector2 screenPosition = new Vector2(
                _positionPercentage.x * Screen.width,
                _positionPercentage.y * Screen.height
            );

            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasTransform,
                screenPosition,
                null,
                out canvasPosition
            );

            _holderTransform.anchoredPosition = canvasPosition;
        }

        private void CheckScreenBounds()
        {
            if (!_isInitialized) return;

            Vector3[] corners = new Vector3[4];
            _holderTransform.GetWorldCorners(corners);
            Rect safeArea = Screen.safeArea;

            Vector2 clampedPercentage = _positionPercentage;
            bool needsUpdate = false;

            if (corners[0].x < safeArea.x)
            {
                clampedPercentage.x = (safeArea.x + (corners[2].x - corners[0].x) * 0.5f) / Screen.width;
                needsUpdate = true;
            }
            else if (corners[2].x > safeArea.x + safeArea.width)
            {
                clampedPercentage.x = (safeArea.x + safeArea.width - (corners[2].x - corners[0].x) * 0.5f) / Screen.width;
                needsUpdate = true;
            }

            if (corners[0].y < safeArea.y)
            {
                clampedPercentage.y = (safeArea.y + (corners[2].y - corners[0].y) * 0.5f) / Screen.height;
                needsUpdate = true;
            }
            else if (corners[2].y > safeArea.y + safeArea.height)
            {
                clampedPercentage.y = (safeArea.y + safeArea.height - (corners[2].y - corners[0].y) * 0.5f) / Screen.height;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                _positionPercentage = clampedPercentage;
                UpdatePositionFromPercentage();
            }
        }

        private IEnumerator UpdatePositionNextFrame()
        {
            yield return null;
            UpdatePositionFromPercentage();
            CheckScreenBounds();
        }

        private void SavePosition(ConsoleStorage storage)
        {
            storage.GeneralSettings.OpenButtonPosition = _positionPercentage;
        }

        private void LoadPosition(ConsoleStorage storage = null)
        {
            var storageToUse = storage ?? ConsoleStorageController.Instance.ConsoleStorage;
            if (storageToUse?.GeneralSettings == null) return;

            var savedPosition = storageToUse.GeneralSettings.OpenButtonPosition;

            if (savedPosition == Vector2.zero)
            {
                SetDefaultPosition();
            }
            else
            {
                _positionPercentage = savedPosition;
                UpdatePositionFromPercentage();
            }

            _isInitialized = true;
        }

        private void SetDefaultPosition()
        {
            Rect safeArea = Screen.safeArea;
            float safeAreaLeft = safeArea.x / Screen.width;
            float safeAreaBottom = safeArea.y / Screen.height;

            _positionPercentage = new Vector2(safeAreaLeft + 0.05f, safeAreaBottom + 0.8f * (safeArea.height / Screen.height));
            UpdatePositionFromPercentage();
        }

        private void OnScaleChanged(float scale)
        {
            UpdatePositionFromPercentage();
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

        private IEnumerator RestoreVisibility()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
    }
}