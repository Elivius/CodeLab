using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public sealed class LeaderboardManager
    {
        public static string GetBadgeTier(BadgeProgress badge) { int n=badge.Target; if(badge.Metric=="points") return n>=10000?"Legend":n>=5000?"Expert":n>=3000?"Skilled":"Starter"; return n>=50?"Legend":n>=25?"Expert":n>=10?"Skilled":"Starter"; }
        private readonly LeaderboardRepository repository = new LeaderboardRepository();
        public OperationResult<LeaderboardData> GetPage(int page, bool byLevel = false)
        {
            if (!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner"))
                return OperationResult<LeaderboardData>.Fail("Please sign in with an active learner account.");
            try
            {
                int id = SessionHelper.GetCurrentUserID();
                var data = repository.GetPage(id, page, byLevel);
                if (data.Me == null) return OperationResult<LeaderboardData>.Fail("Your learner account is unavailable.");
                ApplyLevel(data.Me);
                foreach(var entry in data.Entries) ApplyLevel(entry);
                foreach(var entry in data.Podium) ApplyLevel(entry);
                var counts = repository.GetAchievements(id);
                if (counts == null) return OperationResult<LeaderboardData>.Fail("Your learner account is unavailable.");
                data.Badges = LoadBadges(id, counts);
                var badgeCounts = new Dictionary<int,int> { { id, data.EarnedCount } };
                foreach (var entry in data.Entries) SetBadgeCount(entry, badgeCounts);
                foreach (var entry in data.Podium) SetBadgeCount(entry, badgeCounts);
                data.Me.Badges=data.EarnedCount;
                return OperationResult<LeaderboardData>.Ok(data);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                LogHelper.LogError("Loading leaderboard", ex);
                return OperationResult<LeaderboardData>.Fail("The leaderboard is temporarily unavailable. Please try again.");
            }
        }
        public OperationResult<bool> SetFeaturedBadge(int badgeId)
        {
            if(!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner") || badgeId<0)
                return OperationResult<bool>.Fail("Choose an earned badge from your learner account.");
            try {
                return repository.SetFeaturedBadge(SessionHelper.GetCurrentUserID(),badgeId)
                    ? OperationResult<bool>.Ok(true) : OperationResult<bool>.Fail("You can only feature a badge you have earned.");
            } catch(System.Data.SqlClient.SqlException ex) {
                LogHelper.LogError("Saving featured badge",ex);
                return OperationResult<bool>.Fail("Your badge could not be saved. Please try again.");
            }
        }
        public OperationResult<int> GetEarnedCount()
        {
            if (!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner"))
                return OperationResult<int>.Fail("Learner access required.");
            try
            {
                var counts = repository.GetAchievements(SessionHelper.GetCurrentUserID());
                return counts == null ? OperationResult<int>.Fail("Account unavailable.") : OperationResult<int>.Ok(LoadBadges(SessionHelper.GetCurrentUserID(),counts).FindAll(b => b.Earned).Count);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                LogHelper.LogError("Loading badge count", ex);
                return OperationResult<int>.Fail("Badges unavailable.");
            }
        }
        private static void ApplyLevel(LeaderboardEntry entry) { var stage=LearnerManager.GetLevel(entry.Points); entry.Level=stage.Level; entry.LevelTitle=stage.Title; }
        private void SetBadgeCount(LeaderboardEntry entry, Dictionary<int,int> counts)
        {
            if (!counts.ContainsKey(entry.LearnerId))
                counts[entry.LearnerId]=LoadBadges(entry.LearnerId,entry.Counts).FindAll(b=>b.Earned).Count;
            entry.Badges=counts[entry.LearnerId];
        }
        private List<BadgeProgress> LoadBadges(int learnerId, AchievementCounts counts)
        {
            var badges=repository.GetBadges(learnerId);
            var awards=new List<int>();
            foreach(var badge in badges)
            {
                badge.Current=ProgressFor(badge.Metric,counts);
                if (!badge.Earned && badge.Current>=badge.Target) awards.Add(badge.Id);
            }
            if(awards.Count>0)
            {
                repository.SaveAwards(learnerId,awards);
                badges=repository.GetBadges(learnerId);
                foreach(var badge in badges) badge.Current=ProgressFor(badge.Metric,counts);
            }
            return badges;
        }
        private static int ProgressFor(string metric, AchievementCounts counts)
        {
            switch(metric)
            {
                case "lessons": return Math.Max(0,counts.Lessons);
                case "quizzes": return Math.Max(0,counts.Quizzes);
                case "challenges": return Math.Max(0,counts.Challenges);
                case "points": return Math.Max(0,counts.Points);
                default: throw new InvalidOperationException("Unsupported badge metric.");
            }
        }
    }
}
