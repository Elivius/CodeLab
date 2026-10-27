using System;
using System.Collections.Generic;
using System.Web.UI;
using CodeLab.Models;

namespace CodeLab.UserControls
{
    /// <summary>
    /// Course cards for the landing page and the public catalogue.
    /// </summary>
    public partial class CourseGrid : UserControl
    {
        /// <summary>
        /// The courses to show. Null or empty shows "New courses are on the way" instead.
        /// </summary>
        public List<Course> Courses { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            bool hasCourses = Courses != null && Courses.Count > 0;
            rptCourses.Visible = hasCourses;
            phEmpty.Visible = !hasCourses;

            if (hasCourses)
            {
                rptCourses.DataSource = Courses;
                rptCourses.DataBind();
            }
        }

        protected static string LessonText(int count)
        {
            return count == 1 ? "1 Lesson" : count + " Lessons";
        }
    }
}
