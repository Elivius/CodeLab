using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class UserEdit : Page
    {
        // True after a save, so the form shows what is stored now rather than what was typed
        private bool _refreshForm;

        // The account's id from ?id=, or 0 when there is no ?id= because this page is creating an account.
        // A bad id such as ?id=abc gives -1, which no account has, so the page says it was not found.
        // Unvalidated is safe here because the text is only ever read as a number.
        private int EditingId
        {
            get
            {
                string id = Request.Unvalidated.QueryString["id"];
                if (id == null)
                {
                    return 0;
                }

                int userId;
                return int.TryParse(id, out userId) && userId > 0 ? userId : -1;
            }
        }

        private bool IsNew
        {
            get { return EditingId == 0; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // One copy of each rule: the browser checks use the same patterns as the BLL
            revFullName.ValidationExpression = UserAdminManager.FullNamePattern;
            revUsername.ValidationExpression = UserAdminManager.UsernamePattern;
            revEmail.ValidationExpression = UserManager.EmailPattern;
            revPassword.ValidationExpression = UserManager.PasswordPattern;

            // The username cannot be changed once the account exists, and older accounts may not match the pattern
            txtUsername.ReadOnly = !IsNew;
            rfvUsername.Enabled = IsNew;
            revUsername.Enabled = IsNew;
            cvUsernameTaken.Enabled = IsNew;

            phNewAccount.Visible = IsNew;
            phAccount.Visible = !IsNew;

            Title = IsNew ? "Create User" : "Edit User";
            litHeading.Text = IsNew ? "Create a user" : "Edit user";
            btnSave.Text = IsNew ? "Create user" : "Save changes";
        }

        // PreRender runs after the button clicks, so the page always shows the account as it is now
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (IsNew)
            {
                litRoleNote.Text = "A new tutor applies to teach from their Profile page, then an admin approves them.";
                return;
            }

            OperationResult<AdminUserRow> result = new UserAdminManager().GetUser(EditingId);
            if (!result.Success)
            {
                ShowResult(result);
                phUser.Visible = false;
                return;
            }

            if (!IsPostBack && Request.QueryString["created"] == "1")
            {
                ShowResult(OperationResult.Ok("Account created. Give the user their password so they can log in."));
            }

            AdminUserRow user = result.Data;
            if (!IsPostBack || _refreshForm)
            {
                txtFullName.Text = user.FullName;
                txtUsername.Text = user.Username;
                txtEmail.Text = user.Email;
                ddlRole.SelectedValue = user.Role;
            }

            ShowAccount(user);
        }

        // These show the message beside the field. The BLL checks again when it saves.
        protected void cvUsernameTaken_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !new UserManager().IsUsernameTaken(args.Value);
        }

        protected void cvEmailTaken_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !new UserAdminManager().IsEmailTaken(EditingId, args.Value);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            UserAdminManager manager = new UserAdminManager();
            if (IsNew)
            {
                User account = new User
                {
                    FullName = txtFullName.Text,
                    Username = txtUsername.Text,
                    Email = txtEmail.Text,
                    Role = ddlRole.SelectedValue
                };

                OperationResult<int> created = manager.Create(account, txtPassword.Text, txtConfirmPassword.Text);
                if (created.Success)
                {
                    Response.Redirect("~/Modules/Admin/UserEdit?id=" + created.Data + "&created=1");
                }

                ShowResult(created);
                return;
            }

            OperationResult saved = manager.Update(EditingId, txtFullName.Text, txtEmail.Text, ddlRole.SelectedValue);
            ShowResult(saved);
            _refreshForm = saved.Success;
        }

        protected void btnSuspend_Click(object sender, EventArgs e)
        {
            ShowResult(new UserAdminManager().Suspend(EditingId));
        }

        protected void btnReactivate_Click(object sender, EventArgs e)
        {
            ShowResult(new UserAdminManager().Reactivate(EditingId));
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            OperationResult result = new UserAdminManager().Delete(EditingId);
            if (result.Success)
            {
                // The account is gone, so there is nothing left to show here
                Response.Redirect("~/Modules/Admin/Users?deleted=1");
            }

            ShowResult(result);
        }

        // Shows only the actions this account allows. See UserAdminManager for the rules.
        private void ShowAccount(AdminUserRow user)
        {
            bool isSelf = user.Id == SessionHelper.GetCurrentUserID();

            lblStatus.Text = HttpUtility.HtmlEncode(user.Status);
            lblStatus.CssClass = "cl-admin-badge " + DisplayHelper.UserStatusBadgeCss(user.Status);
            litJoined.Text = TutorDisplayHelper.LocalDate(user.JoinedDate);
            litLastLogin.Text = DisplayHelper.LastLogin(user.LastLogin);
            litCourses.Text = user.CourseCount + " taught";

            ddlRole.Enabled = !isSelf;
            btnSuspend.Visible = user.CanSuspend && !isSelf;
            btnReactivate.Visible = user.CanReactivate && !isSelf;
            btnDelete.Visible = !user.HasActivity && !isSelf;

            if (isSelf)
            {
                litRoleNote.Text = "You cannot change your own role.";
                litAccountNote.Text = "This is your account, so you cannot suspend or delete it.";
            }
            else if (user.CourseCount > 0)
            {
                litRoleNote.Text = "This role cannot change while the tutor teaches courses.";
                litAccountNote.Text = "This account has activity, so it cannot be deleted. Suspend it instead.";
            }
            else
            {
                litRoleNote.Text = "Changing the role logs the user out on their next click.";
                litAccountNote.Text = user.HasActivity
                    ? "This account has activity, so it cannot be deleted. Suspend it instead."
                    : "This account has no activity, so it can be deleted.";
            }
        }

        private void ShowResult(OperationResult result)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(result.Message);
            lblMessage.CssClass = result.Success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }
    }
}
