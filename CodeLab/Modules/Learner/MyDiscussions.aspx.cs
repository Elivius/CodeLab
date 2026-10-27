using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;

namespace CodeLab.Modules.Learner
{
    public partial class MyDiscussions : Page
    {
        private const int PageSize = 20;
        private readonly LearnerDiscussionManager manager = new LearnerDiscussionManager();
        private int learnerId;
        private bool canAccess;
        private bool ShowingReplies { get { return Request.QueryString["tab"] == "replies"; } }

        private int CurrentPage
        {
            get { return (int)(ViewState["MyDiscussionsPage"] ?? 1); }
            set { ViewState["MyDiscussionsPage"] = Math.Max(1, value); }
        }

        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlError.Visible = false;
            litError.Text = string.Empty;
            canAccess = SessionHelper.IsLoggedIn() &&
                (SessionHelper.IsUserInRole("Learner") || SessionHelper.IsUserInRole("Tutor")) &&
                SessionHelper.GetCurrentUserID() > 0;
            pnlMyDiscussions.Visible = canAccess;

            if (!canAccess)
            {
                Response.Redirect(ResolveUrl("~/Modules/Public/Login"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            learnerId = SessionHelper.GetCurrentUserID();
            BindNavigationIdentity();
            string tab = ShowingReplies ? "replies" : "posts";
            if (!string.Equals(ViewState["ActivityTab"] as string, tab, StringComparison.Ordinal))
                CurrentPage = 1;
            ViewState["ActivityTab"] = tab;

            lnkMyPosts.CssClass = ShowingReplies ? string.Empty : "is-active";
            lnkMyReplies.CssClass = ShowingReplies ? "is-active" : string.Empty;
            lnkMyPosts.Attributes.Remove("aria-current");
            lnkMyReplies.Attributes.Remove("aria-current");
            (ShowingReplies ? lnkMyReplies : lnkMyPosts).Attributes["aria-current"] = "page";
            pnlPosts.Visible = !ShowingReplies;
            pnlReplies.Visible = ShowingReplies;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (canAccess)
            {
                try
                {
                    if (ShowingReplies)
                        BindActivity(rptReplies, pnlNoReplies,
                            (page, size) => manager.GetMyReplies(learnerId, page, size));
                    else
                        BindActivity(rptPosts, pnlNoPosts,
                            (page, size) => manager.GetPosts(learnerId,
                                onlyMine: true, pageNumber: page, pageSize: size));
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Trace.TraceError("Loading my discussions: {0}", ex);
                    pnlMyDiscussions.Visible = false;
                    litError.Text = "Your discussions could not be loaded. Please try again.";
                    pnlError.Visible = true;
                }
            }
            base.OnPreRender(e);
        }

        private void BindActivity<T>(Repeater repeater, Panel emptyPanel,
            Func<int, int, List<T>> load)
        {
            var items = load(CurrentPage, PageSize);
            // Deleted discussions or changed course access can empty a later page.
            if (items.Count == 0 && CurrentPage > 1)
            {
                CurrentPage = 1;
                items = load(CurrentPage, PageSize);
            }
            bool hasNext = items.Count == PageSize &&
                load(checked(CurrentPage * PageSize + 1), 1).Count > 0;
            repeater.DataSource = items;
            repeater.DataBind();
            emptyPanel.Visible = items.Count == 0;
            btnPrevious.Enabled = CurrentPage > 1;
            btnNext.Enabled = hasNext;
            pnlPagination.Visible = CurrentPage > 1 || hasNext;
            litPageNumber.Text = "Page " + CurrentPage.ToString(CultureInfo.InvariantCulture);
        }

        protected void btnPrevious_Click(object sender, EventArgs e)
        {
            if (canAccess) CurrentPage--;
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (canAccess && btnNext.Enabled && CurrentPage < (int.MaxValue - 1) / PageSize)
                CurrentPage++;
        }

        protected string GetPostUrl(object value)
        {
            return GetActivityUrl(value, "posts");
        }

        protected string GetReplyDiscussionUrl(object value)
        {
            return GetActivityUrl(value, "replies");
        }

        private string GetActivityUrl(object value, string tab)
        {
            int id = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return HttpUtility.HtmlAttributeEncode(
                ResolveUrl("~/Modules/Learner/DiscussionDetails.aspx") +
                "?id=" + id.ToString(CultureInfo.InvariantCulture) + "&from=my&tab=" + tab);
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
                    "My Discussions navigation stats could not load: {0}", ex);
            }
        }
    }
}
