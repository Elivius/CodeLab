using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class Dashboard : Page
    {
        // How many rows each Overview panel lists; the rest are on that panel's tab
        private const int PendingPreviewCount = 5;

        // Recent users sits beside two panels, so it lists more rows to match their height
        private const int RecentUserCount = 7;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindUsers();
                BindPendingTutors();
                BindCourses();
            }
        }

        private void BindUsers()
        {
            OperationResult<List<AdminUserRow>> result = new UserAdminManager().GetUsers(null);
            if (!result.Success)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                litUserSummary.Text = "Could not be loaded.";
                return;
            }

            int count = result.Data.Count;
            litUserSummary.Text = count > RecentUserCount
                ? "Newest " + RecentUserCount + " of " + count + " accounts."
                : count + (count == 1 ? " account" : " accounts") + ", newest first.";
            rptUsers.DataSource = result.Data.Take(RecentUserCount);
            rptUsers.DataBind();
        }

        // The courses that need an admin: waiting for review first, then locked
        private void BindCourses()
        {
            OperationResult<List<AdminCourseRow>> result = new CourseModerationManager().GetCourses(null, null);
            if (!result.Success)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                litCourseSummary.Text = "Could not be loaded.";
                return;
            }

            List<AdminCourseRow> courses = result.Data
                .Where(c => c.Status == CourseStatuses.PendingReview || c.Status == CourseStatuses.Locked)
                .ToList();
            int pending = courses.Count(c => c.Status == CourseStatuses.PendingReview);

            litCourseSummary.Text = pending + " waiting for review, " + (courses.Count - pending) + " locked.";

            phNoCourses.Visible = courses.Count == 0;
            rptCourses.DataSource = courses.Take(PendingPreviewCount);
            rptCourses.DataBind();
        }

        private void BindPendingTutors()
        {
            OperationResult<List<TutorProfile>> result = new TutorApprovalManager().GetApplications(TutorStatuses.Pending);
            if (!result.Success)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                litPendingSummary.Text = "Could not be loaded.";
                return;
            }

            int count = result.Data.Count;
            litPendingSummary.Text = count == 0
                ? "Nothing to review right now."
                : count + (count == 1 ? " candidate is" : " candidates are") + " waiting for review, oldest first.";

            phNoPending.Visible = count == 0;
            rptPendingTutors.DataSource = result.Data.Take(PendingPreviewCount);
            rptPendingTutors.DataBind();
        }
    }
}
