using System.Collections.Generic;
using System.Text;

namespace Jahro.Core.Logging
{
    internal static class JahroLogsFormatter
    {
        private const string ERROR = "<color=#FA002D>ERROR</color>:";
        private const string ASSERT = "<color=#FA002D>ASSERT</color>:";
        private const string EXCEPTION = "<color=#FA002D><b>EXCEPTION</b></color>:";
        private const string LOG = "<color=#D6D7DF>#</color>";
        private const string WARNING = "<color=#FF5C01>WARNING</color>:";

        private const string JAHRO_COMMAND = "<color=#17E96B>{0}</color>";
        private const string JAHRO_COMMAND_OBJ_COUNT = "<color=#FF5C01>[objects:{0}]</color>";

        private const string TEXT_COLOR_WITH_FILTER = "#6A6A6A";
        private const string TEXT_COLOR_WITHOUT_FILTER = "#D6D7DF";

        internal static string FormatToConsoleMessage(JahroLogEntity entity, string filter)
        {
            var prefix = GetPrefix(entity);

            var filteredMessage = GetFilteredString(entity.Message, filter);
            var color = string.IsNullOrEmpty(filter) ? TEXT_COLOR_WITHOUT_FILTER : TEXT_COLOR_WITH_FILTER;

            return $"{prefix} <color={color}>{filteredMessage}</color>";
        }

        internal static string FormatCommand(string command, object[] parameters)
        {
            string prefixed = "> " + command;
            string commandColored = string.Format(JAHRO_COMMAND, prefixed);
            string parametersAll = "";
            string resultedString = commandColored;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    parametersAll += param.ToString() + " ";
                }
                resultedString += " " + parametersAll;
            }

            return resultedString;
        }

        internal static string FormatCommandResult(string command, object[] parameters, string result)
        {
            string resultedString = "= ";
            string commandColored = string.Format(JAHRO_COMMAND, resultedString);

            if (string.IsNullOrEmpty(result) == false)
            {
                commandColored += result;
            }
            return commandColored;
        }

        internal static string GetPrefix(JahroLogEntity entity)
        {
            string prefix = "";
            switch (entity.LogType)
            {
                case EJahroLogType.JahroError:
                case EJahroLogType.Error:
                    prefix = ERROR;
                    break;
                case EJahroLogType.Assert:
                    prefix = ASSERT;
                    break;
                case EJahroLogType.JahroWarning:
                case EJahroLogType.Warning:
                    prefix = WARNING;
                    break;
                case EJahroLogType.Log:
                    prefix = LOG;
                    break;
                case EJahroLogType.JahroException:
                case EJahroLogType.Exception:
                    prefix = EXCEPTION;
                    break;
                case EJahroLogType.JahroInfo:
                case EJahroLogType.JahroDebug:
                case EJahroLogType.JahroCommand:
                    break;
            }
            return prefix;
        }


        internal static string GetFilteredString(string message, string filter)
        {
            if (!string.IsNullOrEmpty(filter) && message.Contains(filter))
            {
                message = message.Replace(filter, $"<color={TEXT_COLOR_WITHOUT_FILTER}><b>{filter}</b></color>");
            }

            return message;
        }

        internal static string EntriesToReadableFormat(List<JahroLogEntity> entries)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in entries)
            {
                sb.Append(entry.Message);
                sb.AppendLine();
                sb.Append(entry.StackTrace);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        internal static string EntryToReadableFormat(JahroLogEntity entry)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(entry.Message);
            sb.AppendLine();
            sb.Append(entry.StackTrace);
            return sb.ToString();
        }
    }
}