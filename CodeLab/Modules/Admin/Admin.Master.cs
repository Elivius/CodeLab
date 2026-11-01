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
        // e.g. "~/Modules/Admin/TutorApplication.aspx" gives "TutorApplication"
        private string CurrentPage
        {
            get { return Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath); }
        }

        // PreRender runs after the page's button clicks, so the numbers are already up to date
        // when an admin has just approved or rejected something on this request
        protected void Page_PreRender(object sender, EventArgs e)
        {
            OperationResult<AdminStats> result = new AdminStatsManager().GetStats();
            AdminStats stats = result.Success ? result.Data : new AdminStats();

            // The summary is for the Overview, so the other tabs open straight on their content.
            // Without numbers it stays hidden, but the tabs still work.
            bool isOverview = string.Equals(CurrentPage, "Dashboard", StringComparison.OrdinalIgnoreCase);
            phSummary.Visible = result.Success && isOverview;
            if (phSummary.Visible)
            {
                ShowSummary(stats);
            }

            rptTabs.DataSource = BuildTabs(stats.PendingTutors, stats.PendingCourses);
            rptTabs.DataBind();
        }

        private void ShowSummary(AdminStats stats)
        {
            bool hasWork = stats.PendingTotal > 0;

            // The band changes colour with its state: raspberry when work is waiting, quiet when it is not
            pnlQueue.CssClass = hasWork ? "cl-admin-queue" : "cl-admin-queue is-clear";
            phQueueClear.Visible = !hasWork;
            litQueueTitle.Text = !hasWork
                ? "Nothing needs your review"
                : Count(stats.PendingTotal, "item") + (stats.PendingTotal == 1 ? " needs your review" : " need your review");

            hlReviewTutors.Visible = stats.PendingTutors > 0;
            litReviewTutors.Text = "Review " + Count(stats.PendingTutors, "tutor application");
            hlReviewCourses.Visible = stats.PendingCourses > 0;
            litReviewCourses.Text = "Review " + Count(stats.PendingCourses, "course");

            litAccountsFigure.Text = stats.ActiveUsers + " active";
            litAccountsDetail.Text = Count(stats.Learners, "learner") + ", " + Count(stats.Tutors, "tutor") + " and "
                + Count(stats.Admins, "admin") + ". " + stats.NewUsersThisMonth + " joined this month.";

            litCoursesFigure.Text = Count(stats.TotalCourses, "course");
            litCoursesDetail.Text = stats.PublishedCourses + " published, " + stats.PendingCourses + " in review, "
                + stats.LockedCourses + " locked and " + Count(stats.DraftCourses, "draft") + ".";

            litDatabaseFigure.Text = stats.DatabaseSizeMb.ToString("0.0") + " MB";
            litDatabaseDetail.Text = Count(stats.TableCount, "table") + ", test query "
                + (stats.QueryMilliseconds < 1 ? "under 1 ms" : stats.QueryMilliseconds + " ms") + ".";
            litLastBackup.Text = stats.LastBackupAt.HasValue
                ? "Last backup: " + DisplayHelper.TimeAgo(stats.LastBackupAt.Value)
                : "No backups yet";
        }

        // e.g. "1 course", "3 courses"
        private static string Count(int count, string noun)
        {
            return count + " " + noun + (count == 1 ? string.Empty : "s");
        }

        // One entry per Admin tab. Each phase that adds a page adds its tab here.
        private List<AdminTab> BuildTabs(int pendingTutors, int pendingCourses)
        {
            List<AdminTab> tabs = new List<AdminTab>
            {
                new AdminTab("Overview", "layout-dashboard", "~/Modules/Admin/Dashboard", 0, "Dashboard"),
                new AdminTab("Users", "users", "~/Modules/Admin/Users", 0, "Users", "UserEdit"),
                new AdminTab("Course moderation", "book-open", "~/Modules/Admin/Courses", pendingCourses, "Courses", "CourseReview"),
                new AdminTab("Tutor applications", "graduation-cap", "~/Modules/Admin/TutorApplications", pendingTutors, "TutorApplications", "TutorApplication"),
                new AdminTab("Audit logs", "shield", "~/Modules/Admin/AuditLogs", 0, "AuditLogs"),
                new AdminTab("Database backups", "hard-drive", "~/Modules/Admin/Backups", 0, "Backups")
            };

            string currentPage = CurrentPage;
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
