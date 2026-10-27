using System;
using System.Collections.Generic;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab
{
    public partial class _Default : Page
    {
        private const int FeaturedCourseCount = 3;

        protected void Page_Load(object sender, EventArgs e)
        {
            CourseManager courseManager = new CourseManager();

            OperationResult<List<Course>> featured = courseManager.GetFeatured(FeaturedCourseCount);
            courseGrid.Courses = featured.Success ? featured.Data : null;

            OperationResult<List<Course>> catalogue = courseManager.GetCatalogue();
            litCourseCount.Text = catalogue.Success ? catalogue.Data.Count.ToString() : "0";
        }
    }
}
