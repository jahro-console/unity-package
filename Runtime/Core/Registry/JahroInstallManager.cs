using Jahro.Core.Data;
using UnityEditor;

namespace Jahro.Core.Registry
{

    [InitializeOnLoad]
    internal class JahroInstallManager : AssetPostprocessor
    {
        static JahroInstallManager()
        {
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                JahroProjectSettings.LoadOrCreate();
            }
        }
    }
}