using System;
using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// Everything the Tutor Analytics page shows, for all of a tutor's courses or for one course.
    /// Built by AnalyticsManager from enrollments, lesson completions, quiz attempts, code submissions
    /// and discussion posts.
    /// </summary>
    public class TutorAnalytics
    {
        // ----- Summary cards -----
        public int StudentCount { get; set; }
        public int EnrollmentCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal AverageProgress { get; set; }

        public int QuizAttemptCount { get; set; }
        public int QuizPassedCount { get; set; }
        public double? AverageQuizScore { get; set; }

        public int SubmissionCount { get; set; }
        public int SubmissionPassedCount { get; set; }
        public int SolverCount { get; set; }

        public int UnansweredPostCount { get; set; }

        // ----- Charts -----
        public List<CourseProgressRow> CourseProgress { get; set; } = new List<CourseProgressRow>();

        // Only filled when one course is selected
        public List<LessonFunnelRow> LessonFunnel { get; set; } = new List<LessonFunnelRow>();

        // Attempts scoring 0-49, 50-74, 75-89 and 90-100 percent
        public int[] ScoreBuckets { get; set; } = new int[4];

        // Last 8 weeks, oldest first
        public List<WeeklyActivityRow> WeeklyActivity { get; set; } = new List<WeeklyActivityRow>();

        public int SubmissionFailedCount { get; set; }
        public int SubmissionErrorCount { get; set; }

        // ----- Tables -----
        public List<QuizStatRow> Quizzes { get; set; } = new List<QuizStatRow>();
        public List<ChallengeStatRow> Challenges { get; set; } = new List<ChallengeStatRow>();
        public List<QuizAttemptRow> RecentAttempts { get; set; } = new List<QuizAttemptRow>();
        public List<SubmissionRow> RecentSubmissions { get; set; } = new List<SubmissionRow>();
        public List<TopLearnerRow> TopLearners { get; set; } = new List<TopLearnerRow>();
        public List<UnansweredPostRow> UnansweredPosts { get; set; } = new List<UnansweredPostRow>();

        public bool HasLearnerActivity
        {
            get { return EnrollmentCount > 0 || QuizAttemptCount > 0 || SubmissionCount > 0; }
        }

        public class CourseProgressRow
        {
            public int CourseId { get; set; }
            public string Title { get; set; }
            public int Enrolled { get; set; }
            public int Completed { get; set; }
            public decimal AverageProgress { get; set; }
        }

        public class LessonFunnelRow
        {
            public int OrderIndex { get; set; }
            public string Title { get; set; }
            public int Completions { get; set; }
        }

        public class WeeklyActivityRow
        {
            public DateTime WeekStart { get; set; }
            public int QuizAttempts { get; set; }
            public int Submissions { get; set; }
        }

        public class QuizStatRow
        {
            public int CourseId { get; set; }
            public string CourseTitle { get; set; }
            public string QuizTitle { get; set; }
            public int QuestionCount { get; set; }
            public int PassMark { get; set; }
            public int Attempts { get; set; }
            public int Learners { get; set; }
            public double? AverageScore { get; set; }
            public int Passed { get; set; }
        }

        public class ChallengeStatRow
        {
            public int CourseId { get; set; }
            public string CourseTitle { get; set; }
            public string Title { get; set; }
            public string Language { get; set; }
            public int Submissions { get; set; }
            public int Learners { get; set; }
            public int Solvers { get; set; }
        }

        public class QuizAttemptRow
        {
            public string LearnerName { get; set; }
            public string LearnerAvatarUrl { get; set; }
            public string QuizTitle { get; set; }
            public string CourseTitle { get; set; }
            public int ScorePercent { get; set; }
            public int CorrectCount { get; set; }
            public int TotalQuestions { get; set; }
            public bool Passed { get; set; }
            public DateTime AttemptedAt { get; set; }
        }

        public class SubmissionRow
        {
            public string LearnerName { get; set; }
            public string LearnerAvatarUrl { get; set; }
            public string ChallengeTitle { get; set; }
            public string Language { get; set; }
            public string Status { get; set; }
            public DateTime SubmittedAt { get; set; }
        }

        public class TopLearnerRow
        {
            public string LearnerName { get; set; }
            public string LearnerAvatarUrl { get; set; }
            public int Attempts { get; set; }
            public double AverageScore { get; set; }
            public int BestScore { get; set; }
            public int Passed { get; set; }
        }

        public class UnansweredPostRow
        {
            public string Title { get; set; }
            public string CourseTitle { get; set; }
            public string AuthorName { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
