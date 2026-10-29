
using System;
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
            // Redirect users who are already logged in
            if (!IsPostBack && SessionHelper.IsLoggedIn())
            {
                RedirectByRole(SessionHelper.GetCurrentRole());
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

            // Use the authenticated user's role directly
            RedirectByRole(result.Data.Role);
        }

        private void RedirectByRole(string role)
        {
            string destination;

            switch ((role ?? "").Trim().ToLowerInvariant())
            {
                case "learner":
                    destination =
                        "~/Modules/Learner/Dashboard.aspx";
                    break;

                case "tutor":
                    destination = "~/";
                    break;

                case "admin":
                    destination = "~/";
                    break;

                default:
                    destination = "~/";
                    break;
            }

            Response.Redirect(
                ResolveUrl(destination),
                false
            );

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
