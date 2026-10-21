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
