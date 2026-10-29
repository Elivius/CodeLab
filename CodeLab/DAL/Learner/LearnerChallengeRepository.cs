using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public sealed class LearnerChallengeRepository
    {
        /// <summary>
        /// Loads challenges from published courses the learner
        /// is currently enrolled in.
        ///
        /// Submission statistics belong only to this learner.
        /// Starter code and expected output are not loaded
        /// for the browsing page.
        /// </summary>
        public List<LearnerChallenge> GetChallenges(int learnerId)
        {
            if (learnerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid learner ID.",
                    "learnerId");
            }

            const string sql = @"
SELECT
    ch.Id,
    CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.CourseEnrollments ce WHERE ce.CourseId=ch.CourseId AND ce.LearnerId=@learner AND ce.IsCompleted=1) THEN 1 ELSE 0 END AS bit) AS CanAccess,
    ch.CourseId,
    c.Title AS CourseTitle,
    ch.Title,
    ch.Description,
    ch.Difficulty,
    ch.Language,
    ch.PointsReward,
    COALESCE(s.AttemptCount, 0) AS AttemptCount,
    CAST(COALESCE(s.HasPassed, 0) AS bit) AS IsSolved,
    s.LastSubmittedAt
FROM dbo.CodingChallenges ch
INNER JOIN dbo.Courses c
    ON c.Id = ch.CourseId
LEFT JOIN
(
    SELECT
        ChallengeId,
        COUNT(*) AS AttemptCount,
        MAX(
            CASE WHEN Status = N'Passed'
                 THEN 1 ELSE 0 END
        ) AS HasPassed,
        MAX(SubmittedAt) AS LastSubmittedAt
    FROM dbo.CodeSubmissions
    WHERE LearnerId = @learner
    GROUP BY ChallengeId
) s
    ON s.ChallengeId = ch.Id
WHERE c.Status = N'Published'
  AND EXISTS
  (
      SELECT 1
      FROM dbo.CourseEnrollments e
      WHERE e.CourseId = c.Id
        AND e.LearnerId = @learner

  )
ORDER BY
    c.Title,
    c.Id,
    CASE ch.Difficulty
        WHEN N'Beginner' THEN 1
        WHEN N'Intermediate' THEN 2
        WHEN N'Advanced' THEN 3
        ELSE 4
    END,
    ch.Title,
    ch.Id;";

            var challenges = new List<LearnerChallenge>();

            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            using (SqlCommand command =
                new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@learner",
                    SqlDbType.Int).Value = learnerId;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        challenges.Add(new LearnerChallenge
                        {
                            Id = Convert.ToInt32(reader["Id"]), CanAccess = Convert.ToBoolean(reader["CanAccess"]),

                            CourseId = Convert.ToInt32(
                                reader["CourseId"]),

                            CourseTitle = Convert.ToString(
                                reader["CourseTitle"]),

                            Title = Convert.ToString(
                                reader["Title"]),

                            Description = Convert.ToString(
                                reader["Description"]),

                            Difficulty = Convert.ToString(
                                reader["Difficulty"]),

                            Language = Convert.ToString(
                                reader["Language"]),

                            PointsReward = Convert.ToInt32(
                                reader["PointsReward"]),

                            AttemptCount = Convert.ToInt32(
                                reader["AttemptCount"]),

                            IsSolved = Convert.ToBoolean(
                                reader["IsSolved"]),

                            LastSubmittedAt =
                                reader["LastSubmittedAt"] == DBNull.Value
                                    ? (DateTime?)null
                                    : DateTime.SpecifyKind(
                                        Convert.ToDateTime(
                                            reader["LastSubmittedAt"]),
                                        DateTimeKind.Utc)
                        });
                    }
                }
            }

            return challenges;
        }
    }
}