using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
namespace CodeLab.Modules.Learner
{
    public partial class Dashboard : Page
    {
        protected System.Data.DataRow[] ResumeCourses = new System.Data.DataRow[0];
        protected string ResumeImage(object value) {
            string path=Convert.ToString(value); Uri uri;
            if(path.StartsWith("~/") && !path.Contains("..")) return ResolveUrl(path);
            if(Uri.TryCreate(path,UriKind.Absolute,out uri) && (uri.Scheme=="https" || uri.Scheme=="http")) return uri.AbsoluteUri;
            return "";
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn()) { Response.Redirect("~/Modules/Public/Login.aspx", true); return; }
            if (!SessionHelper.IsUserInRole("Learner")) { Response.StatusCode = 403; return; }
            if (!IsPostBack) LoadDashboard();
        }
        private void LoadDashboard()
        {
            string fullName = SessionHelper.GetCurrentFullName() ?? "Learner";
            litName.Text = HttpUtility.HtmlEncode(fullName);
            litNavName.Text = HttpUtility.HtmlEncode(fullName);
            litInitial.Text = HttpUtility.HtmlEncode(fullName.Substring(0, 1).ToUpperInvariant());
            try
            {
                var data = new LearnerManager().GetDashboard(SessionHelper.GetCurrentUserID());
                litEnrolled.Text = data.Enrolled.ToString();
                litEnrolledIntro.Text = data.Enrolled.ToString();
                litEnrolledHeading.Text = data.Enrolled.ToString();
                
                var badges = new LeaderboardManager().GetEarnedCount();
                litBadges.Text = badges.Success ? badges.Data.ToString() : "—";
                litBadgesShortcut.Text = litBadges.Text;
                litPoints.Text = data.Points.ToString("N0"); litStreak.Text = data.Streak.ToString();
                litLevel.Text = data.Level.ToString(); litNextLevel.Text = (data.Level + 1).ToString(); litNavPoints.Text = data.Points.ToString("N0"); litLevelTitle.Text = HttpUtility.HtmlEncode(data.LevelTitle);
                litMilestone.Text = data.Milestone.ToString("N0");
                ResumeCourses = data.Courses.Select("IsCompleted = false AND LessonId IS NOT NULL");
                rptCourses.DataSource = data.Courses; rptCourses.DataBind();
                pnlNoCourses.Visible = data.Courses.Rows.Count == 0;
            }
            catch (SqlException) { lblError.Text = "Dashboard data is temporarily unavailable. Check the database connection and schema."; lblError.Visible = true; }
        }
    }
}
