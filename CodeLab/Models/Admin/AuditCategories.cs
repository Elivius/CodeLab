using System;

namespace CodeLab.Models
{
    /// <summary>
    /// The audit log categories. There is no category column: AuditRepository works each one out
    /// from the action name, so existing log rows get a category too.
    /// </summary>
    public static class AuditCategories
    {
        public const string Security = "Security";
        public const string Administrative = "Administrative";
        public const string Content = "Content";
        public const string Deletion = "Deletion";

        /// <summary>
        /// The category that matches, ignoring case, or null if none does.
        /// </summary>
        public static string Find(string category)
        {
            return Array.Find(new[] { Security, Administrative, Content, Deletion },
                item => string.Equals(item, category, StringComparison.OrdinalIgnoreCase));
        }
    }
}
