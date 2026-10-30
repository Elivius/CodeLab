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
        // Someone already signed in never gets this far: Site.Master sends them to their home page
        protected void Page_Load(object sender, EventArgs e)
        {
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

            lblMessage.Visible = false;

            UserManager userManager = new UserManager();

            OperationResult<User> result =
                userManager.Authenticate(
                    txtUsername.Text.Trim(),
                    txtPassword.Text
                );

            if (!result.Success || result.Data == null)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                return;
            }

            // Create login session and authentication cookie
            SessionHelper.SignIn(result.Data);

            // Someone sent here from a protected page goes back to it (GetRedirectUrl only allows this site's
            // own pages); everyone else goes to their role's home page
            bool hasReturnUrl = !string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]);
            RedirectTo(hasReturnUrl
                ? FormsAuthentication.GetRedirectUrl(result.Data.Username, false)
                : SessionHelper.GetHomeUrl(result.Data.Role));
        }

        private void RedirectTo(string url)
        {
            Response.Redirect(ResolveUrl(url), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
