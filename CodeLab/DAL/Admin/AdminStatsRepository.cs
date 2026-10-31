using System;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class AdminStatsRepository
    {
        /// <summary>
        /// Returns every count on the admin stat cards, in one query. size in sys.database_files is
        /// a number of 8 KB pages.
        /// </summary>
        public AdminStats GetStats()
        {
            string query = @"SELECT
                                (SELECT COUNT(*) FROM Users WHERE Status = @Active AND Role = @Learner) AS Learners,
                                (SELECT COUNT(*) FROM Users WHERE Status = @Active AND Role = @Tutor) AS Tutors,
                                (SELECT COUNT(*) FROM Users WHERE Status = @Active AND Role = @Admin) AS Admins,
                                (SELECT COUNT(*) FROM Users
                                  WHERE JoinedDate >= DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1)) AS NewUsersThisMonth,
                                (SELECT COUNT(*) FROM Courses WHERE Status = @Published) AS PublishedCourses,
                                (SELECT COUNT(*) FROM Courses WHERE Status = @PendingReview) AS PendingCourses,
                                (SELECT COUNT(*) FROM Courses WHERE Status = @Locked) AS LockedCourses,
                                (SELECT COUNT(*) FROM Courses WHERE Status = @Draft) AS DraftCourses,
                                (SELECT COUNT(*) FROM TutorProfiles WHERE Status = @TutorPending) AS PendingTutors,
                                (SELECT COUNT(*) FROM sys.tables) AS TableCount,
                                (SELECT SUM(size) * 8 / 1024.0 FROM sys.database_files) AS DatabaseSizeMb";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Active", SqlDbType.NVarChar, 20).Value = UserStatuses.Active;
                    command.Parameters.Add("@Learner", SqlDbType.NVarChar, 20).Value = UserRoles.Learner;
                    command.Parameters.Add("@Tutor", SqlDbType.NVarChar, 20).Value = UserRoles.Tutor;
                    command.Parameters.Add("@Admin", SqlDbType.NVarChar, 20).Value = UserRoles.Admin;
                    command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                    command.Parameters.Add("@PendingReview", SqlDbType.NVarChar, 20).Value = CourseStatuses.PendingReview;
                    command.Parameters.Add("@Locked", SqlDbType.NVarChar, 20).Value = CourseStatuses.Locked;
                    command.Parameters.Add("@Draft", SqlDbType.NVarChar, 20).Value = CourseStatuses.Draft;
                    command.Parameters.Add("@TutorPending", SqlDbType.NVarChar, 20).Value = TutorStatuses.Pending;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return new AdminStats
                        {
                            Learners = Convert.ToInt32(reader["Learners"]),
                            Tutors = Convert.ToInt32(reader["Tutors"]),
                            Admins = Convert.ToInt32(reader["Admins"]),
                            NewUsersThisMonth = Convert.ToInt32(reader["NewUsersThisMonth"]),
                            PublishedCourses = Convert.ToInt32(reader["PublishedCourses"]),
                            PendingCourses = Convert.ToInt32(reader["PendingCourses"]),
                            LockedCourses = Convert.ToInt32(reader["LockedCourses"]),
                            DraftCourses = Convert.ToInt32(reader["DraftCourses"]),
                            PendingTutors = Convert.ToInt32(reader["PendingTutors"]),
                            TableCount = Convert.ToInt32(reader["TableCount"]),
                            DatabaseSizeMb = Convert.ToDecimal(reader["DatabaseSizeMb"])
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Runs the smallest possible query, so AdminStatsManager can time a trip to the database.
        /// </summary>
        public void Ping()
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SELECT 1", connection))
                {
                    connection.Open();
                    command.ExecuteScalar();
                }
            }
        }
    }
}
