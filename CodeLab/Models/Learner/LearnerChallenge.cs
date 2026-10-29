using System;

namespace CodeLab.Models
{
    /// <summary>
    /// Challenge information shown in the learner's workspace.
    /// </summary>
    public sealed class LearnerChallenge
    {
        public bool CanAccess { get; set; }
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Difficulty { get; set; }

        public string Language { get; set; }

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

        public string StatusCssClass
        {
            get
            {
                if (IsSolved)
                {
                    return "cw-status is-solved";
                }

                return AttemptCount > 0
                    ? "cw-status is-attempted"
                    : "cw-status is-new";
            }
        }

        public string ActionText
        {
            get
            {
                if (!CanAccess) return "Complete course to unlock";
                if (IsSolved)
                {
                    return "Practise Again";
                }

                return AttemptCount > 0
                    ? "Try Again"
                    : "Start Challenge";
            }
        }
    }
}