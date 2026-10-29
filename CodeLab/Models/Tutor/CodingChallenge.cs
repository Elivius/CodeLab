using System;

namespace CodeLab.Models
{
    /// <summary>
    /// A course's coding challenge (Code Lab), as stored in the CodingChallenges table.
    /// A course has at most one challenge (see ChallengeManager).
    /// </summary>
    public class CodingChallenge
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }

        // The problem statement, written in Markdown; show it with MarkdownHelper.ToHtml
        public string Description { get; set; }

        // Beginner, Intermediate or Advanced
        public string Difficulty { get; set; }

        // Key from LessonOptions.CodeLanguages, e.g. "javascript"
        public string Language { get; set; }

        // Code the learner starts from
        public string StarterCode { get; set; }

        // What a correct program prints. How learner code is run and compared is not decided yet.
        public string ExpectedOutput { get; set; }

        // Achievement points for solving it
        public int PointsReward { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
