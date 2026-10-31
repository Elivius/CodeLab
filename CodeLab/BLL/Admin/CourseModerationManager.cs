using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// Admin moderation of courses: review submitted courses, publish them or send them back with
    /// feedback, lock published ones, and delete courses. See AdminCourseRow for what each status allows.
    /// </summary>
    public class CourseModerationManager
    {
        public const int MaxReasonLength = 500;

        private const string UnauthorizedMessage = "Unauthorized: Only administrators can moderate courses.";

        private readonly CourseModerationRepository _repository = new CourseModerationRepository();
        private readonly LessonRepository _lessonRepository = new LessonRepository();
        private readonly QuizRepository _quizRepository = new QuizRepository();
        private readonly ChallengeRepository _challengeRepository = new ChallengeRepository();

        /// <summary>
        /// Returns the courses with the given status (CourseStatuses), or all of them when status is null,
        /// narrowed to those matching the search text when there is one.
        /// </summary>
        public OperationResult<List<AdminCourseRow>> GetCourses(string status, string search)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<List<AdminCourseRow>>.Fail(UnauthorizedMessage);
            }

            if (status != null && status != CourseStatuses.Draft && status != CourseStatuses.PendingReview
                && status != CourseStatuses.Published && status != CourseStatuses.Locked)
            {
                return OperationResult<List<AdminCourseRow>>.Fail("Unknown course status.");
            }

            try
            {
                string searchText = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
                return OperationResult<List<AdminCourseRow>>.Ok(_repository.GetCourses(status, searchText));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseModerationManager.GetCourses failure", exception);
                return OperationResult<List<AdminCourseRow>>.Fail("The courses could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Returns a course with its lessons, quiz and coding challenge, for the admin to review.
        /// </summary>
        public OperationResult<CourseReviewDetails> GetReview(int courseId)
        {
            OperationResult<AdminCourseRow> course = LoadCourse(courseId);
            if (!course.Success)
            {
                return OperationResult<CourseReviewDetails>.Fail(course.Message);
            }

            try
            {
                CourseReviewDetails review = new CourseReviewDetails
                {
                    Course = course.Data,
                    Lessons = _lessonRepository.GetByCourse(courseId),
                    Quiz = _quizRepository.GetByCourse(courseId),
                    Challenge = _challengeRepository.GetByCourse(courseId)
                };

                if (review.Quiz != null)
                {
                    review.Quiz.Questions = _quizRepository.GetQuestions(review.Quiz.Id);
                }

                return OperationResult<CourseReviewDetails>.Ok(review);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseModerationManager.GetReview failure", exception);
                return OperationResult<CourseReviewDetails>.Fail("The course could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// The number of courses waiting for review, for the tab badge.
        /// </summary>
        public OperationResult<int> CountPending()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<int>.Fail(UnauthorizedMessage);
            }

            try
            {
                return OperationResult<int>.Ok(_repository.CountPending());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseModerationManager.CountPending failure", exception);
                return OperationResult<int>.Fail("The number of pending courses could not be loaded.");
            }
        }

        public OperationResult Approve(int courseId)
        {
            OperationResult<AdminCourseRow> course = LoadCourse(courseId);
            if (!course.Success)
            {
                return course;
            }

            if (!course.Data.CanApproveOrReject)
            {
                return WrongStatus(course.Data, "approved");
            }

            if (!_repository.Approve(courseId))
            {
                return OperationResult.Fail("The course could not be approved. Refresh the page and try again.");
            }

            Record("APPROVE_COURSE", "Approved " + Describe(course.Data) + ".");
            return OperationResult.Ok("'" + course.Data.Title + "' is now published, and learners can enrol.");
        }

        /// <summary>
        /// Sends a submitted course back to Draft. The tutor sees the reason in the Course builder.
        /// </summary>
        public OperationResult Reject(int courseId, string reason)
        {
            reason = (reason ?? string.Empty).Trim();
            string reasonError = ValidateReason(reason);
            if (reasonError != null)
            {
                return OperationResult.Fail(reasonError);
            }

            OperationResult<AdminCourseRow> course = LoadCourse(courseId);
            if (!course.Success)
            {
                return course;
            }

            if (!course.Data.CanApproveOrReject)
            {
                return WrongStatus(course.Data, "rejected");
            }

            if (!_repository.Reject(courseId, reason))
            {
                return OperationResult.Fail("The course could not be rejected. Refresh the page and try again.");
            }

            Record("REJECT_COURSE", "Rejected " + Describe(course.Data) + ". Reason: " + reason);
            return OperationResult.Ok("'" + course.Data.Title + "' was sent back to the tutor with your feedback.");
        }

        /// <summary>
        /// Takes a published course away from learners. The tutor sees the reason in the Course builder.
        /// </summary>
        public OperationResult Lock(int courseId, string reason)
        {
            reason = (reason ?? string.Empty).Trim();
            string reasonError = ValidateReason(reason);
            if (reasonError != null)
            {
                return OperationResult.Fail(reasonError);
            }

            OperationResult<AdminCourseRow> course = LoadCourse(courseId);
            if (!course.Success)
            {
                return course;
            }

            if (!course.Data.CanLock)
            {
                return WrongStatus(course.Data, "locked");
            }

            if (!_repository.Lock(courseId, reason))
            {
                return OperationResult.Fail("The course could not be locked. Refresh the page and try again.");
            }

            Record("LOCK_COURSE", "Locked " + Describe(course.Data) + ". Reason: " + reason);
            return OperationResult.Ok("'" + course.Data.Title + "' is locked. Learners can no longer open it, and the tutor can see your reason.");
        }

        public OperationResult Unlock(int courseId)
        {
            OperationResult<AdminCourseRow> course = LoadCourse(courseId);
            if (!course.Success)
            {
                return course;
            }

            if (!course.Data.CanUnlock)
            {
                return WrongStatus(course.Data, "unlocked");
            }

            if (!_repository.Unlock(courseId))
            {
                return OperationResult.Fail("The course could not be unlocked. Refresh the page and try again.");
            }

            Record("UNLOCK_COURSE", "Unlocked " + Describe(course.Data) + ".");
            return OperationResult.Ok("'" + course.Data.Title + "' is unlocked and published again.");
        }

        /// <summary>
        /// Deletes a course in any status, with everything that belongs to it, then removes its uploaded files.
        /// </summary>
        public OperationResult Delete(int courseId)
        {
            OperationResult<AdminCourseRow> course = LoadCourse(courseId);
            if (!course.Success)
            {
                return course;
            }

            // Read before the delete: afterwards the rows that hold these paths are gone
            List<Lesson> lessons;
            try
            {
                lessons = _lessonRepository.GetByCourse(courseId);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseModerationManager.Delete failure", exception);
                return OperationResult.Fail("The course could not be deleted. Please try again.");
            }

            if (!_repository.Delete(courseId))
            {
                return OperationResult.Fail("The course could not be deleted. Refresh the page and try again.");
            }

            // Only once the rows are gone, so a failed delete can never leave a course with missing files
            ImageUploadHelper.DeleteImage(course.Data.ThumbnailUrl, ImageUploadHelper.CoverFolder);
            foreach (Lesson lesson in lessons)
            {
                ImageUploadHelper.DeleteImage(lesson.AttachmentUrl, FileUploadHelper.AttachmentFolder);
            }

            Record("DELETE_COURSE", "Deleted " + Describe(course.Data) + " by " + course.Data.TutorName
                + ", with " + course.Data.StudentCount + " enrolled.");

            return OperationResult.Ok("'" + course.Data.Title + "' was deleted, with its lessons, quiz, challenge and discussions.");
        }

        // Every action loads the course here first, so every action gets the admin check
        private OperationResult<AdminCourseRow> LoadCourse(int courseId)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<AdminCourseRow>.Fail(UnauthorizedMessage);
            }

            try
            {
                AdminCourseRow course = _repository.GetCourse(courseId);
                return course == null
                    ? OperationResult<AdminCourseRow>.Fail("This course could not be found. It may have been deleted.")
                    : OperationResult<AdminCourseRow>.Ok(course);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseModerationManager.LoadCourse failure", exception);
                return OperationResult<AdminCourseRow>.Fail("The course could not be loaded. Please try again.");
            }
        }

        private static string ValidateReason(string reason)
        {
            if (reason.Length == 0)
            {
                return "Please give a reason. The tutor will see it.";
            }

            return reason.Length > MaxReasonLength
                ? "The reason must be " + MaxReasonLength + " characters or fewer."
                : null;
        }

        // A clear message when the status changed first: a double click, or a second admin in another tab
        private static OperationResult WrongStatus(AdminCourseRow course, string action)
        {
            return OperationResult.Fail("This course is " + course.Status + ", so it cannot be " + action + ".");
        }

        private static string Describe(AdminCourseRow course)
        {
            return "course '" + course.Title + "' (course " + course.Id + ")";
        }

        private static void Record(string action, string details)
        {
            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), action, details);
        }
    }
}
