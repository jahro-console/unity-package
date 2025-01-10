using System;
using System.Collections.Generic;
using Jahro.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Jahro.Core.Data;
using Jahro.Core.Logging;
using Jahro.Core.Notifications;
using UnityEngine.Events;

namespace Jahro.View
{
    internal class JahroScrollView : MonoBehaviour, IDropHandler, IScrollHandler
    {
        private const float MAX_SCROLL_DELTA = 1000f;
        private const int POOL_LENGTH = 60;
        private ScrollRect scrollRect;
        [SerializeField]
        private RectTransform content;
        private Rect container;
        private RectTransform[] rectTransformCache;
        private JahroScrollItem[] objectPool;

        private bool isAutoscrollEnabled;
        private float previousTopPosition;
        private int previousPositionTopPosition;
        private int previousPosition = -1;
        private int count;

        private Dictionary<int, int> heightValuesCache;
        private Dictionary<int, float> positionValuesCache;

        public delegate int HeightItem(int index);

        public HeightItem OnGetHeightForIndexPath;

        public RectTransform ContentRect => content;

        public event Action<int, JahroScrollItem> OnFillAtIndexPath = delegate { };
        public event Action<JahroScrollItem> OnItemExpanded = delegate { };
        public event Action<bool> OnAutoscrollStateChanged = delegate (bool b) { };
        public event Action<JahroLogEntity> OnEntityCopy = delegate { };
        public event Action OnResetExpand = delegate { };

        public bool IsSelectMode { get; private set; }
        public bool MobileMode { get; set; }

        public bool IsAutoscrollEnabled
        {
            get => isAutoscrollEnabled;
            set
            {
                if (value != isAutoscrollEnabled)
                {
                    InvokeAutoscrollChangedEvent(value);
                }
                isAutoscrollEnabled = value;
                if (isAutoscrollEnabled)
                {
                    ScrollDown();
                }
            }
        }
        public GameObject Prefab;

        public int TopPadding = 10;
        public int BottomPadding = 10;
        public int ItemSpacing = 2;


        private void Awake()
        {
            container = GetComponent<RectTransform>().rect;
            scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(OnScrollChange);

            heightValuesCache = new Dictionary<int, int>();
            positionValuesCache = new Dictionary<int, float>();
        }

