using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class FlexGrid : LayoutGroup
    {
        public static Vector2 Spacing = new Vector2(10,10);

        internal bool NativeOrder {get; set;} 

        private int _rows = 0;

        private float _itemHeight;

        private IFlexGridLayout _gridLayout;

        public void Init(IFlexGridLayout gridLayout)
        {
            _gridLayout = gridLayout;
        }

        public override void CalculateLayoutInputVertical()
        {
            if (_gridLayout == null)
            {
                return;
            }

            CalculateAndSetChildPositions();

            float height = _itemHeight * _rows;
            height += _rows > 1 ? Spacing.y * (_rows - 1) : 0;

            SetLayoutInputForAxis(5, height, 0, 1);
        }

        public override void SetLayoutHorizontal()
        {
            
        }

        public override void SetLayoutVertical()
        {
            
        }

        private void CalculateAndSetChildPositions()
        {
            float gridWidth = rectTransform.rect.width;
            float gridHeight = rectTransform.rect.height;

            var gridItems = _gridLayout.GetOrderedItems();
            Dictionary<int, List<IFlexGridItem>> items = new Dictionary<int, List<IFlexGridItem>>();

            float itemPreferedWidth = _gridLayout.GetPreferedWidth();
            float itemPreferedHeight = _gridLayout.GetPreferedHeight();

            foreach(var item in gridItems)
            {
                var child = item.GetRectTransform();
                float childWidth = itemPreferedWidth * item.GetRequeredSize() + Spacing.x * (item.GetRequeredSize()-1);
                float childHeight = itemPreferedHeight;
                _itemHeight = childHeight;
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, childWidth);
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childHeight);

                int row;
                if (NativeOrder)
                {
                    if (items.Count != 0)
                    {
                        float itemsWidth = GetItemsWidth(items.Last().Value);
                        if (itemsWidth + Spacing.x + childWidth <= gridWidth)
                        {
                            row = items.Last().Key;
                        }   
                        else
                        {
                            row = -1;
                        }
                    }
                    else
                    {
                        row = -1;
                    }
                }
                else
                {
                    row = FindRowForItem(items, gridWidth, childWidth);
                }

                if (row == -1)
                {
                    row = items.Keys.Count;
                    items.Add(items.Keys.Count, new List<IFlexGridItem>());
                }

                float posX = items[row].Count > 0 ? GetItemsWidth(items[row]) + Spacing.x : GetItemsWidth(items[row]);
                float posY = row * _itemHeight + row * Spacing.y;
                SetChildAlongAxis(child, 0, posX);
                SetChildAlongAxis(child, 1, posY);

                items[row].Add(item);
            }

            _rows = items.Count;
        }

        private int FindRowForItem(Dictionary<int, List<IFlexGridItem>> items, float gridWidth, float newItemWidth)
        {
            if (items.Keys.Count == 0)
            {
                return -1;
            }

            foreach(var row in items.Keys)
            {
                float itemsWidth = GetItemsWidth(items[row]);

                if (itemsWidth + Spacing.x + newItemWidth <= gridWidth)
                {
                    return row;
                }
            }
            return -1;
        }

        private float GetItemsWidth(List<IFlexGridItem> items)
        {
            if (items.Count == 0)
            {
                return 0;
            }
            var rectTransform = items.Last().GetRectTransform();
            return rectTransform.anchoredPosition.x + rectTransform.rect.width/2f;
        }
    }

    internal interface IFlexGridItem
    {
        int GetRequeredSize();

        RectTransform GetRectTransform();
    }

    internal interface IFlexGridLayout
    {
        IEnumerable<IFlexGridItem> GetOrderedItems();

        float GetPreferedWidth();

        float GetPreferedHeight();
    }
}