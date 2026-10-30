using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    public partial class LessonEditor : Page
    {
        private const string EditorUrl = "~/Modules/Tutor/LessonEditor";
        private const string CourseEditorUrl = "~/Modules/Tutor/CourseEditor";

        // A Draft course's lessons can be changed; any other status is read-only
        protected bool CanEdit { get; private set; }

        // The bar at the bottom of the page: back to the previous step, on to the next one
        protected string StepNavHtml { get; private set; }

        protected int CourseId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["courseId"], out id) && id > 0 ? id : 0;
            }
        }

        // The lesson open in the form, or 0 when adding a new one
        private int LessonId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["lessonId"], out id) && id > 0 ? id : 0;
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

            ddlLanguage.Items.Add(new ListItem("None", string.Empty));
            foreach (KeyValuePair<string, string> language in LessonOptions.CodeLanguages)
            {
                ddlLanguage.Items.Add(new ListItem(language.Value, language.Key));
            }

            if (Request.QueryString["created"] == "1")
            {
                ShowSuccess("The lesson has been added. Add the next one, or pick a lesson to edit it.");
            }
            else if (Request.QueryString["saved"] == "1")
            {
                ShowSuccess("Your changes have been saved.");
            }
            else if (Request.QueryString["deleted"] == "1")
            {
                ShowSuccess("The lesson has been deleted.");
            }
            else if (Request.QueryString["courseCreated"] == "1")
            {
                // Arrived from step 1 with "Create & continue"
                ShowSuccess("Your course has been created as a draft. Now add your first lesson.");
            }
            else if (Request.QueryString["detailsSaved"] == "1")
            {
                // Arrived from step 1 with "Save & continue"
                ShowSuccess("Course details saved.");
            }

            LoadPage(true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("Lesson");
            if (!Page.IsValid)
            {
                LoadPage(false);
                return;
            }

            Lesson lesson = new Lesson
            {
                Id = LessonId,
                CourseId = CourseId,
                Title = txtTitle.Text,
                Content = txtContent.Text,
                CodeSnippet = txtCode.Text,
                VideoUrl = txtVideoUrl.Text,
                CodeLanguage = ddlLanguage.SelectedValue
            };

            bool hasFile = fuAttachment.HasFile;
            string fileName = hasFile ? fuAttachment.PostedFile.FileName : null;
            int fileLength = hasFile ? fuAttachment.PostedFile.ContentLength : 0;
            Stream fileStream = hasFile ? fuAttachment.PostedFile.InputStream : null;

            LessonManager manager = new LessonManager();

            if (LessonId == 0)
            {
                OperationResult<int> created = manager.CreateLesson(lesson, fileName, fileLength, fileStream);
                if (!created.Success)
                {
                    ShowError(created.Message);
                    LoadPage(false);
                    return;
                }

                // Back to an empty form, ready for the next lesson, and a refresh cannot add it twice
                Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&created=1");
                return;
            }

            OperationResult updated = manager.UpdateLesson(lesson, fileName, fileLength, fileStream, chkRemoveAttachment.Checked);
            if (!updated.Success)
            {
                ShowError(updated.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&lessonId=" + LessonId + "&saved=1");
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            // The video plays above the text, the way learners will see it
            string video = string.Empty;
            if (!string.IsNullOrWhiteSpace(txtVideoUrl.Text))
            {
                video = YouTubeHelper.EmbedHtml(txtVideoUrl.Text, txtTitle.Text);
                if (video.Length == 0)
                {
                    video = "<p class=\"text-danger small\">That video link is not a YouTube video link.</p>";
                }
            }

            string html = video
                + MarkdownHelper.ToHtml(txtContent.Text)
                + MarkdownHelper.CodeBlockToHtml(txtCode.Text, ddlLanguage.SelectedValue);

            litPreview.Text = string.IsNullOrEmpty(html)
                ? "<p class=\"text-muted mb-0\">Nothing to preview yet. Write some content first.</p>"
                : html;
            phPreview.Visible = true;
        }

        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int lessonId;
            if (e.CommandName != "delete" || !int.TryParse(e.CommandArgument as string, out lessonId))
            {
                return;
            }

            OperationResult result = new LessonManager().DeleteLesson(CourseId, lessonId);
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&deleted=1");
        }

        /// <summary>
        /// Decides what the page shows: the locked notice, "not found", or the syllabus with the lesson form.
        /// fillForm copies the saved lesson into the form; it is false after a failed save so what the
        /// tutor typed is kept.
        /// </summary>
        private void LoadPage(bool fillForm)
        {
            if (!Master.IsApproved)
            {
                litHeading.Text = "Lessons";
                litSubheading.Text = "Course authoring unlocks once your tutor application is approved.";
                phLocked.Visible = true;
                return;
            }

            LessonManager manager = new LessonManager();
            OperationResult<Course> courseResult = new CourseManager().GetCourse(CourseId);
            OperationResult<List<Lesson>> lessonsResult = courseResult.Success
                ? manager.GetLessons(CourseId)
                : OperationResult<List<Lesson>>.Fail(courseResult.Message);

            if (!courseResult.Success || !lessonsResult.Success)
            {
                litHeading.Text = "Lessons";
                litSubheading.Text = "Open a course from your dashboard to manage its lessons.";
                phNotFound.Visible = true;
                return;
            }

            Course course = courseResult.Data;
            List<Lesson> lessons = lessonsResult.Data;
            CanEdit = CourseStatuses.IsEditable(course.Status);

            litHeading.Text = "Lessons";
            litSubheading.Text = course.Title;
            hlBackToCourse.NavigateUrl = CourseEditorUrl + "?id=" + course.Id;
            hlBackToCourse.Visible = true;

            phMain.Visible = true;
            phReadOnlyNote.Visible = !CanEdit;
            litReadOnlyText.Text = "This course is " + course.Status + ". Lessons can only be changed while the course is a Draft.";

            litLessonCount.Text = lessons.Count == 1 ? "1 lesson" : lessons.Count + " lessons";
            rptLessons.Visible = lessons.Count > 0;
            phNoLessons.Visible = lessons.Count == 0;
            rptLessons.DataSource = lessons;
            rptLessons.DataBind();

            // Once there is a lesson, point the tutor to the Review & submit step
            hlSubmitHint.Visible = CanEdit && lessons.Count > 0;
            hlSubmitHint.NavigateUrl = Master.StepUrl(5, course.Id);

            // A course needs a lesson before it can be submitted, so a Draft cannot move on without one
            StepNavHtml = Master.StepNav(2, course.Id, null,
                CanEdit && lessons.Count == 0 ? "Add at least one lesson to continue." : null);

            if (!CanEdit)
            {
                return;
            }

            hlNewLesson.NavigateUrl = EditorUrl + "?courseId=" + course.Id;
            hlNewLesson.Visible = true;
            phForm.Visible = true;
            hlCancel.NavigateUrl = EditorUrl + "?courseId=" + course.Id;

            Lesson lesson = null;
            if (LessonId > 0)
            {
                lesson = lessons.Find(l => l.Id == LessonId);
                if (lesson == null)
                {
                    ShowError("That lesson could not be found.");
                }
            }

            if (lesson == null)
            {
                // New lessons are added after the last one
                litFormTitle.Text = "New lesson " + (lessons.Count + 1);
                btnSave.Text = "Add lesson";
                hlCancel.Visible = false;
                phCurrentAttachment.Visible = false;
                return;
            }

            litFormTitle.Text = "Edit lesson " + lesson.OrderIndex;
            btnSave.Text = "Save lesson";
            hlCancel.Text = "Close";

            phCurrentAttachment.Visible = lesson.HasAttachment;
            if (lesson.HasAttachment)
            {
                // HyperLink.Text is written as raw HTML, so encode it
                hlCurrentAttachment.Text = HttpUtility.HtmlEncode(lesson.AttachmentName);
                hlCurrentAttachment.NavigateUrl = lesson.AttachmentUrl;
            }

            if (fillForm)
            {
                txtTitle.Text = lesson.Title;
                txtContent.Text = lesson.Content;
                txtCode.Text = lesson.CodeSnippet;
                txtVideoUrl.Text = lesson.VideoUrl;
                ListItem language = ddlLanguage.Items.FindByValue(lesson.CodeLanguage ?? string.Empty);
                if (language != null)
                {
                    ddlLanguage.ClearSelection();
                    language.Selected = true;
                }
            }
        }

        protected bool IsSelected(Lesson lesson)
        {
            return lesson.Id == LessonId;
        }

        // The lesson's title: a link to edit it while the course is a Draft, plain text otherwise
        protected string LessonTitleHtml(Lesson lesson)
        {
            string title = HttpUtility.HtmlEncode(lesson.Title);
            if (!CanEdit)
            {
                return "<span class=\"cl-lesson-title\">" + title + "</span>";
            }

            string url = ResolveUrl(EditorUrl) + "?courseId=" + lesson.CourseId + "&lessonId=" + lesson.Id;
            return "<a class=\"cl-lesson-title\" href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\">" + title + "</a>";
        }

        // Small badges under the title: code language and attachment
        protected string LessonMetaHtml(Lesson lesson)
        {
            List<string> parts = new List<string>();

            if (lesson.HasCode)
            {
                string label = lesson.CodeLanguage;
                KeyValuePair<string, string> language = LessonOptions.CodeLanguages.Find(l => l.Key == lesson.CodeLanguage);
                if (language.Key != null)
                {
                    label = language.Value;
                }

                parts.Add("<span><i data-lucide=\"code\" class=\"cl-icon\" aria-hidden=\"true\"></i>" + HttpUtility.HtmlEncode(label) + "</span>");
            }

            if (lesson.HasVideo)
            {
                parts.Add("<span><i data-lucide=\"circle-play\" class=\"cl-icon\" aria-hidden=\"true\"></i>Video</span>");
            }

            if (lesson.HasAttachment)
            {
                parts.Add("<span><i data-lucide=\"paperclip\" class=\"cl-icon\" aria-hidden=\"true\"></i>Attachment</span>");
            }

            return parts.Count == 0 ? "<span>Text only</span>" : string.Join(string.Empty, parts);
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
