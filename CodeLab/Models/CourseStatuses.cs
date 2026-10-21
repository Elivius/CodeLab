namespace CodeLab.Models
{
    /// <summary>
    /// Course states as stored in Courses.Status.
    /// </summary>
    public static class CourseStatuses
    {
        public const string Draft = "Draft";
        public const string PendingReview = "Pending Review";
        public const string Published = "Published";
        public const string Locked = "Locked";
        public const string Rejected = "Rejected";
    }
}
