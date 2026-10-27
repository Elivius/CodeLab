using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;

namespace CodeLab.Modules.Learner
{
    public partial class DiscussionForum : Page
    {
        private const int PageSize = 20;
        protected bool GeneralScope { get { return Request.QueryString["scope"] != "courses"; } }

        private readonly LearnerDiscussionManager manager =
            new LearnerDiscussionManager();

        private int learnerId;
        private bool canAccessForum;
        private bool redirecting;

        private int CurrentPage
        {
            get
            {
                object value = ViewState["ForumPage"];
                return value == null ? 1 : (int)value;
            }
            set
            {
                ViewState["ForumPage"] = Math.Max(1, value);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            // Bind page state to the current session.
            ViewStateUserKey = Session.SessionID;

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            canAccessForum = false;

            pnlError.Visible = false;
            litError.Text = string.Empty;

            pnlCreateError.Visible = false;
            litCreateError.Text = string.Empty;

            if (!CodeLab.Helpers.SessionHelper.IsLoggedIn() ||
                !(CodeLab.Helpers.SessionHelper.IsUserInRole("Learner") || CodeLab.Helpers.SessionHelper.IsUserInRole("Tutor")) ||
                CodeLab.Helpers.SessionHelper.GetCurrentUserID() <= 0)
            {
                redirecting = true;

                Response.Redirect(
                    ResolveUrl("~/Modules/Public/Login"),
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            learnerId =
                CodeLab.Helpers.SessionHelper.GetCurrentUserID();

            litPostingName.Text =
                CodeLab.Helpers.SessionHelper.GetCurrentFullName();

            BindNavigationIdentity();

            try
            {
                BindCourseOptions();

                canAccessForum = true;
            }
            catch (SqlException ex)
            {
                LogDatabaseError("Loading forum courses", ex);

                pnlForum.Visible = false;
                pnlCreateAction.Visible = false;
                pnlNoCourses.Visible = false;
                btnPublish.Enabled = false;

                ShowError(
                    "The discussion forum is temporarily unavailable. " +
                    "Please try again.");
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (canAccessForum && !redirecting && pnlForum.Visible)
            {
                BindPosts();
            }

            base.OnPreRender(e);
        }

        private void BindCourseOptions()
        {
            // Preserve submitted selections while refreshing access.
            string selectedFilter = ddlCourse.SelectedValue;
            string selectedPostCourse = ddlPostCourse.SelectedValue;

            var courses = manager.GetEnrolledCourses(learnerId);

            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(
                new ListItem(GeneralScope ? "General Discussion" : "All Enrolled Courses", ""));

            ddlPostCourse.Items.Clear();
            ddlPostCourse.Items.Add(
                new ListItem("General Discussion", "general"));

            foreach (var course in courses)
            {
                string id = course.Key.ToString(
                    CultureInfo.InvariantCulture);

                ddlCourse.Items.Add(new ListItem(course.Value, id));
                ddlPostCourse.Items.Add(new ListItem(course.Value, id));
            }

            RestoreSelection(ddlCourse, selectedFilter);
            RestoreSelection(ddlPostCourse, selectedPostCourse);

            bool hasCourses = courses.Count > 0;

            pnlForum.Visible = true;
            ddlCourse.Enabled = !GeneralScope && hasCourses;
            pnlNoCourses.Visible = !GeneralScope && !hasCourses;
            pnlCreateAction.Visible = true;
            btnPublish.Enabled = true;

            if (!hasCourses && !GeneralScope)
            {
                CurrentPage = 1;
            }

            // A removed course filter falls back to all accessible courses.
            if (!string.IsNullOrEmpty(selectedFilter) &&
                ddlCourse.SelectedValue != selectedFilter)
            {
                CurrentPage = 1;
            }
        }

        private static void RestoreSelection(
            DropDownList control,
            string value)
        {
            ListItem item = control.Items.FindByValue(
                value ?? string.Empty);

            if (item != null)
            {
                control.ClearSelection();
                item.Selected = true;
            }
        }

        private void BindPosts()
        {
            try
            {
                int? courseId = GeneralScope ? null : GetSelectedCourseId();
                string search = txtSearch.Text ?? string.Empty;

                var posts = manager.GetPosts(
                    learnerId,
                    courseId,
                    search,
                    false,
                    CurrentPage,
                    PageSize, GeneralScope ? "general" : "courses");

                // Recover if posts were removed while viewing a later page.
                if (posts.Count == 0 && CurrentPage > 1)
                {
                    CurrentPage = 1;

                    posts = manager.GetPosts(
                        learnerId,
                        courseId,
                        search,
                        false,
                        CurrentPage,
                        PageSize, GeneralScope ? "general" : "courses");
                }

                bool hasNextPage = false;

                if (posts.Count == PageSize)
                {
                    // Fetch one record immediately after this page.
                    // A page size of 1 makes pageNumber the row position.
                    int nextRow = checked(CurrentPage * PageSize + 1);

                    var nextRecord = manager.GetPosts(
                        learnerId,
                        courseId,
                        search,
                        false,
                        nextRow,
                        1, GeneralScope ? "general" : "courses");

                    hasNextPage = nextRecord.Count > 0;
                }

                rptPosts.DataSource = posts;
                rptPosts.DataBind();

                pnlEmpty.Visible = posts.Count == 0;

                btnPrevious.Enabled = CurrentPage > 1;
                btnNext.Enabled = hasNextPage;

                pnlPagination.Visible =
                    CurrentPage > 1 || hasNextPage;

                litPageNumber.Text = "Page " +
                    CurrentPage.ToString(CultureInfo.InvariantCulture);
            }
            catch (ArgumentException ex)
            {
                ClearResults();
                ShowError(ex.Message);
            }
            catch (SqlException ex)
            {
                LogDatabaseError("Loading forum discussions", ex);

                ClearResults();

                ShowError(
                    "Discussions could not be loaded. Please try again.");
            }
        }

        private int? GetSelectedCourseId()
        {
            int courseId;

            if (int.TryParse(ddlCourse.SelectedValue, out courseId) &&
                courseId > 0)
            {
                return courseId;
            }

            return null;
        }

        private void ClearResults()
        {
            rptPosts.DataSource = null;
            rptPosts.DataBind();

            // Avoid presenting a loading failure as an empty forum.
            pnlEmpty.Visible = false;
            pnlPagination.Visible = false;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!canAccessForum)
            {
                return;
            }

            CurrentPage = 1;
        }

        protected void ddlCourse_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (!canAccessForum)
            {
                return;
            }

            CurrentPage = 1;
        }

        protected void btnClearFilters_Click(
            object sender,
            EventArgs e)
        {
            if (!canAccessForum)
            {
                return;
            }

            txtSearch.Text = string.Empty;

            if (ddlCourse.Items.Count > 0)
            {
                ddlCourse.SelectedIndex = 0;
            }

            CurrentPage = 1;
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            if (!canAccessForum)
            {
                return;
            }

            CurrentPage--;
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (!canAccessForum || !btnNext.Enabled)
            {
                return;
            }

            // Keep the pagination offset within integer bounds.
            if (CurrentPage < (int.MaxValue - 1) / PageSize)
            {
                CurrentPage++;
            }
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            if (!canAccessForum || !btnPublish.Enabled)
            {
                return;
            }

            int courseId = 0;

            if (ddlPostCourse.SelectedValue != "general" && (!int.TryParse(ddlPostCourse.SelectedValue, out courseId) ||
                courseId <= 0))
            {
                ShowCreateError("Please select an enrolled course.");
                return;
            }

            int postId;

            try
            {
                // Author identity always comes from the session.
                postId = manager.CreatePost(
                    learnerId,
                    ddlPostCourse.SelectedValue == "general" ? (int?)null : courseId,
                    txtPostTitle.Text,
                    txtPostContent.Text);
            }
            catch (ArgumentException ex)
            {
                ShowCreateError(ex.Message);
                return;
            }
            catch (InvalidOperationException ex)
            {
                ShowCreateError(ex.Message);
                return;
            }
            catch (SqlException ex)
            {
                LogDatabaseError("Creating a discussion", ex);

                ShowCreateError(
                    "We could not confirm whether your discussion was saved. " +
                    "Check My Discussions before publishing it again.");

                return;
            }

            // Redirect after saving so refreshing does not repost.
            redirecting = true;

            string url = ResolveUrl(
                "~/Modules/Learner/DiscussionDetails.aspx") +
                "?id=" + postId.ToString(CultureInfo.InvariantCulture);

            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowError(string message)
        {
            litError.Text = message;
            pnlError.Visible = true;
        }

        private void ShowCreateError(string message)
        {
            litCreateError.Text = message;
            pnlCreateError.Visible = true;

            const string script = @"
                (function () {
                    function openCreateDialog() {
                        var dialog =
                            document.getElementById('lfCreateDialog');

                        if (dialog && !dialog.open) {
                            dialog.showModal();
                        }
                    }

                    if (document.readyState === 'loading') {
                        document.addEventListener(
                            'DOMContentLoaded',
                            openCreateDialog
                        );
                    } else {
                        openCreateDialog();
                    }
                })();";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "OpenForumCreateDialog",
                script,
                true);
        }

        protected bool IsCurrentLearner(object authorId)
        {
            return Convert.ToInt32(
                authorId,
                CultureInfo.InvariantCulture) == learnerId;
        }

        protected string GetInitial(object authorName)
        {
            string name = Convert.ToString(authorName).Trim();

            if (name.Length == 0)
            {
                return "?";
            }

            return StringInfo.GetNextTextElement(name)
                .ToUpperInvariant();
        }

        protected string GetPreview(object content)
        {
            string text = Convert.ToString(content).Trim();

            if (text.Length <= 240)
            {
                return text;
            }

            int length = 240;

            // Avoid splitting a UTF-16 surrogate pair.
            if (char.IsHighSurrogate(text[length - 1]))
            {
                length--;
            }

            return text.Substring(0, length).TrimEnd() + "…";
        }

        protected string FormatReplyCount(object count)
        {
            int value = Convert.ToInt32(
                count,
                CultureInfo.InvariantCulture);

            return value.ToString(CultureInfo.InvariantCulture) +
                (value == 1 ? " reply" : " replies");
        }

        protected string FormatDate(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime utc = DateTime.SpecifyKind(
                Convert.ToDateTime(value, CultureInfo.InvariantCulture),
                DateTimeKind.Utc);

            return utc.AddHours(8).ToString(
                "dd MMM yyyy, hh:mm tt",
                CultureInfo.InvariantCulture);
        }

        protected string FormatUtc(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            DateTime utc = DateTime.SpecifyKind(
                Convert.ToDateTime(value, CultureInfo.InvariantCulture),
                DateTimeKind.Utc);

            return utc.ToString("o", CultureInfo.InvariantCulture);
        }

        protected string GetDiscussionUrl(object value)
        {
            int postId = Convert.ToInt32(
                value,
                CultureInfo.InvariantCulture);

            string url = ResolveUrl(
                "~/Modules/Learner/DiscussionDetails.aspx") +
                "?id=" + postId.ToString(CultureInfo.InvariantCulture) +
                "&from=forum&scope=" + (GeneralScope ? "general" : "courses");

            return HttpUtility.HtmlAttributeEncode(url);
        }

        private static void LogDatabaseError(
            string operation,
            SqlException exception)
        {
            System.Diagnostics.Trace.TraceError(
                "{0}: {1}",
                operation,
                exception);
        }

        private void BindNavigationIdentity()
        {
            string name =
                (CodeLab.Helpers.SessionHelper.GetCurrentFullName()
                ?? string.Empty).Trim();

            litNavName.Text = name;

            litNavInitial.Text = name.Length == 0
                ? "?"
                : System.Globalization.StringInfo
                    .GetNextTextElement(name)
                    .ToUpperInvariant();

            litNavLevel.Text = "—";
            litNavPoints.Text = "—";

            if (CodeLab.Helpers.SessionHelper.IsUserInRole("Tutor")) return;
            try
            {
                var dashboard = new CodeLab.BLL.LearnerManager()
                    .GetDashboard(
                        CodeLab.Helpers.SessionHelper.GetCurrentUserID());

                if (dashboard != null)
                {
                    litNavLevel.Text = dashboard.Level.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);

                    litNavPoints.Text = dashboard.Points.ToString(
                        "N0",
                        System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Forum navigation stats could not load: {0}", ex);
            }
        }
    }
}