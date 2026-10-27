using System;
using System.IO;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class CourseManager
    {
        private const int MaxTitleLength = 200;
        private const int MaxDescriptionLength = 4000;

        /// <summary>Largest cover image accepted: 2 MB.</summary>
        public const int MaxCoverBytes = 2 * 1024 * 1024;

        private readonly CourseRepository _courseRepository = new CourseRepository();
        private readonly TutorRepository _tutorRepository = new TutorRepository();

        /// <summary>
        /// Returns one of the signed-in tutor's own courses. A course that does not exist and a course
        /// owned by someone else give the same message, so ids cannot be probed.
        /// </summary>
        public OperationResult<Course> GetCourse(int courseId)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor) && !SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<Course>.Fail("Unauthorized: Only tutors can manage courses.");
            }

            try
            {
                Course course = _courseRepository.GetById(courseId);
                if (course == null || course.InstructorId != SessionHelper.GetCurrentUserID())
                {
                    return OperationResult<Course>.Fail("That course could not be found.");
                }

                return OperationResult<Course>.Ok(course);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseManager.GetCourse failure", exception);
                return OperationResult<Course>.Fail("The course could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Creates a new Draft course owned by the signed-in tutor. Data holds the new course id.
        /// coverContent may be null when no cover image was chosen.
        /// </summary>
        public OperationResult<int> CreateCourse(Course course, string coverFileName, int coverLength, Stream coverContent)
        {
            OperationResult authorization = CheckCanAuthor();
            if (!authorization.Success)
            {
                return OperationResult<int>.Fail(authorization.Message);
            }

            string validationError = CleanAndValidate(course);
            if (validationError != null)
            {
                return OperationResult<int>.Fail(validationError);
            }

            int tutorId = SessionHelper.GetCurrentUserID();
            course.InstructorId = tutorId;
            course.ThumbnailUrl = null;

            string newCoverUrl = null;
            if (coverContent != null && coverLength > 0)
            {
                OperationResult<string> cover = SaveCover(coverFileName, coverLength, coverContent, tutorId);
                if (!cover.Success)
                {
                    return OperationResult<int>.Fail(cover.Message);
                }

                newCoverUrl = cover.Data;
                course.ThumbnailUrl = newCoverUrl;
            }

            int courseId = _courseRepository.Insert(course);
            if (courseId == 0)
            {
                ImageUploadHelper.DeleteImage(newCoverUrl, ImageUploadHelper.CoverFolder);
                return OperationResult<int>.Fail("A system error occurred while saving the course. Please try again.");
            }

            new AuditManager().RecordActivity(tutorId, "CREATE_COURSE", "Created course '" + course.Title + "' as a draft.");
            return OperationResult<int>.Ok(courseId, "Your course has been created as a draft.");
        }

        /// <summary>
        /// Saves changes to a Draft course. A new cover image replaces the old one; removeCover clears it.
        /// </summary>
        public OperationResult UpdateCourse(Course course, string coverFileName, int coverLength, Stream coverContent, bool removeCover)
        {
            OperationResult authorization = CheckCanAuthor();
            if (!authorization.Success)
            {
                return authorization;
            }

            OperationResult<Course> existing = GetCourse(course.Id);
            if (!existing.Success)
            {
                return OperationResult.Fail(existing.Message);
            }

            if (!CourseStatuses.IsEditable(existing.Data.Status))
            {
                return OperationResult.Fail("Only Draft courses can be edited.");
            }

            string validationError = CleanAndValidate(course);
            if (validationError != null)
            {
                return OperationResult.Fail(validationError);
            }

            int tutorId = SessionHelper.GetCurrentUserID();
            course.InstructorId = tutorId;
            course.ThumbnailUrl = existing.Data.ThumbnailUrl;

            string newCoverUrl = null;
            if (coverContent != null && coverLength > 0)
            {
                OperationResult<string> cover = SaveCover(coverFileName, coverLength, coverContent, tutorId);
                if (!cover.Success)
                {
                    return OperationResult.Fail(cover.Message);
                }

                newCoverUrl = cover.Data;
                course.ThumbnailUrl = newCoverUrl;
            }
            else if (removeCover)
            {
                course.ThumbnailUrl = null;
            }

            if (!_courseRepository.UpdateDraft(course))
            {
                ImageUploadHelper.DeleteImage(newCoverUrl, ImageUploadHelper.CoverFolder);
                return OperationResult.Fail("A system error occurred while saving the course. Please try again.");
            }

            // The old cover file is only deleted once the new details are saved
            if (course.ThumbnailUrl != existing.Data.ThumbnailUrl)
            {
                ImageUploadHelper.DeleteImage(existing.Data.ThumbnailUrl, ImageUploadHelper.CoverFolder);
            }

            new AuditManager().RecordActivity(tutorId, "UPDATE_COURSE", "Updated course '" + course.Title + "'.");
            return OperationResult.Ok("Your changes have been saved.");
        }

        /// <summary>
        /// Sends a Draft course to the admins for review. The course must have at least one lesson.
        /// Admins find it in their review queue (Courses with Status = Pending Review). To reject it, an
        /// admin sets it back to Draft with feedback in RejectionReason.
        /// </summary>
        public OperationResult SubmitForReview(int courseId)
        {
            OperationResult authorization = CheckCanAuthor();
            if (!authorization.Success)
            {
                return authorization;
            }

            OperationResult<Course> course = GetCourse(courseId);
            if (!course.Success)
            {
                return OperationResult.Fail(course.Message);
            }

            if (!CourseStatuses.IsEditable(course.Data.Status))
            {
                return OperationResult.Fail("Only Draft courses can be submitted for review.");
            }

            int lessonCount;
            try
            {
                lessonCount = new LessonRepository().GetByCourse(courseId).Count;
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseManager.SubmitForReview lesson check failure", exception);
                return OperationResult.Fail("A system error occurred while submitting the course. Please try again.");
            }

            if (lessonCount == 0)
            {
                return OperationResult.Fail("Add at least one lesson before submitting the course for review.");
            }

            // The quiz is optional, but if the course has one it needs at least one question (proposal rule)
            try
            {
                QuizRepository quizzes = new QuizRepository();
                Quiz quiz = quizzes.GetByCourse(courseId);
                if (quiz != null && quizzes.GetQuestions(quiz.Id).Count == 0)
                {
                    return OperationResult.Fail("The quiz has no questions. Add at least one question, or delete the quiz, before submitting.");
                }
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseManager.SubmitForReview quiz check failure", exception);
                return OperationResult.Fail("A system error occurred while submitting the course. Please try again.");
            }

            if (!_courseRepository.SubmitForReview(courseId, SessionHelper.GetCurrentUserID()))
            {
                return OperationResult.Fail("A system error occurred while submitting the course. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "SUBMIT_COURSE",
                "Submitted course '" + course.Data.Title + "' for review (" + lessonCount + " lessons).");
            return OperationResult.Ok("Your course has been submitted for review. An administrator will check it soon.");
        }

        /// <summary>
        /// Only approved tutors (and admins) may create or change course content. Used by every authoring manager.
        /// </summary>
        public OperationResult CheckCanAuthor()
        {
            if (SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult.Ok();
            }

            if (!SessionHelper.IsUserInRole(UserRoles.Tutor))
            {
                return OperationResult.Fail("Unauthorized: Only tutors can manage courses.");
            }

            try
            {
                string status = _tutorRepository.GetApplicationStatus(SessionHelper.GetCurrentUserID());
                return status == TutorStatuses.Approved
                    ? OperationResult.Ok()
                    : OperationResult.Fail("Your tutor application must be approved before you can create or change course content.");
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseManager.CheckCanAuthor failure", exception);
                return OperationResult.Fail("A system error occurred. Please try again.");
            }
        }

        // Validates a cover image and saves the file. Data holds its app-relative path.
        private static OperationResult<string> SaveCover(string fileName, int length, Stream content, int tutorId)
        {
            byte[] data;
            string extension;
            string error = ImageUploadHelper.ReadValidatedImage(fileName, length, content, MaxCoverBytes, "cover image", out data, out extension);
            if (error != null)
            {
                return OperationResult<string>.Fail(error);
            }

            try
            {
                return OperationResult<string>.Ok(ImageUploadHelper.SaveImage(data, extension, ImageUploadHelper.CoverFolder, "c" + tutorId));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("CourseManager.SaveCover failure", exception);
                return OperationResult<string>.Fail("A system error occurred while saving the cover image. Please try again.");
            }
        }

        // Trims the form values, then returns the first problem with them, or null if they are valid
        private static string CleanAndValidate(Course course)
        {
            course.Title = Clean(course.Title);
            course.Description = Clean(course.Description);
            course.Category = Clean(course.Category);
            course.Difficulty = Clean(course.Difficulty);

            if (course.Title == null || course.Description == null)
            {
                return "Title and description are required.";
            }

            if (course.Title.Length > MaxTitleLength)
            {
                return "Title must be " + MaxTitleLength + " characters or fewer.";
            }

            if (course.Description.Length > MaxDescriptionLength)
            {
                return "Description must be " + MaxDescriptionLength + " characters or fewer.";
            }

            if (!CourseOptions.IsValidCategory(course.Category))
            {
                return "Choose a category from the list.";
            }

            if (!CourseOptions.IsValidDifficulty(course.Difficulty))
            {
                return "Choose a difficulty from the list.";
            }

            return null;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
