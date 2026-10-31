using System;
using System.Security.Principal;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using CodeLab.BLL;
using CodeLab.Helpers;

namespace CodeLab
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // 1. Read the cookie
        // 2. Attaches the role stored in the login ticket to the request, so the
        // <allow roles="..."> rules in the Modules/*/Web.config files can see it.
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Context.User == null)
            {
                return;
            }

            FormsIdentity identity = Context.User.Identity as FormsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return;
            }

            string role = SessionHelper.GetRoleFromTicket(identity.Ticket);
            Context.User = new GenericPrincipal(identity, new string[] { role });
        }

        // The login cookie cannot know that an admin has since suspended or deleted this account, or
        // changed its role, so check the database once per page. This runs here rather than in
        // Application_AuthenticateRequest because signing out needs the session, which is loaded by now.
        // Requests with no session (images, scripts, styles) are skipped.
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            FormsIdentity identity = Context.User == null ? null : Context.User.Identity as FormsIdentity;
            if (identity == null || !identity.IsAuthenticated || Context.Session == null)
            {
                return;
            }

            int userId = SessionHelper.GetUserIdFromTicket(identity.Ticket);
            string role = SessionHelper.GetRoleFromTicket(identity.Ticket);
            if (new UserManager().IsLoginStillValid(userId, role))
            {
                return;
            }

            SessionHelper.SignOut();
            Response.Redirect("~/Modules/Public/Login?reason=changed", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

            // A mistyped URL is not a fault in the system; keep it out of the error log
            HttpException httpException = exception as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404)
            {
                return;
            }

            LogHelper.LogError("Unhandled exception at " + Request.RawUrl, exception);
        }
    }
}
