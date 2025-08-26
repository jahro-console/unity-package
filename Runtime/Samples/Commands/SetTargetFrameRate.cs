using System;
using System.Text;
using UnityEngine;

namespace JahroConsole.Samples
{
    public static class SetTargetFrameRate
    {
        [JahroCommand(name: "set-target-framerate", group: "Samples", description: "Sets target frame rate and measures actual FPS performance")]
        public static string SetTargetFrameRateCommand(int fps = -1)
        {
            try
            {
                return SetFrameRateAndMeasure(fps);
            }
            catch (Exception ex)
            {
                return $"Set target frame rate failed: {ex.Message}";
            }
        }

        private static string SetFrameRateAndMeasure(int fps)
        {
            var stringBuilder = new StringBuilder(1024);

            Application.targetFrameRate = fps;

            stringBuilder.AppendLine($"SetTargetFrameRate -> {fps}");
            stringBuilder.AppendLine("--------------------------------");

            if (fps == -1)
            {
                stringBuilder.AppendLine("Using platform default frame rate");
            }
            else
            {
                stringBuilder.AppendLine($"Target frame rate set to {fps} FPS");
            }

            return stringBuilder.ToString();
        }
    }
}
