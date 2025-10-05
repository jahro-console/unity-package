using System;
using JahroConsole.Core.Logging;

namespace JahroConsole
{
    public static partial class Jahro
    {
        /// <summary>
        /// Prints a message to the Jahro Console.
        /// </summary>
        /// <param name="message">The primary message to log.</param>
        [Obsolete("Use Unity Debug.Log instead")]
        public static void Log(string message)
        {
            Log(message, string.Empty);
        }

        /// <summary>
        /// Prints a detailed message to the Jahro Console.
        /// </summary>
        /// <param name="message">The primary message.</param>
        /// <param name="details">Additional details that will be displayed when expanded.</param>
        [Obsolete("Use Unity Debug.Log instead")]
        public static void Log(string message, string details)
        {
            LogDebug(message, details);
        }

        /// <summary>
        /// Prints a debug-level message to the Jahro Console.
        /// </summary>
        /// <param name="message">The debug message to log.</param>
        [Obsolete("Use Unity Debug.Log instead")]
        public static void LogDebug(string message)
        {
            LogDebug(message, string.Empty);
        }

        /// <summary>
        /// Prints a debug-level message with details to the Jahro Console.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="details">Additional details that will be displayed when expanded.</param>
        [Obsolete("Use Unity Debug.Log instead")]
        public static void LogDebug(string message, string details)
        {
            JahroLogger.Log(message, details, EJahroLogType.JahroDebug);
        }

        /// <summary>
        /// Prints a warning-level message to the Jahro Console.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        [Obsolete("Use Unity Debug.LogWarning instead")]
        public static void LogWarning(string message)
        {
            LogWarning(message, string.Empty);
        }

        /// <summary>
        /// Prints a warning-level message with details to the Jahro Console.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="details">Additional details that will be displayed when expanded.</param>
        [Obsolete("Use Unity Debug.LogWarning instead")]
        public static void LogWarning(string message, string details)
        {
            JahroLogger.Log(message, details, EJahroLogType.JahroWarning);
        }

        /// <summary>
        /// Prints a formatted exception message to the Jahro Console.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        [Obsolete("Use Unity Debug.LogException instead")]
        public static void LogException(Exception exception)
        {
            JahroLogger.Log(exception.Message, exception.StackTrace, EJahroLogType.JahroException);
        }

        /// <summary>
        /// Prints a formatted exception message with a custom message to the Jahro Console.
        /// </summary>
        /// <param name="message">A custom message describing the exception.</param>
        /// <param name="exception">The exception to log.</param>
        [Obsolete("Use Unity Debug.LogException instead")]
        public static void LogException(string message, Exception exception)
        {
            JahroLogger.Log(message, exception.StackTrace, EJahroLogType.JahroException);
        }

        /// <summary>
        /// Prints an error-level message to the Jahro Console.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        [Obsolete("Use Unity Debug.LogError instead")]
        public static void LogError(string message)
        {
            LogError(message, string.Empty);
        }

        /// <summary>
        /// Prints an error-level message with details to the Jahro Console.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="details">Additional details that will be displayed when expanded.</param>
        [Obsolete("Use Unity Debug.LogError instead")]
        public static void LogError(string message, string details)
        {
            JahroLogger.Log(message, details, EJahroLogType.JahroError);
        }

        /// <summary>
        /// Clears all logs from the Jahro Console.
        /// </summary>
        [Obsolete("Use Unity Debug.LogClearAllLogs instead")]
        public static void ClearAllLogs()
        {
            JahroLogger.ClearAllLogs();
        }
    }
}