using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class Courses : Page
    {
        // The values of ?status=
        private const string FilterAll = "all";
        private const string FilterPending = "pending";
        private const string FilterPublished = "published";
        private const string FilterLocked = "locked";
        private const string FilterDraft = "draft";

        // The filter in ?status=; anything unknown shows every course, with the review queue on top
        private string SelectedFilter
        {
            get
            {
                string filter = (Request.QueryString["status"] ?? string.Empty).ToLowerInvariant();
                return filter == FilterPending || filter == FilterPublished || filter == FilterLocked || filter == FilterDraft
                    ? filter
                    : FilterAll;
            }
        }

        // The text in ?q=. Unvalidated because it may hold characters such as <; see the note in the markup.
        private string SearchText
        {
            get { return (Request.Unvalidated.QueryString["q"] ?? string.Empty).Trim(); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtSearch.Text = SearchText;
                if (Request.QueryString["deleted"] == "1")
                {
                    ShowResult(OperationResult.Ok("The course was deleted."));
                }

                BindCourses();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(FilterUrl(SelectedFilter, txtSearch.Text.Trim()));
        }

        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int courseId;
            if (!int.TryParse(e.CommandArgument.ToString(), out courseId))
            {
                return;
            }

            CourseModerationManager manager = new CourseModerationManager();
            switch (e.CommandName)
            {
                case "Approve":
                    ShowResult(manager.Approve(courseId));
                    break;
                case "Unlock":
                    ShowResult(manager.Unlock(courseId));
                    break;
                case "Delete":
                    ShowResult(manager.Delete(courseId));
                    break;
                default:
                    return;
            }

            BindCourses();
        }

        // Loads every course matching the search once, so each filter can show its count, then lists the selected one
        private void BindCourses()
        {
            OperationResult<List<AdminCourseRow>> result = new CourseModerationManager().GetCourses(null, SearchText);
            if (!result.Success)
            {
                ShowResult(result);
                phEmpty.Visible = false;
                rptCourses.Visible = false;
                return;
            }

            List<AdminCourseRow> all = result.Data;
            string filter = SelectedFilter;
            string search = SearchText;

            SetFilter(hlFilterAll, "All", all.Count, FilterAll, filter, search);
            SetFilter(hlFilterPending, "Pending", all.Count(c => c.Status == CourseStatuses.PendingReview), FilterPending, filter, search);
            SetFilter(hlFilterPublished, "Published", all.Count(c => c.Status == CourseStatuses.Published), FilterPublished, filter, search);
            SetFilter(hlFilterLocked, "Locked", all.Count(c => c.Status == CourseStatuses.Locked), FilterLocked, filter, search);
            SetFilter(hlFilterDraft, "Draft", all.Count(c => c.Status == CourseStatuses.Draft), FilterDraft, filter, search);

            hlClearSearch.Visible = search.Length > 0;
            hlClearSearch.NavigateUrl = FilterUrl(filter, null);

            string status = StatusOf(filter);
            List<AdminCourseRow> shown = status == null ? all : all.Where(c => c.Status == status).ToList();

            rptCourses.Visible = shown.Count > 0;
            rptCourses.DataSource = shown;
            rptCourses.DataBind();

            phEmpty.Visible = shown.Count == 0;
            litEmpty.Text = search.Length > 0 ? "No courses match your search." : "There are no courses to show here.";
        }

        private static string StatusOf(string filter)
        {
            switch (filter)
            {
                case FilterPending: return CourseStatuses.PendingReview;
                case FilterPublished: return CourseStatuses.Published;
                case FilterLocked: return CourseStatuses.Locked;
                case FilterDraft: return CourseStatuses.Draft;
                default: return null;
            }
        }

        private static void SetFilter(HyperLink link, string label, int count, string filter, string selected, string search)
        {
            link.Text = label + " (" + count + ")";
            link.NavigateUrl = FilterUrl(filter, search);
            link.CssClass = filter == selected ? "active" : string.Empty;
        }

        private static string FilterUrl(string filter, string search)
        {
            string url = "~/Modules/Admin/Courses?status=" + filter;
            return string.IsNullOrEmpty(search) ? url : url + "&q=" + HttpUtility.UrlEncode(search);
        }

        private void ShowResult(OperationResult result)
        {
            lblMessage.Text = Server.HtmlEncode(result.Message);
            lblMessage.CssClass = result.Success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        protected static string LessonText(int count)
        {
            return count == 1 ? "1 lesson" : count + " lessons";
        }
    }
}
