using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JahroConsole.Core.Logging;

namespace JahroConsole.Core.Data
{
    internal class ConsoleCommandsFilter
    {
        private List<JahroLogEntity> filteredList = new List<JahroLogEntity>(JahroCommandsDataSource.DEFAULT_SCROLL_CAPACITY);
        private bool isFiltering;
        private bool isFilteringFinished;
        private JahroDatasourceFilterSettings filterSettings;
        public event Action<List<JahroLogEntity>> FilteringFinished;

        public ConsoleCommandsFilter(JahroDatasourceFilterSettings settings)
        {
            filterSettings = settings;
        }

        public void Update()
        {
            if (isFiltering && isFilteringFinished)
            {
                FilteringFinished?.Invoke(new List<JahroLogEntity>(filteredList));
                isFiltering = false;
                filteredList.Clear();
            }
        }

        public void FilterCommands(List<JahroLogEntity> unfiltered)
        {
#if UNITY_WEBGL	
            PerformFiltering(unfiltered);
#else
            ThreadPool.QueueUserWorkItem(PerformFiltering, unfiltered);
#endif
        }

        private void PerformFiltering(object unfiltered)
        {
            isFiltering = true;
            isFilteringFinished = false;
            filteredList.Clear();
            var unfilteredList = unfiltered as List<JahroLogEntity>;
            if (unfilteredList == null)
            {
                throw new InvalidDataException("Data seems to be corrupted");
            }

            foreach (var jahroCommandEntity in unfilteredList)
            {
                if (IsCommandCorrespondsToFilter(jahroCommandEntity))
                {
                    filteredList.Add(jahroCommandEntity);
                }
            }

            isFilteringFinished = true;
        }

        public bool IsCommandCorrespondsToFilter(JahroLogEntity command)
        {
            var messageAtIndexType = command.LogType;

            if (messageAtIndexType == EJahroLogType.JahroInfo)
            {
                return true;
            }

            if (!filterSettings.ShowErrors && (messageAtIndexType == EJahroLogType.Assert || messageAtIndexType == EJahroLogType.Error
                || messageAtIndexType == EJahroLogType.Exception || messageAtIndexType == EJahroLogType.JahroError || messageAtIndexType == EJahroLogType.JahroException))
            {
                return false;
            }

            if (!filterSettings.ShowWarnings && (messageAtIndexType == EJahroLogType.Warning || messageAtIndexType == EJahroLogType.JahroWarning))
            {
                return false;
            }

            if (!filterSettings.ShowLogs && (messageAtIndexType == EJahroLogType.Log || messageAtIndexType == EJahroLogType.JahroDebug))
            {
                return false;
            }

            if (!filterSettings.ShowJahroLogs && (messageAtIndexType == EJahroLogType.JahroCommand))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(filterSettings.SearchString) && (!command.Message.Contains(filterSettings.SearchString)))
            {
                return false;
            }

            return true;
        }
    }
}