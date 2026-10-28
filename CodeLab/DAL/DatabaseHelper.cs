using System.Configuration;
using System.Data.SqlClient;

namespace CodeLab.DAL
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Returns a new, unopened connection to the CodeLab database. Wrap it in a using block.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CodeLabDb"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}
