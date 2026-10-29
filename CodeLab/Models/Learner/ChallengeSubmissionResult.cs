namespace CodeLab.Models
{
    public sealed class ChallengeSubmissionResult
    {
        // True only after the submission is saved successfully.
        public bool Saved { get; set; }

        public int? SubmissionId { get; set; }

        // Passed, Failed, or Error for a saved submission.
        public string Status { get; set; } = string.Empty;

        public string Output { get; set; } = string.Empty;

        // Zero for unsuccessful attempts or an already-solved challenge.
        public int PointsAwarded { get; set; }

        // Explains why execution or saving could not be completed.
        public string ErrorMessage { get; set; } = string.Empty;
    }
}