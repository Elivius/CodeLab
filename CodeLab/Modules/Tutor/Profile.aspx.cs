using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    public partial class Profile : Page
    {
        private const string ProfileUrl = "~/Modules/Tutor/Profile";

        // What the main column shows
        private enum PageMode
        {
            View,       // Pending or Approved: read-only overview
            Edit,       // Pending or Approved, after "Edit profile": change details, no new review
            Apply,      // Not applied yet: tutor application
            Resubmit,   // Rejected: fix the application and send it for review again
            Admin       // Administrators have no tutor details
        }

        // Browser URL of the profile photo, or null to show initials
        protected string AvatarSrc { get; private set; }

        protected string Initials { get; private set; } = "T";

        protected void Page_Load(object sender, EventArgs e)
        {
            // This page shows the approval status itself
            Master.HideApprovalNotice = true;

            if (IsPostBack)
            {
                return;
            }

            ShowMessageFromQueryString();
            LoadProfile(true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("Profile");
            if (!Page.IsValid)
            {
                LoadProfile(false);
                return;
            }

            TutorProfile details = new TutorProfile
            {
                FullName = txtFullName.Text,
                Specialty = txtSpecialty.Text,
                Qualifications = txtQualifications.Text,
                PortfolioUrl = txtPortfolioUrl.Text,
                Bio = txtBio.Text,
                CurriculumPitch = txtPitch.Text
            };

            // The current status decides whether this is an edit or an application
            OperationResult<TutorProfile> current = new TutorManager().GetProfile();
            bool isEdit = current.Success
                && (current.Data.Status == TutorStatuses.Pending || current.Data.Status == TutorStatuses.Approved);

            OperationResult result = isEdit
                ? new TutorManager().UpdateProfile(details)
                : new TutorManager().SubmitApplication(details);

            if (!result.Success)
            {
                ShowError(result.Message);
                LoadProfile(false);
                return;
            }

            // Reload so the sidebar and header show the change, and a refresh cannot save twice
            Response.Redirect(ProfileUrl + (isEdit ? "?saved=1" : "?submitted=1"));
        }

        protected void btnUploadAvatar_Click(object sender, EventArgs e)
        {
            OperationResult result = fuAvatar.HasFile
                ? new TutorManager().UpdateAvatar(fuAvatar.PostedFile.FileName, fuAvatar.PostedFile.ContentLength, fuAvatar.PostedFile.InputStream)
                : OperationResult.Fail("Choose an image to upload first.");

            FinishAvatarChange(result, "updated");
        }

        protected void btnRemoveAvatar_Click(object sender, EventArgs e)
        {
            FinishAvatarChange(new TutorManager().RemoveAvatar(), "removed");
        }

        private void FinishAvatarChange(OperationResult result, string flagValue)
        {
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadProfile(false);
                return;
            }

            // Stay in edit mode if the photo was changed from the edit form
            string mode = Request.QueryString["mode"] == "edit" ? "&mode=edit" : string.Empty;
            Response.Redirect(ProfileUrl + "?photo=" + flagValue + mode);
        }

        private void ShowMessageFromQueryString()
        {
            if (Request.QueryString["submitted"] == "1")
            {
                ShowSuccess("Your application has been submitted. An administrator will review it soon.");
            }
            else if (Request.QueryString["saved"] == "1")
            {
                ShowSuccess("Your profile has been updated.");
            }
            else if (Request.QueryString["photo"] == "updated")
            {
                ShowSuccess("Your profile photo has been updated.");
            }
            else if (Request.QueryString["photo"] == "removed")
            {
                ShowSuccess("Your profile photo has been removed.");
            }
        }

        /// <summary>
        /// Shows the profile. fillForm copies the saved details into the form; it is false on a
        /// postback so what the tutor just typed is kept.
        /// </summary>
        private void LoadProfile(bool fillForm)
        {
            OperationResult<TutorProfile> result = new TutorManager().GetProfile();
            if (!result.Success)
            {
                ShowError(result.Message);
                phProfile.Visible = false;
                return;
            }

            TutorProfile profile = result.Data;
            PageMode mode = GetMode(profile);

            ShowHeader(profile);
            ShowAccount(profile);
            ShowTimeline(profile);

            phOverview.Visible = mode == PageMode.View;
            phAdminNote.Visible = mode == PageMode.Admin;
            phTimeline.Visible = mode != PageMode.Admin;
            hlEditProfile.Visible = mode == PageMode.View;

            if (mode == PageMode.View)
            {
                litBio.Text = profile.Bio;
                litPitch.Text = string.IsNullOrWhiteSpace(profile.CurriculumPitch) ? "Not provided yet." : profile.CurriculumPitch;
                litQualifications.Text = profile.Qualifications;
            }

            bool showForm = mode == PageMode.Edit || mode == PageMode.Apply || mode == PageMode.Resubmit;
            phForm.Visible = showForm;
            if (showForm)
            {
                ShowForm(profile, mode, fillForm);
            }

            if (profile.Status == TutorStatuses.Approved || mode == PageMode.Admin)
            {
                ShowStats();
            }
        }

        private PageMode GetMode(TutorProfile profile)
        {
            if (SessionHelper.IsUserInRole(UserRoles.Admin))
            {
                return PageMode.Admin;
            }

            if (!profile.HasApplied)
            {
                return PageMode.Apply;
            }

            if (profile.Status == TutorStatuses.Rejected)
            {
                return PageMode.Resubmit;
            }

            return Request.QueryString["mode"] == "edit" ? PageMode.Edit : PageMode.View;
        }

        private void ShowHeader(TutorProfile profile)
        {
            bool isAdmin = SessionHelper.IsUserInRole(UserRoles.Admin);

            Initials = TutorDisplayHelper.Initials(profile.FullName, "T");
            AvatarSrc = string.IsNullOrEmpty(profile.AvatarUrl) ? null : ResolveUrl(profile.AvatarUrl);
            btnRemoveAvatar.Visible = AvatarSrc != null;

            litName.Text = profile.FullName;
            litHeadline.Text = isAdmin
                ? "CodeLab administrator"
                : (string.IsNullOrWhiteSpace(profile.Specialty) ? "Tutor at CodeLab" : profile.Specialty + " tutor");
            litTutorCode.Text = profile.TutorCode;
            litJoinedShort.Text = "Joined " + profile.JoinedDate.ToLocalTime().ToString("MMM yyyy");

            string badgeText;
            string badgeCss;
            if (isAdmin)
            {
                badgeText = "Administrator";
                badgeCss = "cl-badge-info";
            }
            else if (profile.Status == TutorStatuses.Approved)
            {
                badgeText = "Approved tutor";
                badgeCss = "cl-badge-success";
            }
            else if (profile.Status == TutorStatuses.Pending)
            {
                badgeText = "Pending approval";
                badgeCss = "cl-badge-warning";
            }
            else if (profile.Status == TutorStatuses.Rejected)
            {
                badgeText = "Not approved";
                badgeCss = "cl-badge-danger";
            }
            else
            {
                badgeText = "Not applied";
                badgeCss = "cl-badge-neutral";
            }

            lblStatusBadge.Text = badgeText;
            lblStatusBadge.CssClass = "cl-badge cl-status " + badgeCss;

            // Only http(s) links become clickable, so a stored value can never run script
            if (TutorManager.IsWebUrl(profile.PortfolioUrl))
            {
                hlPortfolioMeta.NavigateUrl = profile.PortfolioUrl;
                hlPortfolioMeta.Attributes["rel"] = "noopener noreferrer";
                hlPortfolioMeta.Visible = true;
            }
        }

        private void ShowAccount(TutorProfile profile)
        {
            litUsername.Text = profile.Username;
            litEmail.Text = profile.Email;
            litSpecialty.Text = string.IsNullOrWhiteSpace(profile.Specialty) ? "Not set" : profile.Specialty;
            litJoined.Text = profile.JoinedDate.ToLocalTime().ToString("d MMMM yyyy");

            // HyperLink.Text is written as raw HTML, so encode it
            hlPortfolio.Text = string.IsNullOrWhiteSpace(profile.PortfolioUrl)
                ? "Not set"
                : HttpUtility.HtmlEncode(profile.PortfolioUrl);
            if (TutorManager.IsWebUrl(profile.PortfolioUrl))
            {
                hlPortfolio.NavigateUrl = profile.PortfolioUrl;
                hlPortfolio.Attributes["rel"] = "noopener noreferrer";
            }
        }

        private void ShowForm(TutorProfile profile, PageMode mode, bool fillForm)
        {
            switch (mode)
            {
                case PageMode.Edit:
                    litFormTitle.Text = "Edit profile";
                    litFormIntro.Text = "Changes are saved straight away and do not need another review.";
                    btnSave.Text = "Save changes";
                    break;
                case PageMode.Resubmit:
                    litFormTitle.Text = "Update and resubmit your application";
                    litFormIntro.Text = string.IsNullOrWhiteSpace(profile.RejectionReason)
                        ? "Your last application was not approved. Review your details and send it for review again."
                        : "Your last application was not approved. The administrator wrote: \"" + profile.RejectionReason
                            + "\" Update your details and send it for review again.";
                    btnSave.Text = "Resubmit application";
                    break;
                default:
                    litFormTitle.Text = "Apply to teach on CodeLab";
                    litFormIntro.Text = "An administrator reviews every application. Course authoring unlocks once yours is approved.";
                    btnSave.Text = "Submit application";
                    break;
            }

            phPhotoRow.Visible = mode == PageMode.Edit;
            hlCancelEdit.Visible = mode == PageMode.Edit;

            if (fillForm)
            {
                txtFullName.Text = profile.FullName;
                txtSpecialty.Text = profile.Specialty;
                txtQualifications.Text = profile.Qualifications;
                txtPortfolioUrl.Text = profile.PortfolioUrl;
                txtBio.Text = profile.Bio;
                txtPitch.Text = profile.CurriculumPitch;
            }
        }

        // Three steps: submitted, under review, decision
        private void ShowTimeline(TutorProfile profile)
        {
            List<TimelineStep> steps = new List<TimelineStep>();
            string status = profile.Status;

            if (!profile.HasApplied)
            {
                steps.Add(new TimelineStep("is-current", "file-pen-line", "Submit your application", "Tell us about your teaching experience."));
                steps.Add(new TimelineStep("is-upcoming", "hourglass", "Admin review", "An administrator checks your details."));
                steps.Add(new TimelineStep("is-upcoming", "badge-check", "Start teaching", "Create courses once you are approved."));
            }
            else
            {
                steps.Add(new TimelineStep("is-done", "check", "Application submitted", "Your details were sent for review."));

                if (status == TutorStatuses.Pending)
                {
                    steps.Add(new TimelineStep("is-current", "hourglass", "Under review", "An administrator is checking your application."));
                    steps.Add(new TimelineStep("is-upcoming", "badge-check", "Start teaching", "Authoring unlocks once you are approved."));
                }
                else if (status == TutorStatuses.Rejected)
                {
                    steps.Add(new TimelineStep("is-done", "check", "Reviewed", "An administrator reviewed your application."));
                    steps.Add(new TimelineStep("is-failed", "x", "Not approved", "Update your details and resubmit."));
                }
                else
                {
                    steps.Add(new TimelineStep("is-done", "check", "Reviewed", "An administrator reviewed your application."));
                    steps.Add(new TimelineStep("is-done", "badge-check", "Approved",
                        profile.ApprovedDate.HasValue
                            ? "Approved on " + profile.ApprovedDate.Value.ToLocalTime().ToString("d MMM yyyy") + "."
                            : "All authoring tools are unlocked."));
                }
            }

            rptTimeline.DataSource = steps;
            rptTimeline.DataBind();
        }

        private void ShowStats()
        {
            OperationResult<TutorDashboard> result = new TutorManager().GetDashboard();
            if (!result.Success)
            {
                return;
            }

            litStatCourses.Text = result.Data.CourseCount.ToString();
            litStatPublished.Text = result.Data.PublishedCount.ToString();
            litStatStudents.Text = result.Data.StudentCount.ToString();
            phStats.Visible = true;
        }

        private void ShowSuccess(string message)
        {
            lblSuccess.Text = HttpUtility.HtmlEncode(message);
            lblSuccess.Visible = true;
        }

        private void ShowError(string message)
        {
            // The page remembers the green message from the last save; hide it next to an error
            lblSuccess.Visible = false;
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.Visible = true;
        }

        // One step of the application timeline (public so the Repeater can read its properties)
        public class TimelineStep
        {
            public TimelineStep(string cssClass, string icon, string title, string text)
            {
                CssClass = cssClass;
                Icon = icon;
                Title = title;
                Text = text;
            }

            public string CssClass { get; private set; }
            public string Icon { get; private set; }
            public string Title { get; private set; }
            public string Text { get; private set; }
        }
    }
}
