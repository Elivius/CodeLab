using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class ChallengeRepository
    {
        private const string ChallengeColumns =
            "Id, CourseId, Title, Description, Difficulty, Language, StarterCode, ExpectedOutput, PointsReward, CreatedAt";

        /// <summary>
        /// Returns the course's coding challenge, or null if it has none.
        /// </summary>
        public CodingChallenge GetByCourse(int courseId)
        {
            string query = "SELECT TOP 1 " + ChallengeColumns + " FROM CodingChallenges WHERE CourseId = @CourseId ORDER BY Id";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadChallenge(reader) : null;
                    }
                }
            }
        }

        /// <summary>
        /// Ids of the tutor's courses that have a coding challenge.
        /// </summary>
        public HashSet<int> GetCourseIdsWithChallenge(int tutorId)
        {
            HashSet<int> courseIds = new HashSet<int>();
            string query = @"SELECT DISTINCT ch.CourseId
                               FROM CodingChallenges ch
                               INNER JOIN Courses c ON c.Id = ch.CourseId
                              WHERE c.InstructorId = @TutorId";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@TutorId", SqlDbType.Int).Value = tutorId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courseIds.Add(Convert.ToInt32(reader["CourseId"]));
                        }
                    }
                }
            }

            return courseIds;
        }

        /// <summary>
        /// Adds a challenge to a course and returns its id, or 0 if it could not be saved. The insert only
        /// runs when the course has no challenge yet, so a course never gets a second one.
        /// </summary>
        public int Insert(CodingChallenge challenge)
        {
            string query = @"INSERT INTO CodingChallenges (CourseId, Title, Description, Difficulty, Language, StarterCode, ExpectedOutput, PointsReward)
                             OUTPUT INSERTED.Id
                             SELECT @CourseId, @Title, @Description, @Difficulty, @Language, @StarterCode, @ExpectedOutput, @PointsReward
                              WHERE NOT EXISTS (SELECT 1 FROM CodingChallenges WHERE CourseId = @CourseId)";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = challenge.CourseId;
                        AddParameters(command, challenge);

                        connection.Open();
                        object id = command.ExecuteScalar();
                        return id == null ? 0 : Convert.ToInt32(id);
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("ChallengeRepository.Insert failure", sqlException);
                return 0;
            }
        }

        /// <summary>
        /// Saves changes to a challenge. Returns false if nothing was updated.
        /// </summary>
        public bool Update(CodingChallenge challenge)
        {
            string query = @"UPDATE CodingChallenges
                                SET Title = @Title, Description = @Description, Difficulty = @Difficulty, Language = @Language,
                                    StarterCode = @StarterCode, ExpectedOutput = @ExpectedOutput, PointsReward = @PointsReward
                              WHERE Id = @Id AND CourseId = @CourseId";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = challenge.Id;
                        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = challenge.CourseId;
                        AddParameters(command, challenge);

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("ChallengeRepository.Update failure", sqlException);
                return false;
            }
        }

        /// <summary>
        /// Deletes a challenge. Its learner submissions are deleted with it (ON DELETE CASCADE).
        /// </summary>
        public bool Delete(int courseId, int challengeId)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(
                        "DELETE FROM CodingChallenges WHERE Id = @Id AND CourseId = @CourseId", connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = challengeId;
                        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("ChallengeRepository.Delete failure", sqlException);
                return false;
            }
        }

        private static void AddParameters(SqlCommand command, CodingChallenge challenge)
        {
            command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = challenge.Title;
            command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = challenge.Description;
            command.Parameters.Add("@Difficulty", SqlDbType.NVarChar, 20).Value = challenge.Difficulty;
            command.Parameters.Add("@Language", SqlDbType.NVarChar, 50).Value = challenge.Language;
            command.Parameters.Add("@StarterCode", SqlDbType.NVarChar, -1).Value = challenge.StarterCode ?? string.Empty;
            command.Parameters.Add("@ExpectedOutput", SqlDbType.NVarChar, -1).Value = challenge.ExpectedOutput;
            command.Parameters.Add("@PointsReward", SqlDbType.Int).Value = challenge.PointsReward;
        }

        private static CodingChallenge ReadChallenge(SqlDataReader reader)
        {
            return new CodingChallenge
            {
                Id = Convert.ToInt32(reader["Id"]),
                CourseId = Convert.ToInt32(reader["CourseId"]),
                Title = reader["Title"].ToString(),
                Description = reader["Description"].ToString(),
                Difficulty = reader["Difficulty"].ToString(),
                Language = reader["Language"].ToString(),
                StarterCode = reader["StarterCode"].ToString(),
                ExpectedOutput = reader["ExpectedOutput"].ToString(),
                PointsReward = Convert.ToInt32(reader["PointsReward"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
