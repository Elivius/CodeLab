namespace CodeLab.Models
{
    /// <summary>
    /// Account states as stored in Users.Status. Only Active accounts can log in.
    /// Tutor approval is tracked separately in TutorProfiles.Status.
    /// </summary>
    public static class UserStatuses
    {
        public const string Active = "Active";
        public const string Suspended = "Suspended";
        public const string Pending = "Pending";
    }
}
