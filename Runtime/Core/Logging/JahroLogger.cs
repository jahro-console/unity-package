using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
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
        internal struct LogMessageData
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

        internal static event JahroCommandInputHandler OnLogEvent = delegate { };

        internal static event Action OnClearAllLogs = delegate { };

        private static JahroLogger _instance;

        private List<LogMessageData> _preSplashLogMessages;

        private ConcurrentQueue<LogMessageData> _logQueue = new ConcurrentQueue<LogMessageData>();

        private static SynchronizationContext _mainThreadContext;

        internal void StartCatching()
        {
            _preSplashLogMessages = new List<LogMessageData>();
            Application.logMessageReceivedThreaded += ApplicationOnlogMessageReceived;

            if (_mainThreadContext == null)
            {
                _mainThreadContext = SynchronizationContext.Current;
            }
        }

        private void ApplicationOnlogMessageReceived(string logString, string stacktrace, LogType type)
        {
            var jahroLogType = (EJahroLogType)(int)type;
            LogUnity(logString, stacktrace, jahroLogType);
        }

        private void ProcessLogs()
        {
            const int maxLogsPerBatch = 100;
            int processedCount = 0;

            while (processedCount < maxLogsPerBatch && _logQueue.TryDequeue(out var logEntry))
            {
                ProcessLogDirectly(logEntry.Log, logEntry.StackTrace, logEntry.Type);
                processedCount++;
            }

            if (_logQueue.Count > 0)
            {
                _mainThreadContext?.Post(_ => ProcessLogs(), null);
            }
        }

        private void ProcessLogDirectly(string message, string details, EJahroLogType logType)
        {
            if (_preSplashLogMessages != null)
            {
                _preSplashLogMessages.Add(new LogMessageData()
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
            Application.logMessageReceivedThreaded -= ApplicationOnlogMessageReceived;
        }

        internal static void LogUnity(string message, string details, EJahroLogType logType)
        {
            if (Thread.CurrentThread.ManagedThreadId == 1 || SynchronizationContext.Current == _mainThreadContext)
            {
                Instance.ProcessLogDirectly(message, details, logType);
            }
            else
            {
                Instance._logQueue.Enqueue(new LogMessageData()
                {
                    Log = message,
                    StackTrace = details,
                    Type = logType
                });
                _mainThreadContext?.Post(_ => Instance.ProcessLogs(), null);
            }
        }

        internal static void Log(string message, string details, EJahroLogType logType)
        {
            LogUnity(message, details, logType);
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

        internal static string GroupToCommonString(EJahroLogType logType)
        {
            switch (logType)
            {
                case EJahroLogType.JahroInfo:
                case EJahroLogType.Assert:
                case EJahroLogType.Log:
                case EJahroLogType.JahroDebug:
                    return "DEBUG";

                case EJahroLogType.Warning:
                case EJahroLogType.JahroWarning:
                    return "WARNING";

                case EJahroLogType.JahroCommand:
                    return "COMMAND";

                case EJahroLogType.Error:
                case EJahroLogType.JahroError:
                    return "ERROR";

                case EJahroLogType.Exception:
                case EJahroLogType.JahroException:
                    return "EXCEPTION";
            }
            return null;
        }
    }
}