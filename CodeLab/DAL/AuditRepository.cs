using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class AuditRepository
    {
        // Works out the category from the action name. An action that fits no rule counts as Content.
        private const string CategorySql =
            @"CASE WHEN a.[Action] LIKE 'DELETE[_]%' OR a.[Action] LIKE 'REMOVE[_]%' THEN '" + AuditCategories.Deletion + @"'
                   WHEN a.[Action] IN ('LOGIN', 'LOGOUT', 'LOGIN_FAILED', 'REGISTER', 'SUSPEND_USER', 'REACTIVATE_USER') THEN '" + AuditCategories.Security + @"'
                   WHEN a.[Action] LIKE 'APPROVE[_]%' OR a.[Action] LIKE 'REJECT[_]%'
                     OR a.[Action] LIKE 'LOCK[_]%' OR a.[Action] LIKE 'UNLOCK[_]%'
                     OR a.[Action] IN ('CREATE_USER', 'UPDATE_USER', 'CREATE_BACKUP', 'DOWNLOAD_BACKUP') THEN '" + AuditCategories.Administrative + @"'
                   ELSE '" + AuditCategories.Content + "' END";

        // The rows matching @Category and @Search. A derived table, so WHERE can use the Category alias.
        // CHARINDEX rather than LIKE, so a typed % or _ is treated as plain text.
        private const string MatchingLogs =
            @"(SELECT a.Id, a.UserId, u.Username, u.FullName, a.[Action], a.Details, a.[Timestamp], " + CategorySql + @" AS Category
               FROM AuditLogs a
               INNER JOIN Users u ON a.UserId = u.Id) AS logs
              WHERE (@Category IS NULL OR logs.Category = @Category)
                AND (@Search IS NULL
                     OR CHARINDEX(@Search, logs.Username) > 0
                     OR CHARINDEX(@Search, logs.FullName) > 0
                     OR CHARINDEX(@Search, logs.[Action]) > 0
                     OR CHARINDEX(@Search, logs.Details) > 0)";

        public bool InsertAuditLog(AuditLog auditRecord)
        {
            string query = @"INSERT INTO AuditLogs (UserId, Action, Details, Timestamp)
                             VALUES (@UserId, @Action, @Details, @Timestamp)";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@UserId", SqlDbType.Int).Value = auditRecord.UserId;
                        command.Parameters.Add("@Action", SqlDbType.VarChar, 50).Value = auditRecord.Action;
                        command.Parameters.Add("@Details", SqlDbType.NVarChar, 500).Value =
                            (object)auditRecord.Details ?? DBNull.Value;
                        command.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = auditRecord.Timestamp;

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("AuditRepository.InsertAuditLog failure", sqlException);
                return false;
            }
        }

        /// <summary>
        /// Counts the rows in this category (null for every category) that match the search text (null for no search).
        /// </summary>
        public int CountLogs(string search, string category)
        {
            string query = "SELECT COUNT(*) FROM " + MatchingLogs;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddFilters(command, search, category);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Returns one page of the same rows, newest first.
        /// </summary>
        public List<AuditLog> GetLogs(string search, string category, int skip, int take)
        {
            List<AuditLog> logList = new List<AuditLog>();
            string query = "SELECT logs.* FROM " + MatchingLogs + @"
                            ORDER BY logs.[Timestamp] DESC, logs.Id DESC
                            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    AddFilters(command, search, category);
                    command.Parameters.Add("@Skip", SqlDbType.Int).Value = skip;
                    command.Parameters.Add("@Take", SqlDbType.Int).Value = take;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logList.Add(new AuditLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Username = reader["Username"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Action = reader["Action"].ToString(),
                                Details = reader["Details"].ToString(),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                                Category = reader["Category"].ToString()
                            });
                        }
                    }
                }
            }

            return logList;
        }

        private static void AddFilters(SqlCommand command, string search, string category)
        {
            command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = (object)search ?? DBNull.Value;
            command.Parameters.Add("@Category", SqlDbType.NVarChar, 20).Value = (object)category ?? DBNull.Value;
        }
    }
}
