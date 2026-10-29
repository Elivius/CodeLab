using System;
using System.Collections.Generic;
using System.IO;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class LessonManager
    {
        private const int MaxTitleLength = 200;
        private const int MaxContentLength = 20000;
        private const int MaxCodeLength = 10000;

        /// <summary>Largest attachment accepted: 5 MB.</summary>
        public const int MaxAttachmentBytes = 5 * 1024 * 1024;

        private readonly LessonRepository _lessonRepository = new LessonRepository();
        private readonly CourseManager _courseManager = new CourseManager();

        /// <summary>
        /// Returns a course's lessons in order. Only the course's own tutor can see them.
        /// </summary>
        public OperationResult<List<Lesson>> GetLessons(int courseId)
        {
            OperationResult<Course> course = _courseManager.GetCourse(courseId);
            if (!course.Success)
            {
                return OperationResult<List<Lesson>>.Fail(course.Message);
            }

            try
            {
                return OperationResult<List<Lesson>>.Ok(_lessonRepository.GetByCourse(courseId));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("LessonManager.GetLessons failure", exception);
                return OperationResult<List<Lesson>>.Fail("The lessons could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Returns one lesson of one of the signed-in tutor's courses.
        /// </summary>
        public OperationResult<Lesson> GetLesson(int courseId, int lessonId)
        {
            OperationResult<Course> course = _courseManager.GetCourse(courseId);
            if (!course.Success)
            {
                return OperationResult<Lesson>.Fail(course.Message);
            }

            try
            {
                Lesson lesson = _lessonRepository.GetById(courseId, lessonId);
                return lesson == null
                    ? OperationResult<Lesson>.Fail("That lesson could not be found.")
                    : OperationResult<Lesson>.Ok(lesson);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("LessonManager.GetLesson failure", exception);
                return OperationResult<Lesson>.Fail("The lesson could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Adds a lesson to the end of a Draft course. Data holds the new lesson id.
        /// attachmentContent may be null when no file was chosen.
        /// </summary>
        public OperationResult<int> CreateLesson(Lesson lesson, string attachmentName, int attachmentLength, Stream attachmentContent)
        {
            OperationResult<Course> course = GetEditableCourse(lesson.CourseId);
            if (!course.Success)
            {
                return OperationResult<int>.Fail(course.Message);
            }

            // New lessons always go after the last one
            OperationResult<List<Lesson>> current = GetLessons(lesson.CourseId);
            if (!current.Success)
            {
                return OperationResult<int>.Fail(current.Message);
            }

            lesson.OrderIndex = current.Data.Count + 1;

            string validationError = CleanAndValidate(lesson);
            if (validationError != null)
            {
                return OperationResult<int>.Fail(validationError);
            }

            lesson.AttachmentUrl = null;
            lesson.AttachmentName = null;

            string newAttachmentUrl = null;
            if (attachmentContent != null && attachmentLength > 0)
            {
                OperationResult<string> saved = SaveAttachment(lesson, attachmentName, attachmentLength, attachmentContent);
                if (!saved.Success)
                {
                    return OperationResult<int>.Fail(saved.Message);
                }

                newAttachmentUrl = saved.Data;
            }

            int lessonId = _lessonRepository.Insert(lesson);
            if (lessonId == 0)
            {
                ImageUploadHelper.DeleteImage(newAttachmentUrl, FileUploadHelper.AttachmentFolder);
                return OperationResult<int>.Fail("A system error occurred while saving the lesson. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "CREATE_LESSON",
                "Added lesson '" + lesson.Title + "' to course '" + course.Data.Title + "'.");
            return OperationResult<int>.Ok(lessonId, "The lesson has been added.");
        }

        /// <summary>
        /// Saves changes to a lesson of a Draft course; it keeps its position. A new attachment replaces
        /// the old one; removeAttachment clears it.
        /// </summary>
        public OperationResult UpdateLesson(Lesson lesson, string attachmentName, int attachmentLength, Stream attachmentContent, bool removeAttachment)
        {
            OperationResult<Course> course = GetEditableCourse(lesson.CourseId);
            if (!course.Success)
            {
                return OperationResult.Fail(course.Message);
            }

            OperationResult<Lesson> existing = GetLesson(lesson.CourseId, lesson.Id);
            if (!existing.Success)
            {
                return OperationResult.Fail(existing.Message);
            }

            // Lessons are not reordered, so an edited lesson stays where it is
            lesson.OrderIndex = existing.Data.OrderIndex;

            string validationError = CleanAndValidate(lesson);
            if (validationError != null)
            {
                return OperationResult.Fail(validationError);
            }

            lesson.AttachmentUrl = existing.Data.AttachmentUrl;
            lesson.AttachmentName = existing.Data.AttachmentName;

            string newAttachmentUrl = null;
            if (attachmentContent != null && attachmentLength > 0)
            {
                OperationResult<string> saved = SaveAttachment(lesson, attachmentName, attachmentLength, attachmentContent);
                if (!saved.Success)
                {
                    return OperationResult.Fail(saved.Message);
                }

                newAttachmentUrl = saved.Data;
            }
            else if (removeAttachment)
            {
                lesson.AttachmentUrl = null;
                lesson.AttachmentName = null;
            }

            if (!_lessonRepository.Update(lesson))
            {
                ImageUploadHelper.DeleteImage(newAttachmentUrl, FileUploadHelper.AttachmentFolder);
                return OperationResult.Fail("A system error occurred while saving the lesson. Please try again.");
            }

            // The old file is only deleted once the new details are saved
            if (lesson.AttachmentUrl != existing.Data.AttachmentUrl)
            {
                ImageUploadHelper.DeleteImage(existing.Data.AttachmentUrl, FileUploadHelper.AttachmentFolder);
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "UPDATE_LESSON",
                "Updated lesson '" + lesson.Title + "' in course '" + course.Data.Title + "'.");
            return OperationResult.Ok("Your changes have been saved.");
        }

        /// <summary>
        /// Deletes a lesson from a Draft course, along with its attachment.
        /// </summary>
        public OperationResult DeleteLesson(int courseId, int lessonId)
        {
            OperationResult<Course> course = GetEditableCourse(courseId);
            if (!course.Success)
            {
                return OperationResult.Fail(course.Message);
            }

            OperationResult<Lesson> existing = GetLesson(courseId, lessonId);
            if (!existing.Success)
            {
                return OperationResult.Fail(existing.Message);
            }

            if (!_lessonRepository.Delete(courseId, lessonId))
            {
                return OperationResult.Fail("A system error occurred while deleting the lesson. Please try again.");
            }

            ImageUploadHelper.DeleteImage(existing.Data.AttachmentUrl, FileUploadHelper.AttachmentFolder);
            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "DELETE_LESSON",
                "Deleted lesson '" + existing.Data.Title + "' from course '" + course.Data.Title + "'.");
            return OperationResult.Ok("The lesson has been deleted.");
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
                return OperationResult<Course>.Fail("Lessons can only be changed while the course is a Draft.");
            }

            return course;
        }

        // Validates an attachment and saves the file. Sets the lesson's attachment fields; Data holds the path.
        private static OperationResult<string> SaveAttachment(Lesson lesson, string fileName, int length, Stream content)
        {
            byte[] data;
            string extension;
            string displayName;
            string error = FileUploadHelper.ReadValidatedAttachment(fileName, length, content, MaxAttachmentBytes,
                out data, out extension, out displayName);
            if (error != null)
            {
                return OperationResult<string>.Fail(error);
            }

            try
            {
                string url = ImageUploadHelper.SaveImage(data, extension, FileUploadHelper.AttachmentFolder, "l" + lesson.CourseId);
                lesson.AttachmentUrl = url;
                lesson.AttachmentName = displayName;
                return OperationResult<string>.Ok(url);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("LessonManager.SaveAttachment failure", exception);
                return OperationResult<string>.Fail("A system error occurred while saving the attachment. Please try again.");
            }
        }

        // Trims the form values, then returns the first problem with them, or null if they are valid
        private static string CleanAndValidate(Lesson lesson)
        {
            lesson.Title = Clean(lesson.Title);
            lesson.Content = Clean(lesson.Content);
            // Only the end of a code snippet is trimmed, so the first line keeps its indentation
            lesson.CodeSnippet = string.IsNullOrWhiteSpace(lesson.CodeSnippet) ? null : lesson.CodeSnippet.TrimEnd();
            lesson.CodeLanguage = Clean(lesson.CodeLanguage);
            lesson.VideoUrl = Clean(lesson.VideoUrl);

            if (lesson.Title == null || lesson.Content == null)
            {
                return "Title and content are required.";
            }

            if (lesson.Title.Length > MaxTitleLength)
            {
                return "Title must be " + MaxTitleLength + " characters or fewer.";
            }

            if (lesson.Content.Length > MaxContentLength)
            {
                return "Content must be " + MaxContentLength + " characters or fewer.";
            }

            if (lesson.OrderIndex < 1)
            {
                return "The order must be a positive whole number.";
            }

            if (lesson.VideoUrl != null)
            {
                string videoId;
                if (!YouTubeHelper.TryGetVideoId(lesson.VideoUrl, out videoId))
                {
                    return "The video must be a YouTube link, e.g. https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                }

                lesson.VideoUrl = YouTubeHelper.ToWatchUrl(videoId);
            }

            if (lesson.CodeSnippet == null)
            {
                lesson.CodeLanguage = null;
            }
            else
            {
                if (lesson.CodeSnippet.Length > MaxCodeLength)
                {
                    return "The code snippet must be " + MaxCodeLength + " characters or fewer.";
                }

                if (!LessonOptions.IsValidLanguage(lesson.CodeLanguage))
                {
                    return "Choose the language of the code snippet.";
                }
            }

            return null;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
