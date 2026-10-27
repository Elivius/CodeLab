using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Models;

namespace CodeLab.DAL
{
    public sealed class LearnerDiscussionRepository
    {
        // Every query supplies @LearnerId.
        // The course alias must be "c".
        private const string CourseAccessCondition = @"
            c.Status = N'Published'
            AND EXISTS
            (
                SELECT 1
                FROM dbo.CourseEnrollments AS e
                WHERE e.CourseId = c.Id
                  AND e.LearnerId = @LearnerId
            )
            AND EXISTS
            (
                SELECT 1
                FROM dbo.Users AS viewer
                WHERE viewer.Id = @LearnerId
                  AND viewer.Role = N'Learner'
                  AND viewer.Status = N'Active'
            )";

        // General discussions allow active learners and tutors. Course access remains enrolment-based.
        private const string AccessCondition = @"
            ((p.CourseId IS NULL AND EXISTS
                (SELECT 1 FROM dbo.Users v WHERE v.Id=@LearnerId
                 AND v.Role IN (N'Learner',N'Tutor') AND v.Status=N'Active'))
             OR (p.CourseId IS NOT NULL AND " + CourseAccessCondition + @"))";
        private const string PostSelect = @"
            SELECT
                p.Id,
                p.CourseId,
                COALESCE(c.Title, N'General Discussion') AS CourseTitle,
                p.AuthorId,
                u.FullName AS AuthorName,
                u.Username AS AuthorUsername,
                u.Role AS AuthorRole,
                p.Title,
                p.Content,
                p.CreatedAt,
                stats.ReplyCount,
                stats.LastReplyAt,
                stats.HasTutorReply
            FROM dbo.DiscussionPosts AS p
            LEFT JOIN dbo.Courses AS c
                ON c.Id = p.CourseId
            INNER JOIN dbo.Users AS u
                ON u.Id = p.AuthorId
            OUTER APPLY
            (
                SELECT
                    COUNT(*) AS ReplyCount,
                    MAX(r.CreatedAt) AS LastReplyAt,
                    ISNULL(MAX(
                        CASE WHEN author.Role = N'Tutor'
                             THEN 1 ELSE 0 END
                    ), 0) AS HasTutorReply
                FROM dbo.DiscussionReplies AS r
                INNER JOIN dbo.Users AS author
                    ON author.Id = r.AuthorId
                WHERE r.PostId = p.Id
            ) AS stats";

        private const string ReplySelect = @"
            SELECT
                r.Id,
                r.PostId,
                p.Title AS PostTitle,
                p.CourseId,
                COALESCE(c.Title, N'General Discussion') AS CourseTitle,
                r.AuthorId,
                u.FullName AS AuthorName,
                u.Username AS AuthorUsername,
                u.Role AS AuthorRole,
                r.Content,
                r.CreatedAt
            FROM dbo.DiscussionReplies AS r
            INNER JOIN dbo.DiscussionPosts AS p
                ON p.Id = r.PostId
            LEFT JOIN dbo.Courses AS c
                ON c.Id = p.CourseId
            INNER JOIN dbo.Users AS u
                ON u.Id = r.AuthorId";

