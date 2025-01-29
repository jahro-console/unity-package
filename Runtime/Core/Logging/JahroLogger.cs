using System;
using System.Collections.Generic;
using JahroConsole.Core.Notifications;
using UnityEngine;

namespace JahroConsole.Core.Logging
{
    internal delegate void JahroCommandInputHandler(string message, string context, EJahroLogType logType);

    internal enum EJahroLogType
    {
        Error,
        Assert,
        Warning,
        Log,
        Exception,
        JahroException,
        JahroWarning,
        JahroCommand,
        JahroError,
        JahroDebug,
        JahroInfo
    }

    internal class JahroLogger : IDisposable
    {
        internal struct PreSplashLogMessage
        {
            internal string Log;
            internal string StackTrace;
            internal EJahroLogType Type;
        }

        internal static JahroLogger Instance
        {
            get
            {
                if (_instance == null) _instance = new JahroLogger();
                return _instance;
            }
        }

        internal static bool DuplicateToUnityConsole { get; set; }

        internal static event JahroCommandInputHandler OnLogEvent = delegate { };

        internal static event Action OnClearAllLogs = delegate { };

        private static JahroLogger _instance;

        private bool _onHoldForIncomingLog;

        private List<PreSplashLogMessage> _preSplashLogMessages;

        internal void StartCatching()
        {
#if JAHRO_DEBUG
            Debug.Log("Logger start catching...");
#endif
            _preSplashLogMessages = new List<PreSplashLogMessage>();
            Application.logMessageReceived += ApplicationOnlogMessageReceived;
        }

        private void ApplicationOnlogMessageReceived(string logString, string stacktrace, LogType type)
        {
            if (DuplicateToUnityConsole && _onHoldForIncomingLog)
            {
                _onHoldForIncomingLog = false;
                return;
            }

            var jahroLogType = (EJahroLogType)(int)type;
            JahroLogger.LogUnity(logString, stacktrace, jahroLogType);
        }

        internal void DumpPreSplashScreenMessages()
        {
            if (_preSplashLogMessages != null)
            {
                foreach (var m in _preSplashLogMessages)
                {
                    OnLogEvent(m.Log, m.StackTrace, m.Type);
                    NotificationService.Instance.InvokeLogAdded(JahroLogGroup.MatchGroup(m.Type));
                }
                ResetDump();
            }
        }

        internal void ResetDump()
        {
            if (_preSplashLogMessages != null)
            {
                _preSplashLogMessages.Clear();
                _preSplashLogMessages = null;
            }
        }

        public void Dispose()
        {
            Application.logMessageReceived -= ApplicationOnlogMessageReceived;
        }

        internal static void LogUnity(string message, string details, EJahroLogType logType)
        {
            if (Instance._preSplashLogMessages != null)
            {
                Instance._preSplashLogMessages.Add(new PreSplashLogMessage()
                {
                    StackTrace = details,
                    Log = message,
                    Type = logType
                });
            }
            else
            {
                OnLogEvent(message, details, logType);
                NotificationService.Instance.InvokeLogAdded(JahroLogGroup.MatchGroup(logType));
            }
        }

        internal static void Log(string message, string details, EJahroLogType logType)
        {
            LogUnity(message, details, logType);

            if (DuplicateToUnityConsole)
            {
                Instance._onHoldForIncomingLog = true;
                ToUnityConsole(message, details, logType);
            }
        }

        internal static void LogCommand(string command, object[] parameters)
        {
            Log(JahroLogsFormatter.FormatCommand(command, parameters), "", EJahroLogType.JahroCommand);
        }

        internal static void LogCommandResult(string command, object[] parameters, string result)
        {
            Log(JahroLogsFormatter.FormatCommandResult(command, parameters, result), "", EJahroLogType.JahroCommand);
        }

        internal static void ClearAllLogs()
        {
            OnClearAllLogs();
            NotificationService.Instance.InvokeLogsClear();
        }

        internal static void LogDebug(string message)
        {
            Log(message, string.Empty, EJahroLogType.JahroDebug);
        }

        internal static void LogWarning(string message)
        {
            Log(message, string.Empty, EJahroLogType.JahroWarning);
        }

        internal static void LogError(string message)
        {
            Log(message, string.Empty, EJahroLogType.JahroError);
        }

        internal static void LogException(string message, Exception e)
        {
            Log(message, e.StackTrace, EJahroLogType.JahroException);
        }

        internal static void ToUnityConsole(string message, string details, EJahroLogType logType)
        {
            var logGroup = JahroLogGroup.MatchGroup(logType);
            switch (logGroup)
            {
                case JahroLogGroup.EJahroLogGroup.Internal:
                    break;
                case JahroLogGroup.EJahroLogGroup.Debug:
                    Debug.Log(message);
                    break;
                case JahroLogGroup.EJahroLogGroup.Warning:
                    Debug.LogWarning(message);
                    break;
                case JahroLogGroup.EJahroLogGroup.Error:
                    Debug.LogError(message);
                    break;
                case JahroLogGroup.EJahroLogGroup.Command:
                    Debug.Log(message + "\n" + details);
                    break;
            }
        }


    }

    internal class JahroLogGroup
    {

        internal enum EJahroLogGroup
        {
            Internal,
            Debug,
            Warning,
            Error,
            Command
        }

        internal static EJahroLogGroup MatchGroup(EJahroLogType logType)
        {
            EJahroLogGroup group = EJahroLogGroup.Internal;
            switch (logType)
            {
                case EJahroLogType.JahroInfo:
                    break;

                case EJahroLogType.Assert:
                case EJahroLogType.Log:
                case EJahroLogType.JahroDebug:
                    group = EJahroLogGroup.Debug;
                    break;

                case EJahroLogType.Warning:
                case EJahroLogType.JahroWarning:
                    group = EJahroLogGroup.Warning;
                    break;

                case EJahroLogType.JahroCommand:
                    group = EJahroLogGroup.Command;
                    break;

                case EJahroLogType.Error:
                case EJahroLogType.Exception:
                case EJahroLogType.JahroException:
                case EJahroLogType.JahroError:
                    group = EJahroLogGroup.Error;
                    break;
            }
            return group;
        }
    }
}