using System.Collections.Generic;
using System.Linq;
using JahroConsole.Core.Logging;
using JahroConsole.Logging;
using JahroConsole.View;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    internal partial class JahroCommandsDataSource
    {
        public const int DEFAULT_SCROLL_CAPACITY = 1000;
        private const int DEFAULT_ITEM_SIZE = 20;
        private const int SEPARATORS_FOR_PORTRAIGHT = 9;
        private const int SEPARATORS_FOR_LANDSCAPE = 2;
        private const float MIN_CONTENT_WIDTH = 370;

        private List<JahroLogEntity> allCommandsCache = new List<JahroLogEntity>(DEFAULT_SCROLL_CAPACITY);
        private List<JahroLogEntity> filteredCommandsCache = new List<JahroLogEntity>(DEFAULT_SCROLL_CAPACITY);
        private List<JahroLogEntity> commandsBatch = new List<JahroLogEntity>(DEFAULT_SCROLL_CAPACITY);
        private JahroDatasourceFilterSettings filterSettings = new JahroDatasourceFilterSettings();
        private ConsoleCommandsFilter filter;
        private JahroScrollView jahroScrollView;
        private JahroCommandsDataSourceCounter counter = new JahroCommandsDataSourceCounter();

        internal JahroCommandsDataSource(JahroScrollView scrollView)
        {
            jahroScrollView = scrollView;
            filter = new ConsoleCommandsFilter(filterSettings);
            filter.FilteringFinished += OnFilteringFinished;

            var separatorsCount = Screen.height > Screen.width ? SEPARATORS_FOR_PORTRAIGHT : SEPARATORS_FOR_LANDSCAPE;
            for (int i = 0; i < separatorsCount; i++)
            {
                allCommandsCache.Insert(0, new JahroLogEntity(string.Empty, string.Empty, EJahroLogType.JahroInfo));
            }
            allCommandsCache.Insert(separatorsCount, new JahroLogEntity(MessagesResource.LogWelcomeMessage, string.Empty, EJahroLogType.JahroInfo));

            jahroScrollView.OnGetHeightForIndexPath += ScrollOnGetOnGetHeightForIndexPath;
            jahroScrollView.OnFillAtIndexPath += OnFillAtIndexPath;
            jahroScrollView.OnItemExpanded += OnItemExpanded;
            jahroScrollView.OnResetExpand += OnResetExpand;

            var text = jahroScrollView.Prefab.GetComponent<JahroScrollItem>();
            TextHeightCaluculator.Instance.SetTextComponents(text.ConsoleCommandTextComponent, text.ConsoleDetailsTextComponent);
        }

        private void OnFilteringFinished(List<JahroLogEntity> obj)
        {
            filteredCommandsCache = obj;
            ResetScroller();
        }

        public void SetFilter(string searchedString)
        {
            filterSettings.SearchString = searchedString;
            filter.FilterCommands(allCommandsCache);
        }

        public void SetFilter(bool showLog, bool showWarning, bool showError, bool showJahroLogs)
        {
            filterSettings.ShowLogs = showLog;
            filterSettings.ShowErrors = showError;
            filterSettings.ShowWarnings = showWarning;
            filterSettings.ShowJahroLogs = showJahroLogs;
            filter.FilterCommands(allCommandsCache);
        }

        private void ResetScroller()
        {
            jahroScrollView.UpdateData(filteredCommandsCache.Count);
        }

        private int GetHeightAtIndexPath(int index)
        {
            var item = filteredCommandsCache[index];
            if (item == null)
            {
                return DEFAULT_ITEM_SIZE;
            }

            if (string.IsNullOrEmpty(item.Message))
            {
                return DEFAULT_ITEM_SIZE;
            }

            if (item.Message == MessagesResource.LogWelcomeMessage)
            {
                return 150;
            }

            // Use cached height if available, otherwise calculate and cache it
            if (item.CachedHeight == -1)
            {
                item.CachedHeight = CalculateItemHeight(item);
            }

            return item.CachedHeight;
        }

        private int CalculateItemHeight(JahroLogEntity item)
        {
            var resultingHeight = TextHeightCaluculator.Instance.GetMainTextHeight(item.Message);

            if (item.Expanded && item.HasDetails)
            {
                resultingHeight += TextHeightCaluculator.Instance.GetDetailsTextHeight(item.StackTrace);
            }

            return resultingHeight;
        }

        public void Update()
        {
            var shouldScrollDown = false;
            if (commandsBatch.Count > 0)
            {
                shouldScrollDown = commandsBatch.Any(x => x.LogType == EJahroLogType.JahroCommand);
                allCommandsCache.AddRange(commandsBatch);
                filter.FilterCommands(allCommandsCache);
                commandsBatch.Clear();
            }
            filter.Update();
            if (shouldScrollDown)
            {
                jahroScrollView.ScrollDown();
            }
        }

        public void SelectAll()
        {
            foreach (var item in filteredCommandsCache)
            {
                if (item.Selectable)
                {
                    item.Selected = true;
                }
            }
            jahroScrollView.SelectAll();
        }

        public void ResetSelection()
        {
            foreach (var item in filteredCommandsCache)
            {
                item.Selected = false;
            }
        }

        public List<JahroLogEntity> GetSelectedItems()
        {
            List<JahroLogEntity> items = new List<JahroLogEntity>();
            foreach (var item in filteredCommandsCache)
            {
                if (item.Selected)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        public JahroCommandsDataSourceCounter GetCounter()
        {
            return counter;
        }

        public void UpdateReferenceSize()
        {
            // Clear global cache and update reference size
            TextHeightCaluculator.Instance.UpdateReferenceSize(jahroScrollView.ContentRect.rect.width);

            // Clear all cached heights in entities
            ClearAllEntityHeights();

            // Force complete recalculation of all heights
            jahroScrollView.ForceHeightRecalculation();
        }

        private void ClearAllEntityHeights()
        {
            // Clear cached heights for all entities
            foreach (var entity in allCommandsCache)
            {
                entity.CachedHeight = -1;
            }
            foreach (var entity in filteredCommandsCache)
            {
                entity.CachedHeight = -1;
            }
        }

        private void OnFillAtIndexPath(int arg1, JahroScrollItem item)
        {
            item.gameObject.SetActive(true);
            item.SetUp(filteredCommandsCache[arg1], filterSettings.SearchString, jahroScrollView.IsSelectMode);
        }

        private int ScrollOnGetOnGetHeightForIndexPath(int index)
        {
            return GetHeightAtIndexPath(index);
        }

        private void OnItemExpanded(JahroScrollItem item)
        {
            // Clear the specific entity's cached height when it expands/collapses
            if (item.AssignedEntity != null)
            {
                item.AssignedEntity.CachedHeight = -1;
            }

            // Force recalculation when item expands/collapses
            jahroScrollView.ForceHeightRecalculation();
        }

        private void OnResetExpand()
        {

        }

        public void Append(string message, string context, EJahroLogType logType)
        {
            commandsBatch.Add(new JahroLogEntity(message, context, logType));
            counter.AppendLog(logType);
        }

        public void Clear()
        {
            allCommandsCache.Clear();
            filter.FilterCommands(allCommandsCache);
            counter.Clear();
        }
    }
}