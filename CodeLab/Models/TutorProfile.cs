using System;

namespace CodeLab.Models
{
    /// <summary>
    /// A tutor's account details plus their tutor application (TutorProfiles row).
    /// Also used to carry a new application from the Profile page to the BLL.
    /// </summary>
    public class TutorProfile
    {
        // From Users
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime JoinedDate { get; set; }

        // App-relative path of the profile photo, e.g. "~/Uploads/Avatars/u2-....png"; null if none
        public string AvatarUrl { get; set; }

        // From TutorProfiles; all null when the tutor has not applied yet
        public string Specialty { get; set; }
        public string Qualifications { get; set; }
        public string PortfolioUrl { get; set; }
        public string Bio { get; set; }
        public string CurriculumPitch { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public bool HasApplied
        {
            get { return Status != null; }
        }

        /// <summary>
        /// The ID shown to people, e.g. "TUT-0002".
        /// </summary>
        public string TutorCode
        {
            get { return "TUT-" + UserId.ToString("D4"); }
        }
    }
}
