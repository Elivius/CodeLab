using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class LessonRepository
    {
        private const string LessonColumns =
            "Id, CourseId, Title, Content, CodeSnippet, CodeLanguage, VideoUrl, AttachmentUrl, AttachmentName, OrderIndex, CreatedAt";

        /// <summary>
        /// Returns a course's lessons in the order learners see them.
        /// </summary>
        public List<Lesson> GetByCourse(int courseId)
        {
            List<Lesson> lessons = new List<Lesson>();
            string query = "SELECT " + LessonColumns + " FROM Lessons WHERE CourseId = @CourseId ORDER BY OrderIndex, Id";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lessons.Add(ReadLesson(reader));
                        }
                    }
                }
            }

            return lessons;
        }

        /// <summary>
        /// Returns one lesson of a course, or null if the course has no lesson with this id.
        /// </summary>
        public Lesson GetById(int courseId, int lessonId)
        {
            string query = "SELECT " + LessonColumns + " FROM Lessons WHERE Id = @Id AND CourseId = @CourseId";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = lessonId;
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadLesson(reader) : null;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a lesson at the position in lesson.OrderIndex (clamped to 1..count+1) and moves the
        /// lessons after it down, so positions stay 1, 2, 3... Returns the new id, or 0 on failure.
        /// </summary>
        public int Insert(Lesson lesson)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        int count = CountLessons(connection, transaction, lesson.CourseId);
                        int position = Math.Max(1, Math.Min(lesson.OrderIndex, count + 1));

                        string insert = @"INSERT INTO Lessons (CourseId, Title, Content, CodeSnippet, CodeLanguage, VideoUrl, AttachmentUrl, AttachmentName, OrderIndex)
                                          OUTPUT INSERTED.Id
                                          VALUES (@CourseId, @Title, @Content, @CodeSnippet, @CodeLanguage, @VideoUrl, @AttachmentUrl, @AttachmentName, @OrderIndex)";

                        int newId;
                        using (SqlCommand command = new SqlCommand(insert, connection, transaction))
                        {
                            AddLessonParameters(command, lesson);
                            command.Parameters.Add("@CourseId", SqlDbType.Int).Value = lesson.CourseId;
                            command.Parameters.Add("@OrderIndex", SqlDbType.Int).Value = position;
                            newId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        Renumber(connection, transaction, lesson.CourseId, newId, position, position);
                        transaction.Commit();
                        return newId;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("LessonRepository.Insert failure", sqlException);
                return 0;
            }
        }

        /// <summary>
        /// Saves changes to a lesson and moves it to lesson.OrderIndex, shifting the others to make room.
        /// Returns false if the course has no such lesson or nothing could be saved.
        /// </summary>
        public bool Update(Lesson lesson)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        int oldPosition;
                        using (SqlCommand command = new SqlCommand(
                            "SELECT OrderIndex FROM Lessons WHERE Id = @Id AND CourseId = @CourseId", connection, transaction))
                        {
                            command.Parameters.Add("@Id", SqlDbType.Int).Value = lesson.Id;
                            command.Parameters.Add("@CourseId", SqlDbType.Int).Value = lesson.CourseId;
                            object found = command.ExecuteScalar();
                            if (found == null)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            oldPosition = Convert.ToInt32(found);
                        }

                        int count = CountLessons(connection, transaction, lesson.CourseId);
                        int newPosition = Math.Max(1, Math.Min(lesson.OrderIndex, count));

                        string update = @"UPDATE Lessons
                                             SET Title = @Title, Content = @Content, CodeSnippet = @CodeSnippet, CodeLanguage = @CodeLanguage,
                                                 VideoUrl = @VideoUrl, AttachmentUrl = @AttachmentUrl, AttachmentName = @AttachmentName, OrderIndex = @OrderIndex
                                           WHERE Id = @Id AND CourseId = @CourseId";

                        using (SqlCommand command = new SqlCommand(update, connection, transaction))
                        {
                            AddLessonParameters(command, lesson);
                            command.Parameters.Add("@Id", SqlDbType.Int).Value = lesson.Id;
                            command.Parameters.Add("@CourseId", SqlDbType.Int).Value = lesson.CourseId;
                            command.Parameters.Add("@OrderIndex", SqlDbType.Int).Value = newPosition;
                            command.ExecuteNonQuery();
                        }

                        Renumber(connection, transaction, lesson.CourseId, lesson.Id, newPosition, oldPosition);
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("LessonRepository.Update failure", sqlException);
                return false;
            }
        }

        /// <summary>
        /// Deletes a lesson and closes the gap it leaves in the order. Returns false if nothing was deleted.
        /// </summary>
        public bool Delete(int courseId, int lessonId)
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
                            "DELETE FROM Lessons WHERE Id = @Id AND CourseId = @CourseId", connection, transaction))
                        {
                            command.Parameters.Add("@Id", SqlDbType.Int).Value = lessonId;
                            command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                            deleted = command.ExecuteNonQuery();
                        }

                        if (deleted == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        Renumber(connection, transaction, courseId, 0, 0, 0);
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError("LessonRepository.Delete failure", sqlException);
                return false;
            }
        }

        private static int CountLessons(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Lessons WHERE CourseId = @CourseId", connection, transaction))
            {
                command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        // Puts a course's lessons back in order as 1, 2, 3... after one lesson was added, moved or removed.
        // The moved lesson goes before the lesson that already holds its new position when it moves up,
        // and after it when it moves down; pass lessonId 0 when nothing moved (after a delete).
        private static void Renumber(SqlConnection connection, SqlTransaction transaction, int courseId,
            int lessonId, int newPosition, int oldPosition)
        {
            string query = @"WITH Ranked AS (
                                 SELECT Id,
                                        ROW_NUMBER() OVER (ORDER BY
                                            CASE WHEN Id = @LessonId
                                                 THEN @NewPosition * 2 + CASE WHEN @NewPosition > @OldPosition THEN 1 ELSE -1 END
                                                 ELSE OrderIndex * 2 END,
                                            Id) AS Position
                                   FROM Lessons
                                  WHERE CourseId = @CourseId)
                             UPDATE l SET OrderIndex = r.Position
                               FROM Lessons l
                               INNER JOIN Ranked r ON r.Id = l.Id
                              WHERE l.OrderIndex <> r.Position";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                command.Parameters.Add("@LessonId", SqlDbType.Int).Value = lessonId;
                command.Parameters.Add("@NewPosition", SqlDbType.Int).Value = newPosition;
                command.Parameters.Add("@OldPosition", SqlDbType.Int).Value = oldPosition;
                command.ExecuteNonQuery();
            }
        }

        private static void AddLessonParameters(SqlCommand command, Lesson lesson)
        {
            command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = lesson.Title;
            command.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = lesson.Content;
            command.Parameters.Add("@CodeSnippet", SqlDbType.NVarChar, -1).Value = (object)lesson.CodeSnippet ?? DBNull.Value;
            command.Parameters.Add("@CodeLanguage", SqlDbType.NVarChar, 50).Value = (object)lesson.CodeLanguage ?? DBNull.Value;
            command.Parameters.Add("@AttachmentUrl", SqlDbType.NVarChar, 500).Value = (object)lesson.AttachmentUrl ?? DBNull.Value;
            command.Parameters.Add("@AttachmentName", SqlDbType.NVarChar, 255).Value = (object)lesson.AttachmentName ?? DBNull.Value;
            command.Parameters.Add("@VideoUrl", SqlDbType.NVarChar, 500).Value = (object)lesson.VideoUrl ?? DBNull.Value;
        }

        private static Lesson ReadLesson(SqlDataReader reader)
        {
            return new Lesson
            {
                Id = Convert.ToInt32(reader["Id"]),
                CourseId = Convert.ToInt32(reader["CourseId"]),
                Title = reader["Title"].ToString(),
                Content = reader["Content"].ToString(),
                CodeSnippet = reader["CodeSnippet"] as string,
                CodeLanguage = reader["CodeLanguage"] as string,
                AttachmentUrl = reader["AttachmentUrl"] as string,
                AttachmentName = reader["AttachmentName"] as string,
                VideoUrl = reader["VideoUrl"] as string,
                OrderIndex = Convert.ToInt32(reader["OrderIndex"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
