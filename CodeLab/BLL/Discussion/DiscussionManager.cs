using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// The course discussion forum, shared by the Tutor and Learner pages. Rules (follow the prototype):
    /// - General discussions plus published course boards; learners require enrolment for course boards.
    /// - Learners and tutors can participate in boards they can access.
    /// - Replies are flat: a post has many replies, a reply has no replies of its own.
    /// - Only the author can edit or delete their own post or reply. Deleting a post removes its replies.
    /// </summary>
    public class DiscussionManager
    {
        public const int MaxTitleLength = 250;
        public const int MaxPostLength = 5000;
        public const int MaxReplyLength = 3000;
        private const int MaxSearchLength = 100;

        /// <summary>Largest page the paged methods return.</summary>
        public const int MaxPageSize = 100;

        // Pages construct managers before ASP.NET attaches session state.
        // Resolve the viewer at operation time, after authentication checks.
        private DiscussionRepository _discussionRepository
        {
            get { return new DiscussionRepository(SessionHelper.GetCurrentUserID()); }
        }

        /// <summary>
        /// Posts with their replies, newest post first. courseId 0 shows every published course.
        /// </summary>
        public OperationResult<List<DiscussionPost>> GetPosts(int courseId, string search)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return OperationResult<List<DiscussionPost>>.Fail("Please log in to view the discussion forum.");
            }

            string cleanSearch = Clean(search);
            if (cleanSearch != null && cleanSearch.Length > MaxSearchLength)
            {
                cleanSearch = cleanSearch.Substring(0, MaxSearchLength);
            }

            try
            {
                return OperationResult<List<DiscussionPost>>.Ok(_discussionRepository.GetPosts(Math.Max(-2, courseId), cleanSearch));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetPosts failure", exception);
                return OperationResult<List<DiscussionPost>>.Fail("The discussions could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// One page of posts, newest first, with reply counts but without the replies (the Learner forum).
        /// courseId 0 shows every published course; onlyMine shows just the signed-in user's posts.
        /// </summary>
        public OperationResult<List<DiscussionPost>> GetPostPage(int courseId, string search, bool onlyMine, int pageNumber, int pageSize)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return OperationResult<List<DiscussionPost>>.Fail("Please log in to view the discussion forum.");
            }

            if (!IsPageInRange(ref pageNumber, ref pageSize))
            {
                return OperationResult<List<DiscussionPost>>.Ok(new List<DiscussionPost>());
            }

            try
            {
                int authorId = onlyMine ? SessionHelper.GetCurrentUserID() : 0;
                return OperationResult<List<DiscussionPost>>.Ok(
                    _discussionRepository.GetPostPage(Math.Max(-2, courseId), CleanSearch(search), authorId, pageNumber, pageSize));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetPostPage failure", exception);
                return OperationResult<List<DiscussionPost>>.Fail("The discussions could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// One post with its reply count. Fails when it does not exist or its course is not Published.
        /// </summary>
        public OperationResult<DiscussionPost> GetPost(int postId)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return OperationResult<DiscussionPost>.Fail("Please log in to view the discussion forum.");
            }

            if (postId <= 0)
            {
                return OperationResult<DiscussionPost>.Fail("The discussion link is invalid.");
            }

            try
            {
                DiscussionPost post = _discussionRepository.GetPost(postId);
                return post == null
                    ? OperationResult<DiscussionPost>.Fail("This discussion is unavailable. It may have been deleted, or its course is no longer published.")
                    : OperationResult<DiscussionPost>.Ok(post);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetPost failure", exception);
                return OperationResult<DiscussionPost>.Fail("This discussion could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// One page of a post's replies, oldest first.
        /// </summary>
        public OperationResult<List<DiscussionReply>> GetReplyPage(int postId, int pageNumber, int pageSize)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return OperationResult<List<DiscussionReply>>.Fail("Please log in to view the discussion forum.");
            }

            if (postId <= 0 || !IsPageInRange(ref pageNumber, ref pageSize))
            {
                return OperationResult<List<DiscussionReply>>.Ok(new List<DiscussionReply>());
            }

            try
            {
                return OperationResult<List<DiscussionReply>>.Ok(_discussionRepository.GetReplyPage(postId, pageNumber, pageSize));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetReplyPage failure", exception);
                return OperationResult<List<DiscussionReply>>.Fail("Replies could not be loaded. Please reload the page.");
            }
        }

        /// <summary>
        /// One page of the signed-in user's own replies, newest first, with the post each one answers.
        /// </summary>
        public OperationResult<List<DiscussionReply>> GetMyReplies(int pageNumber, int pageSize)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return OperationResult<List<DiscussionReply>>.Fail("Please log in to view the discussion forum.");
            }

            if (!IsPageInRange(ref pageNumber, ref pageSize))
            {
                return OperationResult<List<DiscussionReply>>.Ok(new List<DiscussionReply>());
            }

            try
            {
                return OperationResult<List<DiscussionReply>>.Ok(
                    _discussionRepository.GetRepliesByAuthor(SessionHelper.GetCurrentUserID(), pageNumber, pageSize));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetMyReplies failure", exception);
                return OperationResult<List<DiscussionReply>>.Fail("Your replies could not be loaded. Please try again.");
            }
        }

        // Keeps paging values sensible: page from 1, size 1 to MaxPageSize. False when the page is so far
        // out that it cannot exist (the row offset would overflow).
        private static bool IsPageInRange(ref int pageNumber, ref int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(MaxPageSize, pageSize));
            return pageNumber - 1 <= (int.MaxValue - pageSize) / pageSize;
        }

        private static string CleanSearch(string search)
        {
            string clean = Clean(search);
            return clean != null && clean.Length > MaxSearchLength ? clean.Substring(0, MaxSearchLength) : clean;
        }

        /// <summary>
        /// The courses a post can be filed under: every Published course.
        /// </summary>
        // Browsing includes every published course, independently of enrolment.
        public OperationResult<List<Course>> GetBrowseCourses()
        {
            try
            {
                return OperationResult<List<Course>>.Ok(new CourseRepository().GetPublished());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetBrowseCourses failure", exception);
                return OperationResult<List<Course>>.Fail("The courses could not be loaded. Please try again.");
            }
        }

        public OperationResult<List<Course>> GetCourses()
        {
            try
            {
                var courses = new CourseRepository().GetPublished();
                if (SessionHelper.IsUserInRole(UserRoles.Learner))
                {
                    var enrolled = new LearnerCourseRepository().GetCourses(SessionHelper.GetCurrentUserID(), true, "", "");
                    var ids = new HashSet<int>();
                    foreach (var course in enrolled) ids.Add(course.Id);
                    courses.RemoveAll(course => !ids.Contains(course.Id));
                }
                return OperationResult<List<Course>>.Ok(courses);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("DiscussionManager.GetCourses failure", exception);
                return OperationResult<List<Course>>.Fail("The courses could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Starts a new discussion. Data holds the new post id.
        /// </summary>
        public OperationResult<int> CreatePost(int courseId, string title, string content)
        {
            OperationResult permission = CheckCanParticipate();
            if (!permission.Success)
            {
                return OperationResult<int>.Fail(permission.Message);
            }

            DiscussionPost post = new DiscussionPost
            {
                CourseId = courseId == -1 ? (int?)null : courseId,
                AuthorId = SessionHelper.GetCurrentUserID(),
                Title = Clean(title),
                Content = CleanText(content)
            };

            string error = ValidatePost(post);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            if (courseId <= 0 && courseId != -1)
            {
                return OperationResult<int>.Fail("Choose the course this discussion is about.");
            }

            int postId = _discussionRepository.InsertPost(post);
            if (postId == 0)
            {
                return OperationResult<int>.Fail(courseId == -1
                    ? "Your general discussion could not be saved. Please reload the page and try again."
                    : "That course is not open for discussion. Choose an accessible published course and try again.");
            }

            new AuditManager().RecordActivity(post.AuthorId, "CREATE_DISCUSSION_POST", "Started a discussion: '" + post.Title + "'.");
            return OperationResult<int>.Ok(postId, "Your discussion has been posted.");
        }

        /// <summary>
        /// Changes the title and text of the signed-in user's own post.
        /// </summary>
        public OperationResult UpdatePost(int postId, string title, string content)
        {
            OperationResult permission = CheckCanParticipate();
            if (!permission.Success)
            {
                return permission;
            }

            DiscussionPost post = new DiscussionPost
            {
                Id = postId,
                AuthorId = SessionHelper.GetCurrentUserID(),
                Title = Clean(title),
                Content = CleanText(content)
            };

            string error = ValidatePost(post);
            if (error != null)
            {
                return OperationResult.Fail(error);
            }

            if (!_discussionRepository.UpdatePost(post))
            {
                return OperationResult.Fail("You can only edit your own posts.");
            }

            new AuditManager().RecordActivity(post.AuthorId, "UPDATE_DISCUSSION_POST", "Edited the discussion '" + post.Title + "'.");
            return OperationResult.Ok("Your post has been updated.");
        }

        /// <summary>
        /// Deletes the signed-in user's own post, together with all of its replies.
        /// </summary>
        public OperationResult DeletePost(int postId)
        {
            OperationResult permission = CheckCanParticipate();
            if (!permission.Success)
            {
                return permission;
            }

            int userId = SessionHelper.GetCurrentUserID();
            if (!_discussionRepository.DeletePost(postId, userId))
            {
                return OperationResult.Fail("You can only delete your own posts.");
            }

            new AuditManager().RecordActivity(userId, "DELETE_DISCUSSION_POST", "Deleted discussion post #" + postId + " and its replies.");
            return OperationResult.Ok("Your post has been deleted.");
        }

        /// <summary>
        /// Adds a reply to a post. Data holds the new reply id.
        /// </summary>
        public OperationResult<int> CreateReply(int postId, string content)
        {
            OperationResult permission = CheckCanParticipate();
            if (!permission.Success)
            {
                return OperationResult<int>.Fail(permission.Message);
            }

            DiscussionReply reply = new DiscussionReply
            {
                PostId = postId,
                AuthorId = SessionHelper.GetCurrentUserID(),
                Content = CleanText(content)
            };

            string error = ValidateReply(reply);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            int replyId = _discussionRepository.InsertReply(reply);
            if (replyId == 0)
            {
                return OperationResult<int>.Fail("That discussion is no longer available.");
            }

            new AuditManager().RecordActivity(reply.AuthorId, "CREATE_DISCUSSION_REPLY", "Replied to discussion post #" + postId + ".");
            return OperationResult<int>.Ok(replyId, "Your reply has been posted.");
        }

        /// <summary>
        /// Changes the text of the signed-in user's own reply.
        /// </summary>
        public OperationResult UpdateReply(int replyId, string content)
        {
            OperationResult permission = CheckCanParticipate();
            if (!permission.Success)
            {
                return permission;
            }

            DiscussionReply reply = new DiscussionReply
            {
                Id = replyId,
                AuthorId = SessionHelper.GetCurrentUserID(),
                Content = CleanText(content)
            };

            string error = ValidateReply(reply);
            if (error != null)
            {
                return OperationResult.Fail(error);
            }

            if (!_discussionRepository.UpdateReply(reply))
            {
                return OperationResult.Fail("You can only edit your own replies.");
            }

            new AuditManager().RecordActivity(reply.AuthorId, "UPDATE_DISCUSSION_REPLY", "Edited discussion reply #" + replyId + ".");
            return OperationResult.Ok("Your reply has been updated.");
        }

        /// <summary>
        /// Deletes the signed-in user's own reply.
        /// </summary>
        public OperationResult DeleteReply(int replyId)
        {
            OperationResult permission = CheckCanParticipate();
            if (!permission.Success)
            {
                return permission;
            }

            int userId = SessionHelper.GetCurrentUserID();
            if (!_discussionRepository.DeleteReply(replyId, userId))
            {
                return OperationResult.Fail("You can only delete your own replies.");
            }

            new AuditManager().RecordActivity(userId, "DELETE_DISCUSSION_REPLY", "Deleted discussion reply #" + replyId + ".");
            return OperationResult.Ok("Your reply has been deleted.");
        }

        /// <summary>
        /// True when the signed-in user may post and reply: Learners and Tutors.
        /// </summary>
        public static bool CanParticipate()
        {
            return SessionHelper.IsLoggedIn()
                && (SessionHelper.IsUserInRole(UserRoles.Learner) || SessionHelper.IsUserInRole(UserRoles.Tutor));
        }

        private static OperationResult CheckCanParticipate()
        {
            return CanParticipate()
                ? OperationResult.Ok()
                : OperationResult.Fail("Only learners and tutors can post in the discussion forum.");
        }

        private static string ValidatePost(DiscussionPost post)
        {
            if (post.Title == null || post.Content == null)
            {
                return "A discussion needs a title and a message.";
            }

            if (post.Title.Length > MaxTitleLength)
            {
                return "The title must be " + MaxTitleLength + " characters or fewer.";
            }

            if (post.Content.Length > MaxPostLength)
            {
                return "The message must be " + MaxPostLength + " characters or fewer.";
            }

            return null;
        }

        private static string ValidateReply(DiscussionReply reply)
        {
            if (reply.Content == null)
            {
                return "Write a reply first.";
            }

            if (reply.Content.Length > MaxReplyLength)
            {
                return "A reply must be " + MaxReplyLength + " characters or fewer.";
            }

            return null;
        }

        // Trims a single-line value; blank becomes null
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        // Trims a message but keeps its line breaks; blank becomes null
        private static string CleanText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim().Replace("\r\n", "\n");
        }
    }
}
