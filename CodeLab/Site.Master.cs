using System;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab
{
    public partial class SiteMaster : MasterPage
    {
        private const string TutorHome = "~/Modules/Tutor/Dashboard";

        protected void Page_Init(object sender, EventArgs e)
        {
            // A signed-in tutor works inside the Tutor area: the landing page and other guest
            // pages (including Login) send them to their dashboard instead
            if (SessionHelper.IsUserInRole(UserRoles.Tutor) && IsGuestPage())
            {
                Response.Redirect(TutorHome);
            }
        }

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

            bool isTutor = SessionHelper.IsUserInRole(UserRoles.Tutor);

            phAdminNav.Visible = SessionHelper.IsUserInRole(UserRoles.Admin);
            phTutorNav.Visible = isTutor;
            phLearnerNav.Visible = SessionHelper.IsUserInRole(UserRoles.Learner);

            // Tutors navigate with their sidebar, and the logo takes them to their dashboard
            if (isTutor)
            {
                phPublicNav.Visible = false;
                lnkBrand.HRef = TutorHome;
                lnkFooterBrand.HRef = TutorHome;
            }
        }

        // Pages outside Modules/, plus Modules/Public/ (Login, Register)
        private bool IsGuestPage()
        {
            string path = Page.AppRelativeVirtualPath ?? string.Empty;
            return !path.StartsWith("~/Modules/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("~/Modules/Public/", StringComparison.OrdinalIgnoreCase);
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
