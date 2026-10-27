using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Models;
using Newtonsoft.Json;

namespace CodeLab.Modules.Tutor
{
    public partial class Analytics : Page
    {
        // The course chosen in the filter, or 0 for all of the tutor's courses
        private int CourseId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["courseId"], out id) && id > 0 ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // This page explains the locked state itself
            Master.HideApprovalNotice = true;

            if (IsPostBack)
            {
                return;
            }

            if (!Master.IsApproved)
            {
                litSubheading.Text = "Learner analytics unlock once your tutor application is approved.";
                phLocked.Visible = true;
                return;
            }

            OperationResult<TutorDashboard> dashboard = new TutorManager().GetDashboard();
            List<TutorCourseSummary> courses = dashboard.Success ? dashboard.Data.Courses : new List<TutorCourseSummary>();

            OperationResult<TutorAnalytics> result = new AnalyticsManager().GetAnalytics(CourseId);
            if (!result.Success)
            {
                ShowError(CourseId > 0 ? "That course could not be found. Showing all of your courses instead." : result.Message);
                if (CourseId == 0)
                {
                    return;
                }

                // Fall back to all courses when the link points at a course that is not the tutor's
                result = new AnalyticsManager().GetAnalytics(0);
                if (!result.Success)
                {
                    return;
                }
            }

            TutorAnalytics analytics = result.Data;
            TutorCourseSummary selected = courses.Find(c => c.Id == CourseId);
            bool oneCourse = selected != null;

            litSubheading.Text = oneCourse
                ? "Learner activity in " + selected.Title + "."
                : "Learner activity across all of your courses.";

            BindFilter(courses, oneCourse ? selected.Id : 0);
            BindSummary(analytics);
            BindTables(analytics);

            litProgressTitle.Text = oneCourse ? "Lesson completion" : "Enrolment and completion";
            litProgressNote.Text = oneCourse
                ? "Learners who finished each lesson, in order. A big drop shows where learners stop."
                : "Learners enrolled in each course, and how many finished it.";
            litChartJson.Text = BuildChartJson(analytics, oneCourse);

