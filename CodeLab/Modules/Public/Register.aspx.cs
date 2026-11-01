using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Public
{
    public partial class Register : Page
    {
        // The Tutor tab is open only for ?role=tutor; anything else shows the Learner tab
        private bool IsTutor
        {
            get { return string.Equals(Request.QueryString["role"], "tutor", StringComparison.OrdinalIgnoreCase); }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // One copy of each rule: the browser checks use the same patterns as UserManager.Register
            revEmail.ValidationExpression = UserManager.EmailPattern;
            revPassword.ValidationExpression = UserManager.PasswordPattern;
            revPortfolioUrl.ValidationExpression = TutorManager.PortfolioUrlPattern;
            revPortfolioUrl.ErrorMessage = TutorManager.PortfolioUrlMessage;
        }

        // Someone already signed in never gets this far: Site.Master sends them to their home page
        protected void Page_Load(object sender, EventArgs e)
        {
            MarkActiveTab(IsTutor ? lnkTutorTab : lnkLearnerTab);

            // Tutors fill in three steps (account, background, teaching); learners only see the account step
            phStepOne.Visible = IsTutor;
            phNextStep.Visible = IsTutor;
            phTutorFields.Visible = IsTutor;
            phBackStep.Visible = IsTutor;
        }

        protected void cvUsernameTaken_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !new UserManager().IsUsernameTaken(args.Value);
        }

        protected void cvEmailTaken_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !new UserManager().IsEmailTaken(args.Value);
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // The button skips automatic validation so each step can be checked on its own in the browser
            Page.Validate("Account");
            if (IsTutor)
            {
                Page.Validate("Background");
                Page.Validate("Teaching");
            }

            if (!Page.IsValid)
            {
                return;
            }

            User account = new User
            {
                FullName = txtFullName.Text,
                Username = txtUsername.Text,
                Email = txtEmail.Text,
                Role = IsTutor ? UserRoles.Tutor : UserRoles.Learner
            };

            TutorProfile application = !IsTutor ? null : new TutorProfile
            {
                Specialty = txtSpecialty.Text,
                PortfolioUrl = txtPortfolioUrl.Text,
                Bio = txtBio.Text,
                CurriculumPitch = txtPitch.Text,
                Qualifications = txtQualifications.Text
            };

            OperationResult result = new UserManager().Register(account, txtPassword.Text, txtConfirm.Text, application);
            if (!result.Success)
            {
                lblMessage.Text = Server.HtmlEncode(result.Message);
                lblMessage.Visible = true;
                return;
            }

            Response.Redirect("~/Modules/Public/Login?registered=" + (IsTutor ? "tutor" : "learner"));
        }

        private static void MarkActiveTab(System.Web.UI.HtmlControls.HtmlAnchor tab)
        {
            tab.Attributes["class"] = "active";
            tab.Attributes["aria-current"] = "page";
        }
    }
}
