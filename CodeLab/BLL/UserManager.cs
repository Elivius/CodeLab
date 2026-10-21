using System;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class UserManager
    {
        // The same message for every credential failure, so the page does not reveal which usernames exist
        private const string InvalidLoginMessage = "Invalid username or password.";

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
        /// Records that the given user signed out. The page clears the session afterwards.
        /// </summary>
        public void RecordLogout(int userId)
        {
            AuditManager auditManager = new AuditManager();
            auditManager.RecordActivity(userId, "LOGOUT", "Signed out.");
        }
    }
}
