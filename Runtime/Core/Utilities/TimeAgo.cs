using System;

namespace JahroConsole.Core.Utilities
{
    internal static class TimeAgo
    {
        /// <summary>
        /// Converts a DateTime to a user-friendly "time ago" string
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>A UX-focused string describing how much time has passed</returns>
        internal static string GetTimeAgo(DateTime dateTime)
        {
            var now = DateTime.Now;
            var timeSpan = now - dateTime;

            // Handle future dates
            if (timeSpan.TotalSeconds < 0)
            {
                return "in the future";
            }

            // Less than a minute
            if (timeSpan.TotalSeconds < 5)
            {
                return "just now";
            }

            if (timeSpan.TotalSeconds < 60)
            {
                var seconds = (int)timeSpan.TotalSeconds;
                return seconds <= 1 ? "just now" : $"{seconds} seconds ago";
            }

            // Less than an hour
            if (timeSpan.TotalMinutes < 60)
            {
                var minutes = (int)timeSpan.TotalMinutes;
                return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }

            // Less than a day
            if (timeSpan.TotalHours < 24)
            {
                var hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }

            // Less than a week
            if (timeSpan.TotalDays < 7)
            {
                var days = (int)timeSpan.TotalDays;
                return days == 1 ? "1 day ago" : $"{days} days ago";
            }

            // Less than a month (30 days)
            if (timeSpan.TotalDays < 30)
            {
                var weeks = (int)(timeSpan.TotalDays / 7);
                return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
            }

            // Less than a year
            if (timeSpan.TotalDays < 365)
            {
                var months = (int)(timeSpan.TotalDays / 30);
                return months == 1 ? "1 month ago" : $"{months} months ago";
            }

            // More than a year
            var years = (int)(timeSpan.TotalDays / 365);
            return years == 1 ? "1 year ago" : $"{years} years ago";
        }

    }
}
