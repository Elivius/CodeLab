using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// Admin management of accounts: find, create and edit them, change roles, suspend, reactivate and delete.
    ///
    /// An admin can never suspend, delete or change the role of their own account. That also protects
    /// the last active admin: whoever makes the change is an active admin, and they stay one.
    /// </summary>
    public class UserAdminManager
    {
        /// <summary>The rule the learner profile page uses, so these accounts can be saved there too.</summary>
        public const string UsernamePattern = @"^[a-zA-Z0-9_]{3,50}$";

        private const int MaxFullNameLength = 120;
        private const int MaxEmailLength = 255;
        private const int MaxPasswordLength = 128;

        private const string UnauthorizedMessage = "Unauthorized: Only administrators can manage users.";

        private readonly UserAdminRepository _repository = new UserAdminRepository();
        private readonly UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Returns every account, newest first, narrowed to those matching the search text when there is one.
        /// </summary>
        public OperationResult<List<AdminUserRow>> GetUsers(string search)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<List<AdminUserRow>>.Fail(UnauthorizedMessage);
            }

            try
            {
                string searchText = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
                return OperationResult<List<AdminUserRow>>.Ok(_repository.GetUsers(searchText));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserAdminManager.GetUsers failure", exception);
                return OperationResult<List<AdminUserRow>>.Fail("The users could not be loaded. Please try again.");
            }
        }

        // Every action loads the account here first, so every action gets the admin check
        public OperationResult<AdminUserRow> GetUser(int userId)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<AdminUserRow>.Fail(UnauthorizedMessage);
            }

            try
            {
                AdminUserRow user = _repository.GetById(userId);
                return user == null
                    ? OperationResult<AdminUserRow>.Fail("This account could not be found. It may have been deleted.")
                    : OperationResult<AdminUserRow>.Ok(user);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserAdminManager.GetUser failure", exception);
                return OperationResult<AdminUserRow>.Fail("The account could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// Creates an Active account with any role. account carries FullName, Username, Email and Role.
        /// On success, Data holds the new user's id. A new tutor still applies to teach from their Profile
        /// page, and an admin approves it like any other application.
        /// </summary>
        public OperationResult<int> Create(User account, string password, string confirmPassword)
        {
            if (!SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return OperationResult<int>.Fail(UnauthorizedMessage);
            }

            if (account == null)
            {
                return OperationResult<int>.Fail("All fields are required.");
            }

            account.FullName = (account.FullName ?? string.Empty).Trim();
            account.Username = (account.Username ?? string.Empty).Trim();
            account.Email = (account.Email ?? string.Empty).Trim();

            string detailsError = ValidateDetails(account.FullName, account.Email, account.Role);
            if (detailsError != null)
            {
                return OperationResult<int>.Fail(detailsError);
            }

            if (!Regex.IsMatch(account.Username, UsernamePattern))
            {
                return OperationResult<int>.Fail("Username must be 3 to 50 letters, numbers or underscores.");
            }

            if (string.IsNullOrEmpty(password) || password.Length > MaxPasswordLength
                || !Regex.IsMatch(password, UserManager.PasswordPattern))
            {
                return OperationResult<int>.Fail("Password must be 8 to " + MaxPasswordLength + " characters and include a letter, a number and a symbol.");
            }

            if (password != confirmPassword)
            {
                return OperationResult<int>.Fail("Passwords do not match.");
            }

            UserManager userManager = new UserManager();
            if (userManager.IsUsernameTaken(account.Username))
            {
                return OperationResult<int>.Fail("That username is already taken.");
            }

            if (userManager.IsEmailTaken(account.Email))
            {
                return OperationResult<int>.Fail("That email is already registered.");
            }

            account.PasswordHash = PasswordHelper.HashPassword(password);
            account.Status = UserStatuses.Active;

            int userId = _userRepository.Insert(account, null);
            if (userId == 0)
            {
                return OperationResult<int>.Fail("The account could not be created. Please try again.");
            }

            Record("CREATE_USER", "Created the " + account.Role + " account '" + account.Username + "' (user " + userId + ").");
            return OperationResult<int>.Ok(userId);
        }

        /// <summary>
        /// Saves an account's full name, email and role. A role change logs the user out on their next
        /// click, so they log in again with the new role.
        /// </summary>
        public OperationResult Update(int userId, string fullName, string email, string role)
        {
            OperationResult<AdminUserRow> user = GetUser(userId);
            if (!user.Success)
            {
                return user;
            }

            AdminUserRow current = user.Data;
            fullName = (fullName ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();

            string detailsError = ValidateDetails(fullName, email, role);
            if (detailsError != null)
            {
                return OperationResult.Fail(detailsError);
            }

            bool roleChanged = role != current.Role;
            if (roleChanged && IsSelf(userId))
            {
                return OperationResult.Fail("You cannot change the role of your own account.");
            }

            if (roleChanged && current.CourseCount > 0)
            {
                return OperationResult.Fail("This tutor still teaches " + current.CourseCount
                    + (current.CourseCount == 1 ? " course" : " courses") + ", which would be left without a tutor. Delete those courses first.");
            }

            List<string> changes = new List<string>();
            if (fullName != current.FullName)
            {
                changes.Add("name changed to '" + fullName + "'");
            }

            if (email != current.Email)
            {
                changes.Add("email changed to " + email);
            }

            if (roleChanged)
            {
                changes.Add("role changed from " + current.Role + " to " + role);
            }

            if (changes.Count == 0)
            {
                return OperationResult.Ok("There were no changes to save.");
            }

            try
            {
                if (_repository.IsEmailTakenByOther(userId, email))
                {
                    return OperationResult.Fail("That email is already registered.");
                }
            }
            catch (Exception exception)
            {
                LogHelper.LogError("UserAdminManager.Update failure", exception);
                return OperationResult.Fail("The account could not be saved. Please try again.");
            }

            if (!_repository.UpdateDetails(userId, fullName, email, role))
            {
                return OperationResult.Fail("The account could not be saved. Refresh the page and try again.");
            }

            if (IsSelf(userId))
            {
                SessionHelper.UpdateFullName(fullName);
            }

            Record("UPDATE_USER", "Updated " + Describe(current) + ": " + string.Join(", ", changes) + ".");

            return OperationResult.Ok(roleChanged
                ? "Saved. " + fullName + " is logged out on their next click, then logs in with the new role."
                : "Saved.");
        }

        /// <summary>
        /// Stops an account logging in. Global.asax logs the user out on their next click.
        /// </summary>
        public OperationResult Suspend(int userId)
        {
            OperationResult<AdminUserRow> user = GetUser(userId);
            if (!user.Success)
            {
                return user;
            }

            if (IsSelf(userId))
            {
                return OperationResult.Fail("You cannot suspend your own account.");
            }

            if (!user.Data.CanSuspend)
            {
                return WrongStatus(user.Data, "suspended");
            }

            if (!_repository.Suspend(userId))
            {
                return OperationResult.Fail("The account could not be suspended. Refresh the page and try again.");
            }

            Record("SUSPEND_USER", "Suspended " + Describe(user.Data) + ".");
            return OperationResult.Ok(user.Data.FullName + " is suspended. They are logged out on their next click and cannot log in.");
        }

        public OperationResult Reactivate(int userId)
        {
            OperationResult<AdminUserRow> user = GetUser(userId);
            if (!user.Success)
            {
                return user;
            }

            if (!user.Data.CanReactivate)
            {
                return WrongStatus(user.Data, "reactivated");
            }

            if (!_repository.Reactivate(userId))
            {
                return OperationResult.Fail("The account could not be reactivated. Refresh the page and try again.");
            }

            Record("REACTIVATE_USER", "Reactivated " + Describe(user.Data) + ".");
            return OperationResult.Ok(user.Data.FullName + " is active again and can log in.");
        }

        /// <summary>
        /// Deletes an account with no activity. Logging in and registering both create audit entries, so in
        /// practice this is for an account an admin created by mistake. Everyone else can be suspended.
        /// </summary>
        public OperationResult Delete(int userId)
        {
            OperationResult<AdminUserRow> user = GetUser(userId);
            if (!user.Success)
            {
                return user;
            }

            if (IsSelf(userId))
            {
                return OperationResult.Fail("You cannot delete your own account.");
            }

            if (user.Data.HasActivity)
            {
                return OperationResult.Fail("This account has activity, so it cannot be deleted. Suspend it instead.");
            }

            if (!_repository.Delete(userId))
            {
                return OperationResult.Fail("The account could not be deleted. Refresh the page and try again.");
            }

            Record("DELETE_USER", "Deleted " + Describe(user.Data) + ", " + user.Data.Email + ".");
            return OperationResult.Ok(user.Data.FullName + "'s account was deleted.");
        }

        private static string ValidateDetails(string fullName, string email, string role)
        {
            if (fullName.Length == 0 || email.Length == 0)
            {
                return "All fields are required.";
            }

            if (fullName.Length > MaxFullNameLength)
            {
                return "Full name must be " + MaxFullNameLength + " characters or fewer.";
            }

            if (email.Length > MaxEmailLength || !Regex.IsMatch(email, UserManager.EmailPattern))
            {
                return "Please enter a valid email address.";
            }

            return role == UserRoles.Learner || role == UserRoles.Tutor || role == UserRoles.Admin
                ? null
                : "Please choose a role.";
        }

        private static bool IsSelf(int userId)
        {
            return userId == SessionHelper.GetCurrentUserID();
        }

        // A clear message when the status changed first: a double click, or a second admin in another tab
        private static OperationResult WrongStatus(AdminUserRow user, string action)
        {
            return OperationResult.Fail("This account is " + user.Status + ", so it cannot be " + action + ".");
        }

        private static string Describe(AdminUserRow user)
        {
            return "'" + user.Username + "' (user " + user.Id + ")";
        }

        private static void Record(string action, string details)
        {
            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), action, details);
        }
    }
}
