using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    /// <summary>
    /// Read-only learner results for a tutor's quizzes and coding challenges.
    /// Every query only looks at the tutor's own Published courses (Courses.InstructorId = tutorId),
    /// the only courses learners can take.
    /// </summary>
    public class LearnerResultsRepository
    {
        // ---------- Quizzes ----------

        /// <summary>
        /// The tutor's quizzes with their results (quizId 0 = all of them).
        /// </summary>
        public List<QuizResultSummary> GetQuizSummaries(int tutorId, int quizId)
        {
            const string query = @"
SELECT q.Id AS QuizId, q.Title AS QuizTitle, q.PassMark,
       c.Title AS CourseTitle,
       (SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizId = q.Id) AS QuestionCount,
       (SELECT COUNT(*) FROM CourseEnrollments e WHERE e.CourseId = c.Id) AS EnrolledCount,
       COUNT(a.Id) AS AttemptCount,
       COUNT(DISTINCT a.LearnerId) AS LearnerCount,
       COUNT(DISTINCT CASE WHEN a.Passed = 1 THEN a.LearnerId END) AS PassedLearnerCount,
       AVG(CAST(a.ScorePercent AS FLOAT)) AS AverageScore
  FROM Quizzes q
  JOIN Courses c ON c.Id = q.CourseId
  LEFT JOIN QuizAttempts a ON a.QuizId = q.Id
 WHERE c.InstructorId = @TutorId AND c.Status = @Published AND (@QuizId = 0 OR q.Id = @QuizId)
 GROUP BY q.Id, q.Title, q.PassMark, c.Id, c.Title
 ORDER BY c.Title, q.Id;";

            List<QuizResultSummary> quizzes = new List<QuizResultSummary>();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                command.Parameters.Add("@QuizId", SqlDbType.Int).Value = quizId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        quizzes.Add(new QuizResultSummary
                        {
                            QuizId = ToInt(reader["QuizId"]),
                            QuizTitle = reader["QuizTitle"].ToString(),
                            PassMark = ToInt(reader["PassMark"]),
                            CourseTitle = reader["CourseTitle"].ToString(),
                            QuestionCount = ToInt(reader["QuestionCount"]),
                            AttemptCount = ToInt(reader["AttemptCount"]),
                            LearnerCount = ToInt(reader["LearnerCount"]),
                            PassedLearnerCount = ToInt(reader["PassedLearnerCount"]),
                            EnrolledCount = ToInt(reader["EnrolledCount"]),
                            AverageScore = ToNullableDouble(reader["AverageScore"])
                        });
                    }
                }
            }

            return quizzes;
        }

        /// <summary>
        /// Every attempt at one of the tutor's quizzes, newest first, numbered per learner.
        /// </summary>
        public List<QuizAttemptRow> GetQuizAttempts(int tutorId, int quizId)
        {
            const string query = @"
SELECT a.Id, a.QuizId, a.LearnerId, u.FullName, u.Username, u.AvatarUrl,
       ROW_NUMBER() OVER (PARTITION BY a.LearnerId ORDER BY a.AttemptedAt, a.Id) AS AttemptNumber,
       a.ScorePercent, a.Passed, a.CorrectCount, a.TotalQuestions, a.AttemptedAt
  FROM QuizAttempts a
  JOIN Quizzes q ON q.Id = a.QuizId
  JOIN Courses c ON c.Id = q.CourseId
  JOIN Users u ON u.Id = a.LearnerId
 WHERE c.InstructorId = @TutorId AND c.Status = @Published AND a.QuizId = @QuizId
 ORDER BY a.AttemptedAt DESC, a.Id DESC;";

            List<QuizAttemptRow> attempts = new List<QuizAttemptRow>();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                command.Parameters.Add("@QuizId", SqlDbType.Int).Value = quizId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QuizAttemptRow row = new QuizAttemptRow();
                        FillAttempt(row, reader);
                        attempts.Add(row);
                    }
                }
            }

            return attempts;
        }

        /// <summary>
        /// One attempt with its answer snapshot, or null if it is not in one of the tutor's courses.
        /// </summary>
        public QuizAttemptDetail GetQuizAttempt(int tutorId, int attemptId)
        {
            const string query = @"
SELECT a.Id, a.QuizId, a.LearnerId, u.FullName, u.Username, u.AvatarUrl,
       (SELECT COUNT(*) FROM QuizAttempts earlier
         WHERE earlier.QuizId = a.QuizId AND earlier.LearnerId = a.LearnerId
           AND (earlier.AttemptedAt < a.AttemptedAt OR (earlier.AttemptedAt = a.AttemptedAt AND earlier.Id <= a.Id))) AS AttemptNumber,
       a.ScorePercent, a.Passed, a.CorrectCount, a.TotalQuestions, a.AttemptedAt,
       a.AnswerReviewJson, q.Title AS QuizTitle, q.PassMark, c.Title AS CourseTitle
  FROM QuizAttempts a
  JOIN Quizzes q ON q.Id = a.QuizId
  JOIN Courses c ON c.Id = q.CourseId
  JOIN Users u ON u.Id = a.LearnerId
 WHERE c.InstructorId = @TutorId AND c.Status = @Published AND a.Id = @AttemptId;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                command.Parameters.Add("@AttemptId", SqlDbType.Int).Value = attemptId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    QuizAttemptDetail detail = new QuizAttemptDetail();
                    FillAttempt(detail, reader);
                    detail.QuizTitle = reader["QuizTitle"].ToString();
                    detail.PassMark = ToInt(reader["PassMark"]);
                    detail.CourseTitle = reader["CourseTitle"].ToString();
                    detail.AnswerReview = QuizAnswerReview.Parse(reader["AnswerReviewJson"] as string);
                    return detail;
                }
            }
        }

        // ---------- Coding challenges ----------

        /// <summary>
        /// The tutor's coding challenges with their results (challengeId 0 = all of them).
        /// </summary>
        public List<ChallengeResultSummary> GetChallengeSummaries(int tutorId, int challengeId)
        {
            const string query = @"
SELECT ch.Id AS ChallengeId, ch.Title, ch.Language, ch.Difficulty,
       c.Title AS CourseTitle,
       COUNT(s.Id) AS SubmissionCount,
       (SELECT COUNT(*) FROM CourseEnrollments e WHERE e.CourseId = c.Id) AS EnrolledCount,
       COUNT(DISTINCT s.LearnerId) AS LearnerCount,
       COUNT(DISTINCT CASE WHEN s.Status = N'Passed' THEN s.LearnerId END) AS SolverCount
  FROM CodingChallenges ch
  JOIN Courses c ON c.Id = ch.CourseId
  LEFT JOIN CodeSubmissions s ON s.ChallengeId = ch.Id
 WHERE c.InstructorId = @TutorId AND c.Status = @Published AND (@ChallengeId = 0 OR ch.Id = @ChallengeId)
 GROUP BY ch.Id, ch.Title, ch.Language, ch.Difficulty, c.Id, c.Title
 ORDER BY c.Title, ch.Id;";

            List<ChallengeResultSummary> challenges = new List<ChallengeResultSummary>();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                command.Parameters.Add("@ChallengeId", SqlDbType.Int).Value = challengeId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        challenges.Add(new ChallengeResultSummary
                        {
                            ChallengeId = ToInt(reader["ChallengeId"]),
                            Title = reader["Title"].ToString(),
                            Language = reader["Language"].ToString(),
                            Difficulty = reader["Difficulty"].ToString(),
                            CourseTitle = reader["CourseTitle"].ToString(),
                            SubmissionCount = ToInt(reader["SubmissionCount"]),
                            LearnerCount = ToInt(reader["LearnerCount"]),
                            SolverCount = ToInt(reader["SolverCount"]),
                            EnrolledCount = ToInt(reader["EnrolledCount"])
                        });
                    }
                }
            }

            return challenges;
        }

        /// <summary>
        /// Every submission to one of the tutor's challenges, newest first, numbered per learner.
        /// </summary>
        public List<ChallengeSubmissionRow> GetSubmissions(int tutorId, int challengeId)
        {
            const string query = @"
SELECT s.Id, s.ChallengeId, s.LearnerId, u.FullName, u.Username, u.AvatarUrl,
       ROW_NUMBER() OVER (PARTITION BY s.LearnerId ORDER BY s.SubmittedAt, s.Id) AS AttemptNumber,
       s.Status, s.SubmittedAt
  FROM CodeSubmissions s
  JOIN CodingChallenges ch ON ch.Id = s.ChallengeId
  JOIN Courses c ON c.Id = ch.CourseId
  JOIN Users u ON u.Id = s.LearnerId
 WHERE c.InstructorId = @TutorId AND c.Status = @Published AND s.ChallengeId = @ChallengeId
 ORDER BY s.SubmittedAt DESC, s.Id DESC;";

            List<ChallengeSubmissionRow> submissions = new List<ChallengeSubmissionRow>();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                command.Parameters.Add("@ChallengeId", SqlDbType.Int).Value = challengeId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ChallengeSubmissionRow row = new ChallengeSubmissionRow();
                        FillSubmission(row, reader);
                        submissions.Add(row);
                    }
                }
            }

            return submissions;
        }

        /// <summary>
        /// One submission with its code and output, or null if it is not in one of the tutor's courses.
        /// </summary>
        public ChallengeSubmissionDetail GetSubmission(int tutorId, int submissionId)
        {
            const string query = @"
SELECT s.Id, s.ChallengeId, s.LearnerId, u.FullName, u.Username, u.AvatarUrl,
       (SELECT COUNT(*) FROM CodeSubmissions earlier
         WHERE earlier.ChallengeId = s.ChallengeId AND earlier.LearnerId = s.LearnerId
           AND (earlier.SubmittedAt < s.SubmittedAt OR (earlier.SubmittedAt = s.SubmittedAt AND earlier.Id <= s.Id))) AS AttemptNumber,
       s.Status, s.SubmittedAt, s.SubmittedCode, s.OutputResult,
       ch.Title AS ChallengeTitle, ch.Language, ch.ExpectedOutput, c.Title AS CourseTitle
  FROM CodeSubmissions s
  JOIN CodingChallenges ch ON ch.Id = s.ChallengeId
  JOIN Courses c ON c.Id = ch.CourseId
  JOIN Users u ON u.Id = s.LearnerId
 WHERE c.InstructorId = @TutorId AND c.Status = @Published AND s.Id = @SubmissionId;";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                command.Parameters.Add("@SubmissionId", SqlDbType.Int).Value = submissionId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    ChallengeSubmissionDetail detail = new ChallengeSubmissionDetail();
                    FillSubmission(detail, reader);
                    detail.SubmittedCode = reader["SubmittedCode"] as string ?? string.Empty;
                    detail.OutputResult = reader["OutputResult"] as string;
                    detail.ChallengeTitle = reader["ChallengeTitle"].ToString();
                    detail.Language = reader["Language"].ToString();
                    detail.ExpectedOutput = reader["ExpectedOutput"] as string;
                    detail.CourseTitle = reader["CourseTitle"].ToString();
                    return detail;
                }
            }
        }

        // ---------- Helpers ----------

        private static void FillAttempt(QuizAttemptRow row, SqlDataReader reader)
        {
            row.AttemptId = ToInt(reader["Id"]);
            row.QuizId = ToInt(reader["QuizId"]);
            row.LearnerId = ToInt(reader["LearnerId"]);
            row.LearnerName = reader["FullName"].ToString();
            row.LearnerUsername = reader["Username"].ToString();
            row.LearnerAvatarUrl = reader["AvatarUrl"] as string;
            row.AttemptNumber = ToInt(reader["AttemptNumber"]);
            row.ScorePercent = ToInt(reader["ScorePercent"]);
            row.Passed = Convert.ToBoolean(reader["Passed"]);
            row.CorrectCount = ToInt(reader["CorrectCount"]);
            row.TotalQuestions = ToInt(reader["TotalQuestions"]);
            row.AttemptedAt = Convert.ToDateTime(reader["AttemptedAt"]);
        }

        private static void FillSubmission(ChallengeSubmissionRow row, SqlDataReader reader)
        {
            row.SubmissionId = ToInt(reader["Id"]);
            row.ChallengeId = ToInt(reader["ChallengeId"]);
            row.LearnerId = ToInt(reader["LearnerId"]);
            row.LearnerName = reader["FullName"].ToString();
            row.LearnerUsername = reader["Username"].ToString();
            row.LearnerAvatarUrl = reader["AvatarUrl"] as string;
            row.AttemptNumber = ToInt(reader["AttemptNumber"]);
            row.Status = reader["Status"].ToString();
            row.SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"]);
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
