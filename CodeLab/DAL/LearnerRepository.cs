using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;
namespace CodeLab.DAL
{
    public class LearnerRepository
    {
        public LearnerDashboardData GetDashboard(int learnerId)
        {
            var data = new LearnerDashboardData { Level = 1, LevelTitle = "Novice Coder", Milestone = 500, Courses = new DataTable() };
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"SELECT COUNT(*), COALESCE(SUM(CASE WHEN IsCompleted=1 THEN 1 ELSE 0 END),0) FROM dbo.CourseEnrollments WHERE LearnerId=@id", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = learnerId;
                    using (var reader = command.ExecuteReader()) if (reader.Read()) { data.Enrolled = reader.GetInt32(0); data.Completed = reader.GetInt32(1); }
                }
                using (var command = new SqlCommand("SELECT AchievementPoints, CurrentLevel, LevelTitle, NextMilestonePoints, StreakDays FROM dbo.LearnerStats WHERE LearnerId=@id", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = learnerId;
                    using (var reader = command.ExecuteReader()) if (reader.Read())
                    {
                        data.Points = reader.GetInt32(0); data.Level = reader.GetInt32(1);
                        data.LevelTitle = reader.GetString(2); data.Milestone = reader.GetInt32(3); data.Streak = reader.GetInt32(4);
                    }
                }
                using (var command = new SqlCommand(@"SELECT c.Title,c.Category,c.Difficulty,e.ProgressPercent FROM dbo.CourseEnrollments e INNER JOIN dbo.Courses c ON c.Id=e.CourseId WHERE e.LearnerId=@id ORDER BY e.EnrolledAt DESC", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = learnerId;
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(data.Courses);
                }
            }
            return data;
        }
    }
}
