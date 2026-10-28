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
            Response.Redirect(FormsAuthentication.GetRedirectUrl(result.Data.Username, false));
        }
    }
}
