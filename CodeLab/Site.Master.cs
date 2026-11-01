using System;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab
{
    public partial class SiteMaster : MasterPage
    {
        protected string AccountAvatar;
        public string LearnerAvatarUrl { get { return AccountAvatar; } }
        protected string AccountInitial { get { string name=SessionHelper.GetCurrentFullName(); return string.IsNullOrWhiteSpace(name)?"?":System.Globalization.StringInfo.GetNextTextElement(name).ToUpperInvariant(); } }
        protected string AccountProfileUrl { get { return ResolveUrl(SessionHelper.IsUserInRole(UserRoles.Tutor)?"~/Modules/Tutor/Profile":"~/Modules/Learner/Profile"); } }

        // Admins have no profile page, so their account menu only offers Sign out
        protected bool HasProfilePage { get { return !SessionHelper.IsUserInRole(UserRoles.Admin); } }

        protected void Page_Init(object sender, EventArgs e)
        {
            new LearnerManager().RecordDailyActivity();
            
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

            lblCurrentUser.Text = Server.HtmlEncode(SessionHelper.GetCurrentFullName());

            if(SessionHelper.IsUserInRole(UserRoles.Learner)) {
                var result=new UserManager().GetLearnerProfile();
                if(result.Success && result.Data!=null) {
                    string avatar=result.Data.AvatarUrl; Uri uri;
                    if(!string.IsNullOrEmpty(avatar) && avatar.StartsWith("~/Uploads/Avatars/") && !avatar.Contains("..") && !avatar.Contains("\\")) AccountAvatar=ResolveUrl(avatar);
                    else if(Uri.TryCreate(avatar,UriKind.Absolute,out uri) && (uri.Scheme=="https" || uri.Scheme=="http")) AccountAvatar=uri.AbsoluteUri;
                }
            }

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

        // The footer links to these on every page, so signed-in users must be able to open them too
        private static readonly string[] OpenPages =
        {
            "~/Modules/Public/Terms.aspx",
            "~/Modules/Public/Privacy.aspx",
            "~/Modules/Public/Contact.aspx"
        };

        // Pages outside Modules/, plus Modules/Public/ (Login, Register), but not OpenPages
        private bool IsGuestPage()
        {
            string path = Page.AppRelativeVirtualPath ?? string.Empty;
            if (Array.Exists(OpenPages, page => string.Equals(page, path, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

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
