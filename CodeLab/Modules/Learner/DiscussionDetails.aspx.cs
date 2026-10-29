using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;

namespace CodeLab.Modules.Learner
{
    public partial class DiscussionDetails : Page
    {
        private const int PageSize = 20;

        // Shared forum rules, the same ones the Tutor forum uses (BLL/Discussion/DiscussionManager)
        private readonly DiscussionManager manager =
            new DiscussionManager();

        private int learnerId;
        private int postId;
        private bool canAccessDiscussion;
        private bool redirecting;
        protected DiscussionPost currentPost;

        private int CurrentPage
        {
            get
            {
                object value = ViewState["ReplyPage"];
                return value == null ? 1 : (int)value;
            }
            set
            {
                ViewState["ReplyPage"] = Math.Max(1, value);
            }
        }

        private string FlashKey
        {
            get
            {
                return "ForumSuccess:" +
                    postId.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            canAccessDiscussion = false;
            currentPost = null;

            pnlDiscussion.Visible = false;
            pnlAuthorActions.Visible = false;
            phAuthorDialogs.Visible = false;

            pnlError.Visible = false;
            pnlSuccess.Visible = false;
            pnlReplyError.Visible = false;
            pnlEditError.Visible = false;
            pnlDeleteError.Visible = false;

            btnPostReply.Enabled = false;
            btnSavePost.Enabled = false;
            btnDeletePost.Enabled = false;

            if (!SessionHelper.IsLoggedIn() ||
                !(SessionHelper.IsUserInRole("Learner") || SessionHelper.IsUserInRole("Tutor")) ||
                SessionHelper.GetCurrentUserID() <= 0)
            {
                RedirectTo(ResolveUrl("~/Modules/Public/Login"));
                return;
            }

            learnerId = SessionHelper.GetCurrentUserID();

            ConfigureBackLink();
            BindNavigationIdentity();

            if (!int.TryParse(Request.QueryString["id"], out postId) ||
                postId <= 0)
            {
                ShowError("The discussion link is invalid.");
                return;
            }

            OperationResult<DiscussionPost> loaded = manager.GetPost(postId);
            if (!loaded.Success)
            {
                ShowError(loaded.Message);
                return;
            }

            currentPost = loaded.Data;

            canAccessDiscussion = true;
            pnlDiscussion.Visible = true;

            BindPost();

            if (!IsPostBack)
            {
                int requestedPage;

                if (int.TryParse(
                    Request.QueryString["page"],
                    out requestedPage) && requestedPage > 0)
                {
                    CurrentPage = requestedPage;
                }

                string message = Session[FlashKey] as string;

                if (!string.IsNullOrEmpty(message))
                {
                    Session.Remove(FlashKey);

                    litSuccess.Text = message;
                    pnlSuccess.Visible = true;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (canAccessDiscussion && !redirecting)
            {
                BindReplies();
            }

            base.OnPreRender(e);
        }

        private void ConfigureBackLink()
        {
            // Only known destinations are allowed.
            if (Request.QueryString["from"] == "my")
            {
                bool replies = Request.QueryString["tab"] == "replies";

                lnkBack.NavigateUrl =
                    "~/Modules/Learner/MyDiscussions.aspx?tab=" +
                    (replies ? "replies" : "posts");

                lnkBack.Text = "← Back to My Discussions";
            }
            else
            {
                lnkBack.NavigateUrl =
                    "~/Modules/Learner/DiscussionForum.aspx";

                lnkBack.Text = "← Back to Discussion Forum";
            }
        }

        private void BindPost()
        {
            Page.Title = currentPost.Title + " | CodeLab";

            litAuthorInitial.Text = GetInitial(currentPost.AuthorName);
            litAuthorName.Text = currentPost.AuthorName;
            litAuthorName.Visible=currentPost.AuthorRole!="Tutor";
            litAuthorUsername.Text = currentPost.AuthorUsername;
            litAuthorRole.Text = currentPost.AuthorRole;
            litCreatedAt.Text = FormatDate(currentPost.CreatedAt);

            litCourseTitle.Text = currentPost.CourseTitle;
            litTitle.Text = currentPost.Title;
            litContent.Text = currentPost.Content;

            litReplyCount.Text = currentPost.ReplyCount.ToString(
                CultureInfo.InvariantCulture);

            litReplyingName.Text = SessionHelper.GetCurrentFullName();

            bool isAuthor = currentPost.AuthorId == learnerId;

            phYou.Visible = isAuthor;
            pnlAuthorActions.Visible = isAuthor;
            phAuthorDialogs.Visible = isAuthor;

            btnPostReply.Enabled = true;
            btnSavePost.Enabled = isAuthor;
            btnDeletePost.Enabled = isAuthor;

            if (!IsPostBack)
            {
                txtEditTitle.Text = currentPost.Title;
                txtEditContent.Text = currentPost.Content;
            }

            txtReply.Attributes["maxlength"] =
                DiscussionManager.MaxReplyLength.ToString(
                    CultureInfo.InvariantCulture);

            txtEditContent.Attributes["maxlength"] =
                DiscussionManager.MaxPostLength.ToString(
                    CultureInfo.InvariantCulture);
        }

        private int GetLastReplyPage(int replyCount)
        {
            return Math.Max(
                1,
                (int)(((long)replyCount + PageSize - 1) / PageSize));
        }

        private void BindReplies()
        {
            int lastPage = GetLastReplyPage(currentPost.ReplyCount);

            CurrentPage = Math.Min(CurrentPage, lastPage);

            OperationResult<List<DiscussionReply>> result =
                manager.GetReplyPage(postId, CurrentPage, PageSize);

            if (!result.Success)
            {
                rptReplies.DataSource = null;
                rptReplies.DataBind();

                pnlNoReplies.Visible = false;
                pnlPagination.Visible = false;

                ShowError(result.Message);
                return;
            }

            List<DiscussionReply> replies = result.Data;

            rptReplies.DataSource = replies;
            rptReplies.DataBind();

            pnlNoReplies.Visible =
                currentPost.ReplyCount == 0 && replies.Count == 0;

            btnPrevious.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < lastPage;

            pnlPagination.Visible = lastPage > 1;

            litPageNumber.Text =
                "Page " +
                CurrentPage.ToString(CultureInfo.InvariantCulture) +
                " of " +
                lastPage.ToString(CultureInfo.InvariantCulture);
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            if (!canAccessDiscussion)
            {
                return;
            }

            CurrentPage--;
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (!canAccessDiscussion)
            {
                return;
            }

            int lastPage = GetLastReplyPage(currentPost.ReplyCount);

            if (CurrentPage < lastPage)
            {
                CurrentPage++;
            }
        }

        protected void btnPostReply_Click(object sender, EventArgs e)
        {
            if (!canAccessDiscussion)
            {
                return;
            }

            OperationResult<int> created = manager.CreateReply(
                postId,
                txtReply.Text);

            if (!created.Success)
            {
                ShowReplyError(created.Message);
                return;
            }

            int replyId = created.Data;

            // Saving succeeded. A subsequent read failure must not
            // be presented as a failed submission.
            int replyCount = currentPost.ReplyCount + 1;

            OperationResult<DiscussionPost> updatedPost = manager.GetPost(postId);
            if (updatedPost.Success)
            {
                replyCount = updatedPost.Data.ReplyCount;
            }

            Session[FlashKey] = "Your reply was posted.";

            string url = BuildDetailsUrl(
                GetLastReplyPage(replyCount));

            url += "#reply-" +
                replyId.ToString(CultureInfo.InvariantCulture);

            // Post/Redirect/Get avoids reposting on browser refresh.
            RedirectTo(url);
        }

        protected void btnSavePost_Click(object sender, EventArgs e)
        {
            if (!CanManagePost())
            {
                ShowError(
                    "You do not have permission to edit this discussion.");
                return;
            }

            OperationResult updated = manager.UpdatePost(
                postId,
                txtEditTitle.Text,
                txtEditContent.Text);

            if (!updated.Success)
            {
                ShowEditError(updated.Message);
                return;
            }

            Session[FlashKey] = "Your discussion was updated.";

            RedirectTo(BuildDetailsUrl(CurrentPage));
        }

        protected void btnDeletePost_Click(object sender, EventArgs e)
        {
            if (!CanManagePost())
            {
                ShowError(
                    "You do not have permission to delete this discussion.");
                return;
            }

            OperationResult deleted = manager.DeletePost(postId);
            if (!deleted.Success)
            {
                ShowDeleteError(deleted.Message);
                return;
            }

            Session.Remove(FlashKey);

            RedirectTo(ResolveUrl(lnkBack.NavigateUrl));
        }

        private bool CanManagePost()
        {
            return canAccessDiscussion &&
                currentPost != null &&
                currentPost.AuthorId == learnerId;
        }

        private string BuildDetailsUrl(int pageNumber)
        {
            string url = ResolveUrl(
                "~/Modules/Learner/DiscussionDetails.aspx") +
                "?id=" + postId.ToString(CultureInfo.InvariantCulture) +
                "&page=" + pageNumber.ToString(
                    CultureInfo.InvariantCulture);

            if (Request.QueryString["from"] == "my")
            {
                url += "&from=my&tab=" +
                    (Request.QueryString["tab"] == "replies"
                        ? "replies"
                        : "posts");
            }
            else
            {
                url += "&from=forum";
            }

            return url;
        }

        private void RedirectTo(string url)
        {
            redirecting = true;

            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowError(string message)
        {
            litError.Text = message;
            pnlError.Visible = true;
        }

        private void ShowReplyError(string message)
        {
            litReplyError.Text = message;
            pnlReplyError.Visible = true;
        }

        private void ShowEditError(string message)
        {
            litEditError.Text = message;
            pnlEditError.Visible = true;

            OpenDialog("lfEditDialog");
        }

        private void ShowDeleteError(string message)
        {
            litDeleteError.Text = message;
            pnlDeleteError.Visible = true;

            OpenDialog("lfDeleteDialog");
        }

        private void OpenDialog(string dialogId)
        {
            // This method accepts only the two fixed dialog IDs below.
            if (dialogId != "lfEditDialog" &&
                dialogId != "lfDeleteDialog")
            {
                return;
            }

            string script = @"
                (function () {
                    function openDialog() {
                        var dialog = document.getElementById('" +
                        dialogId + @"');

                        if (dialog && !dialog.open) {
                            dialog.showModal();
                        }
                    }

                    if (document.readyState === 'loading') {
                        document.addEventListener(
                            'DOMContentLoaded', openDialog);
                    } else {
                        openDialog();
                    }
                })();";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "Open" + dialogId,
                script,
                true);
        }

        protected bool IsCurrentLearner(object authorId)
        {
            return Convert.ToInt32(
                authorId,
                CultureInfo.InvariantCulture) == learnerId;
        }

        protected string GetReplyCssClass(
            object authorId,
            object authorRole)
        {
            string role = Convert.ToString(authorRole);

            if (string.Equals(
                role,
                "Tutor",
                StringComparison.OrdinalIgnoreCase))
            {
                return "lf-reply-card is-tutor";
            }

            return IsCurrentLearner(authorId)
                ? "lf-reply-card is-mine"
                : "lf-reply-card";
        }

        protected string GetInitial(object authorName)
        {
            string name = Convert.ToString(authorName).Trim();

            return name.Length == 0
                ? "?"
                : StringInfo.GetNextTextElement(name).ToUpperInvariant();
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