using System;

namespace CodeLab.Models
{
    /// <summary>
    /// A lesson inside a course, as stored in the Lessons table.
    /// </summary>
    public class Lesson
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }

        // Written in Markdown; show it with MarkdownHelper.ToHtml
        public string Content { get; set; }

        public string CodeSnippet { get; set; }
        public string CodeLanguage { get; set; }

        // App-relative path of the attached file, e.g. "~/Uploads/Attachments/l2-....pdf"; null if none
        public string AttachmentUrl { get; set; }

        // The file's original name, shown to learners
        public string AttachmentName { get; set; }

        // Optional YouTube link, stored as https://www.youtube.com/watch?v=ID; show it with YouTubeHelper.EmbedHtml
        public string VideoUrl { get; set; }

        // Position in the course, starting at 1
        public int OrderIndex { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool HasCode
        {
            get { return !string.IsNullOrWhiteSpace(CodeSnippet); }
        }

        public bool HasVideo
        {
            get { return !string.IsNullOrEmpty(VideoUrl); }
        }

        public bool HasAttachment
        {
            get { return !string.IsNullOrEmpty(AttachmentUrl); }
        }
    }
}
