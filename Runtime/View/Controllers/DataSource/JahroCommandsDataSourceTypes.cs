using Jahro.Logging;
using Jahro.View;

namespace Jahro.Core.Data
{
    internal class JahroDatasourceFilterSettings
    {
        public bool ShowErrors { get; set; } = true;
        public bool ShowLogs { get; set; } = true;
        public bool ShowWarnings { get; set; } = true;
        public bool ShowJahroLogs { get; set; } = true;
        public string SearchString { get; set; }
    }
}