using System;
using System.Collections.Generic;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Models;

namespace CodeLab.Modules.Public
{
    public partial class Courses : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            OperationResult<List<Course>> catalogue = new CourseManager().GetCatalogue();
            courseGrid.Courses = catalogue.Success ? catalogue.Data : null;
        }
    }
}
