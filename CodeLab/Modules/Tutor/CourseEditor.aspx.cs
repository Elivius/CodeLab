using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    public partial class CourseEditor : Page
    {
        private const string EditorUrl = "~/Modules/Tutor/CourseEditor";
        private const string LessonPage = "LessonEditor";

        // Browser URL of the cover image, or null to show the default placeholder
        protected string CoverSrc { get; private set; }

        protected string PlaceholderLetter { get; private set; } = "C";

        // Background colour behind the cover letter, picked by course id like on the dashboard
        protected string CoverToneCss { get; private set; } = "cl-cover-tone-1";

        // True once the course has at least one lesson, which it needs before it can be submitted
        protected bool LessonsReady { get; private set; }

        protected int LessonCount { get; private set; }

        // The course being edited, or 0 when creating a new one
        protected int CourseId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["id"], out id) && id > 0 ? id : 0;
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

            BindOptions();

            if (Request.QueryString["created"] == "1")
            {
                ShowSuccess("Your course has been created as a draft. Fill in the rest, then add lessons.");
            }
            else if (Request.QueryString["saved"] == "1")
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

                // Reload on the new course so a refresh cannot create it twice
                Response.Redirect(EditorUrl + "?id=" + created.Data + "&created=1");
                return;
            }

            OperationResult updated = manager.UpdateCourse(course, coverName, coverLength, coverStream, chkRemoveCover.Checked);
            if (!updated.Success)
            {
                ShowError(updated.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?id=" + CourseId + "&saved=1");
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

            // The course is now Pending Review, so the page reloads as a read-only summary
            Response.Redirect(EditorUrl + "?id=" + CourseId + "&submitted=1");
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
        /// or a read-only summary for a course that is no longer a Draft. fillForm copies the saved
        /// values into the form; it is false after a failed save so what the tutor typed is kept.
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

            if (CourseStatuses.IsEditable(course.Status))
            {
                ShowEditForm(course, fillForm);
            }
            else
            {
                ShowReadOnly(course);
            }
        }

        private void ShowCreateForm()
        {
            litHeading.Text = "Create a new course";
            litSubheading.Text = "Start with the basics. You can change them any time while the course is a draft.";
            btnSave.Text = "Create course";
            phForm.Visible = true;
            phNextSteps.Visible = true;
        }

        private void ShowCourseHeader(Course course)
        {
            litHeading.Text = course.Title;
            CoverSrc = string.IsNullOrEmpty(course.ThumbnailUrl) ? null : ResolveUrl(course.ThumbnailUrl);
            PlaceholderLetter = GetLetter(course.Title);
            CoverToneCss = "cl-cover-tone-" + (course.Id % 4 + 1);

            OperationResult<System.Collections.Generic.List<Lesson>> lessons = new LessonManager().GetLessons(course.Id);
            LessonCount = lessons.Success ? lessons.Data.Count : 0;

            lblCourseStatus.Text = course.Status;
            lblCourseStatus.CssClass = "cl-badge cl-status cl-badge-lg " + StatusCss(course.Status);
            lblCourseStatus.Visible = true;
        }

        private void ShowEditForm(Course course, bool fillForm)
        {
            litSubheading.Text = "Edit your draft. Last updated " + course.UpdatedAt.ToLocalTime().ToString("d MMM yyyy") + ".";
            btnSave.Text = "Save changes";
            phForm.Visible = true;

            phRemoveCover.Visible = CoverSrc != null;

            if (fillForm)
            {
                txtTitle.Text = course.Title;
                txtDescription.Text = course.Description;
                SelectIfPresent(ddlCategory, course.Category);
                SelectIfPresent(ddlDifficulty, course.Difficulty);
            }

            // An admin rejected the course: it is back in Draft with their feedback attached
            if (!string.IsNullOrWhiteSpace(course.RejectionReason))
            {
                litSubheading.Text = "An administrator asked for changes. Fix them, then submit the course again.";
                litRejectedReason.Text = course.RejectionReason;
                phRejected.Visible = true;
            }

            ShowSubmitCard(course);
        }

        // The "Submit for review" card: what is ready, a link to the lessons, and the submit button
        private void ShowSubmitCard(Course course)
        {
            int lessonCount = LessonCount;
            LessonsReady = lessonCount > 0;
            litLessonsReady.Text = LessonsReady
                ? (lessonCount == 1 ? "1 lesson added" : lessonCount + " lessons added")
                : "Add at least one lesson";

            btnSubmitReview.Enabled = LessonsReady;
            btnSubmitReview.ToolTip = LessonsReady ? string.Empty : "Add at least one lesson first.";
            btnSubmitReview.Text = string.IsNullOrWhiteSpace(course.RejectionReason) ? "Submit for review" : "Submit again";

            if (Master.IsPageAvailable(LessonPage))
            {
                hlNextStep.Text = LessonsReady ? "Manage lessons" : "Add lessons";
                hlNextStep.NavigateUrl = Master.GetPageUrl(LessonPage) + "?courseId=" + course.Id;
            }
            else
            {
                hlNextStep.Visible = false;
            }

            phSubmit.Visible = true;
        }

        private void ShowReadOnly(Course course)
        {
            litSubheading.Text = "This course can no longer be edited.";
            litReadOnlyTitle.Text = course.Title;
            litReadOnlyCategory.Text = course.Category;
            litReadOnlyDifficulty.Text = course.Difficulty;
            litReadOnlyDescription.Text = course.Description;

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

            phReadOnly.Visible = true;
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

        // Traffic-light colours: green = live, amber = waiting, red = blocked
        private static string StatusCss(string status)
        {
            switch (status)
            {
                case CourseStatuses.Published: return "cl-badge-success";
                case CourseStatuses.PendingReview: return "cl-badge-warning";
                case CourseStatuses.Locked: return "cl-badge-danger";
                default: return "cl-badge-neutral";
            }
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
