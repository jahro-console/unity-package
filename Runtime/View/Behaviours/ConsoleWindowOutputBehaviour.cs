using System;
using System.Collections;
using Jahro.Core.Data;
using Jahro.Core.Logging;
using Jahro.Core.Notifications;
using Jahro.Logging;
using UnityEngine;

namespace Jahro.View
{
	internal class ConsoleWindowOutputBehaviour : MonoBehaviour
	{
		public JahroCommandsDataSource DataSource { get; private set; }

		internal FilterTogglesBehaviour FilterTogglesBehaviour;

		[SerializeField]
		internal JahroScrollView JahroScrollView;

		private void Update()
		{
			DataSource.Update();
		}

		public void Start()
		{
			StartCoroutine(LateStart());
		}

		public void Init(bool mobileMode)
		{
			DataSource = new JahroCommandsDataSource(JahroScrollView);
			FilterTogglesBehaviour = GetComponentInChildren<FilterTogglesBehaviour>();

			JahroLogger.OnLogEvent += OnLogEvent;
			JahroLogger.OnClearAllLogs += ClearAll;
			DumpEarlyLogs();
			JahroScrollView.MobileMode = mobileMode;
			JahroScrollView.OnEntityCopy += CopyItem;
		}

		public void ScrollToBottom()
		{
			JahroScrollView.ScrollDown();
		}

		public void SelectMode(bool enabled)
		{
			if (!enabled)
			{
				DataSource?.ResetSelection();
			}
			JahroScrollView.SelectMode(enabled);
		}

		public void CopySelectedItems()
		{
			var items = DataSource.GetSelectedItems();
			if (items.Count > 0)
			{
				JahroEntriesCopyHelper.CopyToClipboard(JahroLogsFormatter.EntriesToReadableFormat(items));
			}
			NotificationService.Instance.SendNotification(new Notification("Copied to clipboard"));
		}

		public void SelectAll()
		{
			DataSource?.SelectAll();
		}

		private void CopyItem(JahroLogEntity item)
		{
			JahroEntriesCopyHelper.CopyToClipboard(JahroLogsFormatter.EntryToReadableFormat(item));
		}

		public void SetFilter(bool showLogs, bool showWarnings, bool showErrors, bool showCommands)
		{
			DataSource?.SetFilter(showLogs, showWarnings, showErrors, showCommands);
		}

		public void SetFilter(string messageString)
		{
			DataSource?.SetFilter(messageString);
		}

		public void OnMainWindowRectChanged(Rect rect)
		{
			DataSource?.UpdateReferenceSize();
		}

		private IEnumerator LateStart()
		{
			yield return new WaitForSeconds(0.1f);
			DataSource?.UpdateReferenceSize();
		}

		private void ClearAll()
		{
			DataSource?.Clear();
		}

		private void OnLogEvent(string message, string context, EJahroLogType logType)
		{
			DataSource?.Append(message, context, logType);
		}

		private void OnDestroy()
		{
			JahroLogger.OnLogEvent -= OnLogEvent;
			JahroLogger.OnClearAllLogs -= ClearAll;
		}

		private void DumpEarlyLogs()
		{
			JahroLogger.Instance.DumpPreSplashScreenMessages();
		}
	}
}