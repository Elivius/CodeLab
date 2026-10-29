using System;
using System.Text.RegularExpressions;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class UserManager
    {
        // The same message for every credential failure, so the page does not reveal which usernames exist
        private const string InvalidLoginMessage = "Invalid username or password.";

        private const int MaxFullNameLength = 120;
        private const int MaxUsernameLength = 50;
        private const int MaxEmailLength = 255;

        /// <summary>Email format check, shared by the Register page's validators and Register().</summary>
        public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        /// <summary>Password rule from the proposal: at least 8 characters with at least one number.</summary>
        public const string PasswordPattern = @"^(?=.*\d).{8,}$";

        private readonly UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Checks a username and password. On success, Data holds the matching user.
        /// </summary>
        public OperationResult<User> Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return OperationResult<User>.Fail("Username and password are required.");
            }

            User user;
            try
            {
                user = _userRepository.GetByUsername(username.Trim());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserManager.Authenticate failure", exception);
                return OperationResult<User>.Fail("A system error occurred while signing in. Please try again.");
            }

            if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                return OperationResult<User>.Fail(InvalidLoginMessage);
            }

            if (user.Status == UserStatuses.Suspended)
            {
                return OperationResult<User>.Fail("This account has been suspended. Please contact an administrator.");
            }

            if (user.Status != UserStatuses.Active)
            {
                return OperationResult<User>.Fail("This account has not been activated yet.");
            }

            _userRepository.UpdateLastLogin(user.Id);

            AuditManager auditManager = new AuditManager();
            auditManager.RecordActivity(user.Id, "LOGIN", "Signed in.");

            return OperationResult<User>.Ok(user);
        }

        /// <summary>
        /// Returns the display name for a username, or null if it cannot be found.
        /// </summary>
        public string GetFullName(string username)
        {
            try
            {
                User user = _userRepository.GetByUsername(username);
                return user == null ? null : user.FullName;
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserManager.GetFullName failure", exception);
                return null;
        /// Creates a Learner or Tutor account. account carries FullName, Username, Email and Role;
        /// application is the tutor application (null for learners). Tutors start Pending but can log in.
        /// </summary>
        public OperationResult Register(User account, string password, string confirmPassword, TutorProfile application)
        {
            if (account == null)
            {
                return OperationResult.Fail("All fields are required.");
            }

            account.FullName = (account.FullName ?? string.Empty).Trim();
            account.Username = (account.Username ?? string.Empty).Trim();
            account.Email = (account.Email ?? string.Empty).Trim();

            // The role comes from the page's link, so never trust it: anything but Tutor makes a Learner
            account.Role = account.Role == UserRoles.Tutor ? UserRoles.Tutor : UserRoles.Learner;

            if (account.FullName.Length == 0 || account.Username.Length == 0 || account.Email.Length == 0
                || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return OperationResult.Fail("All fields are required.");
            }

            if (account.FullName.Length > MaxFullNameLength)
            {
                return OperationResult.Fail("Full name must be " + MaxFullNameLength + " characters or fewer.");
            }

            if (account.Username.Length > MaxUsernameLength)
            {
                return OperationResult.Fail("Username must be " + MaxUsernameLength + " characters or fewer.");
            }

            if (account.Email.Length > MaxEmailLength || !Regex.IsMatch(account.Email, EmailPattern))
            {
                return OperationResult.Fail("Please enter a valid email address.");
            }

            if (!Regex.IsMatch(password, PasswordPattern))
            {
                return OperationResult.Fail("Password must be at least 8 characters and contain at least one number.");
            }

            if (password != confirmPassword)
            {
                return OperationResult.Fail("Passwords do not match.");
            }

            // A tutor account is never created without a valid application
            if (account.Role == UserRoles.Tutor)
            {
                if (application == null)
                {
                    return OperationResult.Fail("Please fill in the tutor application.");
                }

                application.FullName = account.FullName;
                string applicationError = TutorManager.CleanAndValidate(application);
                if (applicationError != null)
                {
                    return OperationResult.Fail(applicationError);
                }
            }

            string takenMessage = GetTakenMessage(account);
            if (takenMessage != null)
            {
                return OperationResult.Fail(takenMessage);
            }

            account.PasswordHash = PasswordHelper.HashPassword(password);
            account.Status = UserStatuses.Active;

            int userId = _userRepository.Insert(account, account.Role == UserRoles.Tutor ? application : null);
            if (userId == 0)
            {
                // Someone may have taken the username or email since the check above
                return OperationResult.Fail(GetTakenMessage(account)
                    ?? "A system error occurred while creating your account. Please try again.");
            }

            AuditManager auditManager = new AuditManager();
            auditManager.RecordActivity(userId, "REGISTER", "Registered as " + account.Role + ".");
            if (account.Role == UserRoles.Tutor)
            {
                // Same entry TutorManager.SubmitApplication records, so admins see one kind of application entry
                auditManager.RecordActivity(userId, "SUBMIT_TUTOR_APPLICATION",
                    "Submitted a tutor application for review (specialty: " + application.Specialty + ").");
            }

            return OperationResult.Ok("Account created.");
        }

        /// <summary>
        /// True if the username is already used. Returns false if the check fails; Register() checks again.
        /// </summary>
        public bool IsUsernameTaken(string username)
        {
            try
            {
                return _userRepository.IsUsernameTaken((username ?? string.Empty).Trim());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserManager.IsUsernameTaken failure", exception);
                return false;
            }
        }

        /// <summary>
        /// True if the email is already used. Returns false if the check fails; Register() checks again.
        /// </summary>
        public bool IsEmailTaken(string email)
        {
            try
            {
                return _userRepository.IsEmailTaken((email ?? string.Empty).Trim());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserManager.IsEmailTaken failure", exception);
                return false;
            }
        }

        // The message for a username or email that is already used, or null if both are free
        private string GetTakenMessage(User account)
        {
            if (IsUsernameTaken(account.Username))
            {
                return "That username is already taken.";
            }

            return IsEmailTaken(account.Email) ? "That email is already registered." : null;
        }

        /// <summary>
        /// Records that the given user signed out. The page clears the session afterwards.
        /// </summary>
        public void RecordLogout(int userId)
        {
            AuditManager auditManager = new AuditManager();
            auditManager.RecordActivity(userId, "LOGOUT", "Signed out.");
        }
    }
}
