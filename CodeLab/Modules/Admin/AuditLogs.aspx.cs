using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class AuditLogs : Page
    {
        // The category in ?category=, or null for every category
        private string SelectedCategory
        {
            get { return AuditCategories.Find(Request.QueryString["category"]); }
        }

        // The text in ?q=. Unvalidated because it may hold characters such as <; see the note in the markup.
        private string SearchText
        {
            get { return (Request.Unvalidated.QueryString["q"] ?? string.Empty).Trim(); }
        }

        // The page in ?page=; the BLL corrects anything out of range
        private int SelectedPage
        {
            get
            {
                int page;
                return int.TryParse(Request.QueryString["page"], out page) ? page : 1;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtSearch.Text = SearchText;
                BindAuditLogs();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(PageUrl(SelectedCategory, txtSearch.Text.Trim(), 1));
        }

        private void BindAuditLogs()
        {
            string category = SelectedCategory;
            string search = SearchText;

            SetFilter(hlCategoryAll, null, category, search);
            SetFilter(hlCategorySecurity, AuditCategories.Security, category, search);
            SetFilter(hlCategoryAdministrative, AuditCategories.Administrative, category, search);
            SetFilter(hlCategoryContent, AuditCategories.Content, category, search);
            SetFilter(hlCategoryDeletion, AuditCategories.Deletion, category, search);

            hlClearSearch.Visible = search.Length > 0;
            hlClearSearch.NavigateUrl = PageUrl(category, null, 1);

            OperationResult<AuditLogPage> result = new AuditManager().Search(search, category, SelectedPage);
            if (!result.Success)
            {
                lblMessage.Text = Server.HtmlEncode(result.Message);
                lblMessage.Visible = true;
                litPageInfo.Text = string.Empty;
                hlPrevious.Visible = false;
                hlNext.Visible = false;
                return;
            }

            AuditLogPage logs = result.Data;

            gvAuditLogs.EmptyDataText = category == null && search.Length == 0
                ? "No activity has been recorded yet."
                : "No activity matches this search.";
            gvAuditLogs.DataSource = logs.Rows;
            gvAuditLogs.DataBind();

            litPageInfo.Text = "Page " + logs.Page + " of " + logs.PageCount + " (" + logs.TotalCount
                + (logs.TotalCount == 1 ? " event)" : " events)");

            hlPrevious.Visible = logs.Page > 1;
            hlPrevious.NavigateUrl = PageUrl(category, search, logs.Page - 1);
            hlNext.Visible = logs.Page < logs.PageCount;
            hlNext.NavigateUrl = PageUrl(category, search, logs.Page + 1);
        }

        private static void SetFilter(HyperLink link, string category, string selected, string search)
        {
            link.NavigateUrl = PageUrl(category, search, 1);
            link.CssClass = category == selected ? "active" : string.Empty;
        }

        private static string PageUrl(string category, string search, int page)
        {
            List<string> query = new List<string>();
            if (category != null)
            {
                query.Add("category=" + category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query.Add("q=" + HttpUtility.UrlEncode(search));
            }

            if (page > 1)
            {
                query.Add("page=" + page);
            }

            string url = "~/Modules/Admin/AuditLogs";
            return query.Count == 0 ? url : url + "?" + string.Join("&", query);
        }
    }
}
