using System;

namespace CodeLab.Models
{
    public sealed class LearnerChallengeSubmission
    {
        public int Id { get; set; }

        public int LearnerId { get; set; }

        public int ChallengeId { get; set; }

        public string SubmittedCode { get; set; }

        public string Status { get; set; }

        public string OutputResult { get; set; }

        // The repository will load this as UTC.
        public DateTime SubmittedAt { get; set; }

        public string StatusCssClass
        {
            get
            {
                switch (Status)
                {
                    case "Passed":
                        return "ch-submission-status is-passed";

                    case "Failed":
                        return "ch-submission-status is-failed";

                    case "Error":
                        return "ch-submission-status is-error";

                    default:
                        return "ch-submission-status";
                }
            }
        }
    }
}