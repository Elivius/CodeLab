using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// Everything an admin reads before deciding on a course (the CourseReview page).
    /// </summary>
    public class CourseReviewDetails
    {
        public AdminCourseRow Course { get; set; }
        public List<Lesson> Lessons { get; set; }

        // With its Questions filled; null if the course has no quiz
        public Quiz Quiz { get; set; }

        // Null if the course has no coding challenge
        public CodingChallenge Challenge { get; set; }
    }
}
