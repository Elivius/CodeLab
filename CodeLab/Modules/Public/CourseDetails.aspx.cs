using System;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Public
{
    public partial class CourseDetails : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // A missing or non-numeric id never reaches the database
            int courseId;
            OperationResult<Course> result = int.TryParse(Request.QueryString["id"], out courseId) && courseId > 0
                ? new CourseManager().GetPublicCourse(courseId)
                : OperationResult<Course>.Fail("Course not found.");

            if (!result.Success)
            {
                litNotFound.Text = result.Message;
                phNotFound.Visible = true;
                phCourse.Visible = false;
                return;
            }

            ShowCourse(result.Data);
        }

        private void ShowCourse(Course course)
        {
            litTitle.Text = course.Title;
            litCategory.Text = course.Category;
            litDescription.Text = course.Description;

            // Difficulty is limited to Beginner/Intermediate/Advanced by the database, so it is safe in a class name
            spanLevel.InnerText = course.Difficulty;
            spanLevel.Attributes["class"] = "cl-level cl-level-" + course.Difficulty.ToLowerInvariant();

            if (course.ThumbnailUrl != null)
            {
                imgCover.ImageUrl = course.ThumbnailUrl;
                imgCover.AlternateText = course.Title;
                imgCover.Visible = true;
                phDefaultThumb.Visible = false;
            }

            // Signed-in learners open the course in their own area; guests are asked to register or log in
            if (SessionHelper.IsUserInRole(UserRoles.Learner))
            {
                hlOpenCourse.NavigateUrl = "~/Modules/Learner/CourseDetails?id=" + course.Id;
                hlOpenCourse.Visible = true;
            }

            phGuestActions.Visible = !SessionHelper.IsLoggedIn();
        }
    }
}
