using System;

namespace CodeLab.Models
{
    public sealed class LearnerChallengeDetail
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Difficulty { get; set; }

        public string Language { get; set; }

        public string StarterCode { get; set; }

        public string ExpectedOutput { get; set; }

        public int PointsReward { get; set; }

        public int AttemptCount { get; set; }

        public bool IsSolved { get; set; }

        public DateTime? LastSubmittedAt { get; set; }

        public string ProgressStatus
        {
            get
            {
                if (IsSolved)
                {
                    return "Solved";
                }

                return AttemptCount > 0
                    ? "Attempted"
                    : "Not attempted";
            }
        }
    }
}