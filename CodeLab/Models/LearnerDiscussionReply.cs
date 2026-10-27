using System;

namespace CodeLab.Models
{
    public sealed class LearnerDiscussionReply
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public string PostTitle { get; set; }

        public int? CourseId { get; set; }

        public string CourseTitle { get; set; }

        public int AuthorId { get; set; }

        public string AuthorName { get; set; }

        public string AuthorUsername { get; set; }

        public string AuthorRole { get; set; }

        public string Content { get; set; }

        // Stored and loaded as UTC.
        public DateTime CreatedAt { get; set; }
    }
}