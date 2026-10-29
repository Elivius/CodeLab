using System;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Helpers
{
    /// <summary>
    /// The single place that knows who is signed in. Use these methods instead of
    /// reading Session["..."] keys directly in code-behind or the BLL.
    /// </summary>
    public static class SessionHelper
    {
        private const string UserIdKey = "UserID";
        private const string UsernameKey = "Username";
        private const string FullNameKey = "FullName";
        private const string RoleKey = "Role";

        /// <summary>
        /// Signs the user in: issues the login cookie and fills the session.
        /// Call this from Login.aspx.cs after UserManager.Authenticate succeeds.
        /// </summary>
        public static void SignIn(User user)
        {
            HttpContext context = HttpContext.Current;

            // The ticket carries the user's id and role so they survive a session timeout
            string userData = user.Id + "|" + user.Role;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                user.Username,
                DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout),
                false,
                userData,
                FormsAuthentication.FormsCookiePath);

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.HttpOnly = true;
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            context.Response.Cookies.Add(cookie);

            context.Session[UserIdKey] = user.Id;
            context.Session[UsernameKey] = user.Username;
            context.Session[FullNameKey] = user.FullName;
            context.Session[RoleKey] = user.Role;
        }

        public static void SignOut()
        {
            HttpContext context = HttpContext.Current;

            FormsAuthentication.SignOut();
            if (context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }
        }

        public static bool IsLoggedIn()
        {
            return GetCurrentUserID() > 0;
        }

        /// <summary>
        /// Returns the signed-in user's id, or 0 if nobody is signed in.
        /// </summary>
        public static int GetCurrentUserID()
        {
            object value = GetValue(UserIdKey);
            return value == null ? 0 : (int)value;
        }

        public static string GetCurrentUsername()
        {
            return GetValue(UsernameKey) as string;
        }

        public static string GetCurrentFullName()
        {
            return GetValue(FullNameKey) as string;
        }

        /// <summary>
        /// Updates the name shown in the header after the signed-in user changes their full name.
        /// </summary>
        public static void UpdateFullName(string fullName)
        {
            HttpContext context = HttpContext.Current;
            if (context != null && context.Session != null && !string.IsNullOrEmpty(fullName))
            {
                context.Session[FullNameKey] = fullName;
            }
        }

        public static string GetCurrentRole()
        {
            return GetValue(RoleKey) as string;
        }

        /// <summary>
        /// True if the signed-in user has exactly this role. Use the UserRoles constants.
        /// See if the current page user is in a role, e.g. SessionHelper.IsUserInRole(UserRoles.Admin)
        /// Role specific behavior
        /// </summary>
        public static bool IsUserInRole(string role)
        {
            string currentRole = GetCurrentRole();
            return currentRole != null && string.Equals(currentRole, role, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads the role out of a login ticket. Used by Global.asax.
        /// On every request, reads the role from the cookie and attaches it to the user
        /// </summary>
        public static string GetRoleFromTicket(FormsAuthenticationTicket ticket)
        {
            string[] parts = SplitUserData(ticket);
            return parts == null ? string.Empty : parts[1];
        }

        private static object GetValue(string key)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null || !context.Request.IsAuthenticated)
            {
                return null;
            }

            if (context.Session[UserIdKey] == null)
            {
                RestoreFromTicket(context);
            }

            return context.Session[key];
        }

        // The session can expire before the login cookie does; rebuild it from the ticket when that happens
        private static void RestoreFromTicket(HttpContext context)
        {
            FormsIdentity identity = context.User.Identity as FormsIdentity;
            if (identity == null)
            {
                return;
            }

            string[] parts = SplitUserData(identity.Ticket);
            int userId;
            if (parts == null || !int.TryParse(parts[0], out userId))
            {
                return;
            }

            HttpSessionState session = context.Session;
            session[UserIdKey] = userId;
            session[UsernameKey] = identity.Name;
            session[FullNameKey] = new UserManager().GetFullName(identity.Name) ?? identity.Name;
            session[RoleKey] = parts[1];
        }

        private static string[] SplitUserData(FormsAuthenticationTicket ticket)
        {
            if (ticket == null || string.IsNullOrEmpty(ticket.UserData))
            {
                return null;
            }

            string[] parts = ticket.UserData.Split('|');
            return parts.Length == 2 ? parts : null;
        }
    }
}
