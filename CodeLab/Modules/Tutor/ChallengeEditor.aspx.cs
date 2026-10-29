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
    public partial class ChallengeEditor : Page
    {
        private const string EditorUrl = "~/Modules/Tutor/ChallengeEditor";
        private const string CourseEditorUrl = "~/Modules/Tutor/CourseEditor";

        protected int CourseId
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

            ddlLanguage.Items.Add(new ListItem("Select a language", string.Empty));
            foreach (KeyValuePair<string, string> language in LessonOptions.CodeLanguages)
            {
                ddlLanguage.Items.Add(new ListItem(language.Value, language.Key));
            }

            foreach (string difficulty in CourseOptions.Difficulties)
            {
                ddlDifficulty.Items.Add(new ListItem(difficulty, difficulty));
            }

            switch (Request.QueryString["done"])
            {
                case "created": ShowSuccess("The coding challenge has been created."); break;
                case "saved": ShowSuccess("Your changes have been saved."); break;
                case "deleted": ShowSuccess("The coding challenge has been deleted."); break;
            }

            LoadPage(true);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("Challenge");
            if (!Page.IsValid)
            {
                LoadPage(false);
                return;
            }

            int points;
            int.TryParse(txtPoints.Text, out points);

            CodingChallenge challenge = new CodingChallenge
            {
                CourseId = CourseId,
                Title = txtTitle.Text,
                Description = txtDescription.Text,
                Difficulty = ddlDifficulty.SelectedValue,
                Language = ddlLanguage.SelectedValue,
                StarterCode = txtStarterCode.Text,
                ExpectedOutput = txtExpectedOutput.Text,
                PointsReward = points
            };

            ChallengeManager manager = new ChallengeManager();
            OperationResult<CodingChallenge> existing = manager.GetChallenge(CourseId);
            bool isNew = existing.Success && existing.Data == null;

            OperationResult<int> result = manager.SaveChallenge(challenge);
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&done=" + (isNew ? "created" : "saved"));
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            OperationResult result = new ChallengeManager().DeleteChallenge(CourseId);
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&done=deleted");
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            string html = MarkdownHelper.ToHtml(txtDescription.Text);
            litPreview.Text = string.IsNullOrEmpty(html)
                ? "<p class=\"text-muted mb-0\">Nothing to preview yet. Write the problem statement first.</p>"
                : html;
            phPreview.Visible = true;
        }

        /// <summary>
        /// Decides what the page shows: the locked notice, the course picker, "not found", the form for a
        /// Draft course, or a read-only view. fillForm copies the saved values into the form; it is false
        /// after a failed save so what the tutor typed is kept.
        /// </summary>
        private void LoadPage(bool fillForm)
        {
            litHeading.Text = "Coding challenge";

            if (!Master.IsApproved)
            {
                litSubheading.Text = "Course authoring unlocks once your tutor application is approved.";
                phLocked.Visible = true;
                return;
            }

            if (CourseId == 0)
            {
                ShowPicker();
                return;
            }

            OperationResult<Course> courseResult = new CourseManager().GetCourse(CourseId);
            OperationResult<CodingChallenge> challengeResult = courseResult.Success
                ? new ChallengeManager().GetChallenge(CourseId)
                : OperationResult<CodingChallenge>.Fail(courseResult.Message);

            if (!courseResult.Success || !challengeResult.Success)
            {
                litSubheading.Text = "Choose one of your courses to manage its coding challenge.";
                phNotFound.Visible = true;
                return;
            }

            Course course = courseResult.Data;
            CodingChallenge challenge = challengeResult.Data;
            bool canEdit = CourseStatuses.IsEditable(course.Status);

            litSubheading.Text = course.Title;
            hlBackToCourse.NavigateUrl = CourseEditorUrl + "?id=" + course.Id;
            hlBackToCourse.Visible = true;
            phMain.Visible = true;
            phReadOnlyNote.Visible = !canEdit;
            litReadOnlyText.Text = "This course is " + course.Status + ". The coding challenge can only be changed while the course is a Draft.";

            if (!canEdit)
            {
                if (challenge == null)
                {
                    phNoChallenge.Visible = true;
                }
                else
                {
                    ShowReadOnly(challenge);
                }

                return;
            }

            phForm.Visible = true;
            hlCancel.NavigateUrl = CourseEditorUrl + "?id=" + course.Id;
            phDelete.Visible = challenge != null;
            btnSave.Text = challenge == null ? "Create challenge" : "Save changes";

            if (!fillForm)
            {
                return;
            }

            if (challenge == null)
            {
                // Start from the course's own difficulty and the usual reward
                SelectIfPresent(ddlDifficulty, course.Difficulty);
                txtPoints.Text = ChallengeManager.DefaultPoints.ToString();
                return;
            }

            txtTitle.Text = challenge.Title;
            txtDescription.Text = challenge.Description;
            SelectIfPresent(ddlDifficulty, challenge.Difficulty);
            SelectIfPresent(ddlLanguage, challenge.Language);
            txtPoints.Text = challenge.PointsReward.ToString();
            txtStarterCode.Text = challenge.StarterCode;
            txtExpectedOutput.Text = challenge.ExpectedOutput;
        }

        private void ShowReadOnly(CodingChallenge challenge)
        {
            phReadOnly.Visible = true;
            litRoTitle.Text = challenge.Title;
            litRoDifficulty.Text = challenge.Difficulty;
            litRoPoints.Text = challenge.PointsReward.ToString();
            litRoLanguage.Text = TutorDisplayHelper.LanguageName(challenge.Language);
            litRoDescription.Text = MarkdownHelper.ToHtml(challenge.Description);
            litRoStarterCode.Text = string.IsNullOrWhiteSpace(challenge.StarterCode)
                ? "<p class=\"cl-builder-note\">No starter code.</p>"
                : MarkdownHelper.CodeBlockToHtml(challenge.StarterCode, challenge.Language);
            litRoExpectedOutput.Text = challenge.ExpectedOutput;
        }

        private void ShowPicker()
        {
            litSubheading.Text = "Choose a course to create or edit its coding challenge.";
            phPicker.Visible = true;

            OperationResult<TutorDashboard> dashboard = new TutorManager().GetDashboard();
            if (!dashboard.Success)
            {
                ShowError(dashboard.Message);
                return;
            }

            HashSet<int> withChallenge = new ChallengeManager().GetCourseIdsWithChallenge();
            List<CoursePickerItem> items = new List<CoursePickerItem>();
            foreach (TutorCourseSummary course in dashboard.Data.Courses)
            {
                bool hasChallenge = withChallenge.Contains(course.Id);
                items.Add(new CoursePickerItem
                {
                    Id = course.Id,
                    Title = course.Title,
                    Status = course.Status,
                    StatusCss = TutorDisplayHelper.CourseStatusCss(course.Status),
                    HasItem = hasChallenge,
                    Note = hasChallenge ? "Coding challenge added" : "No coding challenge yet",
                    Url = ResolveUrl(EditorUrl) + "?courseId=" + course.Id
                });
            }

            rptPickCourses.DataSource = items;
            rptPickCourses.DataBind();
            rptPickCourses.Visible = items.Count > 0;
            phNoCourses.Visible = items.Count == 0;
        }

        private static void SelectIfPresent(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value ?? string.Empty);
            if (item != null)
            {
                list.ClearSelection();
                item.Selected = true;
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