        public List<KeyValuePair<int, string>> GetEnrolledCourses(
            int learnerId)
        {
            ValidateId(learnerId, "learnerId");

            string sql = @"
                SELECT c.Id, c.Title
                FROM dbo.Courses AS c
                WHERE " + CourseAccessCondition + @"
                ORDER BY c.Title, c.Id;";

            var courses = new List<KeyValuePair<int, string>>();

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new KeyValuePair<int, string>(
                            Convert.ToInt32(reader["Id"]),
                            Convert.ToString(reader["Title"])));
                    }
                }
            }

            return courses;
        }

        public List<LearnerDiscussionPost> GetPosts(
            int learnerId,
            int? courseId = null,
            string search = null,
            bool onlyMine = false,
            int pageNumber = 1,
            int pageSize = 20, string scope = "all")
        {
            ValidateId(learnerId, "learnerId");

            if (courseId.HasValue)
            {
                ValidateId(courseId.Value, "courseId");
            }

            ValidatePaging(pageNumber, pageSize);

            string sql = PostSelect + @"
                WHERE " + AccessCondition + @"
                  AND (@CourseId IS NULL OR p.CourseId = @CourseId)
                  AND (@Scope = N'all' OR (@Scope = N'general' AND p.CourseId IS NULL) OR (@Scope = N'courses' AND p.CourseId IS NOT NULL))
                  AND (@OnlyMine = 0 OR p.AuthorId = @LearnerId)
                  AND
                  (
                      @Search = N''
                      OR p.Title LIKE @Pattern ESCAPE N'~'
                      OR p.Content LIKE @Pattern ESCAPE N'~'
                      OR u.FullName LIKE @Pattern ESCAPE N'~'
                      OR c.Title LIKE @Pattern ESCAPE N'~'
                  )
                ORDER BY p.CreatedAt DESC, p.Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;";

            var posts = new List<LearnerDiscussionPost>();

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);

                command.Parameters.Add(
                    "@CourseId", SqlDbType.Int).Value =
                    courseId.HasValue
                        ? (object)courseId.Value
                        : DBNull.Value;

                command.Parameters.Add(
                    "@OnlyMine", SqlDbType.Bit).Value = onlyMine;

                command.Parameters.Add("@Scope", SqlDbType.NVarChar, 10).Value = scope;
                AddSearch(command, search);
                AddPaging(command, pageNumber, pageSize);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        posts.Add(ReadPost(reader));
                    }
                }
            }

            return posts;
        }

        public LearnerDiscussionPost GetPost(
            int learnerId,
            int postId)
        {
            ValidateId(learnerId, "learnerId");
            ValidateId(postId, "postId");

            string sql = PostSelect + @"
                WHERE p.Id = @PostId
                  AND " + AccessCondition + ";";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                AddInt(command, "@PostId", postId);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? ReadPost(reader) : null;
                }
            }
        }

        public List<LearnerDiscussionReply> GetReplies(
            int learnerId,
            int postId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            ValidateId(learnerId, "learnerId");
            ValidateId(postId, "postId");
            ValidatePaging(pageNumber, pageSize);

            string sql = ReplySelect + @"
                WHERE p.Id = @PostId
                  AND " + AccessCondition + @"
                ORDER BY r.CreatedAt ASC, r.Id ASC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                AddInt(command, "@PostId", postId);
                AddPaging(command, pageNumber, pageSize);

                connection.Open();
                return ReadReplies(command);
            }
        }

        public List<LearnerDiscussionReply> GetMyReplies(
            int learnerId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            ValidateId(learnerId, "learnerId");
            ValidatePaging(pageNumber, pageSize);

            string sql = ReplySelect + @"
                WHERE r.AuthorId = @LearnerId
                  AND " + AccessCondition + @"
                ORDER BY r.CreatedAt DESC, r.Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                AddPaging(command, pageNumber, pageSize);

                connection.Open();
                return ReadReplies(command);
            }
        }

        // Returns the new post ID, or null if access is unavailable.
        public int? CreatePost(
            int learnerId,
            int? courseId,
            string title,
            string content)
        {
            ValidateId(learnerId, "learnerId");
            if (courseId.HasValue) ValidateId(courseId.Value, "courseId");

            title = RequiredText(title, 250, "title");
            content = RequiredText(content, 20000, "content");

            string sql = @"
                INSERT INTO dbo.DiscussionPosts
                    (CourseId, AuthorId, Title, Content)
                OUTPUT INSERTED.Id
                SELECT
                    @CourseId, @LearnerId, @Title, @Content
                WHERE (@CourseId IS NULL AND EXISTS
                    (SELECT 1 FROM dbo.Users v WHERE v.Id=@LearnerId
                     AND v.Role IN (N'Learner',N'Tutor') AND v.Status=N'Active'))
                   OR (@CourseId IS NOT NULL AND EXISTS
                    (SELECT 1 FROM dbo.Courses c WHERE c.Id=@CourseId
                     AND " + CourseAccessCondition + "));";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                command.Parameters.Add("@CourseId", SqlDbType.Int).Value = (object)courseId ?? DBNull.Value;

                command.Parameters.Add(
                    "@Title", SqlDbType.NVarChar, 250).Value = title;

                command.Parameters.Add(
                    "@Content", SqlDbType.NVarChar, -1).Value = content;

                connection.Open();
                return ReadInsertedId(command);
            }
        }

        // Returns the new reply ID, or null if access is unavailable.
        public int? CreateReply(
            int learnerId,
            int postId,
            string content)
        {
            ValidateId(learnerId, "learnerId");
            ValidateId(postId, "postId");

            content = RequiredText(content, 10000, "content");

            string sql = @"
                INSERT INTO dbo.DiscussionReplies
                    (PostId, AuthorId, Content)
                OUTPUT INSERTED.Id
                SELECT
                    p.Id, @LearnerId, @Content
                FROM dbo.DiscussionPosts AS p
                LEFT JOIN dbo.Courses AS c
                    ON c.Id = p.CourseId
                WHERE p.Id = @PostId
                  AND " + AccessCondition + ";";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                AddInt(command, "@PostId", postId);

                command.Parameters.Add(
                    "@Content", SqlDbType.NVarChar, -1).Value = content;

                connection.Open();
                return ReadInsertedId(command);
            }
        }

        public bool UpdatePost(
            int learnerId,
            int postId,
            string title,
            string content)
        {
            ValidateId(learnerId, "learnerId");
            ValidateId(postId, "postId");

            title = RequiredText(title, 250, "title");
            content = RequiredText(content, 20000, "content");

            string sql = @"
                UPDATE p
                SET p.Title = @Title,
                    p.Content = @Content
                OUTPUT INSERTED.Id
                FROM dbo.DiscussionPosts AS p
                LEFT JOIN dbo.Courses AS c
                    ON c.Id = p.CourseId
                WHERE p.Id = @PostId
                  AND p.AuthorId = @LearnerId
                  AND " + AccessCondition + ";";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                AddInt(command, "@PostId", postId);

                command.Parameters.Add(
                    "@Title", SqlDbType.NVarChar, 250).Value = title;

                command.Parameters.Add(
                    "@Content", SqlDbType.NVarChar, -1).Value = content;

                connection.Open();
                return ReadInsertedId(command).HasValue;
            }
        }

        public bool DeletePost(int learnerId, int postId)
        {
            ValidateId(learnerId, "learnerId");
            ValidateId(postId, "postId");

            // The existing foreign key cascades deletion to replies.
            string sql = @"
                DELETE p
                OUTPUT DELETED.Id
                FROM dbo.DiscussionPosts AS p
                LEFT JOIN dbo.Courses AS c
                    ON c.Id = p.CourseId
                WHERE p.Id = @PostId
                  AND p.AuthorId = @LearnerId
                  AND " + AccessCondition + ";";

            using (var connection = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddInt(command, "@LearnerId", learnerId);
                AddInt(command, "@PostId", postId);

                connection.Open();
                return ReadInsertedId(command).HasValue;
            }
        }

        private static LearnerDiscussionPost ReadPost(
            SqlDataReader reader)
        {
            return new LearnerDiscussionPost
            {
                Id = Convert.ToInt32(reader["Id"]),
                CourseId = reader["CourseId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CourseId"]),
                CourseTitle = Convert.ToString(reader["CourseTitle"]),
                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                AuthorName = Convert.ToString(reader["AuthorName"]),
                AuthorUsername = Convert.ToString(reader["AuthorUsername"]),
                AuthorRole = Convert.ToString(reader["AuthorRole"]),
                Title = Convert.ToString(reader["Title"]),
                Content = Convert.ToString(reader["Content"]),
                CreatedAt = ReadUtc(reader["CreatedAt"]),
                ReplyCount = Convert.ToInt32(reader["ReplyCount"]),
                LastReplyAt = reader["LastReplyAt"] == DBNull.Value
                    ? (DateTime?)null
                    : ReadUtc(reader["LastReplyAt"]),
                HasTutorReply = Convert.ToInt32(
                    reader["HasTutorReply"]) == 1
            };
        }

        private static List<LearnerDiscussionReply> ReadReplies(
            SqlCommand command)
        {
            var replies = new List<LearnerDiscussionReply>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    replies.Add(new LearnerDiscussionReply
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        PostId = Convert.ToInt32(reader["PostId"]),
                        PostTitle = Convert.ToString(reader["PostTitle"]),
                        CourseId = reader["CourseId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CourseId"]),
                        CourseTitle = Convert.ToString(reader["CourseTitle"]),
                        AuthorId = Convert.ToInt32(reader["AuthorId"]),
                        AuthorName = Convert.ToString(reader["AuthorName"]),
                        AuthorUsername = Convert.ToString(
                            reader["AuthorUsername"]),
                        AuthorRole = Convert.ToString(reader["AuthorRole"]),
                        Content = Convert.ToString(reader["Content"]),
                        CreatedAt = ReadUtc(reader["CreatedAt"])
                    });
                }
            }

            return replies;
        }

        private static int? ReadInsertedId(SqlCommand command)
        {
            object value = command.ExecuteScalar();

            return value == null || value == DBNull.Value
                ? (int?)null
                : Convert.ToInt32(value);
        }

        private static DateTime ReadUtc(object value)
        {
            return DateTime.SpecifyKind(
                Convert.ToDateTime(value),
                DateTimeKind.Utc);
        }

        private static void AddInt(
            SqlCommand command,
            string name,
            int value)
        {
            command.Parameters.Add(name, SqlDbType.Int).Value = value;
        }

        private static void AddPaging(
            SqlCommand command,
            int pageNumber,
            int pageSize)
        {
            command.Parameters.Add(
                "@Offset", SqlDbType.BigInt).Value =
                ((long)pageNumber - 1) * pageSize;

            AddInt(command, "@PageSize", pageSize);
        }

        private static void AddSearch(
            SqlCommand command,
            string search)
        {
            string text = (search ?? string.Empty).Trim();

            if (text.Length > 200)
            {
                throw new ArgumentException(
                    "Search cannot exceed 200 characters.",
                    "search");
            }

            // Treat SQL LIKE wildcard characters as literal text.
            string escaped = text
                .Replace("~", "~~")
                .Replace("%", "~%")
                .Replace("_", "~_")
                .Replace("[", "~[");

            command.Parameters.Add(
                "@Search", SqlDbType.NVarChar, 200).Value = text;

            command.Parameters.Add(
                "@Pattern", SqlDbType.NVarChar, 402).Value =
                "%" + escaped + "%";
        }

        private static string RequiredText(
            string value,
            int maximumLength,
            string parameterName)
        {
            string text = (value ?? string.Empty).Trim();

            if (text.Length == 0)
            {
                throw new ArgumentException(
                    "This field is required.",
                    parameterName);
            }

            if (text.Length > maximumLength)
            {
                throw new ArgumentException(
                    "This field exceeds the allowed length.",
                    parameterName);
            }

            return text;
        }

        private static void ValidateId(int value, string name)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private static void ValidatePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException("pageNumber");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new ArgumentOutOfRangeException("pageSize");
            }
        }
    }
}