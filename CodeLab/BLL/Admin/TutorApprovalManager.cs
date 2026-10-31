using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// Admin review of tutor applications: list them, then approve or reject Pending ones.
    /// An approved tutor can author courses straight away; a rejected one sees the reason on their Profile and can resubmit.
    /// </summary>
    public class TutorApprovalManager
    {
        public const int MaxReasonLength = 500;

        private const string UnauthorizedMessage = "Unauthorized: Only administrators can review tutor applications.";

        private readonly TutorApprovalRepository _repository = new TutorApprovalRepository();

        /// <summary>
        /// Returns the applications with the given status (TutorStatuses), or all of them when status is null.
        /// </summary>
        public OperationResult<List<TutorProfile>> GetApplications(string status)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<List<TutorProfile>>.Fail(UnauthorizedMessage);
            }

            if (status != null && status != TutorStatuses.Pending && status != TutorStatuses.Approved && status != TutorStatuses.Rejected)
            {
                return OperationResult<List<TutorProfile>>.Fail("Unknown application status.");
            }

            try
            {
                return OperationResult<List<TutorProfile>>.Ok(_repository.GetApplications(status));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorApprovalManager.GetApplications failure", exception);
                return OperationResult<List<TutorProfile>>.Fail("The tutor applications could not be loaded. Please try again.");
            }
        }

        public OperationResult<TutorProfile> GetApplication(int userId)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<TutorProfile>.Fail(UnauthorizedMessage);
            }

            try
            {
                TutorProfile application = _repository.GetApplication(userId);
                return application == null
                    ? OperationResult<TutorProfile>.Fail("This tutor application could not be found.")
                    : OperationResult<TutorProfile>.Ok(application);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorApprovalManager.GetApplication failure", exception);
                return OperationResult<TutorProfile>.Fail("The tutor application could not be loaded. Please try again.");
            }
        }

        public OperationResult Approve(int userId)
        {
            OperationResult<TutorProfile> pending = GetPendingApplication(userId);
            if (!pending.Success)
            {
                return pending;
            }

            if (!_repository.Approve(userId))
            {
                return OperationResult.Fail("The application could not be approved. Refresh the page and try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "APPROVE_TUTOR",
                "Approved the tutor application of '" + pending.Data.FullName + "' (user " + userId + ").");

            return OperationResult.Ok(pending.Data.FullName + " is now an approved tutor and can start creating courses.");
        }

        public OperationResult Reject(int userId, string reason)
        {
            reason = (reason ?? string.Empty).Trim();
            if (reason.Length == 0)
            {
                return OperationResult.Fail("Please give a reason. The applicant will see it.");
            }

            if (reason.Length > MaxReasonLength)
            {
                return OperationResult.Fail("The reason must be " + MaxReasonLength + " characters or fewer.");
            }

            OperationResult<TutorProfile> pending = GetPendingApplication(userId);
            if (!pending.Success)
            {
                return pending;
            }

            if (!_repository.Reject(userId, reason))
            {
                return OperationResult.Fail("The application could not be rejected. Refresh the page and try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "REJECT_TUTOR",
                "Rejected the tutor application of '" + pending.Data.FullName + "' (user " + userId + "). Reason: " + reason);

            return OperationResult.Ok(pending.Data.FullName + "'s application was rejected. They can see your reason and resubmit.");
        }

        // Loads the application and checks it is still waiting for review, so a double click or a
        // second admin gets a clear message instead of a silent no-op
        private OperationResult<TutorProfile> GetPendingApplication(int userId)
        {
            OperationResult<TutorProfile> result = GetApplication(userId);
            if (!result.Success)
            {
                return result;
            }

            if (result.Data.Status != TutorStatuses.Pending)
            {
                return OperationResult<TutorProfile>.Fail(
                    "This application has already been " + result.Data.Status.ToLowerInvariant() + ".");
            }

            return result;
        }
    }
}
