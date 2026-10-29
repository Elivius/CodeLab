using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;

namespace CodeLab.Modules.Learner
{
    public partial class Workspace : Page
    {
        private readonly LearnerChallengeManager challengeManager =
            new LearnerChallengeManager();

        private readonly LearnerCourseManager courseManager =
            new LearnerCourseManager();

        private readonly LearnerManager learnerManager =
            new LearnerManager();

        private bool authorised;

        protected void Page_Load(object sender, EventArgs e)
        {
            authorised =
                SessionHelper.IsLoggedIn() &&
                SessionHelper.IsUserInRole("Learner") &&
                SessionHelper.GetCurrentUserID() > 0;

            if (!authorised)
            {
                Response.Redirect(
                    ResolveUrl("~/Modules/Public/Login"),
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            pnlError.Visible = false;
            litError.Text = string.Empty;

            if (!IsPostBack)
            {
                BindNavigation();
                BindWorkspace(true);
            }
        }

        private void BindNavigation()
        {
            string fullName = SessionHelper.GetCurrentFullName();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "Learner";
            }

            fullName = fullName.Trim();

            // Markup literals use Mode="Encode".
            litNavName.Text = fullName;
            litNavInitial.Text =
                StringInfo.GetNextTextElement(fullName).ToUpperInvariant();

            litNavLevel.Text = "—";
            litNavPoints.Text = "—";

            try
            {
                var dashboard = learnerManager.GetDashboard(
                    SessionHelper.GetCurrentUserID());

                litNavLevel.Text = dashboard.Level.ToString(
                    CultureInfo.InvariantCulture);

                string points = dashboard.Points.ToString(
                    "N0",
                    CultureInfo.InvariantCulture);

                litNavPoints.Text = points;
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Workspace learner details could not load: {0}", ex);

                ShowError(
                    "Your level and XP are temporarily unavailable.");
            }
        }

        private void BindWorkspace(bool initialiseFilters)
        {
            if (!authorised)
            {
                return;
            }

            try
            {
                int learnerId = SessionHelper.GetCurrentUserID();

                var allChallenges = challengeManager.List(learnerId);

                // Load courses independently of their challenge records.
                // This follows the existing enrolled-course visibility rules.
                var enrolledCourses = courseManager.List(
                    learnerId,
                    true,
                    string.Empty,
                    string.Empty);

                if (initialiseFilters)
                {
                    ddlCourse.Items.Clear();
                    ddlCourse.Items.Add(
                        new ListItem("All Enrolled Courses", "0"));

                    foreach (var course in enrolledCourses
                        .OrderBy(course => course.Title)
                        .ThenBy(course => course.Id))
                    {
                        ddlCourse.Items.Add(new ListItem(
                            course.Title,
                            course.Id.ToString(
                                CultureInfo.InvariantCulture)));
                    }

                    int requestedCourse;
                    if(int.TryParse(Request.QueryString["courseId"],out requestedCourse)) {
                        var selected=ddlCourse.Items.FindByValue(requestedCourse.ToString(CultureInfo.InvariantCulture));
                        if(selected!=null) ddlCourse.SelectedValue=selected.Value;
                    }
                    ddlLanguage.Items.Clear();
                    ddlLanguage.Items.Add(
                        new ListItem("All Languages", string.Empty));

                    var languages = allChallenges
                        .Select(challenge => challenge.Language)
                        .Where(language =>
                            !string.IsNullOrWhiteSpace(language))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(
                            language => language,
                            StringComparer.OrdinalIgnoreCase);

                    foreach (string language in languages)
                    {
                        ddlLanguage.Items.Add(
                            new ListItem(language, language));
                    }
                }

                int courseId;

                if (!int.TryParse(ddlCourse.SelectedValue, out courseId) ||
                    courseId < 0)
                {
                    courseId = 0;
                }

                string search = (txtSearch.Text ?? string.Empty).Trim();
                string difficulty = ddlDifficulty.SelectedValue;
                string progress = ddlProgress.SelectedValue;
                string languageFilter = ddlLanguage.SelectedValue;

                bool hasManagerFilters =
                    search.Length > 0 ||
                    courseId > 0 ||
                    !string.IsNullOrEmpty(difficulty) ||
                    !string.IsNullOrEmpty(progress);

                var filteredChallenges = hasManagerFilters
                    ? challengeManager.List(
                        learnerId,
                        search,
                        courseId,
                        difficulty,
                        progress)
                    : allChallenges;

                if (!string.IsNullOrEmpty(languageFilter))
                {
                    filteredChallenges = filteredChallenges
                        .Where(challenge => string.Equals(
                            challenge.Language,
                            languageFilter,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                rptChallenges.DataSource = filteredChallenges;
                rptChallenges.DataBind();

                litResultCount.Text =
                    "Showing " +
                    filteredChallenges.Count.ToString(
                        CultureInfo.InvariantCulture) +
                    (filteredChallenges.Count == 1
                        ? " coding challenge"
                        : " coding challenges");

                pnlEmpty.Visible = filteredChallenges.Count == 0;

                if (pnlEmpty.Visible)
                {
                    bool selectedCourseHasChallenges =
                        courseId > 0 &&
                        allChallenges.Any(challenge =>
                            challenge.CourseId == courseId);

                    if (courseId > 0 && !selectedCourseHasChallenges)
                    {
                        litEmptyTitle.Text =
                            "No challenges available for this course yet.";

                        litEmptyMessage.Text =
                            "Complete a course to unlock its coding challenges.";
                    }
                    else if (allChallenges.Count == 0)
                    {
                        litEmptyTitle.Text =
                            "No challenges available yet";

                        litEmptyMessage.Text = enrolledCourses.Count == 0
                            ? "Complete a course to unlock its coding challenges."
                            : "Challenges will appear here when they are " +
                              "available for your enrolled courses.";
                    }
                    else
                    {
                        litEmptyTitle.Text = "No matching challenges";

                        litEmptyMessage.Text =
                            "Try another search or clear the filters to " +
                            "see all available challenges.";
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Workspace challenges could not load: {0}", ex);

                rptChallenges.DataSource = null;
                rptChallenges.DataBind();

                pnlEmpty.Visible = false;
                litResultCount.Text = string.Empty;


                ShowError(
                    "Challenges are temporarily unavailable. " +
                    "Please try again.");
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindWorkspace(false);
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            BindWorkspace(false);
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            if (!authorised)
            {
                return;
            }

            txtSearch.Text = string.Empty;

            ddlCourse.SelectedIndex = 0;
            ddlDifficulty.SelectedIndex = 0;
            ddlProgress.SelectedIndex = 0;
            ddlLanguage.SelectedIndex = 0;

            BindWorkspace(false);
        }

        private void ShowError(string message)
        {
            if (pnlError.Visible &&
                !string.IsNullOrEmpty(litError.Text))
            {
                litError.Text += " " + message;
            }
            else
            {
                litError.Text = message;
            }

            pnlError.Visible = true;
        }
    }
}