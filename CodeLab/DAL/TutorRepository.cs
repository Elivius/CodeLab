using System;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class TutorRepository
    {
        /// <summary>
        /// Returns the tutor's application status (TutorStatuses), or null if they have not applied.
        /// </summary>
        public string GetApplicationStatus(int userId)
        {
            string query = @"SELECT Status FROM TutorProfiles WHERE UserId = @UserId";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    connection.Open();
                    object status = command.ExecuteScalar();
                    return status == null || status == DBNull.Value ? null : status.ToString();
                }
            }
        }

        /// <summary>
        /// Returns the dashboard's headline numbers for one tutor. The course list is filled separately.
        /// </summary>
        public TutorDashboard GetDashboardStats(int tutorId)
        {
            string query = @"SELECT
                                (SELECT COUNT(*) FROM Courses WHERE InstructorId = @TutorId) AS CourseCount,
                                (SELECT COUNT(*) FROM Courses WHERE InstructorId = @TutorId AND Status = @Published) AS PublishedCount,
                                (SELECT COUNT(DISTINCT e.LearnerId)
                                   FROM CourseEnrollments e
                                   INNER JOIN Courses c ON c.Id = e.CourseId
                                  WHERE c.InstructorId = @TutorId) AS StudentCount,
                                (SELECT COUNT(*) FROM Quizzes WHERE CreatedByUserId = @TutorId) AS QuizCount,
                                (SELECT COUNT(*)
                                   FROM QuizAttempts a
                                   INNER JOIN Quizzes q ON q.Id = a.QuizId
                                  WHERE q.CreatedByUserId = @TutorId) AS QuizAttemptCount,
                                (SELECT AVG(CAST(a.ScorePercent AS DECIMAL(5,2)))
                                   FROM QuizAttempts a
                                   INNER JOIN Quizzes q ON q.Id = a.QuizId
                                  WHERE q.CreatedByUserId = @TutorId) AS AverageQuizScore,
                                (SELECT AVG(CASE WHEN a.Passed = 1 THEN 100.0 ELSE 0.0 END)
                                   FROM QuizAttempts a
                                   INNER JOIN Quizzes q ON q.Id = a.QuizId
                                  WHERE q.CreatedByUserId = @TutorId) AS QuizPassRate";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;
                    command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        TutorDashboard dashboard = new TutorDashboard();
                        if (reader.Read())
                        {
                            dashboard.CourseCount = Convert.ToInt32(reader["CourseCount"]);
                            dashboard.PublishedCount = Convert.ToInt32(reader["PublishedCount"]);
                            dashboard.StudentCount = Convert.ToInt32(reader["StudentCount"]);
                            dashboard.QuizCount = Convert.ToInt32(reader["QuizCount"]);
                            dashboard.QuizAttemptCount = Convert.ToInt32(reader["QuizAttemptCount"]);
                            dashboard.AverageQuizScore = ReadNullableDecimal(reader["AverageQuizScore"]);
                            dashboard.QuizPassRate = ReadNullableDecimal(reader["QuizPassRate"]);
                        }

                        return dashboard;
                    }
                }
            }
        }

        private static decimal? ReadNullableDecimal(object value)
        {
            return value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
        }
    }
}
