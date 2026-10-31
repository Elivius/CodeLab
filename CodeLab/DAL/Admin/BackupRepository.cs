using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;

namespace CodeLab.DAL
{
    public class BackupRepository
    {
        private const int BackupTimeoutSeconds = 120;

        /// <summary>
        /// Backs up the whole database to this file. Returns false if the backup failed.
        /// DB_NAME() is needed because the site attaches Database.mdf by path, so its name is not
        /// fixed. COPY_ONLY leaves the database's own backup history as it was.
        /// </summary>
        public bool CreateBackup(string fullPath)
        {
            string query = @"DECLARE @Database SYSNAME = DB_NAME();
                             BACKUP DATABASE @Database TO DISK = @Path WITH INIT, CHECKSUM, COPY_ONLY;";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandTimeout = BackupTimeoutSeconds;
                        command.Parameters.Add("@Path", SqlDbType.NVarChar, 400).Value = fullPath;

                        connection.Open();
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("BackupRepository.CreateBackup failure", sqlException);
                return false;
            }
        }
    }
}
