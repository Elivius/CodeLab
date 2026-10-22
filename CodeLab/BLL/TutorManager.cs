using System;
using System.IO;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class TutorManager
    {
        private const int MaxFullNameLength = 120;
        private const int MaxSpecialtyLength = 100;
        private const int MaxUrlLength = 500;
        private const int MaxLongTextLength = 2000;

        /// <summary>Largest profile photo accepted: 2 MB.</summary>
        public const int MaxAvatarBytes = 2 * 1024 * 1024;

        private readonly UserRepository _userRepository = new UserRepository();

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
        /// Returns the signed-in user's account details and tutor application.
        /// </summary>
        public OperationResult<TutorProfile> GetProfile()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor) && !SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<TutorProfile>.Fail("Unauthorized: Only tutors can view the tutor profile.");
            }

            try
            {
                TutorProfile profile = _tutorRepository.GetProfile(SessionHelper.GetCurrentUserID());
                if (profile == null)
                {
                    return OperationResult<TutorProfile>.Fail("Your account could not be found. Please log in again.");
                }

                return OperationResult<TutorProfile>.Ok(profile);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.GetProfile failure", exception);
                return OperationResult<TutorProfile>.Fail("Your profile could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Submits the signed-in tutor's application for admin review. Allowed when the tutor
        /// has not applied yet or was rejected; never while an application is Pending or Approved.
        /// </summary>
        public OperationResult SubmitApplication(TutorProfile application)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor))
            {
                return OperationResult.Fail("Unauthorized: Only tutor accounts can submit a tutor application.");
            }

            if (application == null)
            {
                return OperationResult.Fail("Please fill in the application form.");
            }

            application.UserId = SessionHelper.GetCurrentUserID();
            string validationError = CleanAndValidate(application);
            if (validationError != null)
            {
                return OperationResult.Fail(validationError);
            }

            string currentStatus;
            try
            {
                currentStatus = _tutorRepository.GetApplicationStatus(application.UserId);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.SubmitApplication status check failure", exception);
                return OperationResult.Fail("A system error occurred while submitting your application. Please try again.");
            }

            if (currentStatus == TutorStatuses.Pending)
            {
                return OperationResult.Fail("You already have an application waiting for review.");
            }

            if (currentStatus == TutorStatuses.Approved)
            {
                return OperationResult.Fail("Your tutor application has already been approved.");
            }

            if (!_tutorRepository.SaveApplication(application))
            {
                return OperationResult.Fail("A system error occurred while submitting your application. Please try again.");
            }

            SessionHelper.UpdateFullName(application.FullName);
            new AuditManager().RecordActivity(
                application.UserId,
                currentStatus == TutorStatuses.Rejected ? "RESUBMIT_TUTOR_APPLICATION" : "SUBMIT_TUTOR_APPLICATION",
                "Submitted a tutor application for review (specialty: " + application.Specialty + ").");

            return OperationResult.Ok("Your application has been submitted. An administrator will review it soon.");
        }

        /// <summary>
        /// Saves changes to the signed-in tutor's name and details. Allowed while the application is
        /// Pending or Approved. The status stays the same, so the changes do not need admin approval.
        /// </summary>
        public OperationResult UpdateProfile(TutorProfile profile)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor))
            {
                return OperationResult.Fail("Unauthorized: Only tutor accounts can update a tutor profile.");
            }

            if (profile == null)
            {
                return OperationResult.Fail("Please fill in your profile details.");
            }

            profile.UserId = SessionHelper.GetCurrentUserID();
            string validationError = CleanAndValidate(profile);
            if (validationError != null)
            {
                return OperationResult.Fail(validationError);
            }

            string currentStatus;
            try
            {
                currentStatus = _tutorRepository.GetApplicationStatus(profile.UserId);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.UpdateProfile status check failure", exception);
                return OperationResult.Fail("A system error occurred while saving your profile. Please try again.");
            }

            if (currentStatus != TutorStatuses.Pending && currentStatus != TutorStatuses.Approved)
            {
                return OperationResult.Fail("Submit a tutor application before editing your profile.");
            }

            if (!_tutorRepository.UpdateProfileDetails(profile))
            {
                return OperationResult.Fail("A system error occurred while saving your profile. Please try again.");
            }

            SessionHelper.UpdateFullName(profile.FullName);
            new AuditManager().RecordActivity(profile.UserId, "UPDATE_TUTOR_PROFILE", "Updated tutor profile details.");

            return OperationResult.Ok("Your profile has been updated.");
        }

        /// <summary>
        /// Replaces the signed-in user's profile photo with an uploaded JPG, PNG or WebP image (max 2 MB).
        /// </summary>
        public OperationResult UpdateAvatar(string fileName, int contentLength, Stream content)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor) && !SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult.Fail("Unauthorized: Only tutors can change their tutor profile photo.");
            }

            if (content == null || contentLength <= 0)
            {
                return OperationResult.Fail("Choose an image to upload first.");
            }

            if (contentLength > MaxAvatarBytes)
            {
                return OperationResult.Fail("The photo must be 2 MB or smaller.");
            }

            if (!ImageUploadHelper.HasAllowedExtension(fileName))
            {
                return OperationResult.Fail("The photo must be a JPG, PNG or WebP image.");
            }

            byte[] data = new byte[contentLength];
            int bytesRead = 0;
            while (bytesRead < contentLength)
            {
                int read = content.Read(data, bytesRead, contentLength - bytesRead);
                if (read == 0)
                {
                    break;
                }

                bytesRead += read;
            }

            // Trust the file's contents, not its name: a renamed file is rejected here
            string extension = ImageUploadHelper.DetectImageExtension(data);
            if (bytesRead != contentLength || extension == null)
            {
                return OperationResult.Fail("The photo must be a JPG, PNG or WebP image.");
            }

            int userId = SessionHelper.GetCurrentUserID();
            string oldAvatarUrl;
            string newAvatarUrl;

            try
            {
                TutorProfile profile = _tutorRepository.GetProfile(userId);
                oldAvatarUrl = profile == null ? null : profile.AvatarUrl;
                newAvatarUrl = ImageUploadHelper.SaveImage(data, extension, ImageUploadHelper.AvatarFolder, "u" + userId);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.UpdateAvatar failure", exception);
                return OperationResult.Fail("A system error occurred while saving your photo. Please try again.");
            }

            if (!_userRepository.UpdateAvatarUrl(userId, newAvatarUrl))
            {
                ImageUploadHelper.DeleteImage(newAvatarUrl, ImageUploadHelper.AvatarFolder);
                return OperationResult.Fail("A system error occurred while saving your photo. Please try again.");
            }

            ImageUploadHelper.DeleteImage(oldAvatarUrl, ImageUploadHelper.AvatarFolder);
            new AuditManager().RecordActivity(userId, "UPDATE_AVATAR", "Updated profile photo.");

            return OperationResult.Ok("Your profile photo has been updated.");
        }

        /// <summary>
        /// Removes the signed-in user's profile photo, so their initials are shown instead.
        /// </summary>
        public OperationResult RemoveAvatar()
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Tutor) && !SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult.Fail("Unauthorized: Only tutors can change their tutor profile photo.");
            }

            int userId = SessionHelper.GetCurrentUserID();
            string oldAvatarUrl;

            try
            {
                TutorProfile profile = _tutorRepository.GetProfile(userId);
                oldAvatarUrl = profile == null ? null : profile.AvatarUrl;
            }
            catch (Exception exception)
            {
                LogHelper.LogError("TutorManager.RemoveAvatar failure", exception);
                return OperationResult.Fail("A system error occurred while removing your photo. Please try again.");
            }

            if (oldAvatarUrl == null)
            {
                return OperationResult.Ok("You do not have a profile photo.");
            }

            if (!_userRepository.UpdateAvatarUrl(userId, null))
            {
                return OperationResult.Fail("A system error occurred while removing your photo. Please try again.");
            }

            ImageUploadHelper.DeleteImage(oldAvatarUrl, ImageUploadHelper.AvatarFolder);
            new AuditManager().RecordActivity(userId, "REMOVE_AVATAR", "Removed profile photo.");

            return OperationResult.Ok("Your profile photo has been removed.");
        }

        // Trims the form values, then returns the first problem with them, or null if they are valid
        private static string CleanAndValidate(TutorProfile application)
        {
            application.FullName = Clean(application.FullName);
            application.Specialty = Clean(application.Specialty);
            application.Qualifications = Clean(application.Qualifications);
            application.PortfolioUrl = Clean(application.PortfolioUrl);
            application.Bio = Clean(application.Bio);
            application.CurriculumPitch = Clean(application.CurriculumPitch);

            if (application.FullName == null || application.Specialty == null || application.Qualifications == null
                || application.PortfolioUrl == null || application.Bio == null || application.CurriculumPitch == null)
            {
                return "All fields are required.";
            }

            if (application.FullName.Length > MaxFullNameLength)
            {
                return "Full name must be " + MaxFullNameLength + " characters or fewer.";
            }

            if (application.Specialty.Length > MaxSpecialtyLength)
            {
                return "Specialty must be " + MaxSpecialtyLength + " characters or fewer.";
            }

            if (application.Qualifications.Length > MaxLongTextLength
                || application.Bio.Length > MaxLongTextLength
                || application.CurriculumPitch.Length > MaxLongTextLength)
            {
                return "Qualifications, biography and curriculum pitch must each be " + MaxLongTextLength + " characters or fewer.";
            }

            if (application.PortfolioUrl.Length > MaxUrlLength || !IsWebUrl(application.PortfolioUrl))
            {
                return "Portfolio link must be a valid web address starting with http:// or https://.";
            }

            return null;
        }

        /// <summary>
        /// True for an absolute http or https address. Used before showing a stored link as a clickable link.
        /// </summary>
        public static bool IsWebUrl(string value)
        {
            Uri uri;
            return !string.IsNullOrWhiteSpace(value)
                && Uri.TryCreate(value, UriKind.Absolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                && !string.IsNullOrEmpty(uri.Host);
        }

        // Trims text and turns empty input into null
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
