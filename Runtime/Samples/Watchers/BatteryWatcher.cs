using UnityEngine;

namespace JahroConsole.Samples
{
    public static class BatteryWatcher
    {
        private const float LOW_BATTERY_THRESHOLD = 0.20f;

        [JahroWatch(name: "battery", group: "Samples", description: "Battery level and charging status")]
        public static string Status
        {
            get
            {
                if (!Application.isPlaying)
                    return null;

                var batteryLevel = SystemInfo.batteryLevel;
                if (batteryLevel < 0)
                    return "Battery status unavailable";

                var status = SystemInfo.batteryStatus switch
                {
                    BatteryStatus.Charging => "Charging",
                    BatteryStatus.Discharging => "Discharging",
                    BatteryStatus.NotCharging => "Plugged (Not Charging)",
                    BatteryStatus.Full => "Fully Charged",
                    _ => "Unknown"
                };

                var level = $"{batteryLevel * 100:F0}%";
                var color = batteryLevel <= LOW_BATTERY_THRESHOLD ? "red" : "#17E96B";
                return $"<color={color}>{level} - {status}</color>";
            }
        }
    }
}