        private void PerformUpdate()
        {
            var anchoredPosition = content.anchoredPosition;
            var currentTopPosition = anchoredPosition.y - ItemSpacing;
            var difference = Mathf.Abs(currentTopPosition - previousTopPosition);
            var updatesCount = Mathf.Lerp(1, POOL_LENGTH, difference / MAX_SCROLL_DELTA);
            previousTopPosition = currentTopPosition;
            for (int i = 0; i < updatesCount; i++)
            {
                UpdateScroller();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            isAutoscrollEnabled = IsInAutoscrollDistance(content.anchoredPosition);
            InvokeAutoscrollChangedEvent();

            PerformUpdate();
        }

        public void OnScroll(PointerEventData eventData)
        {
            OnDrop(eventData);
        }

        public void ScrollDown()
        {
            var position = content.anchoredPosition;
            position.y = content.rect.height;
            content.anchoredPosition = position;
            previousPosition = count - POOL_LENGTH;

            UpdateScroller(forced: true);
            isAutoscrollEnabled = true;
            InvokeAutoscrollChangedEvent();
        }

        public void SelectMode(bool enabled)
        {
            if (objectPool == null)
            {
                return;
            }

            IsSelectMode = enabled;
            foreach (var item in objectPool)
            {
                item.SelectMode(enabled);
            }
        }

        public void SelectAll()
        {
            foreach (var item in objectPool)
            {
                if (item.AssignedEntity != null && item.AssignedEntity.Selected)
                {
                    item.ForceSetSelection();
                }
            }
        }

        private void UpdateScroller(int setIndex = -1, bool forced = false)
        {
            if (count == 0 || content == null)
            {
                return;
            }

            var topPosition = content.anchoredPosition.y - ItemSpacing;
            if ((topPosition <= 0f && rectTransformCache[0].anchoredPosition.y < -TopPadding - 10f) || forced)
            {
                UpdateData(count);
                return;
            }

            if (topPosition < 0f)
            {
                return;
            }

            if (!positionValuesCache.ContainsKey(previousPosition) || !heightValuesCache.ContainsKey(previousPosition))
            {
                return;
            }

            var itemPosition = Mathf.Abs(positionValuesCache[previousPosition]) + heightValuesCache[previousPosition];
            int position = 0;

            if (setIndex > 0)
            {
                position = setIndex;
            }
            else
            {
                position = topPosition > itemPosition ? previousPosition + 1 : previousPosition - 1;
            }

            if (position < 0 || previousPosition == position)
            {
                return;
            }

            if (position > previousPosition)
            {
                if (position - previousPosition > 1)
                {
                    position = previousPosition + 1;
                }

                var newPosition = position % objectPool.Length;
                newPosition--;
                if (newPosition < 0)
                {
                    newPosition = objectPool.Length - 1;
                }

                int index = position + objectPool.Length - 1;
                if (index < count)
                {
                    var anchoredPosition = rectTransformCache[newPosition].anchoredPosition;
                    anchoredPosition.y = positionValuesCache[index];
                    rectTransformCache[newPosition].anchoredPosition = anchoredPosition;
                    var size = rectTransformCache[newPosition].sizeDelta;
                    size.y = heightValuesCache[index];
                    rectTransformCache[newPosition].sizeDelta = size;
                    objectPool[newPosition].name = index.ToString();
                    OnFillAtIndexPath(index, objectPool[newPosition]);
                }
            }
            else
            {
                if (previousPosition - position > 1)
                {
                    position = previousPosition - 1;
                }

                var newIndex = position % objectPool.Length;
                var anchoredPosition = rectTransformCache[newIndex].anchoredPosition;
                anchoredPosition.y = positionValuesCache[position];
                rectTransformCache[newIndex].anchoredPosition = anchoredPosition;
                var size = rectTransformCache[newIndex].sizeDelta;
                size.y = heightValuesCache[position];
                rectTransformCache[newIndex].sizeDelta = size;
                objectPool[newIndex].name = position.ToString();
                OnFillAtIndexPath(position, objectPool[newIndex]);
            }

            previousPosition = Mathf.Clamp(position, 0, heightValuesCache.Count - 1);
        }

        private void OnScrollChange(Vector2 vector)
        {
            UpdateScroller();
        }

        private void OnItemExpand(JahroScrollItem item, bool expanded)
        {
            UpdateScroller(forced: true);
            OnItemExpanded?.Invoke(item);
            IsAutoscrollEnabled = false;
        }

        private void OnItemCopy(JahroScrollItem item)
        {
            OnEntityCopy(item.AssignedEntity);
            NotificationService.Instance.SendNotification(new Notification("Copied to clipboard"));
        }

        public void UpdateData(int itemsCount)
        {
            if (heightValuesCache == null)
            {
                return;
            }
            var height = CalculateSizesAndPositions(itemsCount);
            CreateViews();
            previousPosition = 0;
            count = itemsCount;
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);
            var pos = content.anchoredPosition;
            var shouldAutoscrollDueToPosition = IsInAutoscrollDistance(pos);

            if (shouldAutoscrollDueToPosition && isAutoscrollEnabled)
            {
                pos.y = content.rect.height;
            }
            var size = Vector2.zero;
            content.anchoredPosition = pos;

            var y = TopPadding;
            var showed = false;
            for (int i = 0; i < objectPool.Length; i++)
            {
                showed = i < itemsCount;
                objectPool[i].gameObject.SetActive(showed);
                if (i + 1 > count)
                {
                    continue;
                }

                pos = rectTransformCache[i].anchoredPosition;
                pos.y = positionValuesCache[i];
                pos.x = 0f;
                rectTransformCache[i].anchoredPosition = pos;
                size = rectTransformCache[i].sizeDelta;
                size.y = heightValuesCache[i];
                rectTransformCache[i].sizeDelta = size;
                y += ItemSpacing + heightValuesCache[i];
                objectPool[i].name = i.ToString();
                OnFillAtIndexPath(i, objectPool[i]);
            }

            for (int i = 0; i < count; i++)
            {
                UpdateScroller();
            }
        }

        private float CalculateSizesAndPositions(int count)
        {
            heightValuesCache.Clear();
            positionValuesCache.Clear();
            float result = 0f;
            for (int i = 0; i < count; i++)
            {
                heightValuesCache[i] = OnGetHeightForIndexPath(i);
                positionValuesCache[i] = -(TopPadding + i * ItemSpacing + result);
                result += heightValuesCache[i];
            }

            result += TopPadding + BottomPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        private void CreateViews()
        {
            if (objectPool != null)
            {
                return;
            }

            objectPool = new JahroScrollItem[POOL_LENGTH];
            for (int i = 0; i < POOL_LENGTH; i++)
            {
                var clone = (GameObject)Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                clone.transform.SetParent(content);
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;
                var rect = clone.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = Vector2.one;
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
                var item = clone.GetComponent<JahroScrollItem>();
                item.ParentScrollView = this;
                item.OnExpand += OnItemExpand;
                item.OnCopy += OnItemCopy;
                objectPool[i] = item;
            }

            rectTransformCache = new RectTransform[objectPool.Length];
            for (int i = 0; i < objectPool.Length; i++)
            {
                rectTransformCache[i] = objectPool[i].gameObject.GetComponent<RectTransform>();
            }
        }

        private void InvokeAutoscrollChangedEvent()
        {
            OnAutoscrollStateChanged(isAutoscrollEnabled);
        }

        private void InvokeAutoscrollChangedEvent(bool value)
        {
            OnAutoscrollStateChanged(value);
        }

        private bool IsInAutoscrollDistance(Vector2 contentPosition) => Mathf.Abs((content.rect.height - contentPosition.y) - scrollRect.viewport.rect.height) < Screen.height * 0.1f;
    }
}