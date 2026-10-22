using System;
using System.IO;
using System.Web;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    public partial class TutorMaster : MasterPage
    {
        private const string TutorFolder = "~/Modules/Tutor/";

        private bool _statusLoaded;
        private string _approvalStatus;
        private string _statusError;
        private string _avatarUrl;

        /// <summary>
        /// True when the signed-in user may author content (an approved tutor, or an Admin).
        /// Tutor pages read this through Master.IsApproved.
        /// </summary>
        public bool IsApproved
        {
            get
            {
                EnsureStatusLoaded();
                return _approvalStatus == TutorStatuses.Approved;
            }
        }

        /// <summary>
        /// Set to true on a page that shows the approval status itself (e.g. Profile),
        /// so the notice above the page is not repeated.
        /// </summary>
        public bool HideApprovalNotice { get; set; }

        protected string TutorName
        {
            get { return SessionHelper.GetCurrentFullName() ?? string.Empty; }
        }

        /// <summary>
        /// Browser URL of the user's profile photo, or null to show their initials instead.
        /// </summary>
        protected string AvatarSrc
        {
            get
            {
                EnsureStatusLoaded();
                return string.IsNullOrEmpty(_avatarUrl) ? null : ResolveUrl(_avatarUrl);
            }
        }

        protected string Initials
        {
            get
            {
                string[] words = TutorName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0)
                {
                    return "T";
                }

                string initials = words[0].Substring(0, 1);
                if (words.Length > 1)
                {
                    initials += words[words.Length - 1].Substring(0, 1);
                }

                return initials.ToUpperInvariant();
            }
        }

        protected string StatusText
        {
            get
            {
                EnsureStatusLoaded();
                if (SessionHelper.IsUserInRole(UserRoles.Admin))
                {
                    return "Administrator";
                }

                switch (_approvalStatus)
                {
                    case TutorStatuses.Approved: return "Approved tutor";
                    case TutorStatuses.Pending: return "Pending approval";
                    case TutorStatuses.Rejected: return "Not approved";
                    default: return "Not applied";
                }
            }
        }

        protected string StatusBadgeCss
        {
            get
            {
                EnsureStatusLoaded();
                if (SessionHelper.IsUserInRole(UserRoles.Admin))
                {
                    return "cl-badge-info";
                }

                switch (_approvalStatus)
                {
                    case TutorStatuses.Approved: return "cl-badge-success";
                    case TutorStatuses.Pending: return "cl-badge-warning";
                    case TutorStatuses.Rejected: return "cl-badge-danger";
                    default: return "cl-badge-neutral";
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ShowApprovalNotice();
        }

        /// <summary>
        /// True if the Tutor page with this name (e.g. "CourseEditor") has been created.
        /// </summary>
        public bool IsPageAvailable(string pageName)
        {
            return File.Exists(Server.MapPath(TutorFolder + pageName + ".aspx"));
        }

        /// <summary>
        /// Returns the browser URL of a Tutor page, e.g. "/Modules/Tutor/CourseEditor".
        /// </summary>
        public string GetPageUrl(string pageName)
        {
            return ResolveUrl(TutorFolder + pageName);
        }

        /// <summary>
        /// Renders one sidebar link. Pages that do not exist yet show as "Soon",
        /// and authoring pages show a lock until the tutor is approved.
        /// </summary>
        protected string NavLink(string pageName, string icon, string text, bool requiresApproval)
        {
            string iconHtml = "<i data-lucide=\"" + icon + "\" class=\"cl-icon\" aria-hidden=\"true\"></i>";
            string label = "<span>" + HttpUtility.HtmlEncode(text) + "</span>";
            bool isLocked = requiresApproval && !IsApproved;

            if (isLocked || !IsPageAvailable(pageName))
            {
                string marker = isLocked
                    ? "<i data-lucide=\"lock\" class=\"cl-icon cl-sidebar-marker\" aria-label=\"Locked\"></i>"
                    : "<span class=\"cl-sidebar-tag\">Soon</span>";
                return "<span class=\"cl-sidebar-link disabled\" aria-disabled=\"true\">" + iconHtml + label + marker + "</span>";
            }

            bool isActive = string.Equals(
                Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath), pageName, StringComparison.OrdinalIgnoreCase);

            return "<a class=\"cl-sidebar-link" + (isActive ? " active" : string.Empty) + "\" href=\""
                + HttpUtility.HtmlAttributeEncode(GetPageUrl(pageName)) + "\""
                + (isActive ? " aria-current=\"page\"" : string.Empty) + ">"
                + iconHtml + label + "</a>";
        }

        private void EnsureStatusLoaded()
        {
            if (_statusLoaded)
            {
                return;
            }

            _statusLoaded = true;

            // One lookup gives both the approval status and the profile photo for the sidebar
            OperationResult<TutorProfile> result = new TutorManager().GetProfile();
            if (result.Success)
            {
                _approvalStatus = SessionHelper.IsUserInRole(UserRoles.Admin) ? TutorStatuses.Approved : result.Data.Status;
                _avatarUrl = result.Data.AvatarUrl;
            }
            else
            {
                _statusError = result.Message;
            }
        }

        private void ShowApprovalNotice()
        {
            if (IsApproved || HideApprovalNotice)
            {
                return;
            }

            // The tutor applies and checks their status on the Profile page
            const string actionPage = "Profile";
            string actionText = null;

            if (_statusError != null)
            {
                litNoticeTitle.Text = "Tutor status unavailable";
                litNoticeText.Text = _statusError;
            }
            else if (_approvalStatus == TutorStatuses.Pending)
            {
                litNoticeTitle.Text = "Your application is under review";
                litNoticeText.Text = "An administrator is reviewing your tutor application. Course authoring unlocks once it is approved.";
                actionText = "View application";
            }
            else if (_approvalStatus == TutorStatuses.Rejected)
            {
                litNoticeTitle.Text = "Your application was not approved";
                litNoticeText.Text = "Review your details and submit a new application to unlock course authoring.";
                actionText = "Update application";
            }
            else
            {
                litNoticeTitle.Text = "Apply to start teaching";
                litNoticeText.Text = "Submit your tutor application. Course authoring unlocks once an administrator approves it.";
                actionText = "Apply now";
            }

            if (actionText != null && IsPageAvailable(actionPage))
            {
                hlNoticeAction.NavigateUrl = TutorFolder + actionPage;
                hlNoticeAction.Text = actionText;
                hlNoticeAction.Visible = true;
            }

            phNotice.Visible = true;
        }
    }
}
