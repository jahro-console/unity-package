using System.Collections.Generic;
using Jahro.Core;
using Jahro.Core.Watcher;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Jahro.Logging;

namespace Jahro.View
{
    internal class WatcherModalView : MonoBehaviour, IPointerClickHandler
    {
        
        public RectTransform ModalViewTransform;

        public Text TitleText;

        public Text DescriptionText;

        public Button CloseButton;

        public Toggle FavoritesToggle;

        public Text DetailsText;

        public ScrollRect DynamicScrollContent;

        public RectTransform ParamsContentHolder;

        internal ConsoleWatcherEntry CurrentWatcherEntry { get; private set; }

        public WatcherItem CurrentWatcherItem { get; private set; }

        private RectTransform _parentRect;

        private LayoutElement _dynamicContentHolderLayout;

        private float _keyboardHeightPlace;

        private bool _subscribed;

        private void Awake()
        {
            _dynamicContentHolderLayout = DynamicScrollContent.GetComponent<LayoutElement>();

            CloseButton.onClick.AddListener(OnCloseClick);
            
            FavoritesToggle.onValueChanged.AddListener(OnFavoritesStateChanged);
        }

        private void Start()
        {
            // gameObject.SetActive(false);
        }

        public void Open(WatcherItem item, ConsoleMainWindow mainWindow, RectTransform parentRect)
        {
            _parentRect = parentRect;
            CurrentWatcherItem = item;
            CurrentWatcherEntry = item.WatcherEntry;

            TitleText.text = CurrentWatcherEntry.Name;

            DynamicScrollContent.verticalScrollbar.value = 1f;
            gameObject.SetActive(true);
            
            if (!_subscribed) mainWindow.OnWindowSizeChanged += OnMainWindowSizeChanged;
            _subscribed = true;

            SetDescription(CurrentWatcherEntry);
            UpdateDetails(CurrentWatcherEntry);
            FavoritesToggle.isOn = CurrentWatcherEntry.Favorite;
            SetPosition(_parentRect);
            ConsoleEvents.Instance.OpenedDetailsWindow();
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (CurrentWatcherEntry != null)
            {
                UpdateDetails(CurrentWatcherEntry);
            }
        }

        void LateUpdate()
        {
            var inp = GetComponentInChildren<InputField>();
            if (IsOpen() && inp != null && inp.isFocused)
            {
                float keyboardHeight = KeyboardTracker.GetSoftKeyboardHeight();

                Vector2 screenKeyboard = new Vector2(0, keyboardHeight*Screen.height);
                Vector2 rectPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(ModalViewTransform, screenKeyboard, null, out rectPoint);                

                float holderHeight = Mathf.Max(0, rectPoint.y - ModalViewTransform.rect.min.y);
                if (holderHeight != 0) 
                {
                    _keyboardHeightPlace = holderHeight;
                    SetPosition(_parentRect);
                }
            }
            else
            {
                _keyboardHeightPlace = 0;
            }
        }

        public void UpdateDynamicContentHolder()
        {
            float height = 0f;
            for (int i=0; i<DynamicScrollContent.content.childCount; i++)
            {
                var child = DynamicScrollContent.content.GetChild(i);
                height += LayoutUtility.GetPreferredHeight((RectTransform)child) + 5f;
            }
            _dynamicContentHolderLayout.preferredHeight = Mathf.Min(height, 55f);
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var clickOnModalView = eventData.pointerPressRaycast.gameObject == this.gameObject;
            if (clickOnModalView)
            {
                Close();
            }
        }

        private void OnCloseClick()
        {
            Close();
        }

        private void OnFavoritesStateChanged(bool isOn)
        {
            CurrentWatcherEntry.SetFavorite(isOn);
        }

        private void OnMainWindowSizeChanged()
        {
            Close();    
        }

        private void SetDescription(ConsoleWatcherEntry entry)
        {
            string descriptionText = "";
            if (string.IsNullOrEmpty(entry.Description) == false)
            {
                descriptionText += entry.Description + "\n";
            }
            descriptionText += entry.GetTypeDescription();
            DescriptionText.text = descriptionText;   
        }

        private void UpdateDetails(ConsoleWatcherEntry entry)
        {
            DetailsText.text = entry.GetFullDetails();

            DynamicScrollContent.gameObject.SetActive(true);
            UpdateDynamicContentHolder();
        }

        private float CalculateModalViewHeight()
        {
            float spacing = ModalViewTransform.GetComponent<VerticalLayoutGroup>().spacing;
            float size = 0;
            size += TitleText.transform.parent.GetComponent<LayoutElement>().preferredHeight + spacing;
            size += CloseButton.transform.parent.GetComponent<LayoutElement>().preferredHeight;
            size += DynamicScrollContent.gameObject.activeSelf ? _dynamicContentHolderLayout.preferredHeight + spacing: 0f;
            size += DescriptionText.gameObject.activeSelf ? LayoutUtility.GetPreferredHeight((RectTransform)DescriptionText.transform) + spacing : 0f;
            return size;
        }

        private void SetPosition(RectTransform parentRect)
        {

            var commandRect = CurrentWatcherItem.GetComponent<RectTransform>();

            ModalViewTransform.position = commandRect.position;
            Vector2 deltaX = Vector2.left * commandRect.rect.width/2f;
            Vector2 deltaY = Vector2.up * commandRect.rect.height/2f;
            Vector2 pivot = new Vector2(0f, 1f);
            ModalViewTransform.pivot = pivot;
            ModalViewTransform.anchoredPosition += deltaX + deltaY;

            deltaY.y = _keyboardHeightPlace;    //Apply shift from keyboard on Y

            Vector2 parentSize = Vector2.Scale(parentRect.rect.size, parentRect.lossyScale); //real size
            Vector2 modalSize = Vector2.Scale(ModalViewTransform.rect.size, ModalViewTransform.lossyScale); //real size

            if (ModalViewTransform.position.x + modalSize.x > parentSize.x) //if view doesn't fit parent width
            {
                pivot.x = 1f;
                ModalViewTransform.pivot = pivot;
                ModalViewTransform.anchoredPosition += Vector2.right * commandRect.rect.width;
            }

            if (parentSize.x < modalSize.x * 2f)    //if view doesn't fit screen
            {
                pivot.x = 0.5f;
                ModalViewTransform.pivot = pivot;
                float x = parentRect.position.x + parentSize.x/2f;
                ModalViewTransform.position = new Vector2(x, commandRect.position.y);
                ModalViewTransform.anchoredPosition += Vector2.up * commandRect.rect.height/2f;
            }
            
            float modalViewHeight = CalculateModalViewHeight();
            float headerHeight = 52f;
            if (Mathf.Abs(ModalViewTransform.anchoredPosition.y - modalViewHeight + headerHeight) > parentRect.rect.size.y)
            {
                pivot.y = 0f;
                deltaY += Vector2.down * commandRect.rect.height;
            }
            
            ModalViewTransform.pivot = pivot;
            ModalViewTransform.anchoredPosition += Vector2.up * deltaY;   
        }
    }
}