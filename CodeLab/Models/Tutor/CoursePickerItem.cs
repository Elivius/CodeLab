namespace CodeLab.Models
{
    /// <summary>
    /// One course in the "choose a course" list on the Quiz and Coding challenge pages.
    /// </summary>
    public class CoursePickerItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string StatusCss { get; set; }

        // e.g. "5 questions" or "No quiz yet"
        public string Note { get; set; }

        // True when the course already has a quiz / challenge
        public bool HasItem { get; set; }

        public string Url { get; set; }
    }
}
