using System;
using System.Text;
using UnityEngine;

namespace JahroConsole.Samples
{
    public static class SetQualityLevel
    {
        [JahroCommand(name: "set-quality-level", group: "Samples", description: "Changes quality level by name or index")]
        public static string SetQualityLevelCommand(string nameOrIndex)
        {
            try
            {
                return ChangeQualityLevel(nameOrIndex);
            }
            catch (Exception ex)
            {
                return $"Set quality level failed: {ex.Message}";
            }
        }

        private static string ChangeQualityLevel(string nameOrIndex)
        {
            var stringBuilder = new StringBuilder(1024);
            var previousLevel = QualitySettings.GetQualityLevel();
            var previousName = QualitySettings.names[previousLevel];

            int newLevel;
            if (int.TryParse(nameOrIndex, out int index))
            {
                if (index < 0 || index >= QualitySettings.names.Length)
                {
                    return $"Invalid quality level index. Valid range: 0-{QualitySettings.names.Length - 1}";
                }
                newLevel = index;
            }
            else
            {
                newLevel = Array.FindIndex(QualitySettings.names, x =>
                    x.Equals(nameOrIndex, StringComparison.OrdinalIgnoreCase));

                if (newLevel == -1)
                {
                    var validLevels = string.Join(", ", QualitySettings.names);
                    return $"Invalid quality level name. Valid names: {validLevels}";
                }
            }

            QualitySettings.SetQualityLevel(newLevel, true);
            var newName = QualitySettings.names[newLevel];

            stringBuilder.AppendLine($"SetQualityLevel -> {newName}");
            stringBuilder.AppendLine("--------------------------------");
            stringBuilder.AppendLine($"Previous: {previousName} (index {previousLevel})");
            stringBuilder.AppendLine($"New: {newName} (index {newLevel})");

            return stringBuilder.ToString();
        }
    }
}
