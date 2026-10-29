namespace CodeLab.Models
{
    /// <summary>
    /// Tutor application states as stored in TutorProfiles.Status.
    /// Only Approved tutors may author content.
    /// </summary>
    public static class TutorStatuses
    {
        public const string Approved = "Approved";
        public const string Pending = "Pending";
        public const string Rejected = "Rejected";
    }
}
