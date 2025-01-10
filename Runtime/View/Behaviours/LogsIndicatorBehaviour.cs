using System.Collections;
using System.Collections.Generic;
using Jahro.Logging;
using UnityEngine;
using Jahro.Core.Data;
using Jahro.Core.Logging;
using Jahro.Core.Notifications;

namespace Jahro.View
{
    internal class LogsIndicatorBehaviour : MonoBehaviour
    {
        [SerializeField]
        private BlinkIndicator debugIndicator;

        [SerializeField]
        private BlinkIndicator warningIndicator;

        [SerializeField]
        private BlinkIndicator errorsIndicator;

        [SerializeField]
        private BlinkIndicator commandsIndicator;

        public JahroCommandsDataSourceCounter DataSourceCounter { get; set; }

        private void Start()
        {

        }

        private void OnEnable()
        {
            UpdateCountIndicator();
            NotificationService.Instance.OnLogAdded += OnLogAdded;
            NotificationService.Instance.OnLogsClear += OnLogsClear;
        }

        private void OnDisable()
        {
            NotificationService.Instance.OnLogAdded -= OnLogAdded;
            NotificationService.Instance.OnLogsClear -= OnLogsClear;
        }

        private void UpdateCountIndicator()
        {
            if (DataSourceCounter == null)
            {
                return;
            }

            debugIndicator?.SetCount(DataSourceCounter.Debug);
            warningIndicator?.SetCount(DataSourceCounter.Warning);
            errorsIndicator?.SetCount(DataSourceCounter.Error);
            commandsIndicator?.SetCount(DataSourceCounter.Commands);
        }

        private void OnLogAdded(JahroLogGroup.EJahroLogGroup group)
        {
            UpdateCountIndicator();
            switch (group)
            {
                case JahroLogGroup.EJahroLogGroup.Internal:
                    break;
                case JahroLogGroup.EJahroLogGroup.Debug:
                    debugIndicator.Blink();
                    break;
                case JahroLogGroup.EJahroLogGroup.Warning:
                    warningIndicator.Blink();
                    break;
                case JahroLogGroup.EJahroLogGroup.Error:
                    errorsIndicator.Blink();
                    break;
                case JahroLogGroup.EJahroLogGroup.Command:
                    commandsIndicator.Blink();
                    break;
            }
        }

        private void OnLogsClear()
        {
            debugIndicator.Clear();
            warningIndicator.Clear();
            errorsIndicator.Clear();
            commandsIndicator.Clear();
        }
    }
}