using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public sealed class LearnerChallengeDetailRepository
    {
        public LearnerChallengeDetail GetChallenge(
            int learnerId,
            int challengeId)
        {
            ValidateIds(learnerId, challengeId);

            const string sql = @"
SELECT
    ch.Id,
    ch.CourseId,
    c.Title AS CourseTitle,
    ch.Title,
    ch.Description,
    ch.Difficulty,
    ch.Language,
    ch.StarterCode,
    ch.ExpectedOutput,
    ch.PointsReward,
    stats.AttemptCount,
    CAST(COALESCE(stats.HasPassed, 0) AS bit) AS IsSolved,
    stats.LastSubmittedAt
FROM dbo.CodingChallenges AS ch
INNER JOIN dbo.Courses AS c
    ON c.Id = ch.CourseId
OUTER APPLY
(
    SELECT
        COUNT(*) AS AttemptCount,
        MAX(
            CASE
                WHEN s.Status = N'Passed' THEN 1
                ELSE 0
            END
        ) AS HasPassed,
        MAX(s.SubmittedAt) AS LastSubmittedAt
    FROM dbo.CodeSubmissions AS s
    WHERE s.ChallengeId = ch.Id
      AND s.LearnerId = @LearnerId
) AS stats
WHERE ch.Id = @ChallengeId
  AND c.Status = N'Published'
  AND EXISTS
  (
      SELECT 1
      FROM dbo.CourseEnrollments AS e
      WHERE e.CourseId = c.Id
        AND e.LearnerId = @LearnerId
        AND e.IsCompleted = 1
  );";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddParameters(command, learnerId, challengeId);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        // Missing challenge or learner does not have access.
                        return null;
                    }

                    return new LearnerChallengeDetail
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CourseId = Convert.ToInt32(reader["CourseId"]),
                        CourseTitle = Convert.ToString(reader["CourseTitle"]),
                        Title = Convert.ToString(reader["Title"]),
                        Description = Convert.ToString(reader["Description"]),
                        Difficulty = Convert.ToString(reader["Difficulty"]),
                        Language = Convert.ToString(reader["Language"]),
                        StarterCode = Convert.ToString(reader["StarterCode"]),
                        ExpectedOutput = Convert.ToString(reader["ExpectedOutput"]),
                        PointsReward = Convert.ToInt32(reader["PointsReward"]),
                        AttemptCount = Convert.ToInt32(reader["AttemptCount"]),
                        IsSolved = Convert.ToBoolean(reader["IsSolved"]),
                        LastSubmittedAt =
                            reader["LastSubmittedAt"] == DBNull.Value
                                ? (DateTime?)null
                                : ReadUtcDate(reader["LastSubmittedAt"])
                    };
                }
            }
        }

        public List<LearnerChallengeSubmission> GetSubmissions(
            int learnerId,
            int challengeId)
        {
            ValidateIds(learnerId, challengeId);

            var submissions = new List<LearnerChallengeSubmission>();

            const string sql = @"
SELECT
    s.Id,
    s.LearnerId,
    s.ChallengeId,
    s.SubmittedCode,
    s.Status,
    s.OutputResult, s.AiFeedback,
    s.SubmittedAt
FROM dbo.CodeSubmissions AS s
INNER JOIN dbo.CodingChallenges AS ch
    ON ch.Id = s.ChallengeId
INNER JOIN dbo.Courses AS c
    ON c.Id = ch.CourseId
WHERE s.LearnerId = @LearnerId
  AND s.ChallengeId = @ChallengeId
  AND c.Status = N'Published'
  AND EXISTS
  (
      SELECT 1
      FROM dbo.CourseEnrollments AS e
      WHERE e.CourseId = c.Id
        AND e.LearnerId = @LearnerId
        AND e.IsCompleted = 1
  )
ORDER BY s.SubmittedAt DESC, s.Id DESC;";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddParameters(command, learnerId, challengeId);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        submissions.Add(new LearnerChallengeSubmission
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            LearnerId = Convert.ToInt32(reader["LearnerId"]),
                            ChallengeId = Convert.ToInt32(reader["ChallengeId"]),
                            SubmittedCode =
                                Convert.ToString(reader["SubmittedCode"]),
                            Status = Convert.ToString(reader["Status"]), AiFeedback = Convert.ToString(reader["AiFeedback"]),
                            OutputResult =
                                reader["OutputResult"] == DBNull.Value
                                    ? string.Empty
                                    : Convert.ToString(reader["OutputResult"]),
                            SubmittedAt = ReadUtcDate(reader["SubmittedAt"])
                        });
                    }
                }
            }

            return submissions;
        }

        private static void AddParameters(
            SqlCommand command,
            int learnerId,
            int challengeId)
        {
            command.Parameters.Add(
                "@LearnerId",
                SqlDbType.Int).Value = learnerId;

            command.Parameters.Add(
                "@ChallengeId",
                SqlDbType.Int).Value = challengeId;
        }

        private static DateTime ReadUtcDate(object value)
        {
            return DateTime.SpecifyKind(
                Convert.ToDateTime(value),
                DateTimeKind.Utc);
        }

        private static void ValidateIds(int learnerId, int challengeId)
        {
            if (learnerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid learner ID.",
                    nameof(learnerId));
            }

            if (challengeId <= 0)
            {
                throw new ArgumentException(
                    "Invalid challenge ID.",
                    nameof(challengeId));
            }
        }

        public ChallengeSubmissionResult SaveSubmission(
    int learnerId,
    int challengeId,
    string submittedCode,
    string status,
    string output, string aiFeedback = "")
        {
            ValidateIds(learnerId, challengeId);

            if (string.IsNullOrWhiteSpace(submittedCode) ||
                submittedCode.Length > 20000)
            {
                throw new ArgumentException(
                    "Submitted code must contain between 1 and 20,000 characters.",
                    nameof(submittedCode));
            }

            if (status != "Passed" &&
                status != "Failed" &&
                status != "Error" && status != "Pending review")
            {
                throw new ArgumentException(
                    "Invalid submission status.",
                    nameof(status));
            }

            // Status must come from server-side grading.
            // Never pass a status supplied by the browser to this method.
            const string sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @LockedLearnerId INT;
DECLARE @Reward INT;
DECLARE @AwardedPoints INT = 0;

-- Serialize submission saves for this learner.
SELECT @LockedLearnerId = Id
FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
WHERE Id = @LearnerId
  AND Role = N'Learner'
  AND Status = N'Active';

IF @LockedLearnerId IS NULL
BEGIN
    THROW 50001, 'The learner account is unavailable.', 1;
END;

-- Check access again when saving.
SELECT @Reward = ch.PointsReward
FROM dbo.CodingChallenges AS ch WITH (HOLDLOCK)
INNER JOIN dbo.Courses AS c WITH (HOLDLOCK)
    ON c.Id = ch.CourseId
WHERE ch.Id = @ChallengeId
  AND c.Status = N'Published'
  AND EXISTS
  (
      SELECT 1
      FROM dbo.CourseEnrollments AS e WITH (HOLDLOCK)
      WHERE e.CourseId = ch.CourseId
        AND e.LearnerId = @LearnerId
        AND e.IsCompleted = 1
  );

IF @Reward IS NULL
BEGIN
    THROW 50002, 'The challenge is unavailable or enrolment has changed.', 1;
END;

-- A previously passed challenge never awards XP again.
IF @Status = N'Passed'
   AND NOT EXISTS
   (
       SELECT 1
       FROM dbo.CodeSubmissions WITH (UPDLOCK, HOLDLOCK)
       WHERE LearnerId = @LearnerId
         AND ChallengeId = @ChallengeId
         AND Status = N'Passed'
   )
BEGIN
    SET @AwardedPoints =
        CASE WHEN @Reward > 0 THEN @Reward ELSE 0 END;
END;

INSERT INTO dbo.CodeSubmissions
(
    LearnerId,
    ChallengeId,
    SubmittedCode,
    Status,
    OutputResult, AiFeedback,
    SubmittedAt
)
VALUES
(
    @LearnerId,
    @ChallengeId,
    @SubmittedCode,
    @Status,
    @Output, @AiFeedback,
    SYSUTCDATETIME()
);

DECLARE @SubmissionId INT =
    CONVERT(INT, SCOPE_IDENTITY());

IF @AwardedPoints > 0
BEGIN
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.LearnerStats WITH (UPDLOCK, HOLDLOCK)
        WHERE LearnerId = @LearnerId
    )
    BEGIN
        INSERT INTO dbo.LearnerStats (LearnerId)
        VALUES (@LearnerId);
    END;

    UPDATE dbo.LearnerStats
    SET AchievementPoints = AchievementPoints + @AwardedPoints
    WHERE LearnerId = @LearnerId;
