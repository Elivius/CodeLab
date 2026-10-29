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

        public int QuizCount { get; set; }
        public int QuizAttemptCount { get; set; }

        // Null when no learner has attempted the tutor's quizzes yet
        public decimal? AverageQuizScore { get; set; }
        public decimal? QuizPassRate { get; set; }

        public List<TutorCourseSummary> Courses { get; set; } = new List<TutorCourseSummary>();
    }
}
