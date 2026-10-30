using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Learner
{
    public partial class DiscussionForum : Page
    {
        private const int PageSize = 20;


        // Shared forum rules, the same ones the Tutor forum uses (BLL/Discussion/DiscussionManager)
        private readonly DiscussionManager manager =
            new DiscussionManager();

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

            if (BindCourseOptions())
            {
                canAccessForum = true;
            }
            else
            {
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

        // Fills the course filter and the "new discussion" course list with every published course
        // (shared rule: any learner or tutor can post in any published course). False if loading failed.
        private bool BindCourseOptions()
        {
            // Preserve submitted selections while refreshing access.
            string selectedFilter = ddlCourse.SelectedValue;
            string selectedPostCourse = ddlPostCourse.SelectedValue;

            OperationResult<List<Course>> result = manager.GetBrowseCourses();
            var postingCourses = manager.GetCourses();
            if (!result.Success || !postingCourses.Success)
            {
                return false;
            }

            List<Course> courses = result.Data;
            courses.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase));

            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("All discussions", "0"));
            ddlCourse.Items.Add(new ListItem("General Discussion", "-1"));

            ddlPostCourse.Items.Clear();
            ddlPostCourse.Items.Add(new ListItem("General Discussion", "-1"));

            foreach (Course course in courses)
            {
                string id = course.Id.ToString(
                    CultureInfo.InvariantCulture);

                ddlCourse.Items.Add(new ListItem(course.Title, id));

            }

            foreach (Course course in postingCourses.Data)
                ddlPostCourse.Items.Add(new ListItem(course.Title, course.Id.ToString(CultureInfo.InvariantCulture)));

            RestoreSelection(ddlCourse, selectedFilter);
            RestoreSelection(ddlPostCourse, selectedPostCourse);

            bool hasCourses = courses.Count > 0;

            pnlForum.Visible = true;
            ddlCourse.Enabled = true;
            pnlNoCourses.Visible = false;
            pnlCreateAction.Visible = true;
            btnPublish.Enabled = true;

            if (!hasCourses)
            {
                CurrentPage = 1;
            }

            // A removed course filter falls back to all courses.
            if (!string.IsNullOrEmpty(selectedFilter) &&
                ddlCourse.SelectedValue != selectedFilter)
            {
                CurrentPage = 1;
            }

            return true;
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
            int courseId = GetSelectedCourseId();
            string search = txtSearch.Text ?? string.Empty;

            OperationResult<List<DiscussionPost>> result =
                manager.GetPostPage(courseId, search, false, CurrentPage, PageSize);

            // Recover if posts were removed while viewing a later page.
            if (result.Success && result.Data.Count == 0 && CurrentPage > 1)
            {
                CurrentPage = 1;
                result = manager.GetPostPage(courseId, search, false, CurrentPage, PageSize);
            }

            if (!result.Success)
            {
                ClearResults();
                ShowError(result.Message);
                return;
            }

            List<DiscussionPost> posts = result.Data;
            bool hasNextPage = false;

            if (posts.Count == PageSize)
            {
                // Fetch one record immediately after this page.
                // A page size of 1 makes pageNumber the row position.
                int nextRow = checked(CurrentPage * PageSize + 1);

                OperationResult<List<DiscussionPost>> nextRecord =
                    manager.GetPostPage(courseId, search, false, nextRow, 1);

                hasNextPage = nextRecord.Success && nextRecord.Data.Count > 0;
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

        // The course chosen in the filter, or 0 for all courses
        private int GetSelectedCourseId()
        {
            int courseId;

            if (int.TryParse(ddlCourse.SelectedValue, out courseId) &&
                (courseId >= 0 || courseId == -1))
            {
                return courseId;
            }

            return 0;
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

            int courseId;

            if (!int.TryParse(ddlPostCourse.SelectedValue, out courseId) ||
                (courseId <= 0 && courseId != -1))
            {
                ShowCreateError("Please select the course this discussion is about.");
                return;
            }

            // Author identity always comes from the session (inside DiscussionManager).
            OperationResult<int> created = manager.CreatePost(
                courseId,
                txtPostTitle.Text,
                txtPostContent.Text);

            if (!created.Success)
            {
                ShowCreateError(created.Message);
                return;
            }

            int postId = created.Data;

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
                "&from=forum";

            return HttpUtility.HtmlAttributeEncode(url);
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