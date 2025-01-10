using System.Collections.Generic;
using System.Linq;
using Jahro.Core.Logging;
using Jahro.Logging;
using Jahro.View;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.Core.Data
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
                return Mathf.RoundToInt(DEFAULT_ITEM_SIZE);
            }
            var str = item.Message;
            if (string.IsNullOrEmpty(str))
            {
                return Mathf.RoundToInt(DEFAULT_ITEM_SIZE);
            }
            if (str == MessagesResource.LogWelcomeMessage)
            {
                return 150;
            }

            var resultingHeight = TextHeightCaluculator.Instance.GetMainTextHeight(str);

            if (item.Expanded & item.HasDetails)
            {
                resultingHeight += TextHeightCaluculator.Instance.GetMainTextHeight(item.StackTrace);
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
            TextHeightCaluculator.Instance.UpdateReferenceSize(jahroScrollView.ContentRect.rect.width);
            ResetScroller();
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