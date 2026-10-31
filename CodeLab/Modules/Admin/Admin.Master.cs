using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class AdminMaster : MasterPage
    {
        // PreRender runs after the page's button clicks, so a badge count is already up to date
        // when an admin has just approved or rejected something on this request
        protected void Page_PreRender(object sender, EventArgs e)
        {
            rptTabs.DataSource = BuildTabs();
            rptTabs.DataBind();
        }

        // One entry per Admin tab. Each phase that adds a page adds its tab here.
        private List<AdminTab> BuildTabs()
        {
            OperationResult<int> pendingTutors = new TutorApprovalManager().CountPending();

            List<AdminTab> tabs = new List<AdminTab>
            {
                new AdminTab("Overview", "layout-dashboard", "~/Modules/Admin/Dashboard", 0, "Dashboard"),
                new AdminTab("Tutor Applications", "graduation-cap", "~/Modules/Admin/TutorApplications",
                    pendingTutors.Success ? pendingTutors.Data : 0, "TutorApplications", "TutorApplication"),
                new AdminTab("Audit Logs", "shield", "~/Modules/Admin/AuditLogs", 0, "AuditLogs")
            };

            // e.g. "~/Modules/Admin/TutorApplication.aspx" gives "TutorApplication"
            string currentPage = Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath);
            foreach (AdminTab tab in tabs)
            {
                tab.IsActive = Array.Exists(tab.Pages,
                    page => string.Equals(page, currentPage, StringComparison.OrdinalIgnoreCase));
            }

            return tabs;
        }

        // One tab in the bar (public so the Repeater can read its properties)
        public class AdminTab
        {
            public AdminTab(string title, string icon, string url, int badge, params string[] pages)
            {
                Title = title;
                Icon = icon;
                Url = url;
                Badge = badge;
                Pages = pages;
            }

            public string Title { get; private set; }
            public string Icon { get; private set; }
            public string Url { get; private set; }

            // Number shown in a bubble next to the title; 0 hides it
            public int Badge { get; private set; }

            // The pages (file names without .aspx) that highlight this tab
            public string[] Pages { get; private set; }

            public bool IsActive { get; set; }
        }
    }
}
