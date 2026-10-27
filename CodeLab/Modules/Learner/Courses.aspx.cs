using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;

namespace CodeLab.Modules.Learner
{
    public partial class Courses : Page
    {
        private readonly LearnerCourseManager manager = new LearnerCourseManager();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn() || !SessionHelper.IsUserInRole("Learner"))
            {
                Response.Redirect(ResolveUrl("~/Modules/Public/Login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!IsPostBack)
            {
                litIdentity.Text = HttpUtility.HtmlEncode(SessionHelper.GetCurrentFullName() ?? "Learner");
                BindCategories();
                BindCourses();
            }
        }
        private void BindCategories()
        {
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("All Categories", ""));
            try
            {
                foreach (string category in manager.Categories())
                    ddlCategory.Items.Add(new ListItem(category, category));
            }
            catch (SqlException) { ShowError("Unable to load course categories."); }
        }
        private void BindCourses()
        {
            try
            {
                int learner = SessionHelper.GetCurrentUserID();
                bool enrolled = hdnMode.Value != "all";
                var all = manager.List(learner, false, "", "");
                var filtered = manager.List(learner, enrolled, txtSearch.Text, ddlCategory.SelectedValue);
                int enrolledCount = 0;
                foreach (var c in all) if (c.IsEnrolled) enrolledCount++;
                litEnrolledCount.Text = enrolledCount.ToString();
                litPublishedCount.Text = all.Count.ToString();
                btnEnrolled.CssClass = "lc-switch-button" + (enrolled ? " active" : "");
                btnExplore.CssClass = "lc-switch-button" + (!enrolled ? " active" : "");
                rptCourses.DataSource = filtered;
                rptCourses.DataBind();
                pnlEmpty.Visible = filtered.Count == 0;
            }
            catch (SqlException) { ShowError("Courses are temporarily unavailable. Check the database connection."); }
        }
        protected void btnEnrolled_Click(object sender, EventArgs e) { hdnMode.Value = "enrolled"; BindCourses(); }
        protected void btnExplore_Click(object sender, EventArgs e) { hdnMode.Value = "all"; BindCourses(); }
        protected void Filter_Changed(object sender, EventArgs e) { BindCourses(); }
        protected string CourseImage(object value)
        {
            string url = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(url)) return ResolveUrl("~/Content/Images/logo.png");
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)) return url;
            if (url.StartsWith("~/")) return ResolveUrl(url);
            return ResolveUrl("~/Content/Images/logo.png");
        }
        private void ShowError(string message) { lblMessage.Text = message; lblMessage.Visible = true; }
    }
}
