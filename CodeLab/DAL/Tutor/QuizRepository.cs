using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;
using Newtonsoft.Json;

namespace CodeLab.DAL
{
    public class QuizRepository
    {
        private const string QuizColumns = "Id, CourseId, Title, PassMark, Instructions, CreatedByUserId, CreatedAt";
        private const string QuestionColumns =
            "Id, QuizId, QuestionText, QuestionType, OptionsJson, CorrectOptionIndex, Explanation, OrderIndex";

        /// <summary>
        /// Returns the course's quiz (without its questions), or null if it has none.
        /// </summary>
        public Quiz GetByCourse(int courseId)
        {
            string query = "SELECT TOP 1 " + QuizColumns + " FROM Quizzes WHERE CourseId = @CourseId ORDER BY Id";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadQuiz(reader) : null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a quiz's questions in order.
        /// </summary>
        public List<QuizQuestion> GetQuestions(int quizId)
        {
            List<QuizQuestion> questions = new List<QuizQuestion>();
            string query = "SELECT " + QuestionColumns + " FROM QuizQuestions WHERE QuizId = @QuizId ORDER BY OrderIndex, Id";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@QuizId", SqlDbType.Int).Value = quizId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            questions.Add(ReadQuestion(reader));
                        }
                    }
                }
            }

            return questions;
        }

        /// <summary>
        /// For each of a tutor's courses that has a quiz: course id → number of questions.
        /// </summary>
        public Dictionary<int, int> GetQuestionCountsByTutor(int tutorId)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            string query = @"SELECT q.CourseId, COUNT(qq.Id) AS QuestionCount
                               FROM Quizzes q
                               INNER JOIN Courses c ON c.Id = q.CourseId
                               LEFT JOIN QuizQuestions qq ON qq.QuizId = q.Id
                              WHERE c.InstructorId = @TutorId
                              GROUP BY q.CourseId";

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
                            counts[Convert.ToInt32(reader["CourseId"])] = Convert.ToInt32(reader["QuestionCount"]);
                        }
                    }
                }
            }

            return counts;
        }

        /// <summary>
        /// Adds a quiz to a course and returns its id, or 0 if it could not be saved. The insert only runs
        /// when the course has no quiz yet, so a course never gets a second one.
        /// </summary>
        public int Insert(Quiz quiz)
        {
            string query = @"INSERT INTO Quizzes (CourseId, Title, PassMark, Instructions, CreatedByUserId)
                             OUTPUT INSERTED.Id
                             SELECT @CourseId, @Title, @PassMark, @Instructions, @CreatedByUserId
                              WHERE NOT EXISTS (SELECT 1 FROM Quizzes WHERE CourseId = @CourseId)";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = quiz.CourseId;
                        command.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = quiz.CreatedByUserId;
                        AddQuizParameters(command, quiz);

                        connection.Open();
                        object id = command.ExecuteScalar();
                        return id == null ? 0 : Convert.ToInt32(id);
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("QuizRepository.Insert failure", sqlException);
                return 0;
            }
        }

        /// <summary>
        /// Saves a quiz's title, pass mark and instructions. Returns false if nothing was updated.
        /// </summary>
        public bool Update(Quiz quiz)
        {
            string query = @"UPDATE Quizzes
                                SET Title = @Title, PassMark = @PassMark, Instructions = @Instructions
                              WHERE Id = @Id AND CourseId = @CourseId";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = quiz.Id;
                        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = quiz.CourseId;
                        AddQuizParameters(command, quiz);

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("QuizRepository.Update failure", sqlException);
                return false;
            }
        }

        /// <summary>
        /// Deletes a quiz. Its questions and attempts are deleted with it (ON DELETE CASCADE).
        /// </summary>
        public bool Delete(int courseId, int quizId)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(
                        "DELETE FROM Quizzes WHERE Id = @Id AND CourseId = @CourseId", connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = quizId;
                        command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("QuizRepository.Delete failure", sqlException);
                return false;
            }
        }

        /// <summary>
        /// Adds a question after the quiz's last one and returns its id, or 0 if it could not be saved.
        /// </summary>
        public int InsertQuestion(QuizQuestion question)
        {
            string query = @"INSERT INTO QuizQuestions (QuizId, QuestionText, QuestionType, OptionsJson, CorrectOptionIndex, Explanation, OrderIndex)
                             OUTPUT INSERTED.Id
                             SELECT @QuizId, @QuestionText, @QuestionType, @OptionsJson, @CorrectOptionIndex, @Explanation,
                                    ISNULL(MAX(OrderIndex), 0) + 1
                               FROM QuizQuestions WITH (UPDLOCK, HOLDLOCK)
                              WHERE QuizId = @QuizId";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@QuizId", SqlDbType.Int).Value = question.QuizId;
                        AddQuestionParameters(command, question);

                        connection.Open();
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("QuizRepository.InsertQuestion failure", sqlException);
                return 0;
            }
        }

        /// <summary>
        /// Saves changes to a question; it keeps its position. Returns false if nothing was updated.
        /// </summary>
        public bool UpdateQuestion(QuizQuestion question)
        {
            string query = @"UPDATE QuizQuestions
                                SET QuestionText = @QuestionText, QuestionType = @QuestionType, OptionsJson = @OptionsJson,
                                    CorrectOptionIndex = @CorrectOptionIndex, Explanation = @Explanation
                              WHERE Id = @Id AND QuizId = @QuizId";

            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = question.Id;
                        command.Parameters.Add("@QuizId", SqlDbType.Int).Value = question.QuizId;
                        AddQuestionParameters(command, question);

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("QuizRepository.UpdateQuestion failure", sqlException);
                return false;
            }
        }

        /// <summary>
        /// Deletes a question and closes the gap it leaves in the order. Returns false if nothing was deleted.
        /// </summary>
        public bool DeleteQuestion(int quizId, int questionId)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        int deleted;
                        using (SqlCommand command = new SqlCommand(
                            "DELETE FROM QuizQuestions WHERE Id = @Id AND QuizId = @QuizId", connection, transaction))
                        {
                            command.Parameters.Add("@Id", SqlDbType.Int).Value = questionId;
                            command.Parameters.Add("@QuizId", SqlDbType.Int).Value = quizId;
                            deleted = command.ExecuteNonQuery();
                        }

                        if (deleted == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        // Number the remaining questions 1, 2, 3...
                        string renumber = @"WITH Ranked AS (
                                                SELECT Id, ROW_NUMBER() OVER (ORDER BY OrderIndex, Id) AS Position
                                                  FROM QuizQuestions WHERE QuizId = @QuizId)
                                            UPDATE q SET OrderIndex = r.Position
                                              FROM QuizQuestions q INNER JOIN Ranked r ON r.Id = q.Id
                                             WHERE q.OrderIndex <> r.Position";
                        using (SqlCommand command = new SqlCommand(renumber, connection, transaction))
                        {
                            command.Parameters.Add("@QuizId", SqlDbType.Int).Value = quizId;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("QuizRepository.DeleteQuestion failure", sqlException);
                return false;
            }
        }

        private static void AddQuizParameters(SqlCommand command, Quiz quiz)
        {
            command.Parameters.Add("@Title", SqlDbType.NVarChar, 150).Value = quiz.Title;
            command.Parameters.Add("@PassMark", SqlDbType.Int).Value = quiz.PassMark;
            command.Parameters.Add("@Instructions", SqlDbType.NVarChar, -1).Value = (object)quiz.Instructions ?? DBNull.Value;
        }

        private static void AddQuestionParameters(SqlCommand command, QuizQuestion question)
        {
            command.Parameters.Add("@QuestionText", SqlDbType.NVarChar, -1).Value = question.QuestionText;
            command.Parameters.Add("@QuestionType", SqlDbType.NVarChar, 30).Value = question.QuestionType;
            command.Parameters.Add("@OptionsJson", SqlDbType.NVarChar, -1).Value = JsonConvert.SerializeObject(question.Options);
            command.Parameters.Add("@CorrectOptionIndex", SqlDbType.Int).Value = question.CorrectOptionIndex;
            command.Parameters.Add("@Explanation", SqlDbType.NVarChar, -1).Value = question.Explanation ?? string.Empty;
        }

        private static Quiz ReadQuiz(SqlDataReader reader)
        {
            return new Quiz
            {
                Id = Convert.ToInt32(reader["Id"]),
                CourseId = Convert.ToInt32(reader["CourseId"]),
                Title = reader["Title"].ToString(),
                PassMark = Convert.ToInt32(reader["PassMark"]),
                Instructions = reader["Instructions"] as string,
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        private static QuizQuestion ReadQuestion(SqlDataReader reader)
        {
            List<string> options;
            try
            {
                options = JsonConvert.DeserializeObject<List<string>>(reader["OptionsJson"].ToString()) ?? new List<string>();
            }
            catch (JsonException)
            {
                options = new List<string>();
            }

            return new QuizQuestion
            {
                Id = Convert.ToInt32(reader["Id"]),
                QuizId = Convert.ToInt32(reader["QuizId"]),
                QuestionText = reader["QuestionText"].ToString(),
                QuestionType = reader["QuestionType"].ToString(),
                Options = options,
                CorrectOptionIndex = Convert.ToInt32(reader["CorrectOptionIndex"]),
                Explanation = reader["Explanation"].ToString(),
                OrderIndex = Convert.ToInt32(reader["OrderIndex"])
            };
        }
    }
}
