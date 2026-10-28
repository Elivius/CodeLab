using System;
using System.Collections.Generic;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class AuditLogs : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindAuditLogs();
            }
        }

        private void BindAuditLogs()
        {
            AuditManager auditManager = new AuditManager();
            OperationResult<List<AuditLog>> result = auditManager.GetRecentAuditLogs();

            if (!result.Success)
            {
                lblMessage.Text = result.Message;
                lblMessage.Visible = true;
                return;
            }

            gvAuditLogs.DataSource = result.Data;
            gvAuditLogs.DataBind();
        }
    }
}
