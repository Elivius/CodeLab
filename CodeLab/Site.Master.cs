using System;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // A signed-in user works inside their own module: the landing page and other guest
            // pages (including Login and Register) send them to their role's home page.
            // An unknown role gets "~/", which is a guest page itself, so it must not redirect.
            string homeUrl = SessionHelper.GetHomeUrl(SessionHelper.GetCurrentRole());
            if (homeUrl != "~/" && IsGuestPage())
            {
                Response.Redirect(homeUrl);
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

            phAdminNav.Visible = SessionHelper.IsUserInRole(UserRoles.Admin);
            phTutorNav.Visible = SessionHelper.IsUserInRole(UserRoles.Tutor);
            phLearnerNav.Visible = SessionHelper.IsUserInRole(UserRoles.Learner);

            // The guest links all lead to the landing page, which only sends a signed-in user
            // back to their home page, so hide them and point the logo straight there
            phPublicNav.Visible = false;
            string homeUrl = SessionHelper.GetHomeUrl(SessionHelper.GetCurrentRole());
            lnkBrand.HRef = homeUrl;
            lnkFooterBrand.HRef = homeUrl;
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
