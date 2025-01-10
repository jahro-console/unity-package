namespace Jahro.Core.Logging
{
    internal class JahroLogEntity
    {
        private string message;
        private string stackTrace;
        private EJahroLogType logType;
        private long detailsPosition = -1;
        internal string Message => message;
        internal EJahroLogType LogType => logType;

        internal bool Expanded { get; set; }
        internal bool Selected { get; set; }
        internal bool HasDetails { get { return detailsPosition != -1; } }
        internal bool Selectable { get { return logType != EJahroLogType.JahroInfo; } }

        internal string StackTrace { get { return ReadStackTrace(); } }

        internal JahroLogEntity(string message, string stackTrace, EJahroLogType logType)
        {
            this.message = message;
            this.logType = logType;

            if (string.IsNullOrEmpty(stackTrace) == false)
            {
                detailsPosition = LogFileManager.SaveDetailsMessage(stackTrace.Trim());
            }
        }

        private string ReadStackTrace()
        {
            if (!HasDetails)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(stackTrace) == false)
            {
                return stackTrace;
            }
            else
            {
                stackTrace = LogFileManager.ReadDetailsMessage(detailsPosition);
                return stackTrace;
            }
        }
    }
}
