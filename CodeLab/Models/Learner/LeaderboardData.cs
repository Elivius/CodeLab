using System.Collections.Generic;

namespace CodeLab.Models
{
    public sealed class LeaderboardEntry
    {
        public int LearnerId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public long Rank { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public int Level { get; set; }
        public string LevelTitle { get; set; }
        public int Completed { get; set; }
        public AchievementCounts Counts { get; set; }
        public int Badges { get; set; }
    }
    public sealed class BadgeProgress
    {
        public int Id { get; set; }
        public string Metric { get; set; }
        public bool IsFeatured { get; set; }
        public System.DateTime? EarnedAtUtc { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Category { get; set; }
        public int Current { get; set; }
        public int Target { get; set; }
        public bool Earned { get { return EarnedAtUtc.HasValue; } }
        public int Percent { get { return Earned ? 100 : (int)System.Math.Min(100, 100L * System.Math.Max(0, Current) / Target); } }
    }
    public sealed class AchievementCounts
    {
        public int Lessons { get; set; }
        public int Quizzes { get; set; }
        public int Challenges { get; set; }
        public int Points { get; set; }
    }
    public sealed class LeaderboardData
    {
        public List<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
        public List<LeaderboardEntry> Podium { get; set; } = new List<LeaderboardEntry>();
        public LeaderboardEntry Me { get; set; }
        public List<BadgeProgress> Badges { get; set; } = new List<BadgeProgress>();
        public int TotalLearners { get; set; }
        public int Page { get; set; }
        public int PageCount { get { return System.Math.Max(1, (TotalLearners + 19) / 20); } }
        public int EarnedCount { get { return Badges.FindAll(b => b.Earned).Count; } }
    }
}
