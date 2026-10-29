using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    /// <summary>
    /// The course builder's step 1 (Details: create or edit a Draft) and step 5 (Review & submit,
    /// CourseEditor?id=1&amp;step=submit). A course that is no longer a Draft is shown read-only.
    /// </summary>
    public partial class CourseEditor : Page
    {
        private const string EditorUrl = "~/Modules/Tutor/CourseEditor";

        // Browser URL of the cover image, or null to show the default placeholder
        protected string CoverSrc { get; private set; }

        protected string PlaceholderLetter { get; private set; } = "C";

        // Background colour behind the cover letter, picked by course id like on the dashboard
        protected string CoverToneCss { get; private set; } = "cl-cover-tone-1";

        protected int LessonCount { get; private set; }

        // The rows of the "Course content" checklist on step 5
        protected string DetailsReadinessHtml { get; private set; }

        protected string LessonsReadinessHtml { get; private set; }

        protected string QuizReadinessHtml { get; private set; }

        protected string ChallengeReadinessHtml { get; private set; }

        // The bar at the bottom of the page: back to the previous step, on to the next one
        protected string StepNavHtml { get; private set; }

        // The course being edited, or 0 when creating a new one
        protected int CourseId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0;
            }
        }

        // True on step 5 (Review & submit) of an existing course
        protected bool IsSubmitStep
        {
            get { return CourseId > 0 && Request.QueryString["step"] == "submit"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // This page explains the locked state itself
            Master.HideApprovalNotice = true;

            if (IsPostBack)
            {
                return;
            }

            BindOptions();

            if (Request.QueryString["saved"] == "1")
            {
                ShowSuccess("Your changes have been saved.");
            }
            else if (Request.QueryString["submitted"] == "1")
            {
                ShowSuccess("Your course has been submitted for review. An administrator will check it soon.");
            }

            LoadPage(true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            SaveCourse(false);
        }

        protected void btnSaveContinue_Click(object sender, EventArgs e)
        {
            SaveCourse(true);
        }

        /// <summary>
        /// Creates or updates the course. A new course always continues to step 2 (Lessons);
        /// an existing one stays on this page unless continueToLessons is true.
        /// </summary>
        private void SaveCourse(bool continueToLessons)
        {
            Page.Validate("Course");
            if (!Page.IsValid)
            {
                LoadPage(false);
                return;
            }

            Course course = new Course
            {
                Id = CourseId,
                Title = txtTitle.Text,
                Description = txtDescription.Text,
                Category = ddlCategory.SelectedValue,
                Difficulty = ddlDifficulty.SelectedValue
            };

            bool hasCover = fuCover.HasFile;
            string coverName = hasCover ? fuCover.PostedFile.FileName : null;
            int coverLength = hasCover ? fuCover.PostedFile.ContentLength : 0;
            System.IO.Stream coverStream = hasCover ? fuCover.PostedFile.InputStream : null;

            CourseManager manager = new CourseManager();

            if (CourseId == 0)
            {
                OperationResult<int> created = manager.CreateCourse(course, coverName, coverLength, coverStream);
                if (!created.Success)
                {
                    ShowError(created.Message);
                    LoadPage(false);
                    return;
                }

                // Redirect so a refresh cannot create the course twice
                Response.Redirect(Master.StepUrl(2, created.Data) + "&courseCreated=1");
                return;
            }

            OperationResult updated = manager.UpdateCourse(course, coverName, coverLength, coverStream, chkRemoveCover.Checked);
            if (!updated.Success)
            {
                ShowError(updated.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(continueToLessons
                ? Master.StepUrl(2, CourseId) + "&detailsSaved=1"
                : EditorUrl + "?id=" + CourseId + "&saved=1");
        }

        protected void btnSubmitReview_Click(object sender, EventArgs e)
        {
            OperationResult result = new CourseManager().SubmitForReview(CourseId);
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            // The course is now Pending Review, so step 5 reloads as a read-only summary
            Response.Redirect(EditorUrl + "?id=" + CourseId + "&step=submit&submitted=1");
        }

        private void BindOptions()
        {
            ddlCategory.Items.Add(new ListItem("Select a category", string.Empty));
            foreach (string category in CourseOptions.Categories)
            {
                ddlCategory.Items.Add(new ListItem(category, category));
            }

            foreach (string difficulty in CourseOptions.Difficulties)
            {
                ddlDifficulty.Items.Add(new ListItem(difficulty, difficulty));
            }
        }

        /// <summary>
        /// Decides what the page shows: the locked notice, the create form, the edit form for a Draft,
        /// step 5 (Review & submit), or a read-only summary for a course that is no longer a Draft.
        /// fillForm copies the saved values into the form; it is false after a failed save so what the
        /// tutor typed is kept.
        /// </summary>
        private void LoadPage(bool fillForm)
        {
            if (!Master.IsApproved)
            {
                litHeading.Text = "Create a new course";
                litSubheading.Text = "Course authoring unlocks once your tutor application is approved.";
                phLocked.Visible = true;
                return;
            }

            Course course = null;
            if (CourseId > 0)
            {
                OperationResult<Course> result = new CourseManager().GetCourse(CourseId);
                if (!result.Success)
                {
                    litHeading.Text = "Course not found";
                    litSubheading.Text = "Check the link, or go back to your dashboard.";
                    phNotFound.Visible = true;
                    return;
                }

                course = result.Data;
            }

            if (course == null)
            {
                ShowCreateForm();
                return;
            }

            ShowCourseHeader(course);
            bool editable = CourseStatuses.IsEditable(course.Status);

            if (IsSubmitStep)
            {
                ShowSummary(course);
                if (editable)
                {
                    ShowReview(course);
                }
                else
                {
                    ShowEditingClosed(course);
                }

                StepNavHtml = Master.StepNav(5, course.Id);
            }
            else if (editable)
            {
                ShowEditForm(course, fillForm);
            }
            else
            {
                ShowSummary(course);
                ShowEditingClosed(course);
                StepNavHtml = Master.StepNav(1, course.Id);
            }
        }

        private void ShowCreateForm()
        {
            litHeading.Text = "Create a new course";
            litSubheading.Text = "Start with the basics. You can change them any time while the course is a draft.";
            btnSave.Visible = false;
            btnSaveContinue.Text = "Create & continue";
            phForm.Visible = true;
            phNextSteps.Visible = true;
        }

        private void ShowCourseHeader(Course course)
        {
            litHeading.Text = course.Title;
            CoverSrc = string.IsNullOrEmpty(course.ThumbnailUrl) ? null : ResolveUrl(course.ThumbnailUrl);
            PlaceholderLetter = GetLetter(course.Title);
            CoverToneCss = "cl-cover-tone-" + (course.Id % 4 + 1);

            OperationResult<List<Lesson>> lessons = new LessonManager().GetLessons(course.Id);
            LessonCount = lessons.Success ? lessons.Data.Count : 0;

            lblCourseStatus.Text = course.Status;
            lblCourseStatus.CssClass = "cl-badge cl-status cl-badge-lg " + TutorDisplayHelper.CourseStatusCss(course.Status);
            lblCourseStatus.Visible = true;
        }

        private void ShowEditForm(Course course, bool fillForm)
        {
            litSubheading.Text = "Edit your draft. Last updated " + course.UpdatedAt.ToLocalTime().ToString("d MMM yyyy") + ".";
            btnSaveContinue.Text = "Save & continue";
            phForm.Visible = true;

            phRemoveCover.Visible = CoverSrc != null;

            if (fillForm)
            {
                txtTitle.Text = course.Title;
                txtDescription.Text = course.Description;
                SelectIfPresent(ddlCategory, course.Category);
                SelectIfPresent(ddlDifficulty, course.Difficulty);
            }

            if (ShowRejection(course))
            {
                litSubheading.Text = "An administrator asked for changes. Fix them, then submit the course again.";
            }
        }

        // An admin rejected the course: it is back in Draft with their feedback attached
        private bool ShowRejection(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.RejectionReason))
            {
                return false;
            }

            litRejectedReason.Text = course.RejectionReason;
            phRejected.Visible = true;
            return true;
        }

        // The course card shown on step 5 and for courses that can no longer be edited
        private void ShowSummary(Course course)
        {
            litReadOnlyTitle.Text = course.Title;
            litReadOnlyCategory.Text = course.Category;
            litReadOnlyDifficulty.Text = course.Difficulty;
            litReadOnlyDescription.Text = course.Description;
            phSummary.Visible = true;
        }

        /// <summary>
        /// Step 5 for a Draft: the content checklist and the Submit for review button.
        /// The course needs a lesson, and a quiz (if it has one) needs a question.
        /// </summary>
        private void ShowReview(Course course)
        {
            litSubheading.Text = ShowRejection(course)
                ? "Check the admin's feedback has been dealt with, then submit the course again."
                : "Check everything is ready, then send the course to an administrator.";

            DetailsReadinessHtml = ReadinessItem("is-ready", "Course details", null, 1, course.Id);

            bool lessonsReady = LessonCount > 0;
            LessonsReadinessHtml = ReadinessItem(lessonsReady ? "is-ready" : "is-missing",
                lessonsReady ? (LessonCount == 1 ? "1 lesson added" : LessonCount + " lessons added") : "Add at least one lesson",
                null, 2, course.Id);

            // Quiz: optional, but once created it needs at least one question
            OperationResult<Quiz> quiz = new QuizManager().GetQuiz(course.Id);
            bool hasQuiz = quiz.Success && quiz.Data != null;
            int questionCount = hasQuiz ? quiz.Data.Questions.Count : 0;
            bool quizReady = !hasQuiz || questionCount > 0;
            QuizReadinessHtml = ReadinessItem(
                !hasQuiz ? "is-optional" : (quizReady ? "is-ready" : "is-missing"),
                !hasQuiz ? "Quiz" : "Quiz · " + (questionCount == 1 ? "1 question" : questionCount + " questions"),
                !hasQuiz ? "(optional)" : (quizReady ? null : "add a question"),
                3, course.Id);

            OperationResult<CodingChallenge> challenge = new ChallengeManager().GetChallenge(course.Id);
            bool hasChallenge = challenge.Success && challenge.Data != null;
            ChallengeReadinessHtml = ReadinessItem(
                hasChallenge ? "is-ready" : "is-optional",
                hasChallenge ? "Coding challenge added" : "Coding challenge",
                hasChallenge ? null : "(optional)",
                4, course.Id);

            string blocked = !lessonsReady ? "Add at least one lesson before submitting."
                : (quizReady ? null : "Add at least one question to the quiz, or delete the quiz, before submitting.");
            btnSubmitReview.Enabled = blocked == null;
            btnSubmitReview.Text = string.IsNullOrWhiteSpace(course.RejectionReason) ? "Submit for review" : "Submit again";
            lblSubmitBlocked.Text = HttpUtility.HtmlEncode(blocked ?? string.Empty);
            lblSubmitBlocked.Visible = blocked != null;

            phChecklist.Visible = true;
            phSubmit.Visible = true;
        }

        // One row of the checklist, linking to the builder step where it is set up
        private string ReadinessItem(string state, string text, string note, int step, int courseId)
        {
            string icon = state == "is-ready" ? "circle-check" : (state == "is-missing" ? "circle-alert" : "circle-dashed");
            string label = HttpUtility.HtmlEncode(text)
                + (note == null ? string.Empty : " <small>" + HttpUtility.HtmlEncode(note) + "</small>");
            string url = Master.StepUrl(step, courseId);
            if (url != null)
            {
                label = "<a href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\">" + label + "</a>";
            }

            return "<li class=\"" + state + "\"><i data-lucide=\"" + icon + "\" class=\"cl-icon\" aria-hidden=\"true\"></i><span>"
                + label + "</span></li>";
        }

        // The "Editing is closed" card for a course that is not a Draft
        private void ShowEditingClosed(Course course)
        {
            litSubheading.Text = "This course can no longer be edited.";

            switch (course.Status)
            {
                case CourseStatuses.PendingReview:
                    litReadOnlyNote.Text = "This course is waiting for an administrator to review it. Only Draft courses can be edited.";
                    break;
                case CourseStatuses.Published:
                    litReadOnlyNote.Text = "This course is published and visible to learners. Only Draft courses can be edited.";
                    break;
                case CourseStatuses.Locked:
                    litReadOnlyNote.Text = "An administrator locked this course"
                        + (string.IsNullOrWhiteSpace(course.LockReason) ? "." : ": " + course.LockReason);
                    break;
                default:
                    litReadOnlyNote.Text = "Only Draft courses can be edited.";
                    break;
            }

            phEditingClosed.Visible = true;
        }

        private static void SelectIfPresent(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value);
            if (item != null)
            {
                list.ClearSelection();
                item.Selected = true;
            }
        }

        private static string GetLetter(string title)
        {
            return string.IsNullOrWhiteSpace(title) ? "C" : title.Trim().Substring(0, 1).ToUpperInvariant();
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
    }
}
