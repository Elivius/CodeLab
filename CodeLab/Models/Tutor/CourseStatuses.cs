namespace CodeLab.Models
{
    /// <summary>
    /// Course states as stored in Courses.Status.
    ///
    /// There is no "Rejected" status: when an admin rejects a submitted course, it goes back to
    /// Draft and the admin's feedback is saved in Courses.RejectionReason. The tutor fixes the
    /// course and submits it again, which clears the feedback.
    /// </summary>
    public static class CourseStatuses
    {
        public const string Draft = "Draft";
        public const string PendingReview = "Pending Review";
        public const string Published = "Published";
        public const string Locked = "Locked";

        /// <summary>
        /// True when the tutor may still change the course and its lessons (only while it is a Draft).
        /// </summary>
        public static bool IsEditable(string status)
        {
            return status == Draft;
        }
    }
}
