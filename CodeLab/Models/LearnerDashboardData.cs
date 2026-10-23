using System.Data;
namespace CodeLab.Models
{
    public class LearnerDashboardData
    {
        public int Enrolled { get; set; }
        public int Completed { get; set; }
        public int Points { get; set; }
        public int Level { get; set; }
        public string LevelTitle { get; set; }
        public int Milestone { get; set; }
        public int Streak { get; set; }
        public DataTable Courses { get; set; }
    }
}
