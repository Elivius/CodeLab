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

        /// <summary>
        /// Email format check, shared by the Register and admin UserEdit validators, Register(), the learner
        /// profile and UserAdminManager. Letters, numbers and . _ % + - before the @, no dots at the start or
        /// end or side by side, then a domain ending in a letters-only part of at least 2, such as .com.
        /// </summary>
        public const string EmailPattern = @"^[A-Za-z0-9_%+-]+(\.[A-Za-z0-9_%+-]+)*@([A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}$";

        /// <summary>Password rule from the proposal: at least 8 characters with at least one number.</summary>
        public const string PasswordPattern = @"^(?=.*\d).{8,}$";

        private readonly UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Checks a username and password. On success, Data holds the matching user.
        /// </summary>
        public OperationResult<User> GetLearnerProfile()
        {
            if(!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner")) return OperationResult<User>.Fail("Learner access required.");
            try {
                var user=_userRepository.GetLearnerProfile(SessionHelper.GetCurrentUserID());
                if(user==null) return OperationResult<User>.Fail("Account unavailable.");
                user.PasswordHash=null;
                return OperationResult<User>.Ok(user);
            } catch(Exception ex) { LogHelper.LogError("Loading learner profile",ex); return OperationResult<User>.Fail("Profile temporarily unavailable."); }
        }
        public OperationResult SaveLearnerProfile(string name,string username,string email)
        {
            if(!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner")) return OperationResult.Fail("Learner access required.");
            name=(name??"").Trim(); username=(username??"").Trim(); email=(email??"").Trim();
            if(name.Length==0 || name.Length>120 || !Regex.IsMatch(username,@"^[a-zA-Z0-9_]{3,50}$") || email.Length>255 || !Regex.IsMatch(email,EmailPattern)) return OperationResult.Fail("Check your name, username, email and field lengths.");
            try {
                var user=_userRepository.GetLearnerProfile(SessionHelper.GetCurrentUserID());
                if(user==null) return OperationResult.Fail("Account unavailable.");

                user.FullName=name;user.Username=username;user.Email=email;
                if(!_userRepository.SaveLearnerProfile(user,user.PasswordHash)) return OperationResult.Fail("Account changed. Please sign in again.");
                SessionHelper.SignIn(user);
                return OperationResult.Ok();
            } catch(System.Data.SqlClient.SqlException ex) {
                if(ex.Number==2601 || ex.Number==2627) return OperationResult.Fail("That username or email is already in use.");
                LogHelper.LogError("Saving learner profile",ex);return OperationResult.Fail("Profile could not be saved.");
            }
        }
        public OperationResult ChangeLearnerPassword(string current,string password,string confirm)
        {
            if(!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner")) return OperationResult.Fail("Learner access required.");
            if(string.IsNullOrEmpty(password) || password.Length>128 || !Regex.IsMatch(password,PasswordPattern) || password!=confirm) return OperationResult.Fail("Use 8–128 characters with a number and matching confirmation.");
            try {
                var user=_userRepository.GetLearnerProfile(SessionHelper.GetCurrentUserID());
                if(user==null || !PasswordHelper.VerifyPassword(current,user.PasswordHash)) return OperationResult.Fail("Current password is incorrect.");
                return _userRepository.ChangeLearnerPassword(user.Id,user.PasswordHash,PasswordHelper.HashPassword(password)) ? OperationResult.Ok() : OperationResult.Fail("Account changed. Please sign in again.");
            } catch(Exception ex) { LogHelper.LogError("Changing learner password",ex);return OperationResult.Fail("Password could not be updated."); }
        }
        public OperationResult UploadLearnerAvatar(string fileName,int length,System.IO.Stream stream)
        {
            if(!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner")) return OperationResult.Fail("Learner access required.");
            byte[] data;string extension;
            string error=ImageUploadHelper.ReadValidatedImage(fileName,length,stream,2*1024*1024,"photo",out data,out extension);
            if(error!=null) return OperationResult.Fail(error);
            string newUrl=null;
            try {
                var user=_userRepository.GetLearnerProfile(SessionHelper.GetCurrentUserID());
                if(user==null) return OperationResult.Fail("Account unavailable.");
                newUrl=ImageUploadHelper.SaveImage(data,extension,ImageUploadHelper.AvatarFolder,"u"+user.Id);
                if(!_userRepository.UpdateAvatarUrl(user.Id,newUrl)) { ImageUploadHelper.DeleteImage(newUrl,ImageUploadHelper.AvatarFolder);return OperationResult.Fail("Photo could not be saved."); }
                ImageUploadHelper.DeleteImage(user.AvatarUrl,ImageUploadHelper.AvatarFolder);
                return OperationResult.Ok();
            } catch(Exception ex) { if(newUrl!=null) ImageUploadHelper.DeleteImage(newUrl,ImageUploadHelper.AvatarFolder);LogHelper.LogError("Uploading learner avatar",ex);return OperationResult.Fail("Photo could not be uploaded. Please try again."); }
        }
        public OperationResult SetLearnerAvatar(int preset)
        {
            if(!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner")) return OperationResult.Fail("Learner access required.");
            string[] photos={"photo-1535713875002-d1d0cf377fde","photo-1570295999919-56ceb5ecca61","photo-1534528741775-53994a69daeb","photo-1507003211169-0a1dd7228f2d","photo-1580489944761-15a19d654956","photo-1500648767791-00dcc994a43e"};
            if(preset<0 || preset>photos.Length) return OperationResult.Fail("Choose a valid avatar.");
            try {
                var user=_userRepository.GetLearnerProfile(SessionHelper.GetCurrentUserID());
                if(user==null) return OperationResult.Fail("Account unavailable.");
                string url=preset==0 ? null : "https://images.unsplash.com/"+photos[preset-1]+"?w=150&auto=format&fit=crop&q=80";
                if(!_userRepository.UpdateAvatarUrl(user.Id,url)) return OperationResult.Fail("Avatar could not be saved.");
                ImageUploadHelper.DeleteImage(user.AvatarUrl,ImageUploadHelper.AvatarFolder);
                return OperationResult.Ok();
            } catch(Exception ex) { LogHelper.LogError("Saving learner avatar",ex);return OperationResult.Fail("Avatar could not be saved."); }
        }
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
            }
        }

        /// <summary>
        /// False if an admin has suspended or deleted this account, or changed its role, since the login
        /// cookie was issued. role is the role in the cookie. Global.asax calls this on every page request.
        /// </summary>
        public bool IsLoginStillValid(int userId, string role)
        {
            try
            {
                User account = _userRepository.GetAccountState(userId);
                return account != null
                    && account.Status == UserStatuses.Active
                    && string.Equals(account.Role, role, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception exception)
            {
                // A database problem must not sign everyone out; the page itself will report it
                LogHelper.LogError("UserManager.IsLoginStillValid failure", exception);
                return true;
            }
        }

        /// <summary>
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
