using JahroConsole.Core.Logging;
using JahroConsole.Core.Notifications;
using JahroConsole.Logging;
using UnityEngine;

namespace JahroConsole.View
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

        private GameObject _debugIndicatorGroup;
        private GameObject _warningIndicatorGroup;
        private GameObject _errorsIndicatorGroup;
        private GameObject _commandsIndicatorGroup;

        public JahroCommandsDataSourceCounter DataSourceCounter { get; set; }

        private void Awake()
        {
            _debugIndicatorGroup = debugIndicator.gameObject.transform.parent.gameObject;
            _warningIndicatorGroup = warningIndicator.gameObject.transform.parent.gameObject;
            _errorsIndicatorGroup = errorsIndicator.gameObject.transform.parent.gameObject;
            _commandsIndicatorGroup = commandsIndicator.gameObject.transform.parent.gameObject;
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
            _debugIndicatorGroup.SetActive(DataSourceCounter.Debug > 0);
            debugIndicator?.SetCount(DataSourceCounter.Debug);
            _warningIndicatorGroup.SetActive(DataSourceCounter.Warning > 0);
            warningIndicator?.SetCount(DataSourceCounter.Warning);
            _errorsIndicatorGroup.SetActive(DataSourceCounter.Error > 0);
            errorsIndicator?.SetCount(DataSourceCounter.Error);
            _commandsIndicatorGroup.SetActive(DataSourceCounter.Commands > 0);
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