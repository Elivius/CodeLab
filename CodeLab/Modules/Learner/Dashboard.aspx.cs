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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn()) { Response.Redirect("~/Modules/Public/Login.aspx", true); return; }
            if (!SessionHelper.IsUserInRole("Learner")) { Response.StatusCode = 403; return; }
            if (!IsPostBack) LoadDashboard();
        }
        private void LoadDashboard()
        {
            litName.Text = HttpUtility.HtmlEncode(SessionHelper.GetCurrentFullName() ?? "Learner");
            try
            {
                var data = new LearnerManager().GetDashboard(SessionHelper.GetCurrentUserID());
                litEnrolled.Text = data.Enrolled.ToString(); litCompleted.Text = data.Completed.ToString();
                litPoints.Text = data.Points.ToString("N0"); litStreak.Text = data.Streak.ToString();
                litLevel.Text = data.Level.ToString(); litLevelTitle.Text = HttpUtility.HtmlEncode(data.LevelTitle);
                litMilestone.Text = data.Milestone.ToString("N0");
                rptCourses.DataSource = data.Courses; rptCourses.DataBind();
                pnlNoCourses.Visible = data.Courses.Rows.Count == 0;
            }
            catch (SqlException) { lblError.Text = "Dashboard data is temporarily unavailable. Check the database connection and schema."; lblError.Visible = true; }
        }
    }
}
