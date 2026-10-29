using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public sealed class LeaderboardRepository
    {
        public LeaderboardData GetPage(int learnerId, int requestedPage, bool byLevel = false)
        {
            const string sql = @"
SET NOCOUNT ON;
DECLARE @Ranks TABLE (LearnerId int, DisplayName nvarchar(120), Points int, Position bigint, RowNumber bigint,
 Username nvarchar(50), Level int, LevelTitle nvarchar(50), Completed int, Lessons int, Quizzes int, Challenges int, AvatarUrl nvarchar(500));
INSERT @Ranks
SELECT u.Id, u.FullName, COALESCE(s.AchievementPoints,0),
       RANK() OVER (ORDER BY COALESCE(s.AchievementPoints,0) DESC),
       ROW_NUMBER() OVER (ORDER BY COALESCE(s.AchievementPoints,0) DESC,u.Id),
       u.Username,COALESCE(s.CurrentLevel,1),COALESCE(s.LevelTitle,N'Novice Coder'),
       (SELECT COUNT(*) FROM dbo.CourseEnrollments e WHERE e.LearnerId=u.Id AND e.IsCompleted=1),
       (SELECT COUNT(*) FROM dbo.LessonCompletions lc WHERE lc.LearnerId=u.Id),
       (SELECT COUNT(DISTINCT QuizId) FROM dbo.QuizAttempts qa WHERE qa.LearnerId=u.Id AND qa.Passed=1),
       (SELECT COUNT(DISTINCT ChallengeId) FROM dbo.CodeSubmissions cs WHERE cs.LearnerId=u.Id AND cs.Status=N'Passed'),u.AvatarUrl
FROM dbo.Users u LEFT JOIN dbo.LearnerStats s ON s.LearnerId=u.Id
WHERE u.Role=N'Learner' AND u.Status=N'Active';
DECLARE @Total int=(SELECT COUNT(*) FROM @Ranks);
DECLARE @Pages int=CASE WHEN @Total=0 THEN 1 ELSE (@Total+19)/20 END;
DECLARE @Page int=CASE WHEN @RequestedPage>@Pages THEN @Pages ELSE @RequestedPage END;
SELECT @Total AS Total, @Page AS Page;
SELECT LearnerId, DisplayName, Points, Position, Username, Level, LevelTitle, Completed, Lessons, Quizzes, Challenges, AvatarUrl FROM @Ranks WHERE RowNumber BETWEEN (@Page-1)*20+1 AND @Page*20 ORDER BY RowNumber;
SELECT LearnerId, DisplayName, Points, Position, Username, Level, LevelTitle, Completed, Lessons, Quizzes, Challenges, AvatarUrl FROM @Ranks WHERE LearnerId=@LearnerId;
SELECT TOP (3) LearnerId, DisplayName, Points, Position, Username, Level, LevelTitle, Completed, Lessons, Quizzes, Challenges, AvatarUrl FROM @Ranks ORDER BY RowNumber;";
            var data = new LeaderboardData();
            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@LearnerId", SqlDbType.Int).Value = learnerId;
                command.Parameters.Add("@ByLevel", SqlDbType.Bit).Value = byLevel;
                command.Parameters.Add("@RequestedPage", SqlDbType.Int).Value = Math.Max(1, requestedPage);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    reader.Read(); data.TotalLearners = reader.GetInt32(0); data.Page = reader.GetInt32(1);
                    reader.NextResult();
                    while (reader.Read()) data.Entries.Add(ReadEntry(reader));
                    reader.NextResult();
                    if (reader.Read()) data.Me = ReadEntry(reader);
                    reader.NextResult(); while (reader.Read()) data.Podium.Add(ReadEntry(reader));
                }
            }
            return data;
        }
        public AchievementCounts GetAchievements(int learnerId)
        {
            const string sql = @"
SELECT
 (SELECT COUNT(*) FROM dbo.LessonCompletions WHERE LearnerId=@Id),
 (SELECT COUNT(DISTINCT QuizId) FROM dbo.QuizAttempts WHERE LearnerId=@Id AND Passed=1),
 (SELECT COUNT(DISTINCT ChallengeId) FROM dbo.CodeSubmissions WHERE LearnerId=@Id AND Status=N'Passed'),
 COALESCE((SELECT AchievementPoints FROM dbo.LearnerStats WHERE LearnerId=@Id),0)
WHERE EXISTS (SELECT 1 FROM dbo.Users WHERE Id=@Id AND Role=N'Learner' AND Status=N'Active');";
            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = learnerId;
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new AchievementCounts { Lessons=reader.GetInt32(0), Quizzes=reader.GetInt32(1), Challenges=reader.GetInt32(2), Points=reader.GetInt32(3) };
                }
            }
        }
        public List<BadgeProgress> GetBadges(int learnerId)
        {
            var badges = new List<BadgeProgress>();
            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(@"SELECT b.Id,b.Name,b.Description,b.Icon,b.Category,b.Metric,b.Target,lb.EarnedAtUtc,CAST(COALESCE(lb.IsFeatured,0) AS bit)
FROM dbo.Badges b LEFT JOIN dbo.LearnerBadges lb ON lb.BadgeId=b.Id AND lb.LearnerId=@Id ORDER BY b.SortOrder,b.Id;", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value=learnerId;
                connection.Open();
                using (var reader=command.ExecuteReader())
                    while(reader.Read()) badges.Add(new BadgeProgress { IsFeatured=reader.GetBoolean(8), Id=reader.GetInt32(0), Name=reader.GetString(1), Description=reader.GetString(2), Icon=reader.GetString(3), Category=reader.GetString(4), Metric=reader.GetString(5), Target=reader.GetInt32(6), EarnedAtUtc=reader.IsDBNull(7) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(7),DateTimeKind.Utc) });
            }
            return badges;
        }
        public bool SetFeaturedBadge(int learnerId,int badgeId)
        {
            using(var connection=DatabaseHelper.GetConnection())
            using(var command=new SqlCommand(@"SET XACT_ABORT ON;
BEGIN TRAN;
IF NOT EXISTS(SELECT 1 FROM dbo.Users WITH(UPDLOCK,HOLDLOCK) WHERE Id=@Id AND Role=N'Learner' AND Status=N'Active')
BEGIN ROLLBACK; SELECT 0; RETURN; END;
IF @BadgeId<>0 AND NOT EXISTS(SELECT 1 FROM dbo.LearnerBadges WHERE LearnerId=@Id AND BadgeId=@BadgeId)
BEGIN ROLLBACK; SELECT 0; RETURN; END;
UPDATE dbo.LearnerBadges SET IsFeatured=0 WHERE LearnerId=@Id AND IsFeatured=1;
UPDATE dbo.LearnerBadges SET IsFeatured=1 WHERE LearnerId=@Id AND BadgeId=@BadgeId;
COMMIT; SELECT 1;",connection))
            {
                command.Parameters.Add("@Id",SqlDbType.Int).Value=learnerId;
                command.Parameters.Add("@BadgeId",SqlDbType.Int).Value=badgeId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar())==1;
            }
        }
        public void SaveAwards(int learnerId, List<int> badgeIds)
        {
            if (badgeIds.Count==0) return;
            using (var connection=DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (var transaction=connection.BeginTransaction())
                {
                    // Serialize awards for this learner so simultaneous requests cannot duplicate them.
                    using (var guard=new SqlCommand("SELECT Id FROM dbo.Users WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Id AND Role=N'Learner' AND Status=N'Active';",connection,transaction))
                    {
                        guard.Parameters.Add("@Id",SqlDbType.Int).Value=learnerId;
                        if (guard.ExecuteScalar()==null) { transaction.Rollback(); return; }
                    }
                    foreach (int badgeId in badgeIds)
                    using (var command=new SqlCommand(@"INSERT dbo.LearnerBadges(LearnerId,BadgeId)
SELECT @Id,@BadgeId WHERE NOT EXISTS (SELECT 1 FROM dbo.LearnerBadges WHERE LearnerId=@Id AND BadgeId=@BadgeId);",connection,transaction))
                    {
                        command.Parameters.Add("@Id",SqlDbType.Int).Value=learnerId;
                        command.Parameters.Add("@BadgeId",SqlDbType.Int).Value=badgeId;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
        private static LeaderboardEntry ReadEntry(SqlDataReader reader)
        {
            return new LeaderboardEntry { LearnerId=reader.GetInt32(0), Name=reader.GetString(1), Points=reader.GetInt32(2), Rank=reader.GetInt64(3), Username=reader.GetString(4), Level=reader.GetInt32(5), LevelTitle=reader.GetString(6), Completed=reader.GetInt32(7), AvatarUrl=reader.IsDBNull(11) ? null : reader.GetString(11), Counts=new AchievementCounts {Points=reader.GetInt32(2),Lessons=reader.GetInt32(8),Quizzes=reader.GetInt32(9),Challenges=reader.GetInt32(10)} };
        }
    }
}
