using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class AdminMaster : MasterPage
    {
        // PreRender runs after the page's button clicks, so the numbers are already up to date
        // when an admin has just approved or rejected something on this request
        protected void Page_PreRender(object sender, EventArgs e)
        {
            OperationResult<AdminStats> result = new AdminStatsManager().GetStats();

            // Without numbers the cards are hidden, but the tabs still work
            phStats.Visible = result.Success;
            AdminStats stats = result.Success ? result.Data : new AdminStats();
            if (result.Success)
            {
                ShowStats(stats);
            }

            rptTabs.DataSource = BuildTabs(stats.PendingTutors, stats.PendingCourses);
            rptTabs.DataBind();
        }

        private void ShowStats(AdminStats stats)
        {
            litDatabaseSize.Text = stats.DatabaseSizeMb.ToString("0.0") + " MB";
            litQueryTime.Text = stats.QueryMilliseconds < 1 ? "under 1 ms" : stats.QueryMilliseconds + " ms";
            litTableCount.Text = stats.TableCount.ToString();
            litLastBackup.Text = stats.LastBackupAt.HasValue ? DisplayHelper.TimeAgo(stats.LastBackupAt.Value) : "never";

            litPendingTotal.Text = stats.PendingTotal.ToString();
            litPendingTutors.Text = stats.PendingTutors.ToString();
            litPendingCourses.Text = stats.PendingCourses.ToString();

            litActiveUsers.Text = stats.ActiveUsers.ToString();
            litNewUsers.Text = stats.NewUsersThisMonth.ToString();
            litLearners.Text = stats.Learners.ToString();
            litTutors.Text = stats.Tutors.ToString();
            litAdmins.Text = stats.Admins.ToString();

            litTotalCourses.Text = stats.TotalCourses.ToString();
            litDraftCourses.Text = stats.DraftCourses.ToString();
            litPublishedCourses.Text = stats.PublishedCourses.ToString();
            litReviewCourses.Text = stats.PendingCourses.ToString();
            litLockedCourses.Text = stats.LockedCourses.ToString();

            phBanner.Visible = stats.PendingTotal > 0;
            litBanner.Text = BannerText(stats);
        }

        // e.g. "2 tutor applications and 1 course are waiting for review."
        private static string BannerText(AdminStats stats)
        {
            List<string> waiting = new List<string>();
            if (stats.PendingTutors > 0)
            {
                waiting.Add(stats.PendingTutors + (stats.PendingTutors == 1 ? " tutor application" : " tutor applications"));
            }

            if (stats.PendingCourses > 0)
            {
                waiting.Add(stats.PendingCourses + (stats.PendingCourses == 1 ? " course" : " courses"));
            }

            return string.Join(" and ", waiting) + (stats.PendingTotal == 1 ? " is" : " are") + " waiting for review.";
        }

        // One entry per Admin tab. Each phase that adds a page adds its tab here.
        private List<AdminTab> BuildTabs(int pendingTutors, int pendingCourses)
        {
            List<AdminTab> tabs = new List<AdminTab>
            {
                new AdminTab("Overview", "layout-dashboard", "~/Modules/Admin/Dashboard", 0, "Dashboard"),
                new AdminTab("Users", "users", "~/Modules/Admin/Users", 0, "Users", "UserEdit"),
                new AdminTab("Course Moderation", "book-open", "~/Modules/Admin/Courses", pendingCourses, "Courses", "CourseReview"),
                new AdminTab("Tutor Applications", "graduation-cap", "~/Modules/Admin/TutorApplications", pendingTutors, "TutorApplications", "TutorApplication"),
                new AdminTab("Audit Logs", "shield", "~/Modules/Admin/AuditLogs", 0, "AuditLogs"),
                new AdminTab("Database Backups", "hard-drive", "~/Modules/Admin/Backups", 0, "Backups")
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
