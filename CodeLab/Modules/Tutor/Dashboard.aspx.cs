using System;
using System.Web;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    public partial class Dashboard : Page
    {
        private const string CourseEditorPage = "CourseEditor";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            litTutorName.Text = SessionHelper.GetCurrentFullName();
            SetUpCreateCourseButton();
            BindDashboard();
        }

        private void SetUpCreateCourseButton()
        {
            hlCreateCourse.NavigateUrl = "~/Modules/Tutor/" + CourseEditorPage;

            if (!CanManageCourses())
            {
                hlCreateCourse.Enabled = false;
                hlCreateCourse.CssClass += " disabled";
                hlCreateCourse.Attributes["aria-disabled"] = "true";
            }
        }

        private void BindDashboard()
        {
            OperationResult<TutorDashboard> result = new TutorManager().GetDashboard();

            if (!result.Success)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                phNoCourses.Visible = true;
                return;
            }

            TutorDashboard dashboard = result.Data;

            litCourseCount.Text = dashboard.CourseCount.ToString();
            litPublishedNote.Text = dashboard.PublishedCount + " published";
            litStudentCount.Text = dashboard.StudentCount.ToString();
            litQuizCount.Text = dashboard.QuizCount.ToString();
            litAttemptNote.Text = dashboard.QuizAttemptCount == 1
                ? "1 learner attempt"
                : dashboard.QuizAttemptCount + " learner attempts";

            if (dashboard.AverageQuizScore.HasValue)
            {
                litAverageScore.Text = Math.Round(dashboard.AverageQuizScore.Value) + "%";
                litPassRateNote.Text = "Pass rate " + Math.Round(dashboard.QuizPassRate ?? 0) + "%";
            }
            else
            {
                litAverageScore.Text = "–";
                litPassRateNote.Text = "No attempts yet";
            }

            bool hasCourses = dashboard.Courses.Count > 0;
            rptCourses.Visible = hasCourses;
            phNoCourses.Visible = !hasCourses;

            rptCourses.DataSource = dashboard.Courses;
            rptCourses.DataBind();
        }

        private bool CanManageCourses()
        {
            return Master.IsApproved && Master.IsPageAvailable(CourseEditorPage);
        }

        // Traffic-light colours from the proposal: green = live, amber = waiting, red = blocked
        protected string StatusCss(string status)
        {
            switch (status)
            {
                case CourseStatuses.Published: return "cl-badge-success";
                case CourseStatuses.PendingReview: return "cl-badge-warning";
                case CourseStatuses.Locked:
                case CourseStatuses.Rejected: return "cl-badge-danger";
                default: return "cl-badge-neutral";
            }
        }

        protected string DifficultyCss(string difficulty)
        {
            switch (difficulty)
            {
                case "Beginner": return "cl-badge-success";
                case "Intermediate": return "cl-badge-info";
                case "Advanced": return "cl-badge-danger";
                default: return "cl-badge-neutral";
            }
        }

        protected string ManageLink(int courseId)
        {
            if (!CanManageCourses())
            {
                return string.Empty;
            }

            string url = Master.GetPageUrl(CourseEditorPage) + "?id=" + courseId;
            return "<a class=\"cl-link-arrow\" href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\">Manage &rarr;</a>";
        }
    }
}
