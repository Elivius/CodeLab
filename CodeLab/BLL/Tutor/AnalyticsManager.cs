using System;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// Learner activity in the signed-in tutor's courses: enrolments, lesson completions, quiz attempts,
    /// code submissions and unanswered discussion posts. Read-only; built from the existing tables.
    /// </summary>
    public class AnalyticsManager
    {
        private readonly AnalyticsRepository _analyticsRepository = new AnalyticsRepository();
        private readonly CourseManager _courseManager = new CourseManager();

        /// <summary>
        /// Analytics for all of the tutor's courses (courseId 0) or for one of them.
        /// </summary>
        public OperationResult<TutorAnalytics> GetAnalytics(int courseId)
        {
            OperationResult authorization = _courseManager.CheckCanAuthor();
            if (!authorization.Success)
            {
                return OperationResult<TutorAnalytics>.Fail(authorization.Message);
            }

            // A course in the link must be one of the tutor's own
            if (courseId > 0)
            {
                OperationResult<Course> course = _courseManager.GetCourse(courseId);
                if (!course.Success)
                {
                    return OperationResult<TutorAnalytics>.Fail(course.Message);
                }
            }

            try
            {
                return OperationResult<TutorAnalytics>.Ok(
                    _analyticsRepository.GetAnalytics(SessionHelper.GetCurrentUserID(), Math.Max(0, courseId), FirstWeekStart()));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("AnalyticsManager.GetAnalytics failure", exception);
                return OperationResult<TutorAnalytics>.Fail("The analytics could not be loaded. Please try again.");
            }
        }

        // Monday of the oldest week in the activity chart. Times are stored in UTC, so weeks are UTC weeks.
        private static DateTime FirstWeekStart()
        {
            DateTime today = DateTime.UtcNow.Date;
            int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            return today.AddDays(-daysSinceMonday).AddDays(-7 * (AnalyticsRepository.ActivityWeeks - 1));
        }
    }
}
