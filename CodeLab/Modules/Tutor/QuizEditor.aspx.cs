using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Tutor
{
    public partial class QuizEditor : Page
    {
        private const string EditorUrl = "~/Modules/Tutor/QuizEditor";
        private const string CourseEditorUrl = "~/Modules/Tutor/CourseEditor";

        // A Draft course's quiz can be changed; any other status is read-only
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

        // The question open in the form, or 0 when writing a new one
        private int QuestionId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["questionId"], out id) && id > 0 ? id : 0;
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

            switch (Request.QueryString["done"])
            {
                case "created": ShowSuccess("The quiz has been created. Now write its first question."); break;
                case "saved": ShowSuccess("The quiz details have been saved."); break;
                case "deleted": ShowSuccess("The quiz has been deleted."); break;
                case "added": ShowSuccess("The question has been added. Write the next one, or pick a question to edit it."); break;
                case "updated": ShowSuccess("The question has been saved."); break;
                case "removed": ShowSuccess("The question has been deleted."); break;
            }

            LoadPage(true);
        }

        protected void btnSaveQuiz_Click(object sender, EventArgs e)
        {
            Page.Validate("Quiz");
            if (!Page.IsValid)
            {
                LoadPage(false);
                return;
            }

            int passMark;
            int.TryParse(txtPassMark.Text, out passMark);

            Quiz quiz = new Quiz
            {
                CourseId = CourseId,
                Title = txtQuizTitle.Text,
                PassMark = passMark,
                Instructions = txtInstructions.Text
            };

            QuizManager manager = new QuizManager();
            OperationResult<Quiz> existing = manager.GetQuiz(CourseId);
            if (existing.Success && existing.Data == null)
            {
                OperationResult<int> created = manager.CreateQuiz(quiz);
                if (!created.Success)
                {
                    ShowError(created.Message);
                    LoadPage(false);
                    return;
                }

                Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&done=created");
                return;
            }

            OperationResult updated = manager.UpdateQuiz(quiz);
            if (!updated.Success)
            {
                ShowError(updated.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&done=saved");
        }

        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            OperationResult result = new QuizManager().DeleteQuiz(CourseId);
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&done=deleted");
        }

        protected void btnSaveQuestion_Click(object sender, EventArgs e)
        {
            Page.Validate("Question");
            if (!Page.IsValid)
            {
                LoadPage(false);
                return;
            }

            RadioButton[] correct = { rbCorrect1, rbCorrect2, rbCorrect3, rbCorrect4 };
            int correctIndex = Array.FindIndex(correct, radio => radio.Checked);

            QuizQuestion question = new QuizQuestion
            {
                Id = QuestionId,
                QuestionText = txtQuestion.Text,
                QuestionType = rbTrueFalse.Checked ? QuizQuestion.TrueFalse : QuizQuestion.MultipleChoice,
                Options = new List<string> { txtOption1.Text, txtOption2.Text, txtOption3.Text, txtOption4.Text },
                CorrectOptionIndex = correctIndex,
                Explanation = txtExplanation.Text
            };

            QuizManager manager = new QuizManager();
            OperationResult result = QuestionId == 0
                ? manager.AddQuestion(CourseId, question)
                : manager.UpdateQuestion(CourseId, question);

            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            // Back to an empty form after adding, so the next question can be written straight away
            Response.Redirect(QuestionId == 0
                ? EditorUrl + "?courseId=" + CourseId + "&done=added#questionForm"
                : EditorUrl + "?courseId=" + CourseId + "&done=updated");
        }

        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int questionId;
            if (e.CommandName != "delete" || !int.TryParse(e.CommandArgument as string, out questionId))
            {
                return;
            }

            OperationResult result = new QuizManager().DeleteQuestion(CourseId, questionId);
            if (!result.Success)
            {
                ShowError(result.Message);
                LoadPage(false);
                return;
            }

            Response.Redirect(EditorUrl + "?courseId=" + CourseId + "&done=removed");
        }

        /// <summary>
        /// Decides what the page shows: the locked notice, the course picker, "not found", or the quiz.
        /// fillForm copies the saved values into the forms; it is false after a failed save so what the
        /// tutor typed is kept.
        /// </summary>
        private void LoadPage(bool fillForm)
        {
            litHeading.Text = "Quiz";

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
            OperationResult<Quiz> quizResult = courseResult.Success
                ? new QuizManager().GetQuiz(CourseId)
                : OperationResult<Quiz>.Fail(courseResult.Message);

            if (!courseResult.Success || !quizResult.Success)
            {
                litSubheading.Text = "Choose one of your courses to manage its quiz.";
                phNotFound.Visible = true;
                return;
            }

            Course course = courseResult.Data;
            Quiz quiz = quizResult.Data;
            CanEdit = CourseStatuses.IsEditable(course.Status);

            litSubheading.Text = course.Title;
            hlBackToCourse.NavigateUrl = CourseEditorUrl + "?id=" + course.Id;
            hlBackToCourse.Visible = true;
            phMain.Visible = true;
            phReadOnlyNote.Visible = !CanEdit;
            litReadOnlyText.Text = "This course is " + course.Status + ". The quiz can only be changed while the course is a Draft.";

            // The quiz is optional: without one, the tutor can skip this step
            StepNavHtml = Master.StepNav(3, course.Id, CanEdit && quiz == null ? "Skip the quiz" : null);

            if (quiz == null)
            {
                if (CanEdit)
                {
                    ShowDetailsForm(null, course, fillForm);
                    phHelp.Visible = true;
                }
                else
                {
                    phNoQuiz.Visible = true;
                }

                return;
            }

            if (CanEdit)
            {
                ShowDetailsForm(quiz, course, fillForm);
            }
            else
            {
                litRoTitle.Text = quiz.Title;
                litRoPassMark.Text = quiz.PassMark.ToString();
                litRoInstructions.Text = string.IsNullOrEmpty(quiz.Instructions) ? "No instructions." : quiz.Instructions;
                phDetailsReadOnly.Visible = true;
            }

            ShowQuestions(quiz, fillForm);
        }

        private void ShowDetailsForm(Quiz quiz, Course course, bool fillForm)
        {
            phDetailsForm.Visible = true;
            btnDeleteQuiz.Visible = quiz != null;

            if (quiz == null)
            {
                litDetailsHeading.Text = "Create the quiz";
                litDetailsNote.Text = "A course has one quiz. Give it a title and a pass mark, then add its questions.";
                btnSaveQuiz.Text = "Create quiz";
                if (fillForm)
                {
                    txtQuizTitle.Text = course.Title + " quiz";
                    txtPassMark.Text = "80";
                }

                return;
            }

            litDetailsHeading.Text = "Quiz details";
            litDetailsNote.Text = "Learners see the title and instructions before they start.";
            btnSaveQuiz.Text = "Save details";
            if (fillForm)
            {
                txtQuizTitle.Text = quiz.Title;
                txtPassMark.Text = quiz.PassMark.ToString();
                txtInstructions.Text = quiz.Instructions;
            }
        }

        private void ShowQuestions(Quiz quiz, bool fillForm)
        {
            List<QuizQuestion> questions = quiz.Questions;
            phQuestions.Visible = true;
            litQuestionCount.Text = (questions.Count == 1 ? "1 question" : questions.Count + " questions")
                + " · pass mark " + quiz.PassMark + "%";
            rptQuestions.Visible = questions.Count > 0;
            phNoQuestions.Visible = questions.Count == 0;
            rptQuestions.DataSource = questions;
            rptQuestions.DataBind();

            if (!CanEdit)
            {
                return;
            }

            phQuestionForm.Visible = true;
            hlCancelQuestion.NavigateUrl = EditorUrl + "?courseId=" + CourseId;

            QuizQuestion question = null;
            if (QuestionId > 0)
            {
                question = questions.Find(q => q.Id == QuestionId);
                if (question == null)
                {
                    ShowError("That question could not be found.");
                }
            }

            if (question == null)
            {
                litQuestionFormTitle.Text = "New question " + (questions.Count + 1);
                btnSaveQuestion.Text = "Add question";
                hlCancelQuestion.Visible = false;
                if (fillForm)
                {
                    rbMultiple.Checked = true;
                    rbCorrect1.Checked = true;
                }

                return;
            }

            litQuestionFormTitle.Text = "Edit question " + question.OrderIndex;
            btnSaveQuestion.Text = "Save question";
            if (!fillForm)
            {
                return;
            }

            txtQuestion.Text = question.QuestionText;
            txtExplanation.Text = question.Explanation;
            rbTrueFalse.Checked = question.QuestionType == QuizQuestion.TrueFalse;
            rbMultiple.Checked = !rbTrueFalse.Checked;

            TextBox[] boxes = { txtOption1, txtOption2, txtOption3, txtOption4 };
            RadioButton[] correct = { rbCorrect1, rbCorrect2, rbCorrect3, rbCorrect4 };
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].Text = i < question.Options.Count ? question.Options[i] : string.Empty;
                correct[i].Checked = i == question.CorrectOptionIndex;
            }
        }

        private void ShowPicker()
        {
            litSubheading.Text = "Choose a course to create or edit its quiz.";
            phPicker.Visible = true;

            OperationResult<TutorDashboard> dashboard = new TutorManager().GetDashboard();
            if (!dashboard.Success)
            {
                ShowError(dashboard.Message);
                return;
            }

            Dictionary<int, int> counts = new QuizManager().GetQuestionCounts();
            List<CoursePickerItem> items = new List<CoursePickerItem>();
            foreach (TutorCourseSummary course in dashboard.Data.Courses)
            {
                int questions;
                bool hasQuiz = counts.TryGetValue(course.Id, out questions);
                items.Add(new CoursePickerItem
                {
                    Id = course.Id,
                    Title = course.Title,
                    Status = course.Status,
                    StatusCss = TutorDisplayHelper.CourseStatusCss(course.Status),
                    HasItem = hasQuiz,
                    Note = !hasQuiz ? "No quiz yet" : (questions == 1 ? "Quiz · 1 question" : "Quiz · " + questions + " questions"),
                    Url = ResolveUrl(EditorUrl) + "?courseId=" + course.Id
                });
            }

            rptPickCourses.DataSource = items;
            rptPickCourses.DataBind();
            rptPickCourses.Visible = items.Count > 0;
            phNoCourses.Visible = items.Count == 0;
        }

        protected bool IsSelected(QuizQuestion question)
        {
            return question.Id == QuestionId;
        }

        protected string EditLinkHtml(QuizQuestion question)
        {
            if (!CanEdit)
            {
                return string.Empty;
            }

            string url = ResolveUrl(EditorUrl) + "?courseId=" + CourseId + "&questionId=" + question.Id + "#questionForm";
            return "<a class=\"cl-question-action\" href=\"" + HttpUtility.HtmlAttributeEncode(url) + "\" title=\"Edit question\">"
                + "<i data-lucide=\"pencil\" class=\"cl-icon\" aria-hidden=\"true\"></i><span class=\"visually-hidden\">Edit question</span></a>";
        }

        // The answer options, with the correct one ticked
        protected string OptionsHtml(QuizQuestion question)
        {
            StringBuilder html = new StringBuilder("<ul class=\"cl-answer-list\">");
            for (int i = 0; i < question.Options.Count; i++)
            {
                bool isCorrect = i == question.CorrectOptionIndex;
                html.Append("<li").Append(isCorrect ? " class=\"is-correct\"" : string.Empty).Append('>')
                    .Append("<i data-lucide=\"").Append(isCorrect ? "circle-check" : "circle").Append("\" class=\"cl-icon\" aria-hidden=\"true\"></i>")
                    .Append(HttpUtility.HtmlEncode(question.Options[i]))
                    .Append(isCorrect ? "<span class=\"visually-hidden\"> (correct answer)</span>" : string.Empty)
                    .Append("</li>");
            }

            return html.Append("</ul>").ToString();
        }

        protected string ExplanationHtml(QuizQuestion question)
        {
            return string.IsNullOrWhiteSpace(question.Explanation)
                ? string.Empty
                : "<p class=\"cl-question-explanation\"><i data-lucide=\"info\" class=\"cl-icon\" aria-hidden=\"true\"></i>"
                    + HttpUtility.HtmlEncode(question.Explanation) + "</p>";
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
