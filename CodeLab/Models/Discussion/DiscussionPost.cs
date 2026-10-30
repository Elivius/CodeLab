using System;
using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// A discussion post (a question or topic) in a course's board, as stored in DiscussionPosts,
    /// with its author's details for display. Shared by the Tutor and Learner forums.
    /// </summary>
    public class DiscussionPost
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public int AuthorId { get; set; }
        public string Title { get; set; }

        // Plain text; show it encoded, keeping line breaks
        public string Content { get; set; }

        // Stored and read as UTC
        public DateTime CreatedAt { get; set; }

        // ----- Filled when reading, for display -----
        public string CourseTitle { get; set; }

        // The course's tutor, so their posts can show an "Instructor" badge
        public int CourseInstructorId { get; set; }

        public string AuthorBadgeTier { get; set; }
        public string AuthorBadgeName { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorRole { get; set; }
        public string AuthorAvatarUrl { get; set; }

        // ----- Summary of the replies -----
        public int ReplyCount { get; set; }

        // When the latest reply was written; null when there are no replies
        public DateTime? LastReplyAt { get; set; }

        // True when at least one tutor has replied
        public bool HasTutorReply { get; set; }

        // The newest activity: the latest reply, or the post itself
        public DateTime LastActivityAt
        {
            get { return LastReplyAt ?? CreatedAt; }
        }

        // The replies themselves, oldest first. Filled by DiscussionManager.GetPosts (the Tutor page);
        // the paged Learner pages load replies separately with GetReplyPage.
        public List<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();
    }
}
