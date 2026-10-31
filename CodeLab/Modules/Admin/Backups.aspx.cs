using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class Backups : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindBackups();
            }
        }

        protected void btnBackup_Click(object sender, EventArgs e)
        {
            ShowResult(new BackupManager().Create());
            BindBackups();
        }

        protected void rptBackups_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Download")
            {
                return;
            }

            string name = e.CommandArgument.ToString();
            OperationResult<string> file = new BackupManager().GetFileForDownload(name);
            if (!file.Success)
            {
                ShowResult(file);
                return;
            }

            // Response.End stops the page here, so its HTML is not added to the end of the file
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + name + "\"");
            Response.TransmitFile(file.Data);
            Response.End();
        }

        private void BindBackups()
        {
            OperationResult<List<BackupFile>> result = new BackupManager().GetBackups();
            if (!result.Success)
            {
                ShowResult(result);
                phEmpty.Visible = false;
                rptBackups.Visible = false;
                return;
            }

            rptBackups.Visible = result.Data.Count > 0;
            rptBackups.DataSource = result.Data;
            rptBackups.DataBind();

            phEmpty.Visible = result.Data.Count == 0;
        }

        private void ShowResult(OperationResult result)
        {
            lblMessage.Text = Server.HtmlEncode(result.Message);
            lblMessage.CssClass = result.Success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }
    }
}
