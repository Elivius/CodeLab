using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class TutorApprovalRepository
    {
        // There is no "submitted at" column: use the latest (re)submission audit entry, or the
        // account's join date for tutors who applied while registering
        private const string SelectApplications =
            @"SELECT u.Id, u.FullName, u.Username, u.Email, u.JoinedDate, u.AvatarUrl,
                     t.Specialty, t.Qualifications, t.PortfolioUrl, t.Bio, t.CurriculumPitch,
                     t.Status, t.ApprovedDate, t.RejectionReason,
                     COALESCE((SELECT MAX(a.Timestamp)
                                 FROM AuditLogs a
                                WHERE a.UserId = t.UserId
                                  AND a.Action IN ('SUBMIT_TUTOR_APPLICATION', 'RESUBMIT_TUTOR_APPLICATION')),
                              u.JoinedDate) AS AppliedAt
              FROM TutorProfiles t
              INNER JOIN Users u ON u.Id = t.UserId";

        /// <summary>
        /// Returns the applications with the given status (TutorStatuses), or every application when
        /// status is null. Pending ones come first, oldest first, so they are reviewed in order.
        /// </summary>
        public List<TutorProfile> GetApplications(string status)
        {
            // Wrapped in a derived table because ORDER BY cannot use the AppliedAt alias inside a CASE
            string query = "SELECT * FROM (" + SelectApplications + @") AS a
                            WHERE @Status IS NULL OR a.Status = @Status
                            ORDER BY CASE WHEN a.Status = @Pending THEN 0 ELSE 1 END,
                                     CASE WHEN a.Status = @Pending THEN a.AppliedAt END ASC,
                                     a.AppliedAt DESC";

            List<TutorProfile> applications = new List<TutorProfile>();

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = (object)status ?? DBNull.Value;
                    command.Parameters.Add("@Pending", SqlDbType.NVarChar, 20).Value = TutorStatuses.Pending;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            applications.Add(ReadApplication(reader));
                        }
                    }
                }
            }

            return applications;
        }

        /// <summary>
        /// Returns one tutor's application, or null if they have not applied.
        /// </summary>
        public TutorProfile GetApplication(int userId)
        {
            string query = SelectApplications + " WHERE t.UserId = @UserId";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadApplication(reader) : null;
                    }
                }
            }
        }

        public int CountPending()
        {
            string query = @"SELECT COUNT(*) FROM TutorProfiles WHERE Status = @Pending";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Pending", SqlDbType.NVarChar, 20).Value = TutorStatuses.Pending;

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Approves a Pending application. Returns false if it was not Pending (already reviewed) or could not be saved.
        /// </summary>
        public bool Approve(int userId)
        {
            string query = @"UPDATE TutorProfiles
                                SET Status = @Approved, ApprovedDate = SYSUTCDATETIME(), RejectionReason = NULL
                              WHERE UserId = @UserId AND Status = @Pending";

            return UpdatePending(query, userId, null, "TutorApprovalRepository.Approve failure");
        }

        /// <summary>
        /// Rejects a Pending application with the admin's reason. Returns false if it was not Pending
        /// (already reviewed) or could not be saved.
        /// </summary>
        public bool Reject(int userId, string reason)
        {
            string query = @"UPDATE TutorProfiles
                                SET Status = @Rejected, RejectionReason = @Reason
                              WHERE UserId = @UserId AND Status = @Pending";

            return UpdatePending(query, userId, reason, "TutorApprovalRepository.Reject failure");
        }

        private static bool UpdatePending(string query, int userId, string reason, string errorContext)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        command.Parameters.Add("@Pending", SqlDbType.NVarChar, 20).Value = TutorStatuses.Pending;
                        command.Parameters.Add("@Approved", SqlDbType.NVarChar, 20).Value = TutorStatuses.Approved;
                        command.Parameters.Add("@Rejected", SqlDbType.NVarChar, 20).Value = TutorStatuses.Rejected;
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

        private static TutorProfile ReadApplication(SqlDataReader reader)
        {
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
                    : Convert.ToDateTime(reader["ApprovedDate"]),
                RejectionReason = ReadNullableString(reader["RejectionReason"]),
                AppliedAt = Convert.ToDateTime(reader["AppliedAt"])
            };
        }

        private static string ReadNullableString(object value)
        {
            return value == DBNull.Value ? null : value.ToString();
        }
    }
}
