using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public class LearnerCourseRepository
    {
        private const string CourseSelect = @"
SELECT c.Id, c.Title, c.Description, c.Category, c.Difficulty,
       u.FullName AS Instructor, c.ThumbnailUrl,
       (SELECT COUNT(*) FROM dbo.Lessons l WHERE l.CourseId=c.Id) AS LessonCount,
       CASE WHEN e.Id IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsEnrolled,
       ISNULL(e.IsCompleted,0) AS IsCompleted,
       ISNULL(e.ProgressPercent,0) AS ProgressPercent
FROM dbo.Courses c
INNER JOIN dbo.Users u ON u.Id=c.InstructorId
LEFT JOIN dbo.CourseEnrollments e ON e.CourseId=c.Id AND e.LearnerId=@learner
WHERE c.Status=N'Published'";

        public List<LearnerCourse> GetCourses(int learnerId, bool enrolledOnly, string search, string category)
        {
            var result = new List<LearnerCourse>();
            using (var conn = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(CourseSelect + @"
AND (@enrolled=0 OR e.Id IS NOT NULL)
AND (@search=N'' OR c.Title LIKE N'%' + @search + N'%' OR c.Category LIKE N'%' + @search + N'%' OR u.FullName LIKE N'%' + @search + N'%')
AND (@category=N'' OR c.Category=@category)
ORDER BY c.CreatedAt DESC, c.Id DESC", conn))
            {
                cmd.Parameters.Add("@learner", SqlDbType.Int).Value = learnerId;
                cmd.Parameters.Add("@enrolled", SqlDbType.Bit).Value = enrolledOnly;
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, 200).Value = search ?? "";
                cmd.Parameters.Add("@category", SqlDbType.NVarChar, 80).Value = category ?? "";
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(ReadCourse(r));
            }
            return result;
        }

        public List<string> GetCategories()
        {
            var result = new List<string>();
            using (var conn = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand("SELECT DISTINCT Category FROM dbo.Courses WHERE Status=N'Published' ORDER BY Category", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(r.GetString(0));
            }
            return result;
        }

        public LearnerCourse GetCourse(int learnerId, int courseId)
        {
            LearnerCourse course = null;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(CourseSelect + " AND c.Id=@course", conn))
                {
                    cmd.Parameters.Add("@learner", SqlDbType.Int).Value = learnerId;
                    cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) course = ReadCourse(r);
                }
                if (course == null) return null;
                using (var cmd = new SqlCommand(@"
SELECT l.Id,l.Title,l.Content,l.CodeSnippet,l.CodeLanguage,l.VideoUrl,l.DurationMinutes,
CASE WHEN lc.LessonId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsCompleted
FROM dbo.Lessons l
LEFT JOIN dbo.LessonCompletions lc ON lc.LessonId=l.Id AND lc.LearnerId=@learner
WHERE l.CourseId=@course ORDER BY l.OrderIndex,l.Id", conn))
                {
                    cmd.Parameters.Add("@learner", SqlDbType.Int).Value = learnerId;
                    cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            course.Lessons.Add(new LearnerLesson {
                                Id = r.GetInt32(0), Title = r.GetString(1),
                                Content = r.GetString(2), CodeSnippet = r.IsDBNull(3) ? "" : r.GetString(3),
                                CodeLanguage = r.IsDBNull(4) ? "" : r.GetString(4),
                                VideoUrl = r.IsDBNull(5) ? "" : r.GetString(5),
                                DurationMinutes = r.GetInt32(6), IsCompleted = r.GetBoolean(7)
                            });
                }
            }
            return course;
        }

        public void Enroll(int learnerId, int courseId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.CourseEnrollments (LearnerId,CourseId)
SELECT @learner,@course WHERE EXISTS(SELECT 1 FROM dbo.Courses WHERE Id=@course AND Status=N'Published')
AND NOT EXISTS(SELECT 1 FROM dbo.CourseEnrollments WHERE LearnerId=@learner AND CourseId=@course)", conn))
            {
                cmd.Parameters.Add("@learner", SqlDbType.Int).Value = learnerId;
                cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Unenroll(int learnerId, int courseId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"
DELETE lc FROM dbo.LessonCompletions lc INNER JOIN dbo.Lessons l ON l.Id=lc.LessonId
WHERE lc.LearnerId=@learner AND l.CourseId=@course;
DELETE FROM dbo.CourseEnrollments WHERE LearnerId=@learner AND CourseId=@course;", conn, tx))
                    {
                        cmd.Parameters.Add("@learner", SqlDbType.Int).Value = learnerId;
                        cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        public void SetLessonCompleted(int learnerId, int courseId, int lessonId, bool completed)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.CourseEnrollments WHERE LearnerId=@learner AND CourseId=@course)
OR NOT EXISTS (SELECT 1 FROM dbo.Lessons l JOIN dbo.Courses c ON c.Id=l.CourseId
               WHERE l.Id=@lesson AND l.CourseId=@course AND c.Status=N'Published')
    THROW 51001, 'Course or enrolment is not valid.', 1;
IF @done=1
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.LessonCompletions WHERE LearnerId=@learner AND LessonId=@lesson)
        INSERT INTO dbo.LessonCompletions (LearnerId,LessonId) VALUES (@learner,@lesson);
END
ELSE
    DELETE FROM dbo.LessonCompletions WHERE LearnerId=@learner AND LessonId=@lesson;

DECLARE @total int = (SELECT COUNT(*) FROM dbo.Lessons WHERE CourseId=@course);
DECLARE @finished int = (SELECT COUNT(*) FROM dbo.LessonCompletions lc
    INNER JOIN dbo.Lessons l ON l.Id=lc.LessonId WHERE lc.LearnerId=@learner AND l.CourseId=@course);
DECLARE @progress decimal(5,2) = CASE WHEN @total=0 THEN 0
    ELSE CAST(100.0 * @finished / @total AS decimal(5,2)) END;
UPDATE dbo.CourseEnrollments
SET ProgressPercent=@progress, IsCompleted=CASE WHEN @total>0 AND @finished=@total THEN 1 ELSE 0 END,
    CompletedAt=CASE WHEN @total>0 AND @finished=@total THEN COALESCE(CompletedAt,SYSUTCDATETIME()) ELSE NULL END
WHERE LearnerId=@learner AND CourseId=@course;", conn, tx))
                    {
                        cmd.Parameters.Add("@learner", SqlDbType.Int).Value = learnerId;
                        cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                        cmd.Parameters.Add("@lesson", SqlDbType.Int).Value = lessonId;
                        cmd.Parameters.Add("@done", SqlDbType.Bit).Value = completed;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        private static LearnerCourse ReadCourse(SqlDataReader r)
        {
            return new LearnerCourse {
                Id=r.GetInt32(0), Title=r.GetString(1), Description=r.GetString(2),
                Category=r.GetString(3), Difficulty=r.GetString(4), Instructor=r.GetString(5),
                ThumbnailUrl=r.IsDBNull(6) ? "" : r.GetString(6), LessonCount=r.GetInt32(7),
                IsEnrolled=r.GetBoolean(8), IsCompleted=r.GetBoolean(9), ProgressPercent=r.GetDecimal(10)
            };
        }
    }
}
