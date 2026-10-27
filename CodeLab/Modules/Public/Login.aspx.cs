using System;
using System.Web.Security;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Public
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Someone already signed in lands here when they open a page their role cannot access
            if (!IsPostBack && SessionHelper.IsLoggedIn())
            {
                Response.Redirect("~/");
            }

            // Register sends new users here
            string registered = IsPostBack ? null : Request.QueryString["registered"];
            if (registered == "learner")
            {
                lblSuccess.Text = "Account created. Please log in.";
                lblSuccess.Visible = true;
            }
            else if (registered == "tutor")
            {
                lblSuccess.Text = "Account created. Your tutor application is waiting for admin review. Log in to track its status.";
                lblSuccess.Visible = true;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            UserManager userManager = new UserManager();
            OperationResult<User> result = userManager.Authenticate(txtUsername.Text.Trim(), txtPassword.Text);

            if (!result.Success)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                return;
            }

            SessionHelper.SignIn(result.Data);

            // Someone sent here from a protected page goes back to it (GetRedirectUrl only allows this site's
            // own pages); everyone else goes to their role's home page
            bool hasReturnUrl = !string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]);
            Response.Redirect(hasReturnUrl
                ? FormsAuthentication.GetRedirectUrl(result.Data.Username, false)
                : GetHomeUrl(result.Data.Role));
        }

        private static string GetHomeUrl(string role)
        {
            switch (role)
            {
                case UserRoles.Tutor: return "~/Modules/Tutor/Dashboard";
                case UserRoles.Learner: return "~/Modules/Learner/Dashboard";
                case UserRoles.Admin: return "~/Modules/Admin/AuditLogs";
                default: return "~/";
            }
        }
    }
}
