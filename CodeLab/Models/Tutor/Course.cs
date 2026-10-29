using System;

namespace CodeLab.Models
{
    /// <summary>
    /// A course as stored in the Courses table.
    /// </summary>
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Difficulty { get; set; }
        public int InstructorId { get; set; }
        public string Status { get; set; }

        // App-relative path of the cover image, e.g. "~/Uploads/Covers/c2-....png"; null if none
        public string ThumbnailUrl { get; set; }

        public string RejectionReason { get; set; }
        public string LockReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Only filled by CourseRepository.GetPublished; 0 everywhere else
        public int LessonCount { get; set; }
    }
}
