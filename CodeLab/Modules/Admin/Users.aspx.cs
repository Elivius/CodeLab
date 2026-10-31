using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class Users : Page
    {
        // The role in ?role=, or null for every role
        private string SelectedRole
        {
            get { return Match(Request.QueryString["role"], UserRoles.Learner, UserRoles.Tutor, UserRoles.Admin); }
        }

        // The status in ?status=, or null for every status
        private string SelectedStatus
        {
            get { return Match(Request.QueryString["status"], UserStatuses.Active, UserStatuses.Suspended); }
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
                    ShowResult(OperationResult.Ok("The account was deleted."));
                }

                BindUsers();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(PageUrl(SelectedRole, SelectedStatus, txtSearch.Text.Trim()));
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(e.CommandArgument.ToString(), out userId))
            {
                return;
            }

            UserAdminManager manager = new UserAdminManager();
            switch (e.CommandName)
            {
                case "Suspend":
                    ShowResult(manager.Suspend(userId));
                    break;
                case "Reactivate":
                    ShowResult(manager.Reactivate(userId));
                    break;
                case "Delete":
                    ShowResult(manager.Delete(userId));
                    break;
                default:
                    return;
            }

            BindUsers();
        }

        // Loads every account matching the search once, so each filter can show its count. The role
        // filters count within the selected status, and the status filters within the selected role.
        private void BindUsers()
        {
            OperationResult<List<AdminUserRow>> result = new UserAdminManager().GetUsers(SearchText);
            if (!result.Success)
            {
                ShowResult(result);
                phEmpty.Visible = false;
                rptUsers.Visible = false;
                return;
            }

            string role = SelectedRole;
            string status = SelectedStatus;
            string search = SearchText;

            List<AdminUserRow> inStatus = result.Data.Where(u => status == null || u.Status == status).ToList();
            List<AdminUserRow> inRole = result.Data.Where(u => role == null || u.Role == role).ToList();

            SetFilter(hlRoleAll, "All", inStatus.Count, role == null, PageUrl(null, status, search));
            SetFilter(hlRoleLearner, "Learners", inStatus.Count(u => u.Role == UserRoles.Learner), role == UserRoles.Learner, PageUrl(UserRoles.Learner, status, search));
            SetFilter(hlRoleTutor, "Tutors", inStatus.Count(u => u.Role == UserRoles.Tutor), role == UserRoles.Tutor, PageUrl(UserRoles.Tutor, status, search));
            SetFilter(hlRoleAdmin, "Admins", inStatus.Count(u => u.Role == UserRoles.Admin), role == UserRoles.Admin, PageUrl(UserRoles.Admin, status, search));

            SetFilter(hlStatusAll, "Any status", inRole.Count, status == null, PageUrl(role, null, search));
            SetFilter(hlStatusActive, "Active", inRole.Count(u => u.Status == UserStatuses.Active), status == UserStatuses.Active, PageUrl(role, UserStatuses.Active, search));
            SetFilter(hlStatusSuspended, "Suspended", inRole.Count(u => u.Status == UserStatuses.Suspended), status == UserStatuses.Suspended, PageUrl(role, UserStatuses.Suspended, search));

            hlClearSearch.Visible = search.Length > 0;
            hlClearSearch.NavigateUrl = PageUrl(role, status, null);

            List<AdminUserRow> shown = inRole.Where(u => status == null || u.Status == status).ToList();

            rptUsers.Visible = shown.Count > 0;
            rptUsers.DataSource = shown;
            rptUsers.DataBind();

            phEmpty.Visible = shown.Count == 0;
            litEmpty.Text = search.Length > 0 ? "No users match your search." : "There are no users to show here.";
        }

        // The allowed value that matches, ignoring case, or null if none does
        private static string Match(string value, params string[] allowed)
        {
            return Array.Find(allowed, item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase));
        }

        private static void SetFilter(HyperLink link, string label, int count, bool isActive, string url)
        {
            link.Text = label + " (" + count + ")";
            link.NavigateUrl = url;
            link.CssClass = isActive ? "active" : string.Empty;
        }

        private static string PageUrl(string role, string status, string search)
        {
            List<string> query = new List<string>();
            if (role != null)
            {
                query.Add("role=" + role);
            }

            if (status != null)
            {
                query.Add("status=" + status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query.Add("q=" + HttpUtility.UrlEncode(search));
            }

            string url = "~/Modules/Admin/Users";
            return query.Count == 0 ? url : url + "?" + string.Join("&", query);
        }

        private void ShowResult(OperationResult result)
        {
            lblMessage.Text = Server.HtmlEncode(result.Message);
            lblMessage.CssClass = result.Success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        protected bool IsSelf(int userId)
        {
            return userId == SessionHelper.GetCurrentUserID();
        }
    }
}
