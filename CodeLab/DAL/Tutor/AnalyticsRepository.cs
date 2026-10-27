using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class AnalyticsRepository
    {
        // Number of weeks in the activity chart
        public const int ActivityWeeks = 8;

        /// <summary>
        /// Reads all analytics for a tutor's courses (courseId 0) or one of their courses, in one round trip.
        /// Every query only looks at courses where Courses.InstructorId = tutorId.
        /// </summary>
        public TutorAnalytics GetAnalytics(int tutorId, int courseId, DateTime weeksStartUtc)
        {
            // The selected courses go into @Courses first; every query below joins to it
            string query = @"
SET NOCOUNT ON;
DECLARE @Courses TABLE (Id INT PRIMARY KEY, Title NVARCHAR(200));
INSERT INTO @Courses (Id, Title)
SELECT Id, Title FROM Courses WHERE InstructorId = @TutorId AND (@CourseId = 0 OR Id = @CourseId);

-- 1. Summary numbers
SELECT
    (SELECT COUNT(DISTINCT e.LearnerId) FROM CourseEnrollments e JOIN @Courses c ON c.Id = e.CourseId) AS StudentCount,
    (SELECT COUNT(*) FROM CourseEnrollments e JOIN @Courses c ON c.Id = e.CourseId) AS EnrollmentCount,
    (SELECT COUNT(*) FROM CourseEnrollments e JOIN @Courses c ON c.Id = e.CourseId WHERE e.IsCompleted = 1) AS CompletedCount,
    (SELECT ISNULL(AVG(e.ProgressPercent), 0) FROM CourseEnrollments e JOIN @Courses c ON c.Id = e.CourseId) AS AverageProgress,
    (SELECT COUNT(*) FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId) AS QuizAttemptCount,
    (SELECT COUNT(*) FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId WHERE a.Passed = 1) AS QuizPassedCount,
    (SELECT AVG(CAST(a.ScorePercent AS FLOAT)) FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId) AS AverageQuizScore,
    (SELECT COUNT(*) FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId) AS SubmissionCount,
    (SELECT COUNT(*) FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId WHERE s.Status = N'Passed') AS SubmissionPassedCount,
    (SELECT COUNT(*) FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId WHERE s.Status = N'Failed') AS SubmissionFailedCount,
    (SELECT COUNT(*) FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId WHERE s.Status = N'Error') AS SubmissionErrorCount,
    (SELECT COUNT(DISTINCT s.LearnerId) FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId WHERE s.Status = N'Passed') AS SolverCount,
    (SELECT COUNT(*) FROM DiscussionPosts p JOIN @Courses c ON c.Id = p.CourseId
      WHERE NOT EXISTS (SELECT 1 FROM DiscussionReplies r WHERE r.PostId = p.Id)) AS UnansweredPostCount;

-- 2. Enrolment and completion per course
SELECT c.Id AS CourseId, c.Title, COUNT(e.Id) AS Enrolled,
       SUM(CASE WHEN e.IsCompleted = 1 THEN 1 ELSE 0 END) AS Completed,
       ISNULL(AVG(e.ProgressPercent), 0) AS AverageProgress
  FROM @Courses c LEFT JOIN CourseEnrollments e ON e.CourseId = c.Id
 GROUP BY c.Id, c.Title
 ORDER BY c.Title;

-- 3. Learners who finished each lesson (one course only)
SELECT l.OrderIndex, l.Title, COUNT(lc.LessonId) AS Completions
  FROM Lessons l JOIN @Courses c ON c.Id = l.CourseId
  LEFT JOIN LessonCompletions lc ON lc.LessonId = l.Id
 WHERE @CourseId > 0
 GROUP BY l.Id, l.OrderIndex, l.Title
 ORDER BY l.OrderIndex, l.Id;

-- 4. Quiz score distribution
SELECT ISNULL(SUM(CASE WHEN a.ScorePercent < 50 THEN 1 ELSE 0 END), 0) AS Band1,
       ISNULL(SUM(CASE WHEN a.ScorePercent BETWEEN 50 AND 74 THEN 1 ELSE 0 END), 0) AS Band2,
       ISNULL(SUM(CASE WHEN a.ScorePercent BETWEEN 75 AND 89 THEN 1 ELSE 0 END), 0) AS Band3,
       ISNULL(SUM(CASE WHEN a.ScorePercent >= 90 THEN 1 ELSE 0 END), 0) AS Band4
  FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId;

-- 5. Quiz attempts per day (recent weeks)
SELECT CAST(a.AttemptedAt AS DATE) AS Day, COUNT(*) AS Total
  FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId
 WHERE a.AttemptedAt >= @Since
 GROUP BY CAST(a.AttemptedAt AS DATE);

-- 6. Code submissions per day (recent weeks)
SELECT CAST(s.SubmittedAt AS DATE) AS Day, COUNT(*) AS Total
  FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId
 WHERE s.SubmittedAt >= @Since
 GROUP BY CAST(s.SubmittedAt AS DATE);

-- 7. Results per quiz
SELECT c.Id AS CourseId, c.Title AS CourseTitle, q.Title AS QuizTitle, q.PassMark,
       (SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizId = q.Id) AS QuestionCount,
       COUNT(a.Id) AS Attempts, COUNT(DISTINCT a.LearnerId) AS Learners,
       AVG(CAST(a.ScorePercent AS FLOAT)) AS AverageScore,
       SUM(CASE WHEN a.Passed = 1 THEN 1 ELSE 0 END) AS Passed
  FROM Quizzes q JOIN @Courses c ON c.Id = q.CourseId
  LEFT JOIN QuizAttempts a ON a.QuizId = q.Id
 GROUP BY c.Id, c.Title, q.Id, q.Title, q.PassMark
 ORDER BY c.Title;

-- 8. Results per coding challenge
SELECT c.Id AS CourseId, c.Title AS CourseTitle, ch.Title, ch.Language,
       COUNT(s.Id) AS Submissions, COUNT(DISTINCT s.LearnerId) AS Learners,
       COUNT(DISTINCT CASE WHEN s.Status = N'Passed' THEN s.LearnerId END) AS Solvers
  FROM CodingChallenges ch JOIN @Courses c ON c.Id = ch.CourseId
  LEFT JOIN CodeSubmissions s ON s.ChallengeId = ch.Id
 GROUP BY c.Id, c.Title, ch.Id, ch.Title, ch.Language
 ORDER BY c.Title;

-- 9. Latest quiz attempts
SELECT TOP 8 u.FullName, u.AvatarUrl, q.Title AS QuizTitle, c.Title AS CourseTitle,
       a.ScorePercent, a.CorrectCount, a.TotalQuestions, a.Passed, a.AttemptedAt
  FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId
  JOIN Users u ON u.Id = a.LearnerId
 ORDER BY a.AttemptedAt DESC, a.Id DESC;

-- 10. Latest code submissions
SELECT TOP 8 u.FullName, u.AvatarUrl, ch.Title AS ChallengeTitle, ch.Language, s.Status, s.SubmittedAt
  FROM CodeSubmissions s JOIN CodingChallenges ch ON ch.Id = s.ChallengeId JOIN @Courses c ON c.Id = ch.CourseId
  JOIN Users u ON u.Id = s.LearnerId
 ORDER BY s.SubmittedAt DESC, s.Id DESC;

-- 11. Learners with the best average quiz score
SELECT TOP 5 u.FullName, u.AvatarUrl, COUNT(*) AS Attempts,
       AVG(CAST(a.ScorePercent AS FLOAT)) AS AverageScore, MAX(a.ScorePercent) AS BestScore,
       SUM(CASE WHEN a.Passed = 1 THEN 1 ELSE 0 END) AS Passed
  FROM QuizAttempts a JOIN Quizzes q ON q.Id = a.QuizId JOIN @Courses c ON c.Id = q.CourseId
  JOIN Users u ON u.Id = a.LearnerId
 GROUP BY u.Id, u.FullName, u.AvatarUrl
 ORDER BY AverageScore DESC, Attempts DESC;

-- 12. Discussion questions nobody has answered yet
SELECT TOP 6 p.Title, c.Title AS CourseTitle, u.FullName AS AuthorName, p.CreatedAt
  FROM DiscussionPosts p JOIN @Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = p.AuthorId
 WHERE NOT EXISTS (SELECT 1 FROM DiscussionReplies r WHERE r.PostId = p.Id)
 ORDER BY p.CreatedAt DESC;";

            TutorAnalytics analytics = new TutorAnalytics();
            Dictionary<DateTime, int> attemptsPerDay = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> submissionsPerDay = new Dictionary<DateTime, int>();

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                    command.Parameters.Add("@Since", SqlDbType.DateTime2).Value = weeksStartUtc;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // 1. Summary
                        if (reader.Read())
                        {
                            analytics.StudentCount = ToInt(reader["StudentCount"]);
                            analytics.EnrollmentCount = ToInt(reader["EnrollmentCount"]);
                            analytics.CompletedCount = ToInt(reader["CompletedCount"]);
                            analytics.AverageProgress = Convert.ToDecimal(reader["AverageProgress"]);
                            analytics.QuizAttemptCount = ToInt(reader["QuizAttemptCount"]);
                            analytics.QuizPassedCount = ToInt(reader["QuizPassedCount"]);
                            analytics.AverageQuizScore = ToNullableDouble(reader["AverageQuizScore"]);
                            analytics.SubmissionCount = ToInt(reader["SubmissionCount"]);
                            analytics.SubmissionPassedCount = ToInt(reader["SubmissionPassedCount"]);
                            analytics.SubmissionFailedCount = ToInt(reader["SubmissionFailedCount"]);
                            analytics.SubmissionErrorCount = ToInt(reader["SubmissionErrorCount"]);
                            analytics.SolverCount = ToInt(reader["SolverCount"]);
                            analytics.UnansweredPostCount = ToInt(reader["UnansweredPostCount"]);
                        }

                        // 2. Per course
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.CourseProgress.Add(new TutorAnalytics.CourseProgressRow
                            {
                                CourseId = ToInt(reader["CourseId"]),
                                Title = reader["Title"].ToString(),
                                Enrolled = ToInt(reader["Enrolled"]),
                                Completed = ToInt(reader["Completed"]),
                                AverageProgress = Convert.ToDecimal(reader["AverageProgress"])
                            });
                        }

                        // 3. Lesson funnel
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.LessonFunnel.Add(new TutorAnalytics.LessonFunnelRow
                            {
                                OrderIndex = ToInt(reader["OrderIndex"]),
                                Title = reader["Title"].ToString(),
                                Completions = ToInt(reader["Completions"])
                            });
                        }

                        // 4. Score bands
                        reader.NextResult();
                        if (reader.Read())
                        {
                            analytics.ScoreBuckets = new[]
                            {
                                ToInt(reader["Band1"]), ToInt(reader["Band2"]), ToInt(reader["Band3"]), ToInt(reader["Band4"])
                            };
                        }

                        // 5 and 6. Activity per day
                        reader.NextResult();
                        while (reader.Read())
                        {
                            attemptsPerDay[Convert.ToDateTime(reader["Day"]).Date] = ToInt(reader["Total"]);
                        }

                        reader.NextResult();
                        while (reader.Read())
                        {
                            submissionsPerDay[Convert.ToDateTime(reader["Day"]).Date] = ToInt(reader["Total"]);
                        }

                        // 7. Per quiz
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.Quizzes.Add(new TutorAnalytics.QuizStatRow
                            {
                                CourseId = ToInt(reader["CourseId"]),
                                CourseTitle = reader["CourseTitle"].ToString(),
                                QuizTitle = reader["QuizTitle"].ToString(),
                                PassMark = ToInt(reader["PassMark"]),
                                QuestionCount = ToInt(reader["QuestionCount"]),
                                Attempts = ToInt(reader["Attempts"]),
                                Learners = ToInt(reader["Learners"]),
                                AverageScore = ToNullableDouble(reader["AverageScore"]),
                                Passed = ToInt(reader["Passed"])
                            });
                        }

                        // 8. Per challenge
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.Challenges.Add(new TutorAnalytics.ChallengeStatRow
                            {
                                CourseId = ToInt(reader["CourseId"]),
                                CourseTitle = reader["CourseTitle"].ToString(),
                                Title = reader["Title"].ToString(),
                                Language = reader["Language"].ToString(),
                                Submissions = ToInt(reader["Submissions"]),
                                Learners = ToInt(reader["Learners"]),
                                Solvers = ToInt(reader["Solvers"])
                            });
                        }

                        // 9. Latest attempts
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.RecentAttempts.Add(new TutorAnalytics.QuizAttemptRow
                            {
                                LearnerName = reader["FullName"].ToString(),
                                LearnerAvatarUrl = reader["AvatarUrl"] as string,
                                QuizTitle = reader["QuizTitle"].ToString(),
                                CourseTitle = reader["CourseTitle"].ToString(),
                                ScorePercent = ToInt(reader["ScorePercent"]),
                                CorrectCount = ToInt(reader["CorrectCount"]),
                                TotalQuestions = ToInt(reader["TotalQuestions"]),
                                Passed = Convert.ToBoolean(reader["Passed"]),
                                AttemptedAt = Convert.ToDateTime(reader["AttemptedAt"])
                            });
                        }

                        // 10. Latest submissions
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.RecentSubmissions.Add(new TutorAnalytics.SubmissionRow
                            {
                                LearnerName = reader["FullName"].ToString(),
                                LearnerAvatarUrl = reader["AvatarUrl"] as string,
                                ChallengeTitle = reader["ChallengeTitle"].ToString(),
                                Language = reader["Language"].ToString(),
                                Status = reader["Status"].ToString(),
                                SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"])
                            });
                        }

                        // 11. Top learners
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.TopLearners.Add(new TutorAnalytics.TopLearnerRow
                            {
                                LearnerName = reader["FullName"].ToString(),
                                LearnerAvatarUrl = reader["AvatarUrl"] as string,
                                Attempts = ToInt(reader["Attempts"]),
                                AverageScore = Convert.ToDouble(reader["AverageScore"]),
                                BestScore = ToInt(reader["BestScore"]),
                                Passed = ToInt(reader["Passed"])
                            });
                        }

                        // 12. Unanswered posts
                        reader.NextResult();
                        while (reader.Read())
                        {
                            analytics.UnansweredPosts.Add(new TutorAnalytics.UnansweredPostRow
                            {
                                Title = reader["Title"].ToString(),
                                CourseTitle = reader["CourseTitle"].ToString(),
                                AuthorName = reader["AuthorName"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }

            // Group the daily counts into weeks, oldest first, including weeks with no activity
            for (int week = 0; week < ActivityWeeks; week++)
            {
                DateTime start = weeksStartUtc.Date.AddDays(7 * week);
                TutorAnalytics.WeeklyActivityRow row = new TutorAnalytics.WeeklyActivityRow { WeekStart = start };
                for (int day = 0; day < 7; day++)
                {
                    int count;
                    DateTime date = start.AddDays(day);
                    if (attemptsPerDay.TryGetValue(date, out count))
                    {
                        row.QuizAttempts += count;
                    }

                    if (submissionsPerDay.TryGetValue(date, out count))
                    {
                        row.Submissions += count;
                    }
                }

                analytics.WeeklyActivity.Add(row);
            }

            return analytics;
        }

        private static int ToInt(object value)
        {
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static double? ToNullableDouble(object value)
        {
            return value == DBNull.Value ? (double?)null : Convert.ToDouble(value);
        }
    }
}