END;

SELECT
    @SubmissionId AS SubmissionId,
    @AwardedPoints AS PointsAwarded;";

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int submissionId;
                        int pointsAwarded;

                        using (var command = new SqlCommand(
                            sql,
                            connection,
                            transaction))
                        {
                            AddParameters(command, learnerId, challengeId);
                            command.Parameters.Add("@AiFeedback",SqlDbType.NVarChar,-1).Value=aiFeedback ?? "";

                            command.Parameters.Add(
                                "@SubmittedCode",
                                SqlDbType.NVarChar,
                                -1).Value = submittedCode;

                            command.Parameters.Add(
                                "@Status",
                                SqlDbType.NVarChar,
                                30).Value = status;

                            command.Parameters.Add(
                                "@Output",
                                SqlDbType.NVarChar,
                                -1).Value = output ?? string.Empty;

                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new InvalidOperationException(
                                        "The submission could not be confirmed.");
                                }

                                submissionId = Convert.ToInt32(
                                    reader["SubmissionId"]);

                                pointsAwarded = Convert.ToInt32(
                                    reader["PointsAwarded"]);
                            }
                        }

                        transaction.Commit();

                        return new ChallengeSubmissionResult
                        {
                            Saved = true,
                            SubmissionId = submissionId,
                            Status = status,
                            Output = output ?? string.Empty,
                            PointsAwarded = pointsAwarded,
                            ErrorMessage = string.Empty
                        };
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (SqlException)
                        {
                            // SQL Server may already have rolled back.
                        }
                        catch (InvalidOperationException)
                        {
                            // The transaction may already be completed.
                        }

                        throw;
                    }
                }
            }
        }
    }
}