using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class CourseRepository
    {
        /// <summary>
        /// Returns the courses one tutor owns, most recently updated first,
        /// with how many lessons and enrolled students each one has.
        /// </summary>
        public List<TutorCourseSummary> GetCourseSummariesByTutor(int tutorId)
        {
            List<TutorCourseSummary> courses = new List<TutorCourseSummary>();
            string query = @"SELECT c.Id, c.Title, c.Category, c.Difficulty, c.Status, c.UpdatedAt,
                                    (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId = c.Id) AS LessonCount,
                                    (SELECT COUNT(*) FROM CourseEnrollments e WHERE e.CourseId = c.Id) AS StudentCount
                             FROM Courses c
                             WHERE c.InstructorId = @TutorId
                             ORDER BY c.UpdatedAt DESC, c.Id DESC";

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
                            courses.Add(new TutorCourseSummary
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Category = reader["Category"].ToString(),
                                Difficulty = reader["Difficulty"].ToString(),
                                Status = reader["Status"].ToString(),
                                LessonCount = Convert.ToInt32(reader["LessonCount"]),
                                StudentCount = Convert.ToInt32(reader["StudentCount"]),
                                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                            });
                        }
                    }
                }
            }

            return courses;
        }
    }
}
