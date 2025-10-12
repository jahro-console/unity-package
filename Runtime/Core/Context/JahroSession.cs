using System;
using System.Collections;
using System.Threading.Tasks;
using JahroConsole.Core.Data;
using JahroConsole.Core.Network;
using JahroConsole.Core.Registry;
using JahroConsole.Core.Snapshots;
using UnityEngine;

namespace JahroConsole.Core.Context
{

    internal class JahroSession
    {

        internal static JahroSession Current
        {
            get
            {
                return _current;
            }
            private set
            {
                _current = value;
            }
        }

        internal string Id
        {
            get
            {
                return _jahroSessionID;
            }
        }

        private static JahroSession _current;

        private JahroContext _context;

        private string _jahroSessionID;

        private JahroSession()
        {
            _jahroSessionID = Guid.NewGuid().ToString();
        }

        internal static void StartNewSession(Action<JahroContext> OnContextLoaded, IProjectSettings projectSettings)
        {
            Current = new JahroSession();

            SnapshotsManager.UploadMode mode = SnapshotsManager.UploadMode.Recording;
            if (projectSettings.SnapshotingMode == IProjectSettings.SnapshotMode.StreamingAll)
            {
                mode = SnapshotsManager.UploadMode.Streaming;
            }
            else if (projectSettings.SnapshotingMode == IProjectSettings.SnapshotMode.StreamingExceptEditor && !Application.isEditor)
            {
                mode = SnapshotsManager.UploadMode.Streaming;
            }
            SnapshotsManager.Instance.Init(Current._jahroSessionID, mode);

            ConsoleStorageController.LoadState();

            JahroContext.Init(ConsoleStorageController.Instance.ConsoleStorage, Current._jahroSessionID, (context) =>
            {
                if (Current == null) return;
                Current._context = context;
                ConsoleCommandsRegistry.Initialize(projectSettings, Current._context);
                if (Current._context.ApiKeyVerified)
                {
                    SnapshotsManager.Instance.InitContext(Current._context);
                }
                OnContextLoaded?.Invoke(Current._context);
            });
        }

        internal static void RefreshSession()
        {
            if (Current._context != null && Current._context.ApiKeyVerified)
            {
                JahroContext.Refresh(ConsoleStorageController.Instance.ConsoleStorage, Current._jahroSessionID, Current._context);
            }
        }

        internal static void PauseSession(bool pause)
        {

        }

        internal static async void EndSession()
        {
            if (Current == null) return;
            await SnapshotsManager.Instance.FlushAsync();
            await NetworkManager.Instance.ShutdownAsync();
            if (Current._context != null) Current._context.Release();
            Current = null;
        }
    }
}