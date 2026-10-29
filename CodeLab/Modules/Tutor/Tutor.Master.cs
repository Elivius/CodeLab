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
        private string _fullName;

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

        /// <summary>
        /// The "New course" shortcut in the top bar: shown to approved tutors, except on the
        /// course builder itself.
        /// </summary>
        protected bool ShowCreateButton
        {
            get
            {
                return IsApproved
                    && IsPageAvailable("CourseEditor")
                    && !string.Equals(Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath), "CourseEditor",
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Link to Tutor.css with its last-changed time added, so browsers fetch the new file after
        /// every change instead of showing an old cached copy.
        /// </summary>
        protected string TutorCssUrl
        {
            get
            {
                const string cssPath = "~/Content/Tutor.css";
                long version = File.GetLastWriteTimeUtc(Server.MapPath(cssPath)).Ticks;
                return ResolveUrl(cssPath) + "?v=" + version;
            }
        }

        // The course builder steps: name, short description, page, and the query string before the course id.
        // Step 5 (Review & submit) is a view of the course page.
        private static readonly string[][] Steps =
        {
            new[] { "Details", "Title, category, cover", "CourseEditor", "?id=", "" },
            new[] { "Lessons", "Notes, code, video", "LessonEditor", "?courseId=", "" },
            new[] { "Quiz", "Check understanding", "QuizEditor", "?courseId=", "" },
            new[] { "Challenge", "Hands-on coding", "ChallengeEditor", "?courseId=", "" },
            new[] { "Submit", "Review and send", "CourseEditor", "?id=", "&step=submit" }
        };

        /// <summary>
        /// Browser URL of a course builder step (1-5) for a course, or null if that page does not exist yet.
        /// </summary>
        public string StepUrl(int number, int courseId)
        {
            string[] step = Steps[number - 1];
            return courseId > 0 && IsPageAvailable(step[2]) ? GetPageUrl(step[2]) + step[3] + courseId + step[4] : null;
        }

        /// <summary>
        /// The course builder's progress bar: Details > Lessons > Quiz > Challenge > Submit.
        /// current is the step this page is (1-5). Steps link to their page once the course exists;
        /// steps whose page is not built yet show as "Soon".
        /// </summary>
        public string BuilderSteps(int current, int courseId)
        {
            string[][] steps = Steps;

            System.Text.StringBuilder html = new System.Text.StringBuilder("<ol class=\"cl-steps\" aria-label=\"Course builder steps\">");
            for (int i = 0; i < steps.Length; i++)
            {
                int number = i + 1;
                string state = number == current ? "is-current" : (number < current ? "is-done" : string.Empty);
                string url = StepUrl(number, courseId);
                string dot = number < current
                    ? "<i data-lucide=\"check\" class=\"cl-icon\" aria-hidden=\"true\"></i>"
                    : number.ToString();
                string label = "<span class=\"cl-steps-dot\">" + dot + "</span>"
                    + "<span class=\"cl-steps-text\"><strong>" + HttpUtility.HtmlEncode(steps[i][0]) + "</strong><small>"
                    + HttpUtility.HtmlEncode(courseId > 0 && !IsPageAvailable(steps[i][2]) ? "Coming soon" : steps[i][1]) + "</small></span>";

                html.Append("<li class=\"").Append(state).Append(url == null && number != current ? " is-disabled" : string.Empty).Append("\"")
                    .Append(number == current ? " aria-current=\"step\"" : string.Empty).Append('>');
                if (url != null && number != current)
                {
                    html.Append("<a href=\"").Append(HttpUtility.HtmlAttributeEncode(url)).Append("\">").Append(label).Append("</a>");
                }
                else
                {
                    html.Append("<span class=\"cl-steps-item\">").Append(label).Append("</span>");
                }

                html.Append("</li>");
            }

            return html.Append("</ol>").ToString();
        }

        /// <summary>
        /// The bar at the bottom of a course builder step: "Back" to the previous step and "Next" to the
        /// following one. nextText replaces the default "Next: ..." label (e.g. "Skip quiz");
        /// blockedReason, when set, disables Next and shows why.
        /// </summary>
        public string StepNav(int current, int courseId, string nextText = null, string blockedReason = null)
        {
            System.Text.StringBuilder html = new System.Text.StringBuilder("<nav class=\"cl-step-nav\" aria-label=\"Course builder navigation\">");

            string backUrl = current > 1 ? StepUrl(current - 1, courseId) : null;
            if (backUrl != null)
            {
                html.Append("<a class=\"btn cl-btn-outline\" href=\"").Append(HttpUtility.HtmlAttributeEncode(backUrl)).Append("\">")
                    .Append("<i data-lucide=\"arrow-left\" class=\"cl-icon\" aria-hidden=\"true\"></i>")
                    .Append("<span>Back: ").Append(HttpUtility.HtmlEncode(Steps[current - 2][0])).Append("</span></a>");
            }
            else
            {
                html.Append("<span></span>");
            }

            string nextUrl = current < Steps.Length ? StepUrl(current + 1, courseId) : null;
            if (nextUrl != null)
            {
                string label = HttpUtility.HtmlEncode(nextText ?? "Next: " + Steps[current][0]);
                string arrow = "<i data-lucide=\"arrow-right\" class=\"cl-icon\" aria-hidden=\"true\"></i>";
                html.Append("<div class=\"cl-step-nav-next\">");
                if (blockedReason != null)
                {
                    html.Append("<small>").Append(HttpUtility.HtmlEncode(blockedReason)).Append("</small>")
                        .Append("<span class=\"btn btn-primary disabled\" aria-disabled=\"true\"><span>").Append(label).Append("</span>").Append(arrow).Append("</span>");
                }
                else
                {
                    html.Append("<a class=\"btn btn-primary\" href=\"").Append(HttpUtility.HtmlAttributeEncode(nextUrl)).Append("\"><span>")
                        .Append(label).Append("</span>").Append(arrow).Append("</a>");
                }

                html.Append("</div>");
            }

            return html.Append("</nav>").ToString();
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            new UserManager().RecordLogout(SessionHelper.GetCurrentUserID());
            SessionHelper.SignOut();
            Response.Redirect("~/Modules/Public/Login");
        }

        /// <summary>
        /// The tutor's full name from their profile. The session only keeps the username after the
        /// site restarts, so the profile is the reliable source. Pages use it through Master.TutorName.
        /// </summary>
        public string TutorName
        {
            get
            {
                EnsureStatusLoaded();
                return _fullName ?? SessionHelper.GetCurrentFullName() ?? string.Empty;
            }
        }

        /// <summary>
        /// Browser URL of the user's profile photo, or null to show their initials instead.
        /// </summary>
        public string AvatarSrc
        {
            get
            {
                EnsureStatusLoaded();
                return string.IsNullOrEmpty(_avatarUrl) ? null : ResolveUrl(_avatarUrl);
            }
        }

        public string Initials
        {
            get { return TutorDisplayHelper.Initials(TutorName, "T"); }
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

        // The role shown in the profile menu
        protected string RoleText
        {
            get { return SessionHelper.IsUserInRole(UserRoles.Admin) ? "Administrator" : "Tutor"; }
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
                _fullName = result.Data.FullName;
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
