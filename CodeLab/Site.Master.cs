using System;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isLoggedIn = SessionHelper.IsLoggedIn();
            phSignedIn.Visible = isLoggedIn;
            phSignedOut.Visible = !isLoggedIn;

            if (!isLoggedIn)
            {
                return;
            }

            lblCurrentUser.Text = Server.HtmlEncode(SessionHelper.GetCurrentFullName() + " (" + SessionHelper.GetCurrentRole() + ")");

            phAdminNav.Visible = SessionHelper.IsUserInRole(UserRoles.Admin);
            phTutorNav.Visible = SessionHelper.IsUserInRole(UserRoles.Tutor);
            phLearnerNav.Visible = SessionHelper.IsUserInRole(UserRoles.Learner);
            phPublicNav.Visible = !phLearnerNav.Visible;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            UserManager userManager = new UserManager();
            userManager.RecordLogout(SessionHelper.GetCurrentUserID());

            SessionHelper.SignOut();
            Response.Redirect("~/Modules/Public/Login");
        }
    }
}
