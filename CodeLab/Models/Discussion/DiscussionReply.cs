using System;

namespace CodeLab.Models
{
    /// <summary>
    /// A reply to a discussion post, as stored in DiscussionReplies. Replies are flat:
    /// a post has many replies, but a reply cannot have replies of its own.
    /// </summary>
    public class DiscussionReply
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int AuthorId { get; set; }

        // Plain text; show it encoded, keeping line breaks
        public string Content { get; set; }

        // Stored and read as UTC
        public DateTime CreatedAt { get; set; }

        // ----- Filled when reading, for display -----
        public string AuthorName { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorRole { get; set; }
        public string AuthorAvatarUrl { get; set; }

        // The post this reply belongs to (filled for "My replies")
        public string PostTitle { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
    }
}
