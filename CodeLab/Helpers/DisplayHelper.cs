using System;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Small formatting helpers for showing data on pages.
    /// </summary>
    public static class DisplayHelper
    {
        /// <summary>
        /// A UTC time as "Just now", "5 min ago", "3 days ago", or "12 Oct 2026" when older than a week.
        /// </summary>
        public static string TimeAgo(DateTime utc)
        {
            DateTime time = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            TimeSpan age = DateTime.UtcNow - time;
            if (age.TotalMinutes < 1) return "Just now";
            if (age.TotalHours < 1) return (int)age.TotalMinutes + " min ago";
            if (age.TotalDays < 1) return (int)age.TotalHours + (age.TotalHours < 2 ? " hour ago" : " hours ago");
            if (age.TotalDays < 7) return (int)age.TotalDays + (age.TotalDays < 2 ? " day ago" : " days ago");
            return time.ToLocalTime().ToString("d MMM yyyy");
        }

        /// <summary>
        /// Up to two initials for an avatar, e.g. "Helena Zhang" gives "HZ".
        /// </summary>
        public static string Initials(string name)
        {
            string[] words = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return "?";
            }

            string first = words[0].Substring(0, 1);
            return words.Length == 1
                ? first.ToUpperInvariant()
                : (first + words[words.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        /// <summary>
        /// The first part of a long text, cut at a word and ending in "...".
        /// </summary>
        public static string Excerpt(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            int cut = text.LastIndexOf(' ', maxLength);
            return text.Substring(0, cut > 0 ? cut : maxLength).TrimEnd() + "...";
        }
    }
}
