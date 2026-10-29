using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;

namespace CodeLab.Modules.Learner
{
    public partial class CourseDetails : Page
    {
        private readonly LearnerCourseManager manager = new LearnerCourseManager();
        protected int InstructorId { get; private set; }
        protected bool IsEnrolled { get; private set; }
        private int CourseId { get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner"))
            {
                Response.Redirect(ResolveUrl("~/Modules/Public/Login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            // Rebind before postback events so Repeater LinkButtons exist
            // when ASP.NET dispatches their commands.
            LoadCourse();
        }
        private void LoadCourse()
        {
            try
            {
                var course = manager.Details(SessionHelper.GetCurrentUserID(), CourseId);
                if (course == null)
                {
                    pnlCourse.Visible = false;
                    ShowError("Course not found or not published.");
                    return;
                }
                pnlCourse.Visible = true;
                IsEnrolled = course.IsEnrolled;
                InstructorId=course.InstructorId;
                litTitle.Text = HttpUtility.HtmlEncode(course.Title);
                litCategory.Text = HttpUtility.HtmlEncode(course.Category);
                litDifficulty.Text = HttpUtility.HtmlEncode(course.Difficulty);
                litDescription.Text = HttpUtility.HtmlEncode(course.Description);
                litInstructor.Text = HttpUtility.HtmlEncode(course.Instructor);
                litLessonCount.Text = course.LessonCount.ToString();
                litProgress.Text = course.ProgressPercent.ToString("0");
                progressBar.Attributes["style"] = "width:" + course.ProgressPercent.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "%";
                pnlProgress.Visible = course.IsEnrolled;
                btnEnroll.Visible = !course.IsEnrolled;
                btnUnenroll.Visible = course.IsEnrolled;
                rptLessons.DataSource = course.Lessons;
                rptLessons.DataBind();
                pnlNoLessons.Visible = course.Lessons.Count == 0;
            }
            catch (SqlException) { ShowError("Course data is unavailable. Check the database connection."); }
        }
        protected void btnEnroll_Click(object sender, EventArgs e)
        {
            try { manager.Enroll(SessionHelper.GetCurrentUserID(), CourseId); LoadCourse(); }
            catch (SqlException) { ShowError("Could not enrol in this course."); }
        }
        protected void btnUnenroll_Click(object sender, EventArgs e)
        {
            try { manager.Unenroll(SessionHelper.GetCurrentUserID(), CourseId); LoadCourse(); }
            catch (SqlException) { ShowError("Could not unenrol from this course."); }
        }
        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "ToggleLesson") return;
            int lessonId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out lessonId)) return;
            try
            {
                var course = manager.Details(SessionHelper.GetCurrentUserID(), CourseId);
                if (course == null || !course.IsEnrolled) { ShowError("Enrol first."); return; }
                var lesson = course.Lessons.Find(l => l.Id == lessonId);
                if (lesson == null) { ShowError("Lesson not found."); return; }
                manager.CompleteLesson(SessionHelper.GetCurrentUserID(), CourseId, lessonId, !lesson.IsCompleted);
                LoadCourse();
            }
            catch (SqlException) { ShowError("Unable to save lesson progress."); }
        }
        protected string SafeVideoUrl(object value)
        {
            string url = Convert.ToString(value);
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) ? url : "";
        }
        private void ShowError(string message) { lblMessage.Text = message; lblMessage.Visible = true; }
    }
}
