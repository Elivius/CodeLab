using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.DAL
{
    /// <summary>
    /// Discussion posts and replies. Only courses with Status = Published have a visible board.
    /// Changes to a post or reply only apply when it belongs to the given author.
    /// </summary>
    public class DiscussionRepository
    {
        private readonly int viewerId;
        public DiscussionRepository(int viewerId) { this.viewerId = viewerId; }

        // Read access is open to active forum users across all published courses.
        private const string BrowseAccessSql = @"EXISTS (SELECT 1 FROM Users viewer
            WHERE viewer.Id=@ViewerId AND viewer.Status=N'Active'
              AND viewer.Role IN (N'Learner',N'Tutor',N'Admin')
              AND (p.CourseId IS NULL OR c.Status=@Published))";
        // General is shared. Learners retain enrolment-based course access; tutors use published boards.
        private const string AccessSql = @"EXISTS (SELECT 1 FROM Users viewer
            WHERE viewer.Id=@ViewerId AND viewer.Status=N'Active'
              AND viewer.Role IN (N'Learner',N'Tutor',N'Admin')
              AND (p.CourseId IS NULL OR (c.Status=@Published AND
                   (viewer.Role IN (N'Tutor',N'Admin') OR EXISTS
                    (SELECT 1 FROM CourseEnrollments e WHERE e.CourseId=c.Id AND e.LearnerId=@ViewerId)))))";
        /// <summary>
        /// Posts in published courses, newest first, each with its replies (oldest first).
        /// courseId 0 means every published course. search (optional) matches the title, the text or the
        /// author's name.
        /// </summary>
        public List<DiscussionPost> GetPosts(int courseId, string search)
        {
            string query = @"
SET NOCOUNT ON;
DECLARE @Posts TABLE (Id INT PRIMARY KEY);

INSERT INTO @Posts (Id)
SELECT p.Id
  FROM DiscussionPosts p
  LEFT JOIN Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = p.AuthorId
 WHERE " + BrowseAccessSql + @"
   AND (@CourseId = 0 OR (@CourseId = -1 AND p.CourseId IS NULL) OR (@CourseId = -2 AND p.CourseId IS NOT NULL) OR p.CourseId = @CourseId)
   AND (@Search IS NULL OR p.Title LIKE @Search OR p.Content LIKE @Search OR u.FullName LIKE @Search);

SELECT p.Id, p.CourseId, p.AuthorId, p.Title, p.Content, p.CreatedAt,
       COALESCE(c.Title, N'General Discussion') AS CourseTitle, COALESCE(c.InstructorId, 0) AS CourseInstructorId,
       (SELECT b.Name FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeName, (SELECT CASE WHEN b.Metric='points' THEN CASE WHEN b.Target>=10000 THEN 'legend' WHEN b.Target>=5000 THEN 'expert' WHEN b.Target>=3000 THEN 'skilled' ELSE 'starter' END ELSE CASE WHEN b.Target>=50 THEN 'legend' WHEN b.Target>=25 THEN 'expert' WHEN b.Target>=10 THEN 'skilled' ELSE 'starter' END END FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeTier, u.FullName AS AuthorName, u.Username AS AuthorUsername, u.Role AS AuthorRole, u.AvatarUrl AS AuthorAvatarUrl
  FROM DiscussionPosts p
  JOIN @Posts f ON f.Id = p.Id
  LEFT JOIN Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = p.AuthorId
 ORDER BY p.CreatedAt DESC, p.Id DESC;

SELECT r.Id, r.PostId, r.AuthorId, r.Content, r.CreatedAt,
       (SELECT b.Name FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeName, (SELECT CASE WHEN b.Metric='points' THEN CASE WHEN b.Target>=10000 THEN 'legend' WHEN b.Target>=5000 THEN 'expert' WHEN b.Target>=3000 THEN 'skilled' ELSE 'starter' END ELSE CASE WHEN b.Target>=50 THEN 'legend' WHEN b.Target>=25 THEN 'expert' WHEN b.Target>=10 THEN 'skilled' ELSE 'starter' END END FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeTier, u.FullName AS AuthorName, u.Username AS AuthorUsername, u.Role AS AuthorRole, u.AvatarUrl AS AuthorAvatarUrl
  FROM DiscussionReplies r
  JOIN @Posts f ON f.Id = r.PostId
  JOIN Users u ON u.Id = r.AuthorId
 ORDER BY r.CreatedAt, r.Id;";

            List<DiscussionPost> posts = new List<DiscussionPost>();
            Dictionary<int, DiscussionPost> byId = new Dictionary<int, DiscussionPost>();

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ViewerId", SqlDbType.Int).Value = viewerId;
                    command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                    command.Parameters.Add("@Search", SqlDbType.NVarChar, 210).Value =
                        string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + EscapeLike(search.Trim()) + "%";

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DiscussionPost post = ReadPost(reader);
                            posts.Add(post);
                            byId[post.Id] = post;
                        }

                        reader.NextResult();
                        while (reader.Read())
                        {
                            DiscussionPost post;
                            if (byId.TryGetValue(Convert.ToInt32(reader["PostId"]), out post))
                            {
                                post.Replies.Add(ReadReply(reader));
                            }
                        }
                    }
                }
            }

            // Fill the reply summary from the loaded replies
            foreach (DiscussionPost post in posts)
            {
                post.ReplyCount = post.Replies.Count;
                post.LastReplyAt = post.Replies.Count == 0 ? (DateTime?)null : post.Replies[post.Replies.Count - 1].CreatedAt;
                post.HasTutorReply = post.Replies.Exists(r => r.AuthorRole == UserRoles.Tutor);
            }

            return posts;
        }

        // Post columns plus the reply summary (count, latest reply, whether a tutor replied).
        // Used with "FROM DiscussionPosts p JOIN Courses c ... JOIN Users u ..."
        private const string PostSummaryColumns = @"
       p.Id, p.CourseId, p.AuthorId, p.Title, p.Content, p.CreatedAt,
       COALESCE(c.Title, N'General Discussion') AS CourseTitle, COALESCE(c.InstructorId, 0) AS CourseInstructorId,
       (SELECT b.Name FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeName, (SELECT CASE WHEN b.Metric='points' THEN CASE WHEN b.Target>=10000 THEN 'legend' WHEN b.Target>=5000 THEN 'expert' WHEN b.Target>=3000 THEN 'skilled' ELSE 'starter' END ELSE CASE WHEN b.Target>=50 THEN 'legend' WHEN b.Target>=25 THEN 'expert' WHEN b.Target>=10 THEN 'skilled' ELSE 'starter' END END FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeTier, u.FullName AS AuthorName, u.Username AS AuthorUsername, u.Role AS AuthorRole, u.AvatarUrl AS AuthorAvatarUrl,
       (SELECT COUNT(*) FROM DiscussionReplies r WHERE r.PostId = p.Id) AS ReplyCount,
       (SELECT MAX(r.CreatedAt) FROM DiscussionReplies r WHERE r.PostId = p.Id) AS LastReplyAt,
       CASE WHEN EXISTS (SELECT 1 FROM DiscussionReplies r JOIN Users ru ON ru.Id = r.AuthorId
                          WHERE r.PostId = p.Id AND ru.Role = @TutorRole) THEN 1 ELSE 0 END AS HasTutorReply";

        /// <summary>
        /// One page of posts in published courses, newest first, with the reply summary but without the
        /// replies themselves. courseId 0 means every published course; authorId 0 means every author.
        /// </summary>
        public List<DiscussionPost> GetPostPage(int courseId, string search, int authorId, int pageNumber, int pageSize)
        {
            string query = "SELECT " + PostSummaryColumns + @"
  FROM DiscussionPosts p
  LEFT JOIN Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = p.AuthorId
 WHERE " + BrowseAccessSql + @"
   AND (@CourseId = 0 OR (@CourseId = -1 AND p.CourseId IS NULL) OR (@CourseId = -2 AND p.CourseId IS NOT NULL) OR p.CourseId = @CourseId)
   AND (@AuthorId = 0 OR p.AuthorId = @AuthorId)
   AND (@Search IS NULL OR p.Title LIKE @Search OR p.Content LIKE @Search OR u.FullName LIKE @Search)
 ORDER BY p.CreatedAt DESC, p.Id DESC
 OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            List<DiscussionPost> posts = new List<DiscussionPost>();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ViewerId", SqlDbType.Int).Value = viewerId;
                    AddReadParameters(command);
                    command.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                    command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = authorId;
                    command.Parameters.Add("@Search", SqlDbType.NVarChar, 210).Value =
                        string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + EscapeLike(search.Trim()) + "%";
                    AddPaging(command, pageNumber, pageSize);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            posts.Add(ReadPost(reader));
                        }
                    }
                }
            }

            return posts;
        }

        /// <summary>
        /// One post with its reply summary, or null when it does not exist or its course is not Published.
        /// </summary>
        public DiscussionPost GetPost(int postId)
        {
            string query = "SELECT " + PostSummaryColumns + @"
  FROM DiscussionPosts p
  LEFT JOIN Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = p.AuthorId
 WHERE p.Id = @PostId AND " + BrowseAccessSql + @";";

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ViewerId", SqlDbType.Int).Value = viewerId;
                    AddReadParameters(command);
                    command.Parameters.Add("@PostId", SqlDbType.Int).Value = postId;

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read() ? ReadPost(reader) : null;
                    }
                }
            }
        }

        /// <summary>
        /// One page of a post's replies, oldest first. Empty when the post's course is not Published.
        /// </summary>
        public List<DiscussionReply> GetReplyPage(int postId, int pageNumber, int pageSize)
        {
            string query = @"
SELECT r.Id, r.PostId, r.AuthorId, r.Content, r.CreatedAt,
       (SELECT b.Name FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeName, (SELECT CASE WHEN b.Metric='points' THEN CASE WHEN b.Target>=10000 THEN 'legend' WHEN b.Target>=5000 THEN 'expert' WHEN b.Target>=3000 THEN 'skilled' ELSE 'starter' END ELSE CASE WHEN b.Target>=50 THEN 'legend' WHEN b.Target>=25 THEN 'expert' WHEN b.Target>=10 THEN 'skilled' ELSE 'starter' END END FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeTier, u.FullName AS AuthorName, u.Username AS AuthorUsername, u.Role AS AuthorRole, u.AvatarUrl AS AuthorAvatarUrl,
       p.Title AS PostTitle, c.Id AS CourseId, COALESCE(c.Title, N'General Discussion') AS CourseTitle
  FROM DiscussionReplies r
  JOIN DiscussionPosts p ON p.Id = r.PostId
  LEFT JOIN Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = r.AuthorId
 WHERE r.PostId = @PostId AND " + BrowseAccessSql + @"
 ORDER BY r.CreatedAt, r.Id
 OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            return ReadReplies(query, command =>
            {
                command.Parameters.Add("@PostId", SqlDbType.Int).Value = postId;
                AddPaging(command, pageNumber, pageSize);
            });
        }

        /// <summary>
        /// One page of a user's own replies in published courses, newest first, with the post they answer.
        /// </summary>
        public List<DiscussionReply> GetRepliesByAuthor(int authorId, int pageNumber, int pageSize)
        {
            string query = @"
SELECT r.Id, r.PostId, r.AuthorId, r.Content, r.CreatedAt,
       (SELECT b.Name FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeName, (SELECT CASE WHEN b.Metric='points' THEN CASE WHEN b.Target>=10000 THEN 'legend' WHEN b.Target>=5000 THEN 'expert' WHEN b.Target>=3000 THEN 'skilled' ELSE 'starter' END ELSE CASE WHEN b.Target>=50 THEN 'legend' WHEN b.Target>=25 THEN 'expert' WHEN b.Target>=10 THEN 'skilled' ELSE 'starter' END END FROM dbo.LearnerBadges lb JOIN dbo.Badges b ON b.Id=lb.BadgeId WHERE lb.LearnerId=u.Id AND lb.IsFeatured=1) AS AuthorBadgeTier, u.FullName AS AuthorName, u.Username AS AuthorUsername, u.Role AS AuthorRole, u.AvatarUrl AS AuthorAvatarUrl,
       p.Title AS PostTitle, c.Id AS CourseId, COALESCE(c.Title, N'General Discussion') AS CourseTitle
  FROM DiscussionReplies r
  JOIN DiscussionPosts p ON p.Id = r.PostId
  LEFT JOIN Courses c ON c.Id = p.CourseId
  JOIN Users u ON u.Id = r.AuthorId
 WHERE r.AuthorId = @AuthorId AND " + BrowseAccessSql + @"
 ORDER BY r.CreatedAt DESC, r.Id DESC
 OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            return ReadReplies(query, command =>
            {
                command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = authorId;
                AddPaging(command, pageNumber, pageSize);
            });
        }

        private List<DiscussionReply> ReadReplies(string query, Action<SqlCommand> addParameters)
        {
            List<DiscussionReply> replies = new List<DiscussionReply>();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ViewerId", SqlDbType.Int).Value = viewerId;
                    command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                    addParameters(command);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DiscussionReply reply = ReadReply(reader);
                            reply.PostTitle = reader["PostTitle"].ToString();
                            reply.CourseId = reader["CourseId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CourseId"]);
                            reply.CourseTitle = reader["CourseTitle"].ToString();
                            replies.Add(reply);
                        }
                    }
                }
            }

            return replies;
        }

        private static void AddReadParameters(SqlCommand command)
        {
            command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
            command.Parameters.Add("@TutorRole", SqlDbType.NVarChar, 20).Value = UserRoles.Tutor;
        }

        // pageNumber starts at 1; the manager keeps both values in range
        private static void AddPaging(SqlCommand command, int pageNumber, int pageSize)
        {
            command.Parameters.Add("@Offset", SqlDbType.Int).Value = (pageNumber - 1) * pageSize;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
        }

        // Reads the post columns; the reply summary columns are read when the query has them
        private static DiscussionPost ReadPost(SqlDataReader reader)
        {
            DiscussionPost post = new DiscussionPost
            {
                Id = Convert.ToInt32(reader["Id"]),
                CourseId = reader["CourseId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CourseId"]),
                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                Title = reader["Title"].ToString(),
                Content = reader["Content"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                CourseTitle = reader["CourseTitle"].ToString(),
                CourseInstructorId = Convert.ToInt32(reader["CourseInstructorId"]),
                AuthorBadgeName = reader["AuthorBadgeName"] as string,
                AuthorBadgeTier = reader["AuthorBadgeTier"] as string,
                AuthorName = reader["AuthorName"].ToString(),
                AuthorUsername = reader["AuthorUsername"].ToString(),
                AuthorRole = reader["AuthorRole"].ToString(),
                AuthorAvatarUrl = reader["AuthorAvatarUrl"] as string
            };

            if (HasColumn(reader, "ReplyCount"))
            {
                post.ReplyCount = Convert.ToInt32(reader["ReplyCount"]);
                post.LastReplyAt = reader["LastReplyAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastReplyAt"]);
                post.HasTutorReply = Convert.ToInt32(reader["HasTutorReply"]) == 1;
            }

            return post;
        }

        private static bool HasColumn(SqlDataReader reader, string name)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a post and returns its id, or 0 if it could not be saved (including when the course is
        /// not Published).
        /// </summary>
        public int InsertPost(DiscussionPost post)
        {
            string query = @"INSERT INTO DiscussionPosts (CourseId, AuthorId, Title, Content)
                             OUTPUT INSERTED.Id
                             SELECT @CourseId, @AuthorId, @Title, @Content
                              FROM (SELECT @CourseId AS CourseId) p LEFT JOIN Courses c ON c.Id=p.CourseId
                              WHERE @AuthorId=@ViewerId AND " + AccessSql + @"";

            return InsertAndReturnId(query, "DiscussionRepository.InsertPost failure", command =>
            {
                command.Parameters.Add("@CourseId", SqlDbType.Int).Value = (object)post.CourseId ?? DBNull.Value;
                command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = post.AuthorId;
                command.Parameters.Add("@Title", SqlDbType.NVarChar, 250).Value = post.Title;
                command.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = post.Content;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
            });
        }

        /// <summary>
        /// Saves a post's new title and text. Returns false unless the post exists and belongs to the author.
        /// </summary>
        public bool UpdatePost(DiscussionPost post)
        {
            return Execute("UPDATE DiscussionPosts SET Title = @Title, Content = @Content WHERE Id = @Id AND AuthorId = @AuthorId AND @AuthorId=@ViewerId AND EXISTS (SELECT 1 FROM DiscussionPosts p LEFT JOIN Courses c ON c.Id=p.CourseId WHERE p.Id=DiscussionPosts.Id AND " + AccessSql + ")",
                "DiscussionRepository.UpdatePost failure", command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = post.Id;
                    command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = post.AuthorId;
                    command.Parameters.Add("@Title", SqlDbType.NVarChar, 250).Value = post.Title;
                    command.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = post.Content;
                });
        }

        /// <summary>
        /// Deletes a post with all of its replies (ON DELETE CASCADE). Returns false unless it belongs to the author.
        /// </summary>
        public bool DeletePost(int postId, int authorId)
        {
            return Execute("DELETE FROM DiscussionPosts WHERE Id = @Id AND AuthorId = @AuthorId AND @AuthorId=@ViewerId AND EXISTS (SELECT 1 FROM DiscussionPosts p LEFT JOIN Courses c ON c.Id=p.CourseId WHERE p.Id=DiscussionPosts.Id AND " + AccessSql + ")",
                "DiscussionRepository.DeletePost failure", command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = postId;
                    command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = authorId;
                });
        }

        /// <summary>
        /// Adds a reply and returns its id, or 0 if it could not be saved (including when the post does not
        /// exist or its course is not Published).
        /// </summary>
        public int InsertReply(DiscussionReply reply)
        {
            string query = @"INSERT INTO DiscussionReplies (PostId, AuthorId, Content)
                             OUTPUT INSERTED.Id
                             SELECT p.Id, @AuthorId, @Content
                               FROM DiscussionPosts p
                               LEFT JOIN Courses c ON c.Id = p.CourseId
                              WHERE p.Id = @PostId AND @AuthorId=@ViewerId AND " + AccessSql + @"";

            return InsertAndReturnId(query, "DiscussionRepository.InsertReply failure", command =>
            {
                command.Parameters.Add("@PostId", SqlDbType.Int).Value = reply.PostId;
                command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = reply.AuthorId;
                command.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = reply.Content;
                command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
            });
        }

        /// <summary>
        /// Saves a reply's new text. Returns false unless the reply exists and belongs to the author.
        /// </summary>
        public bool UpdateReply(DiscussionReply reply)
        {
            return Execute("UPDATE DiscussionReplies SET Content = @Content WHERE Id = @Id AND AuthorId = @AuthorId AND @AuthorId=@ViewerId AND EXISTS (SELECT 1 FROM DiscussionPosts p LEFT JOIN Courses c ON c.Id=p.CourseId WHERE p.Id=DiscussionReplies.PostId AND " + AccessSql + ")",
                "DiscussionRepository.UpdateReply failure", command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = reply.Id;
                    command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = reply.AuthorId;
                    command.Parameters.Add("@Content", SqlDbType.NVarChar, -1).Value = reply.Content;
                });
        }

        /// <summary>
        /// Deletes a reply. Returns false unless it belongs to the author.
        /// </summary>
        public bool DeleteReply(int replyId, int authorId)
        {
            return Execute("DELETE FROM DiscussionReplies WHERE Id = @Id AND AuthorId = @AuthorId AND @AuthorId=@ViewerId AND EXISTS (SELECT 1 FROM DiscussionPosts p LEFT JOIN Courses c ON c.Id=p.CourseId WHERE p.Id=DiscussionReplies.PostId AND " + AccessSql + ")",
                "DiscussionRepository.DeleteReply failure", command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = replyId;
                    command.Parameters.Add("@AuthorId", SqlDbType.Int).Value = authorId;
                });
        }

        // Runs an INSERT ... OUTPUT INSERTED.Id; 0 when no row was inserted or the database failed
        private int InsertAndReturnId(string query, string errorContext, Action<SqlCommand> addParameters)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                    command.Parameters.Add("@ViewerId", SqlDbType.Int).Value = viewerId;
                        addParameters(command);
                        connection.Open();
                        object id = command.ExecuteScalar();
                        return id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError(errorContext, sqlException);
                return 0;
            }
        }

        // Runs an UPDATE or DELETE; true when at least one row changed
        private bool Execute(string query, string errorContext, Action<SqlCommand> addParameters)
        {
            try
            {
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                    command.Parameters.Add("@ViewerId", SqlDbType.Int).Value = viewerId;
                        addParameters(command);
                        command.Parameters.Add("@Published", SqlDbType.NVarChar, 20).Value = CourseStatuses.Published;
                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                LogHelper.LogError(errorContext, sqlException);
                return false;
            }
        }

        // Makes %, _ and [ in the search text match themselves in LIKE
        private static string EscapeLike(string text)
        {
            return text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        private static DiscussionReply ReadReply(SqlDataReader reader)
        {
            return new DiscussionReply
            {
                Id = Convert.ToInt32(reader["Id"]),
                PostId = Convert.ToInt32(reader["PostId"]),
                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                Content = reader["Content"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                AuthorBadgeName = reader["AuthorBadgeName"] as string,
                AuthorBadgeTier = reader["AuthorBadgeTier"] as string,
                AuthorName = reader["AuthorName"].ToString(),
                AuthorUsername = reader["AuthorUsername"].ToString(),
                AuthorRole = reader["AuthorRole"].ToString(),
                AuthorAvatarUrl = reader["AuthorAvatarUrl"] as string
            };
        }
    }
}
