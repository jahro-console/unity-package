using System;
using System.Collections;
using Jahro.Core.Registry;
using Jahro.Core.Data;
using Jahro.Core.Snapshots;
using UnityEngine;

namespace Jahro.Core.Context
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

        private static JahroSession _current;

        private JahroContext _context;

        private string _sessionID;

        private JahroSession()
        {
            _sessionID = Guid.NewGuid().ToString();
        }

        internal static IEnumerator StartNewSession(Action<JahroContext> OnContextLoaded, IProjectSettings projectSettings)
        {
            SnapshotsManager.Instance.StartRecording();
            Current = new JahroSession();
            ConsoleStorageController.LoadState();

            yield return JahroContext.InitCoroutine(ConsoleStorageController.Instance.ConsoleStorage, Current._sessionID, (context) =>
            {
                Current._context = context;
                ConsoleCommandsRegistry.Initialize(projectSettings, Current._context);
                SnapshotsManager.Instance.InitContext(Current._context);
                OnContextLoaded?.Invoke(Current._context);
            });
        }

        internal static IEnumerator RefreshSession()
        {
            if (Current._context != null)
            {
                yield return JahroContext.RefreshCoroutine(ConsoleStorageController.Instance.ConsoleStorage, Current._sessionID, Current._context);
            }
        }

        internal static void PauseSession(bool pause)
        {

        }

        internal static void EndSession()
        {
            SnapshotsManager.Instance.StopRecording();
        }
    }
}