using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.DAL;
using CodeLab.BLL;
using CodeLab.Helpers;

namespace CodeLab.Modules.Learner
{
    public partial class Curriculum : Page
    {
        protected bool IsEnrolled { get; private set; }
        private readonly LearnerCourseManager manager = new LearnerCourseManager();
        private sealed class ModuleRow
        {
            public int Id { get; set; }
            public int OrderIndex { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<LessonRow> Lessons { get; set; } = new List<LessonRow>();
            public List<QuizRow> Quizzes { get; set; } = new List<QuizRow>();
            public List<AssessmentRow> Assessments { get; set; } = new List<AssessmentRow>();
        }
        private sealed class LessonRow
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string CodeSnippet { get; set; }
            public string VideoUrl { get; set; }
            public int DurationMinutes { get; set; }
            public bool IsCompleted { get; set; }
        }
        private sealed class QuizRow
        {
            public string Title { get; set; }
            public int PassMark { get; set; }
            public int QuestionCount { get; set; }
        }
        private sealed class AssessmentRow
        {
            public string Title { get; set; }
            public string AssessmentType { get; set; }
            public int TimeLimitMinutes { get; set; }
            public int MaxAttempts { get; set; }
            public int PassMark { get; set; }
            public int QuestionCount { get; set; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner"))
            {
                Response.Redirect(ResolveUrl("~/Modules/Public/Login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            BindCurriculum(); // Recreate nested Repeater buttons before postback events.
        }
        private void BindCurriculum()
        {
            int courseId;
            if (!int.TryParse(Request.QueryString["id"], out courseId) || courseId <= 0)
            {
                Fail("Invalid course identifier."); return;
            }
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT Title FROM dbo.Courses WHERE Id=@course AND Status=N'Published'", conn))
                    {
                        cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                        object title = cmd.ExecuteScalar();
                        if (title == null) { Fail("Course not found."); return; }
                        litCourse.Text = HttpUtility.HtmlEncode(Convert.ToString(title));
                        
                    }
                    using (var enrollment = new SqlCommand("SELECT COUNT(*) FROM dbo.CourseEnrollments WHERE LearnerId=@learner AND CourseId=@course", conn))
                    {
                        enrollment.Parameters.Add("@learner", SqlDbType.Int).Value = SessionHelper.GetCurrentUserID();
                        enrollment.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                        IsEnrolled = Convert.ToInt32(enrollment.ExecuteScalar()) > 0;
                        btnEnroll.Visible = !IsEnrolled;
                        btnUnenroll.Visible = IsEnrolled;
                        litEnrollment.Text = IsEnrolled
                            ? "<span class='cc-enrolled'>Enrolled</span>"
                            : "<span class='cc-not-enrolled'>Not enrolled</span>";
                    }
                    pnlProgress.Visible = IsEnrolled;
                    if (IsEnrolled)
                    {
                        using (var progress = new SqlCommand(@"SELECT
(SELECT COUNT(*) FROM dbo.Lessons WHERE CourseId=@course),
(SELECT COUNT(*) FROM dbo.LessonCompletions lc
 JOIN dbo.Lessons l ON l.Id=lc.LessonId
 WHERE lc.LearnerId=@learner AND l.CourseId=@course)", conn))
                        {
                            progress.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                            progress.Parameters.Add("@learner", SqlDbType.Int).Value = SessionHelper.GetCurrentUserID();
                            using (var r = progress.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    int total = r.GetInt32(0);
                                    int finished = r.GetInt32(1);
                                    int percent = total == 0 ? 0 : (int)Math.Round(100.0 * finished / total, MidpointRounding.AwayFromZero);
                                    litProgressDetail.Text = finished + " of " + total + " lessons completed";
                                    litProgressPercent.Text = percent.ToString();
                                    litProgressFill.Text = "<div class='cc-progress-fill' style='width:" + percent + "%'></div>";
                                }
                            }
                        }
                    }
                    var modules = new List<ModuleRow>();
                    using (var cmd = new SqlCommand("SELECT Id,OrderIndex,Title,Description FROM dbo.CourseModules WHERE CourseId=@course ORDER BY OrderIndex", conn))
                    {
                        cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                        using (var r = cmd.ExecuteReader())
                            while (r.Read())
                                modules.Add(new ModuleRow {
                                    Id = r.GetInt32(0), OrderIndex = r.GetInt32(1),
                                    Title = r.GetString(2), Description = r.IsDBNull(3) ? "" : r.GetString(3)
                                });
                    }
                    foreach (var module in modules)
                    {
                        using (var cmd = new SqlCommand(@"SELECT l.Id,l.Title,l.Content,l.CodeSnippet,l.VideoUrl,l.DurationMinutes,
 CASE WHEN lc.LessonId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END
 FROM dbo.Lessons l LEFT JOIN dbo.LessonCompletions lc ON lc.LessonId=l.Id AND lc.LearnerId=@learner
 WHERE l.ModuleId=@module ORDER BY l.OrderIndex,l.Id", conn))
                        {
                            cmd.Parameters.Add("@learner", SqlDbType.Int).Value = SessionHelper.GetCurrentUserID();
                            cmd.Parameters.Add("@module", SqlDbType.Int).Value = module.Id;
                            using (var r = cmd.ExecuteReader())
                                while (r.Read()) module.Lessons.Add(new LessonRow {
                                    Id = r.GetInt32(0), Title = r.GetString(1),
                                    Content = r.IsDBNull(2) ? "" : r.GetString(2),
                                    CodeSnippet = r.IsDBNull(3) ? "" : r.GetString(3),
                                    VideoUrl = r.IsDBNull(4) ? "" : r.GetString(4),
                                    DurationMinutes = r.GetInt32(5), IsCompleted = r.GetBoolean(6)
                                });
                        }
                        using (var cmd = new SqlCommand("SELECT q.Title,q.PassMark,(SELECT COUNT(*) FROM dbo.QuizQuestions qq WHERE qq.QuizId=q.Id) FROM dbo.Quizzes q WHERE q.ModuleId=@module ORDER BY q.Id", conn))
                        {
                            cmd.Parameters.Add("@module", SqlDbType.Int).Value = module.Id;
                            using (var r = cmd.ExecuteReader())
                                while (r.Read()) module.Quizzes.Add(new QuizRow { Title=r.GetString(0), PassMark=r.GetInt32(1), QuestionCount=r.GetInt32(2) });
                        }
                        using (var cmd = new SqlCommand(@"SELECT a.Title,a.AssessmentType,a.TimeLimitMinutes,a.MaxAttempts,q.PassMark,(SELECT COUNT(*) FROM dbo.QuizQuestions qq WHERE qq.QuizId=q.Id)
 FROM dbo.CourseAssessments a JOIN dbo.Quizzes q ON q.Id=a.QuizId
 WHERE a.CourseId=@course AND a.AfterModuleOrder=@after ORDER BY CASE WHEN a.AssessmentType=N'Midterm' THEN 0 ELSE 1 END", conn))
                        {
                            cmd.Parameters.Add("@course", SqlDbType.Int).Value = courseId;
                            cmd.Parameters.Add("@after", SqlDbType.Int).Value = module.OrderIndex;
                            using (var r = cmd.ExecuteReader())
                                while (r.Read()) module.Assessments.Add(new AssessmentRow {
                                    Title=r.GetString(0), AssessmentType=r.GetString(1), TimeLimitMinutes=r.GetInt32(2),
                                    MaxAttempts=r.GetInt32(3), PassMark=r.GetInt32(4), QuestionCount=r.GetInt32(5)
                                });
                        }
                    }
                    rptModules.DataSource = modules;
                    rptModules.DataBind();
                    pnlNoModules.Visible = modules.Count == 0;
                }
            }
            catch (SqlException) { Fail("Curriculum database is not ready. Run the migration and seed scripts first."); }
        }
        protected void rptModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var module = (ModuleRow)e.Item.DataItem;
            ((Repeater)e.Item.FindControl("rptLessons")).DataSource = module.Lessons;
            ((Repeater)e.Item.FindControl("rptLessons")).DataBind();
            ((Repeater)e.Item.FindControl("rptQuizzes")).DataSource = module.Quizzes;
            ((Repeater)e.Item.FindControl("rptQuizzes")).DataBind();
            ((Repeater)e.Item.FindControl("rptAssessments")).DataSource = module.Assessments;
            ((Repeater)e.Item.FindControl("rptAssessments")).DataBind();
        }
        private int CourseId
        {
            get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; }
        }
        protected void btnEnroll_Click(object sender, EventArgs e)
        {
            try { manager.Enroll(SessionHelper.GetCurrentUserID(), CourseId); BindCurriculum(); }
            catch (SqlException) { lblError.Text = "Could not enrol in this course."; lblError.Visible = true; }
        }
        protected void btnUnenroll_Click(object sender, EventArgs e)
        {
            try { manager.Unenroll(SessionHelper.GetCurrentUserID(), CourseId); BindCurriculum(); }
            catch (SqlException) { lblError.Text = "Could not unenrol from this course."; lblError.Visible = true; }
        }
        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "ToggleLesson") return;
            int lessonId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out lessonId) || !IsEnrolled) return;
            try
            {
                var course = manager.Details(SessionHelper.GetCurrentUserID(), CourseId);
                if (course == null || !course.IsEnrolled) return;
                var lesson = course.Lessons.Find(l => l.Id == lessonId);
                if (lesson == null) return;
                manager.CompleteLesson(SessionHelper.GetCurrentUserID(), CourseId, lessonId, !lesson.IsCompleted);
                BindCurriculum();
            }
            catch (SqlException) { lblError.Text = "Could not update lesson completion."; lblError.Visible = true; }
        }
        protected string SafeVideoUrl(object value)
        {
            string url = Convert.ToString(value);
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) ? url : "";
        }
        private void Fail(string message) { pnlCurriculum.Visible=false; lblError.Text=message; lblError.Visible=true; }
    }
}
