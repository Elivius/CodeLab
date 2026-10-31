using System;

namespace CodeLab.Models
{
    /// <summary>
    /// Every number on the admin stat cards, the tab badges and the "waiting for review" banner.
    /// </summary>
    public class AdminStats
    {
        // Active accounts only, so the three add up to ActiveUsers
        public int Learners { get; set; }
        public int Tutors { get; set; }
        public int Admins { get; set; }

        // Every account that joined since the 1st of this month (UTC), whatever its status
        public int NewUsersThisMonth { get; set; }

        public int PublishedCourses { get; set; }
        public int PendingCourses { get; set; }
        public int LockedCourses { get; set; }
        public int DraftCourses { get; set; }

        public int PendingTutors { get; set; }

        public decimal DatabaseSizeMb { get; set; }
        public int TableCount { get; set; }

        // How long a test query took, measured by AdminStatsManager
        public long QueryMilliseconds { get; set; }

        // When the newest backup was made (UTC); null if there are none
        public DateTime? LastBackupAt { get; set; }

        public int ActiveUsers
        {
            get { return Learners + Tutors + Admins; }
        }

        public int TotalCourses
        {
            get { return PublishedCourses + PendingCourses + LockedCourses + DraftCourses; }
        }

        public int PendingTotal
        {
            get { return PendingTutors + PendingCourses; }
        }
    }
}
