using System.Collections;
using System.Collections.Generic;
using Jahro.Core.Logging;
using UnityEngine;
using Jahro.Logging;

namespace Jahro.Logging
{
    internal class JahroCommandsDataSourceCounter
    {

        public int Debug { get; private set; }

        public int Warning { get; private set; }

        public int Error { get; private set; }

        public int Commands { get; private set; }

        public void AppendLog(EJahroLogType logType)
        {
            var group = JahroLogGroup.MatchGroup(logType);

            switch (group)
            {
                case JahroLogGroup.EJahroLogGroup.Internal:
                    break;
                case JahroLogGroup.EJahroLogGroup.Debug:
                    Debug++;
                    break;
                case JahroLogGroup.EJahroLogGroup.Warning:
                    Warning++;
                    break;
                case JahroLogGroup.EJahroLogGroup.Error:
                    Error++;
                    break;
                case JahroLogGroup.EJahroLogGroup.Command:
                    Commands++;
                    break;
            }
        }

        public void Clear()
        {
            Debug = 0;
            Warning = 0;
            Error = 0;
            Commands = 0;
        }
    }
}