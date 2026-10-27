using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public sealed class LearnerDiscussionManager
    {
        public const int MaximumTitleLength = 250;
        public const int MaximumPostLength = 20000;
        public const int MaximumReplyLength = 10000;
        public const int MaximumSearchLength = 200;

        private readonly LearnerDiscussionRepository repository =
            new LearnerDiscussionRepository();

        public List<KeyValuePair<int, string>> GetEnrolledCourses(
            int learnerId)
        {
            ValidateLearner(learnerId);

            return repository.GetEnrolledCourses(learnerId);
        }

        public List<LearnerDiscussionPost> GetPosts(
            int learnerId,
            int? courseId = null,
            string search = null,
            bool onlyMine = false,
            int pageNumber = 1,
            int pageSize = 20, string scope = "all")
        {
            ValidateLearner(learnerId);
            if (scope != "all" && scope != "general" && scope != "courses") throw new ArgumentException("Invalid discussion scope.");

            string searchText = (search ?? string.Empty).Trim();

            if (searchText.Length > MaximumSearchLength)
            {
                throw new ArgumentException(
                    "Please keep your search within 200 characters.");
            }

            return repository.GetPosts(
                learnerId,
                courseId,
                searchText,
                onlyMine,
                pageNumber,
                pageSize, scope);
        }

        public LearnerDiscussionPost GetPost(
            int learnerId,
            int postId)
        {
            ValidateLearner(learnerId);

            if (postId <= 0)
            {
                return null;
            }

            // Null means the post is missing or inaccessible.
            return repository.GetPost(learnerId, postId);
        }

        public List<LearnerDiscussionReply> GetReplies(
            int learnerId,
            int postId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            ValidateLearner(learnerId);
            ValidatePostId(postId);

            return repository.GetReplies(
                learnerId,
                postId,
                pageNumber,
                pageSize);
        }

        public List<LearnerDiscussionReply> GetMyReplies(
            int learnerId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            ValidateLearner(learnerId);

            return repository.GetMyReplies(
                learnerId,
                pageNumber,
                pageSize);
        }

        public int CreatePost(
            int learnerId,
            int? courseId,
            string title,
            string content)
        {
            ValidateLearner(learnerId);

            if (courseId.HasValue && courseId.Value <= 0)
            {
                throw new ArgumentException(
                    "Please select an enrolled course.");
            }

            string cleanTitle = ValidateText(
                title,
                MaximumTitleLength,
                "Please enter a discussion title.",
                "The discussion title cannot exceed 250 characters.");

            string cleanContent = ValidateText(
                content,
                MaximumPostLength,
                "Please write your discussion content.",
                "The discussion content cannot exceed 20,000 characters.");

            int? postId = repository.CreatePost(
                learnerId,
                courseId,
                cleanTitle,
                cleanContent);

            if (!postId.HasValue)
            {
                throw new InvalidOperationException(
                    "You cannot post to this course. " +
                    "Please check that you are still enrolled " +
                    "and that the course is available.");
            }

            return postId.Value;
        }

        public int CreateReply(
            int learnerId,
            int postId,
            string content)
        {
            ValidateLearner(learnerId);
            ValidatePostId(postId);

            string cleanContent = ValidateText(
                content,
                MaximumReplyLength,
                "Please write a reply before submitting.",
                "Your reply cannot exceed 10,000 characters.");

            int? replyId = repository.CreateReply(
                learnerId,
                postId,
                cleanContent);

            if (!replyId.HasValue)
            {
                throw new InvalidOperationException(
                    "This discussion is no longer available to you. " +
                    "It may have been deleted, or your course access " +
                    "may have changed.");
            }

            return replyId.Value;
        }

        public void UpdatePost(
            int learnerId,
            int postId,
            string title,
            string content)
        {
            ValidateLearner(learnerId);
            ValidatePostId(postId);

            string cleanTitle = ValidateText(
                title,
                MaximumTitleLength,
                "Please enter a discussion title.",
                "The discussion title cannot exceed 250 characters.");

            string cleanContent = ValidateText(
                content,
                MaximumPostLength,
                "Please write your discussion content.",
                "The discussion content cannot exceed 20,000 characters.");

            bool updated = repository.UpdatePost(
                learnerId,
                postId,
                cleanTitle,
                cleanContent);

            if (!updated)
            {
                throw new InvalidOperationException(
                    "This post could not be updated. " +
                    "You can only edit your own posts in courses " +
                    "you can currently access.");
            }
        }

        public void DeletePost(int learnerId, int postId)
        {
            ValidateLearner(learnerId);
            ValidatePostId(postId);

            bool deleted = repository.DeletePost(
                learnerId,
                postId);

            if (!deleted)
            {
                throw new InvalidOperationException(
                    "This post could not be deleted. " +
                    "It may already have been removed, or you may " +
                    "no longer have permission to delete it.");
            }
        }

        private static string ValidateText(
            string value,
            int maximumLength,
            string requiredMessage,
            string lengthMessage)
        {
            string text = (value ?? string.Empty).Trim();

            if (text.Length == 0)
            {
                throw new ArgumentException(requiredMessage);
            }

            if (text.Length > maximumLength)
            {
                throw new ArgumentException(lengthMessage);
            }

            return text;
        }

        private static void ValidateLearner(int learnerId)
        {
            if (learnerId <= 0 || CodeLab.Helpers.SessionHelper.GetCurrentUserID() != learnerId ||
                !(CodeLab.Helpers.SessionHelper.IsUserInRole("Learner") || CodeLab.Helpers.SessionHelper.IsUserInRole("Tutor")))
            {
                throw new ArgumentException(
                    "Please sign in with your learner account.");
            }
        }

        private static void ValidatePostId(int postId)
        {
            if (postId <= 0)
            {
                throw new ArgumentException(
                    "The discussion link is invalid.");
            }
        }
    }
}