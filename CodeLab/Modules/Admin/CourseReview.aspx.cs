using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Admin
{
    public partial class CourseReview : Page
    {
        // The course id from ?id=, or 0 if it is missing or not a number
        private int CourseId
        {
            get
            {
                int courseId;
                return int.TryParse(Request.QueryString["id"], out courseId) ? courseId : 0;
            }
        }

        // PreRender runs after the button clicks, so the page always shows the course as it is now
        protected void Page_PreRender(object sender, EventArgs e)
        {
            LoadCourse();
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            ShowResult(new CourseModerationManager().Approve(CourseId));
        }

        protected void btnUnlock_Click(object sender, EventArgs e)
        {
            ShowResult(new CourseModerationManager().Unlock(CourseId));
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                ShowReasonResult(new CourseModerationManager().Reject(CourseId, txtReason.Text));
            }
        }

        protected void btnLock_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                ShowReasonResult(new CourseModerationManager().Lock(CourseId, txtReason.Text));
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            OperationResult result = new CourseModerationManager().Delete(CourseId);
            if (result.Success)
            {
                // The course is gone, so there is nothing left to show here
                Response.Redirect("~/Modules/Admin/Courses?deleted=1");
            }

            ShowResult(result);
        }

        private void LoadCourse()
        {
            OperationResult<CourseReviewDetails> result = new CourseModerationManager().GetReview(CourseId);
            if (!result.Success)
            {
                ShowResult(result);
                phCourse.Visible = false;
                return;
            }

            AdminCourseRow course = result.Data.Course;
            Title = course.Title + " - Course Review";

            imgCover.ImageUrl = course.ThumbnailUrl;
            imgCover.Visible = !string.IsNullOrEmpty(course.ThumbnailUrl);

            litTitle.Text = course.Title;
            litSubtitle.Text = course.Category + ", " + course.Difficulty;
            lblStatus.Text = HttpUtility.HtmlEncode(course.Status);
            lblStatus.CssClass = "cl-admin-badge " + DisplayHelper.CourseBadgeCss(course.Status);

            litTutor.Text = course.TutorName;
            litTutorEmail.Text = course.TutorEmail;
            litStudents.Text = course.StudentCount + " enrolled";
            litUpdated.Text = DisplayHelper.TimeAgo(course.UpdatedAt);
            litDescription.Text = course.Description;

            ShowContent(result.Data);
            ShowActions(course);
        }

        private void ShowContent(CourseReviewDetails review)
        {
            litLessonCount.Text = review.Lessons.Count.ToString();
            phNoLessons.Visible = review.Lessons.Count == 0;
            rptLessons.DataSource = review.Lessons;
            rptLessons.DataBind();

            Quiz quiz = review.Quiz;
            litQuiz.Text = quiz == null
                ? "No quiz."
                : "Quiz: " + quiz.Title + " (" + quiz.Questions.Count + (quiz.Questions.Count == 1 ? " question" : " questions")
                    + ", pass mark " + quiz.PassMark + "%).";

            CodingChallenge challenge = review.Challenge;
            litChallenge.Text = challenge == null
                ? "No coding challenge."
                : "Coding challenge: " + challenge.Title + " (" + challenge.Difficulty + ").";
        }

        // Shows only what this status allows. A course can be deleted in any status.
        private void ShowActions(AdminCourseRow course)
        {
            btnApprove.Visible = course.CanApproveOrReject;
            btnReject.Visible = course.CanApproveOrReject;
            btnLock.Visible = course.CanLock;
            btnUnlock.Visible = course.CanUnlock;
            phReasonForm.Visible = course.CanApproveOrReject || course.CanLock;
            lblReason.Text = course.CanLock ? "Reason for locking" : "Feedback for the tutor";
            btnDelete.OnClientClick = DisplayHelper.CourseDeleteConfirm(course.StudentCount);

            switch (course.Status)
            {
                case CourseStatuses.PendingReview:
                    litDecisionNote.Text = "Approving publishes this course to learners straight away.";
                    break;
                case CourseStatuses.Published:
                    litDecisionNote.Text = "This course is live. Lock it if it breaks the rules: learners can no longer open it, and the tutor sees your reason.";
                    break;
                case CourseStatuses.Locked:
                    litDecisionNote.Text = "This course is locked, so learners cannot open it.";
                    litNoteLabel.Text = "Locked because:";
                    break;
                default:
                    litDecisionNote.Text = "The tutor is still working on this draft. It comes back here when they submit it.";
                    litNoteLabel.Text = "Sent back because:";
                    break;
            }

            phNote.Visible = !string.IsNullOrWhiteSpace(course.ModerationNote);
            litNote.Text = course.ModerationNote;
        }

        private void ShowReasonResult(OperationResult result)
        {
            ShowResult(result);
            if (result.Success)
            {
                txtReason.Text = string.Empty;
            }
        }

        private void ShowResult(OperationResult result)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(result.Message);
            lblMessage.CssClass = result.Success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        // e.g. "Video, Attachment, Code"
        protected static string LessonFlags(Lesson lesson)
        {
            List<string> flags = new List<string>();
            if (lesson.HasVideo)
            {
                flags.Add("Video");
            }

            if (lesson.HasAttachment)
            {
                flags.Add("Attachment");
            }

            if (lesson.HasCode)
            {
                flags.Add("Code");
            }

            return string.Join(", ", flags);
        }

        // Only http(s) links become clickable, so a stored value can never run script
        protected static bool IsWebLink(string url)
        {
            return TutorManager.IsWebUrl(url);
        }
    }
}
