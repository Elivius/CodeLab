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

        public List<AuditLog> GetRecentLogsWithUsernames()
        {
            List<AuditLog> logList = new List<AuditLog>();
            string query = @"SELECT TOP 100
                                a.Id, a.UserId, u.Username, u.FullName, a.Action, a.Details, a.Timestamp
                             FROM AuditLogs a
                             INNER JOIN Users u ON a.UserId = u.Id
                             ORDER BY a.Timestamp DESC";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
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
                                Timestamp = Convert.ToDateTime(reader["Timestamp"])
                            });
                        }
                    }
                }
            }

            return logList;
        }
    }
}
