using UnityEngine;

namespace JahroConsole.Samples
{
    public static class ClearPlayerPrefs
    {
        [JahroCommand(name: "clear-player-prefs", group: "Samples", description: "Deletes all PlayerPrefs data")]
        public static string ClearPlayerPrefsCommand()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            return "PlayerPrefs cleared";
        }
    }
}
