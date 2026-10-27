using System;

namespace CodeLab.Models
{
    public sealed class LearnerDiscussionPost
    {
        public int Id { get; set; }

        public int? CourseId { get; set; }

        public string CourseTitle { get; set; }

        public int AuthorId { get; set; }

        public string AuthorName { get; set; }

        public string AuthorUsername { get; set; }

        public string AuthorRole { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        // Stored and loaded as UTC.
        public DateTime CreatedAt { get; set; }

        public int ReplyCount { get; set; }

        public DateTime? LastReplyAt { get; set; }

        public bool HasTutorReply { get; set; }

        public DateTime LastActivityAt
        {
            get { return LastReplyAt ?? CreatedAt; }
        }
    }
}

