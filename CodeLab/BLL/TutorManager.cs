using System;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class TutorManager
    {
        private readonly TutorRepository _tutorRepository = new TutorRepository();
        private readonly CourseRepository _courseRepository = new CourseRepository();

        /// <summary>
        /// Returns the signed-in tutor's application status (TutorStatuses), or null Data if they
        /// have not applied yet. Admins count as Approved so they can use every Tutor page.
        /// </summary>
        public OperationResult<string> GetApprovalStatus()
        {
            if (SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<string>.Ok(TutorStatuses.Approved);
            }

            if (!SessionHelper.IsUserInRole(UserRoles.Tutor))
            {
                return OperationResult<string>.Fail("Unauthorized: Only tutors can use the tutor area.");
            }

            try
            {
                return OperationResult<string>.Ok(_tutorRepository.GetApplicationStatus(SessionHelper.GetCurrentUserID()));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.GetApprovalStatus failure", exception);
                return OperationResult<string>.Fail("Your tutor status could not be loaded. Please refresh the page.");
            }
        }

        /// <summary>
        /// Returns the headline numbers and course list for the signed-in tutor's dashboard.
        /// </summary>
        public OperationResult<TutorDashboard> GetDashboard()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor) && !SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<TutorDashboard>.Fail("Unauthorized: Only tutors can view the tutor dashboard.");
            }

            int tutorId = SessionHelper.GetCurrentUserID();

            try
            {
                TutorDashboard dashboard = _tutorRepository.GetDashboardStats(tutorId);
                dashboard.Courses = _courseRepository.GetCourseSummariesByTutor(tutorId);
                return OperationResult<TutorDashboard>.Ok(dashboard);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.GetDashboard failure", exception);
                return OperationResult<TutorDashboard>.Fail("Your dashboard could not be loaded. Please try again.");
            }
        }
    }
}