            phNoActivity.Visible = !analytics.HasLearnerActivity;
            phMain.Visible = true;
        }

        private void BindFilter(List<TutorCourseSummary> courses, int selectedId)
        {
            StringBuilder options = new StringBuilder();
            options.Append("<option value=\"\">All my courses</option>");
            foreach (TutorCourseSummary course in courses.OrderBy(c => c.Title))
            {
                options.Append("<option value=\"").Append(course.Id).Append('"')
                    .Append(course.Id == selectedId ? " selected" : string.Empty).Append('>')
                    .Append(HttpUtility.HtmlEncode(course.Title)).Append("</option>");
            }

            litCourseOptions.Text = options.ToString();
            phFilter.Visible = courses.Count > 0;
        }

        private void BindSummary(TutorAnalytics a)
        {
            litStudents.Text = a.StudentCount.ToString("N0");
            litStudentsNote.Text = a.EnrollmentCount == 1 ? "1 enrolment" : a.EnrollmentCount.ToString("N0") + " enrolments";

            litCompletion.Text = Percent(a.CompletedCount, a.EnrollmentCount);
            litCompletionNote.Text = a.EnrollmentCount == 0
                ? "No enrolments yet"
                : a.CompletedCount + " finished · avg progress " + Math.Round(a.AverageProgress) + "%";

            litAverageScore.Text = ScoreText(a.AverageQuizScore);
            litAverageScoreNote.Text = a.QuizAttemptCount == 1 ? "From 1 attempt" : "From " + a.QuizAttemptCount.ToString("N0") + " attempts";

            litPassRate.Text = Percent(a.QuizPassedCount, a.QuizAttemptCount);
            litPassRateNote.Text = a.QuizAttemptCount == 0 ? "No attempts yet" : a.QuizPassedCount + " of " + a.QuizAttemptCount + " passed";

            litSubmissions.Text = a.SubmissionCount.ToString("N0");
            litSubmissionsNote.Text = a.SubmissionCount == 0
                ? "No submissions yet"
                : Percent(a.SubmissionPassedCount, a.SubmissionCount) + " passed · " + a.SolverCount + (a.SolverCount == 1 ? " solver" : " solvers");

            litUnanswered.Text = a.UnansweredPostCount.ToString("N0");
        }

        private void BindTables(TutorAnalytics a)
        {
            Bind(rptQuizzes, phNoQuizzes, a.Quizzes);
            Bind(rptChallenges, phNoChallenges, a.Challenges);
            Bind(rptAttempts, phNoAttempts, a.RecentAttempts);
            Bind(rptSubmissions, phNoSubmissions, a.RecentSubmissions);
            Bind(rptTopLearners, phNoTopLearners, a.TopLearners);
            Bind(rptUnanswered, phAllAnswered, a.UnansweredPosts);
        }

        private static void Bind<T>(System.Web.UI.WebControls.Repeater repeater, Control emptyState, List<T> rows)
        {
            repeater.DataSource = rows;
            repeater.DataBind();
            repeater.Visible = rows.Count > 0;
            emptyState.Visible = rows.Count == 0;
        }

        // The numbers for the four charts. Course titles are escaped so they cannot close the script tag.
        private static string BuildChartJson(TutorAnalytics a, bool oneCourse)
        {
            object data = new
            {
                courses = new
                {
                    labels = a.CourseProgress.Select(c => Shorten(c.Title, 28)).ToArray(),
                    enrolled = a.CourseProgress.Select(c => c.Enrolled).ToArray(),
                    completed = a.CourseProgress.Select(c => c.Completed).ToArray()
                },
                lessons = oneCourse
                    ? new
                    {
                        labels = a.LessonFunnel.Select(l => l.OrderIndex + ". " + Shorten(l.Title, 22)).ToArray(),
                        values = a.LessonFunnel.Select(l => l.Completions).ToArray()
                    }
                    : null,
                scores = a.ScoreBuckets,
                activity = new
                {
                    labels = a.WeeklyActivity.Select(w => w.WeekStart.ToString("d MMM", CultureInfo.InvariantCulture)).ToArray(),
                    quiz = a.WeeklyActivity.Select(w => w.QuizAttempts).ToArray(),
                    code = a.WeeklyActivity.Select(w => w.Submissions).ToArray()
                },
                code = new[] { a.SubmissionPassedCount, a.SubmissionFailedCount, a.SubmissionErrorCount }
            };

            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }

        protected static string Percent(int part, int total)
        {
            return total == 0 ? "–" : Math.Round(part * 100.0 / total) + "%";
        }

        protected static string ScoreText(double? score)
        {
            return score.HasValue ? Math.Round(score.Value) + "%" : "–";
        }

        protected static string ScoreText(double score)
        {
            return Math.Round(score) + "%";
        }

        // Pass rate with a small bar; green once it reaches the quiz's pass mark
        protected static string PassRateHtml(TutorAnalytics.QuizStatRow row)
        {
            if (row.Attempts == 0)
            {
                return "<span class=\"text-muted\">No attempts</span>";
            }

            int rate = (int)Math.Round(row.Passed * 100.0 / row.Attempts);
            return RateBar(rate, rate >= row.PassMark ? "is-good" : "is-low", "pass mark " + row.PassMark + "%");
        }

        protected static string SolveRateHtml(TutorAnalytics.ChallengeStatRow row)
        {
            if (row.Learners == 0)
            {
                return "<span class=\"text-muted\">No submissions</span>";
            }

            int rate = (int)Math.Round(row.Solvers * 100.0 / row.Learners);
            return RateBar(rate, rate >= 50 ? "is-good" : "is-low", row.Solvers + " of " + row.Learners + " learners");
        }

        private static string RateBar(int rate, string css, string title)
        {
            return "<div class=\"cl-rate " + css + "\" title=\"" + HttpUtility.HtmlAttributeEncode(title) + "\">"
                + "<span class=\"cl-rate-track\"><span style=\"width:" + Math.Max(0, Math.Min(100, rate)) + "%\"></span></span>"
                + "<strong>" + rate + "%</strong></div>";
        }

        protected static string SubmissionStatusHtml(string status)
        {
            switch (status)
            {
                case "Passed": return "<span class=\"cl-badge cl-status cl-badge-success\">Passed</span>";
                case "Failed": return "<span class=\"cl-badge cl-status cl-badge-warning\">Failed</span>";
                default: return "<span class=\"cl-badge cl-status cl-badge-danger\">" + HttpUtility.HtmlEncode(status) + "</span>";
            }
        }

        // Learner's photo (or initials) and name
        protected string LearnerHtml(string name, string avatarUrl)
        {
            string avatar = string.IsNullOrEmpty(avatarUrl)
                ? "<span class=\"cl-avatar cl-avatar-sm\" aria-hidden=\"true\">" + HttpUtility.HtmlEncode(Initials(name)) + "</span>"
                : "<img class=\"cl-avatar cl-avatar-sm\" src=\"" + HttpUtility.HtmlAttributeEncode(ResolveUrl(avatarUrl)) + "\" alt=\"\" />";
            return "<span class=\"cl-learner\">" + avatar + "<span>" + HttpUtility.HtmlEncode(name) + "</span></span>";
        }

        protected static string LanguageName(string key)
        {
            KeyValuePair<string, string> language = LessonOptions.CodeLanguages.Find(l => l.Key == key);
            return language.Key != null ? language.Value : key;
        }

        // e.g. "5 min ago", "3 days ago", "12 Oct 2026". Times are stored in UTC.
        protected static string TimeAgo(DateTime utc)
        {
            TimeSpan age = DateTime.UtcNow - DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            if (age.TotalMinutes < 1) return "Just now";
            if (age.TotalHours < 1) return (int)age.TotalMinutes + " min ago";
            if (age.TotalDays < 1) return (int)age.TotalHours + (age.TotalHours < 2 ? " hour ago" : " hours ago");
            if (age.TotalDays < 7) return (int)age.TotalDays + (age.TotalDays < 2 ? " day ago" : " days ago");
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("d MMM yyyy");
        }

        private static string Initials(string name)
        {
            string[] words = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "?";
            string initials = words[0].Substring(0, 1);
            if (words.Length > 1) initials += words[words.Length - 1].Substring(0, 1);
            return initials.ToUpperInvariant();
        }

        private static string Shorten(string text, int max)
        {
            return text.Length <= max ? text : text.Substring(0, max - 1).TrimEnd() + "…";
        }

        private void ShowError(string message)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.Visible = true;
        }
    }
}
