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
        // How many pending applications the Overview lists; the rest are on the Tutor Applications tab
        private const int PendingPreviewCount = 5;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPendingTutors();
            }
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
