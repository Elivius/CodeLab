using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class CourseModerationRepository
    {
        private const string SelectCourses =
            @"SELECT c.Id, c.Title, c.Description, c.Category, c.Difficulty, c.Status, c.ThumbnailUrl,
                     c.LockReason, c.RejectionReason, c.UpdatedAt,
                     u.FullName AS TutorName, u.Email AS TutorEmail,
                     (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId = c.Id) AS LessonCount,
                     (SELECT COUNT(*) FROM CourseEnrollments e WHERE e.CourseId = c.Id) AS StudentCount
              FROM Courses c
              INNER JOIN Users u ON u.Id = c.InstructorId";

        /// <summary>
        /// Returns the courses with the given status (CourseStatuses), or every course when status is null.
        /// A search text, when given, must appear in the title or the tutor's name or email.
        /// Pending Review courses come first, oldest first, so they are reviewed in order.
        /// </summary>
        public List<AdminCourseRow> GetCourses(string status, string search)
        {
            // CHARINDEX rather than LIKE, so a typed % or _ is treated as plain text
            string query = SelectCourses + @"
                            WHERE (@Status IS NULL OR c.Status = @Status)
                              AND (@Search IS NULL
                                   OR CHARINDEX(@Search, c.Title) > 0
                                   OR CHARINDEX(@Search, u.FullName) > 0
                                   OR CHARINDEX(@Search, u.Email) > 0)
                            ORDER BY CASE WHEN c.Status = @PendingReview THEN 0 ELSE 1 END,
                                     CASE WHEN c.Status = @PendingReview THEN c.UpdatedAt END ASC,
                                     c.UpdatedAt DESC, c.Id DESC";

            List<AdminCourseRow> courses = new List<AdminCourseRow>();

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
                    command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = (object)search ?? DBNull.Value;
                    command.Parameters.Add("@PendingReview", SqlDbType.NVarChar, 20).Value = CourseStatuses.PendingReview;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(ReadCourse(reader));
                        }
                    }
                }
            }

            return courses;
        }

        /// <summary>
        /// Returns one course, or null if there is none with this id.
        /// </summary>
        public AdminCourseRow GetCourse(int courseId)
        {
            string query = SelectCourses + " WHERE c.Id = @Id";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = courseId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadCourse(reader) : null;
                    }
                }
            }
        }

        public int CountPending()
        {
            string query = @"SELECT COUNT(*) FROM Courses WHERE Status = @PendingReview";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@PendingReview", SqlDbType.NVarChar, 20).Value = CourseStatuses.PendingReview;

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Publishes a Pending Review course. Returns false if it was not Pending Review or could not be saved.
        /// </summary>
        public bool Approve(int courseId)
        {
            string query = @"UPDATE Courses
                                SET Status = @Published, RejectionReason = NULL, UpdatedAt = SYSUTCDATETIME()
                              WHERE Id = @Id AND Status = @PendingReview";

            return UpdateStatus(query, courseId, null, "CourseModerationRepository.Approve failure");
        }

        /// <summary>
        /// Sends a Pending Review course back to Draft with the admin's feedback (there is no Rejected status).
        /// Returns false if it was not Pending Review or could not be saved.
        /// </summary>
        public bool Reject(int courseId, string reason)
        {
            string query = @"UPDATE Courses
                                SET Status = @Draft, RejectionReason = @Reason, UpdatedAt = SYSUTCDATETIME()
                              WHERE Id = @Id AND Status = @PendingReview";

            return UpdateStatus(query, courseId, reason, "CourseModerationRepository.Reject failure");
        }

        /// <summary>
        /// Locks a Published course with the admin's reason. Returns false if it was not Published or could not be saved.
        /// </summary>
        public bool Lock(int courseId, string reason)
        {
            string query = @"UPDATE Courses
                                SET Status = @Locked, LockReason = @Reason, UpdatedAt = SYSUTCDATETIME()
                              WHERE Id = @Id AND Status = @Published";

            return UpdateStatus(query, courseId, reason, "CourseModerationRepository.Lock failure");
        }

        /// <summary>
        /// Publishes a Locked course again. Returns false if it was not Locked or could not be saved.
        /// </summary>
        public bool Unlock(int courseId)
        {
            string query = @"UPDATE Courses
                                SET Status = @Published, LockReason = NULL, UpdatedAt = SYSUTCDATETIME()
                              WHERE Id = @Id AND Status = @Locked";

            return UpdateStatus(query, courseId, null, "CourseModerationRepository.Unlock failure");
        }

        /// <summary>
        /// Deletes a course. Its lessons, enrollments, coding challenge and discussions cascade, but
        /// Quizzes.CourseId is ON DELETE SET NULL, so the quiz is deleted first or it would be left
        /// behind with no course. Both are deleted or neither is. Returns false if nothing was deleted.
        /// </summary>
        public bool Delete(int courseId)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = new SqlCommand(
                                "DELETE FROM Quizzes WHERE CourseId = @Id", connection, transaction))
                            {
                                command.Parameters.Add("@Id", SqlDbType.Int).Value = courseId;
                                command.ExecuteNonQuery();
                            }

                            int deleted;
                            using (SqlCommand command = new SqlCommand(
                                "DELETE FROM Courses WHERE Id = @Id", connection, transaction))
                            {
                                command.Parameters.Add("@Id", SqlDbType.Int).Value = courseId;
                                deleted = command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return deleted > 0;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("CourseModerationRepository.Delete failure", sqlException);
                return false;
            }
        }

        private static bool UpdateStatus(string query, int courseId, string reason, string errorContext)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = courseId;
                        command.Parameters.Add("@Draft", SqlDbType.NVarChar, 20).Value = CourseStatuses.Draft;
                        command.Parameters.Add("@PendingReview", SqlDbType.NVarChar, 20).Value = CourseStatuses.PendingReview;
                        command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                        command.Parameters.Add("@Locked", SqlDbType.NVarChar, 20).Value = CourseStatuses.Locked;
                        command.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = (object)reason ?? DBNull.Value;

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError(errorContext, sqlException);
                return false;
            }
        }

        private static AdminCourseRow ReadCourse(SqlDataReader reader)
        {
            return new AdminCourseRow
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"].ToString(),
                Description = reader["Description"].ToString(),
                Category = reader["Category"].ToString(),
                Difficulty = reader["Difficulty"].ToString(),
                Status = reader["Status"].ToString(),
                ThumbnailUrl = reader["ThumbnailUrl"] as string,
                LockReason = reader["LockReason"] as string,
                RejectionReason = reader["RejectionReason"] as string,
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                TutorName = reader["TutorName"].ToString(),
                TutorEmail = reader["TutorEmail"].ToString(),
                LessonCount = Convert.ToInt32(reader["LessonCount"]),
                StudentCount = Convert.ToInt32(reader["StudentCount"])
            };
        }
    }
}
