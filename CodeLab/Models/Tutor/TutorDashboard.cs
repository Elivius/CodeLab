using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// Everything the Tutor Dashboard shows: headline numbers and the tutor's courses.
    /// </summary>
    public class TutorDashboard
    {
        public int CourseCount { get; set; }
        public int PublishedCount { get; set; }

        // Distinct learners enrolled in any of the tutor's courses
        public int StudentCount { get; set; }

        // Enrolments in the tutor's courses (a learner in two courses counts twice), and how many are finished
        public int EnrollmentCount { get; set; }
        public int CompletedCount { get; set; }

        // Discussion posts in the tutor's published courses
        public int ForumPostCount { get; set; }

        public List<TutorCourseSummary> Courses { get; set; } = new List<TutorCourseSummary>();
    }
}
