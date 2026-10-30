using System;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
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
        /// Returns the user's account details and tutor application, or null if the user does not exist.
        /// The application fields are null when the user has not applied yet.
        /// </summary>
        public TutorProfile GetProfile(int userId)
        {
            string query = @"SELECT u.Id, u.FullName, u.Username, u.Email, u.JoinedDate, u.AvatarUrl,
                                    t.Specialty, t.Qualifications, t.PortfolioUrl, t.Bio, t.CurriculumPitch,
                                    t.Status, t.ApprovedDate
                             FROM Users u
                             LEFT JOIN TutorProfiles t ON t.UserId = u.Id
                             WHERE u.Id = @UserId";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new TutorProfile
                        {
                            UserId = Convert.ToInt32(reader["Id"]),
                            FullName = reader["FullName"].ToString(),
                            Username = reader["Username"].ToString(),
                            Email = reader["Email"].ToString(),
                            JoinedDate = Convert.ToDateTime(reader["JoinedDate"]),
                            AvatarUrl = ReadNullableString(reader["AvatarUrl"]),
                            Specialty = ReadNullableString(reader["Specialty"]),
                            Qualifications = ReadNullableString(reader["Qualifications"]),
                            PortfolioUrl = ReadNullableString(reader["PortfolioUrl"]),
                            Bio = ReadNullableString(reader["Bio"]),
                            CurriculumPitch = ReadNullableString(reader["CurriculumPitch"]),
                            Status = ReadNullableString(reader["Status"]),
                            ApprovedDate = reader["ApprovedDate"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["ApprovedDate"])
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Saves a tutor application as Pending, and the tutor's full name. Creates the TutorProfiles
        /// row, or replaces a Rejected one. Never overwrites an application that is Pending or Approved.
        /// Returns false if nothing was saved.
        /// </summary>
        public bool SaveApplication(TutorProfile application)
        {
            string query = @"IF EXISTS (SELECT 1 FROM TutorProfiles WHERE UserId = @UserId)
                                 UPDATE TutorProfiles
                                    SET Specialty = @Specialty, Qualifications = @Qualifications,
                                        PortfolioUrl = @PortfolioUrl, Bio = @Bio, CurriculumPitch = @CurriculumPitch,
                                        Status = @Pending, ApprovedDate = NULL
                                  WHERE UserId = @UserId AND Status = @Rejected
                             ELSE
                                 INSERT INTO TutorProfiles (UserId, Specialty, Qualifications, PortfolioUrl, Bio, CurriculumPitch, Status)
                                 VALUES (@UserId, @Specialty, @Qualifications, @PortfolioUrl, @Bio, @CurriculumPitch, @Pending)";

            return SaveTutorDetails(query, application, "TutorRepository.SaveApplication failure");
        }

        /// <summary>
        /// Updates the details of a tutor whose application is Pending or Approved, and their full name.
        /// The status does not change, so no new admin review is needed. Returns false if nothing was saved.
        /// </summary>
        public bool UpdateProfileDetails(TutorProfile profile)
        {
            string query = @"UPDATE TutorProfiles
                                SET Specialty = @Specialty, Qualifications = @Qualifications,
                                    PortfolioUrl = @PortfolioUrl, Bio = @Bio, CurriculumPitch = @CurriculumPitch
                              WHERE UserId = @UserId AND Status IN (@Pending, @Approved)";

            return SaveTutorDetails(query, profile, "TutorRepository.UpdateProfileDetails failure");
        }

        // Runs the TutorProfiles query and the full-name update together: both are saved or neither is
        private static bool SaveTutorDetails(string tutorQuery, TutorProfile profile, string errorContext)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand(tutorQuery, connection, transaction))
                        {
                            command.Parameters.Add("@UserId", SqlDbType.Int).Value = profile.UserId;
                            command.Parameters.Add("@Specialty", SqlDbType.NVarChar, 100).Value = profile.Specialty;
                            command.Parameters.Add("@Qualifications", SqlDbType.NVarChar, -1).Value = profile.Qualifications;
                            command.Parameters.Add("@PortfolioUrl", SqlDbType.NVarChar, 500).Value = profile.PortfolioUrl;
                            command.Parameters.Add("@Bio", SqlDbType.NVarChar, -1).Value = profile.Bio;
                            command.Parameters.Add("@CurriculumPitch", SqlDbType.NVarChar, -1).Value = profile.CurriculumPitch;
                            command.Parameters.Add("@Pending", SqlDbType.NVarChar, 20).Value = TutorStatuses.Pending;
                            command.Parameters.Add("@Approved", SqlDbType.NVarChar, 20).Value = TutorStatuses.Approved;
                            command.Parameters.Add("@Rejected", SqlDbType.NVarChar, 20).Value = TutorStatuses.Rejected;

                            if (command.ExecuteNonQuery() == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                            "UPDATE Users SET FullName = @FullName WHERE Id = @UserId", connection, transaction))
                        {
                            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 120).Value = profile.FullName;
                            command.Parameters.Add("@UserId", SqlDbType.Int).Value = profile.UserId;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError(errorContext, sqlException);
                return false;
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
                                (SELECT COUNT(*)
                                   FROM CourseEnrollments e
                                   INNER JOIN Courses c ON c.Id = e.CourseId
                                  WHERE c.InstructorId = @TutorId) AS EnrollmentCount,
                                (SELECT COUNT(*)
                                   FROM CourseEnrollments e
                                   INNER JOIN Courses c ON c.Id = e.CourseId
                                  WHERE c.InstructorId = @TutorId AND e.IsCompleted = 1) AS CompletedCount,
                                (SELECT COUNT(*)
                                   FROM DiscussionPosts p
                                   INNER JOIN Courses c ON c.Id = p.CourseId
                                  WHERE c.InstructorId = @TutorId AND c.Status = @Published) AS ForumPostCount";

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
                            dashboard.EnrollmentCount = Convert.ToInt32(reader["EnrollmentCount"]);
                            dashboard.CompletedCount = Convert.ToInt32(reader["CompletedCount"]);
                            dashboard.ForumPostCount = Convert.ToInt32(reader["ForumPostCount"]);
                        }

                        return dashboard;
                    }
                }
            }
        }

        private static string ReadNullableString(object value)
        {
            return value == DBNull.Value ? null : value.ToString();
        }
    }
}
