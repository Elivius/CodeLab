using System;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class UserRepository
    {
        /// <summary>
        /// Returns the user with the given username, or null if there is none.
        /// </summary>
        public User GetByUsername(string username)
        {
            string query = @"SELECT Id, FullName, Username, Email, PasswordHash, Role, Status, AvatarUrl, JoinedDate, LastLogin
                             FROM Users
                             WHERE Username = @Username";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new User
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FullName = reader["FullName"].ToString(),
                            Username = reader["Username"].ToString(),
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Role = reader["Role"].ToString(),
                            Status = reader["Status"].ToString(),
                            AvatarUrl = reader["AvatarUrl"] as string,
                            JoinedDate = Convert.ToDateTime(reader["JoinedDate"]),
                            LastLogin = reader["LastLogin"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastLogin"])
                        };
                    }
                }
            }
        }

        /// <summary>
        /// True if an account already uses this username. The database collation ignores case,
        /// so "Tutor" matches "tutor".
        /// </summary>
        public bool IsUsernameTaken(string username)
        {
            return Exists("SELECT COUNT(*) FROM Users WHERE Username = @Value", SqlDbType.NVarChar, 50, username);
        }

        /// <summary>
        /// True if an account already uses this email address (case is ignored).
        /// </summary>
        public bool IsEmailTaken(string email)
        {
            return Exists("SELECT COUNT(*) FROM Users WHERE Email = @Value", SqlDbType.NVarChar, 255, email);
        }

        private static bool Exists(string query, SqlDbType type, int size, string value)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Value", type, size).Value = value;

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        /// <summary>
        /// Creates a user and returns the new id, or 0 if it could not be saved. When application is
        /// not null (a tutor), its TutorProfiles row is saved as Pending in the same transaction:
        /// both rows are saved or neither is.
        /// </summary>
        public int Insert(User user, TutorProfile application)
        {
            string userQuery = @"INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status)
                                 OUTPUT INSERTED.Id
                                 VALUES (@FullName, @Username, @Email, @PasswordHash, @Role, @Status)";
            string applicationQuery = @"INSERT INTO TutorProfiles (UserId, Specialty, Qualifications, PortfolioUrl, Bio, CurriculumPitch, Status)
                                        VALUES (@UserId, @Specialty, @Qualifications, @PortfolioUrl, @Bio, @CurriculumPitch, @Status)";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int userId;
                            using (SqlCommand command = new SqlCommand(userQuery, connection, transaction))
                            {
                                command.Parameters.Add("@FullName", SqlDbType.NVarChar, 120).Value = user.FullName;
                                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = user.Username;
                                command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = user.Email;
                                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = user.PasswordHash;
                                command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = user.Role;
                                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = user.Status;
                                userId = Convert.ToInt32(command.ExecuteScalar());
                            }

                            if (application != null)
                            {
                                using (SqlCommand command = new SqlCommand(applicationQuery, connection, transaction))
                                {
                                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                                    command.Parameters.Add("@Specialty", SqlDbType.NVarChar, 100).Value = application.Specialty;
                                    command.Parameters.Add("@Qualifications", SqlDbType.NVarChar, -1).Value = application.Qualifications;
                                    command.Parameters.Add("@PortfolioUrl", SqlDbType.NVarChar, 500).Value = application.PortfolioUrl;
                                    command.Parameters.Add("@Bio", SqlDbType.NVarChar, -1).Value = application.Bio;
                                    command.Parameters.Add("@CurriculumPitch", SqlDbType.NVarChar, -1).Value = application.CurriculumPitch;
                                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = TutorStatuses.Pending;
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return userId;
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
                LogHelper.LogError("UserRepository.Insert failure", sqlException);
                return 0;
            }
        }

        /// <summary>
        /// Sets the user's profile photo path, or clears it when avatarUrl is null.
        /// </summary>
        public bool UpdateAvatarUrl(int userId, string avatarUrl)
        {
            string query = @"UPDATE Users SET AvatarUrl = @AvatarUrl WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@AvatarUrl", SqlDbType.NVarChar, 500).Value = (object)avatarUrl ?? DBNull.Value;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = userId;

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("UserRepository.UpdateAvatarUrl failure", sqlException);
                return false;
            }
        }

        public bool UpdateLastLogin(int userId)
        {
            string query = @"UPDATE Users SET LastLogin = @LastLogin WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@LastLogin", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = userId;

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("UserRepository.UpdateLastLogin failure", sqlException);
                return false;
            }
        }
    }
}
