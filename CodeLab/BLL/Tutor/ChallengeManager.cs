using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// A course's coding challenge. Rules:
    /// - A course has at most one coding challenge (team decision, see TUTOR_DECISIONS.md).
    /// - It can only be changed while the course is a Draft.
    /// </summary>
    public class ChallengeManager
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 10000;
        public const int MaxCodeLength = 10000;
        public const int MaxOutputLength = 5000;
        public const int MaxPoints = 1000;
        public const int DefaultPoints = 50;

        private readonly ChallengeRepository _challengeRepository = new ChallengeRepository();
        private readonly CourseManager _courseManager = new CourseManager();

        /// <summary>
        /// Returns the course's coding challenge. Data is null when the course has none yet.
        /// Only the course's own tutor can see it.
        /// </summary>
        public OperationResult<CodingChallenge> GetChallenge(int courseId)
        {
            OperationResult<Course> course = _courseManager.GetCourse(courseId);
            if (!course.Success)
            {
                return OperationResult<CodingChallenge>.Fail(course.Message);
            }

            try
            {
                return OperationResult<CodingChallenge>.Ok(_challengeRepository.GetByCourse(courseId));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("ChallengeManager.GetChallenge failure", exception);
                return OperationResult<CodingChallenge>.Fail("The coding challenge could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Ids of the signed-in tutor's courses that have a coding challenge.
        /// </summary>
        public HashSet<int> GetCourseIdsWithChallenge()
        {
            try
            {
                return _challengeRepository.GetCourseIdsWithChallenge(SessionHelper.GetCurrentUserID());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("ChallengeManager.GetCourseIdsWithChallenge failure", exception);
                return new HashSet<int>();
            }
        }

        /// <summary>
        /// Creates the course's challenge, or saves it if it already has one. Data holds the challenge id.
        /// </summary>
        public OperationResult<int> SaveChallenge(CodingChallenge challenge)
        {
            OperationResult<Course> course = GetEditableCourse(challenge.CourseId);
            if (!course.Success)
            {
                return OperationResult<int>.Fail(course.Message);
            }

            string validationError = CleanAndValidate(challenge);
            if (validationError != null)
            {
                return OperationResult<int>.Fail(validationError);
            }

            OperationResult<CodingChallenge> existing = GetChallenge(challenge.CourseId);
            if (!existing.Success)
            {
                return OperationResult<int>.Fail(existing.Message);
            }

            if (existing.Data == null)
            {
                int challengeId = _challengeRepository.Insert(challenge);
                if (challengeId == 0)
                {
                    return OperationResult<int>.Fail("A system error occurred while saving the challenge. Please try again.");
                }

                new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "CREATE_CHALLENGE",
                    "Created coding challenge '" + challenge.Title + "' for course '" + course.Data.Title + "'.");
                return OperationResult<int>.Ok(challengeId, "The coding challenge has been created.");
            }

            challenge.Id = existing.Data.Id;
            if (!_challengeRepository.Update(challenge))
            {
                return OperationResult<int>.Fail("A system error occurred while saving the challenge. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "UPDATE_CHALLENGE",
                "Updated coding challenge '" + challenge.Title + "'.");
            return OperationResult<int>.Ok(challenge.Id, "Your changes have been saved.");
        }

        /// <summary>
        /// Deletes the course's coding challenge.
        /// </summary>
        public OperationResult DeleteChallenge(int courseId)
        {
            OperationResult<Course> course = GetEditableCourse(courseId);
            if (!course.Success)
            {
                return OperationResult.Fail(course.Message);
            }

            OperationResult<CodingChallenge> existing = GetChallenge(courseId);
            if (!existing.Success)
            {
                return OperationResult.Fail(existing.Message);
            }

            if (existing.Data == null || !_challengeRepository.Delete(courseId, existing.Data.Id))
            {
                return OperationResult.Fail("The coding challenge could not be deleted. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "DELETE_CHALLENGE",
                "Deleted coding challenge '" + existing.Data.Title + "' from course '" + course.Data.Title + "'.");
            return OperationResult.Ok("The coding challenge has been deleted.");
        }

        // The signed-in tutor's own course, if they are allowed to author and the course is still a Draft
        private OperationResult<Course> GetEditableCourse(int courseId)
        {
            OperationResult authorization = _courseManager.CheckCanAuthor();
            if (!authorization.Success)
            {
                return OperationResult<Course>.Fail(authorization.Message);
            }

            OperationResult<Course> course = _courseManager.GetCourse(courseId);
            if (course.Success && !CourseStatuses.IsEditable(course.Data.Status))
            {
                return OperationResult<Course>.Fail("The coding challenge can only be changed while the course is a Draft.");
            }

            return course;
        }

        // Trims the form values, then returns the first problem with them, or null if they are valid
        private static string CleanAndValidate(CodingChallenge challenge)
        {
            challenge.Title = Clean(challenge.Title);
            challenge.Description = Clean(challenge.Description);
            challenge.Language = Clean(challenge.Language);
            // Code keeps its leading indentation; only the end is trimmed
            challenge.StarterCode = string.IsNullOrWhiteSpace(challenge.StarterCode) ? string.Empty : challenge.StarterCode.TrimEnd();
            challenge.ExpectedOutput = string.IsNullOrWhiteSpace(challenge.ExpectedOutput) ? null : challenge.ExpectedOutput.TrimEnd();

            if (challenge.Title == null || challenge.Description == null)
            {
                return "The title and problem statement are required.";
            }

            if (challenge.Title.Length > MaxTitleLength)
            {
                return "The title must be " + MaxTitleLength + " characters or fewer.";
            }

            if (challenge.Description.Length > MaxDescriptionLength)
            {
                return "The problem statement must be " + MaxDescriptionLength + " characters or fewer.";
            }

            if (!CourseOptions.IsValidDifficulty(challenge.Difficulty))
            {
                return "Choose a difficulty.";
            }

            if (!LessonOptions.IsValidLanguage(challenge.Language))
            {
                return "Choose the programming language.";
            }

            if (challenge.StarterCode.Length > MaxCodeLength)
            {
                return "The starter code must be " + MaxCodeLength + " characters or fewer.";
            }

            if (challenge.ExpectedOutput == null)
            {
                return "The expected output is required, so learners' answers can be checked.";
            }

            if (challenge.ExpectedOutput.Length > MaxOutputLength)
            {
                return "The expected output must be " + MaxOutputLength + " characters or fewer.";
            }

            if (challenge.PointsReward < 0 || challenge.PointsReward > MaxPoints)
            {
                return "Points must be between 0 and " + MaxPoints + ".";
            }

            return null;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
