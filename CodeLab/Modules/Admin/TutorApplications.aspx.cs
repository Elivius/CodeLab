using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class TutorApplications : Page
    {
        // The status in ?status=..., or null for "all". Anything unknown shows Pending, the review queue.
        private string SelectedStatus
        {
            get
            {
                switch ((Request.QueryString["status"] ?? string.Empty).ToLowerInvariant())
                {
                    case "approved": return TutorStatuses.Approved;
                    case "rejected": return TutorStatuses.Rejected;
                    case "all": return null;
                    default: return TutorStatuses.Pending;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindApplications();
            }
        }

        protected void rptApplications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId;
            if (e.CommandName != "Approve" || !int.TryParse(e.CommandArgument.ToString(), out userId))
            {
                return;
            }

            ShowResult(new TutorApprovalManager().Approve(userId));
            BindApplications();
        }

        // Loads every application once, so each filter can show its count, then lists the selected status
        private void BindApplications()
        {
            OperationResult<List<TutorProfile>> result = new TutorApprovalManager().GetApplications(null);
            if (!result.Success)
            {
                ShowResult(result);
                phEmpty.Visible = false;
                rptApplications.Visible = false;
                return;
            }

            List<TutorProfile> all = result.Data;
            string status = SelectedStatus;

            SetFilter(hlFilterPending, "Pending", all.Count(a => a.Status == TutorStatuses.Pending), status == TutorStatuses.Pending);
            SetFilter(hlFilterApproved, "Approved", all.Count(a => a.Status == TutorStatuses.Approved), status == TutorStatuses.Approved);
            SetFilter(hlFilterRejected, "Rejected", all.Count(a => a.Status == TutorStatuses.Rejected), status == TutorStatuses.Rejected);
            SetFilter(hlFilterAll, "All", all.Count, status == null);

            List<TutorProfile> shown = status == null ? all : all.Where(a => a.Status == status).ToList();
            rptApplications.DataSource = shown;
            rptApplications.DataBind();

            phEmpty.Visible = shown.Count == 0;
            litEmpty.Text = status == TutorStatuses.Pending
                ? "No applications are waiting for review."
                : "There are no applications to show here.";
        }

        private static void SetFilter(HyperLink link, string label, int count, bool isActive)
        {
            link.Text = label + " (" + count + ")";
            link.CssClass = isActive ? "active" : string.Empty;
        }

        private void ShowResult(OperationResult result)
        {
            lblMessage.Text = Server.HtmlEncode(result.Message);
            lblMessage.CssClass = result.Success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        protected static bool IsPending(string status)
        {
            return status == TutorStatuses.Pending;
        }

        protected static string BadgeCss(string status)
        {
            switch (status)
            {
                case TutorStatuses.Approved: return "cl-admin-badge-approved";
                case TutorStatuses.Rejected: return "cl-admin-badge-rejected";
                default: return "cl-admin-badge-pending";
            }
        }

        protected static string FullDate(DateTime utc)
        {
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("d MMM yyyy, HH:mm");
        }
    }
}
