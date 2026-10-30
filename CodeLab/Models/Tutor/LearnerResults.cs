using System;

namespace CodeLab.Models
{
    /// <summary>
    /// A course quiz with its results, for the Tutor "Quizzes" results page.
    /// </summary>
    public class QuizResultSummary
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string CourseTitle { get; set; }
        public int PassMark { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }

        // Different learners who attempted it, and how many of them passed at least once
        public int LearnerCount { get; set; }
        public int PassedLearnerCount { get; set; }

        // Learners enrolled in the quiz's course, for "5 out of 10"
        public int EnrolledCount { get; set; }

        public double? AverageScore { get; set; }
    }

    /// <summary>
    /// One quiz attempt in the attempts list.
    /// </summary>
    public class QuizAttemptRow
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public string LearnerUsername { get; set; }
        public string LearnerAvatarUrl { get; set; }

        // 1 for the learner's first attempt at this quiz, 2 for the second, ...
        public int AttemptNumber { get; set; }

        public int ScorePercent { get; set; }
        public bool Passed { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }

        // Stored and read as UTC
        public DateTime AttemptedAt { get; set; }
    }

    /// <summary>
    /// One attempt with its question-by-question answers.
    /// </summary>
    public class QuizAttemptDetail : QuizAttemptRow
    {
        public string QuizTitle { get; set; }
        public string CourseTitle { get; set; }
        public int PassMark { get; set; }

        // Null when the attempt was saved without a snapshot
        public QuizAnswerReview AnswerReview { get; set; }
    }

    /// <summary>
    /// A course coding challenge with its results, for the Tutor "Coding Challenges" results page.
    /// </summary>
    public class ChallengeResultSummary
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Difficulty { get; set; }
        public string CourseTitle { get; set; }
        public int SubmissionCount { get; set; }

        // Different learners who submitted, and how many of them solved it at least once
        public int LearnerCount { get; set; }
        public int SolverCount { get; set; }

        // Learners enrolled in the challenge's course, for "5 out of 10"
        public int EnrolledCount { get; set; }
    }

    /// <summary>
    /// One code submission in the submissions list.
    /// </summary>
    public class ChallengeSubmissionRow
    {
        public int SubmissionId { get; set; }
        public int ChallengeId { get; set; }
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public string LearnerUsername { get; set; }
        public string LearnerAvatarUrl { get; set; }

        // 1 for the learner's first submission to this challenge, 2 for the second, ...
        public int AttemptNumber { get; set; }

        // Passed, Failed or Error
        public string Status { get; set; }

        // Stored and read as UTC
        public DateTime SubmittedAt { get; set; }
    }

    /// <summary>
    /// One submission with the learner's code and output, next to what was expected.
    /// </summary>
    public class ChallengeSubmissionDetail : ChallengeSubmissionRow
    {
        public string ChallengeTitle { get; set; }
        public string Language { get; set; }
        public string CourseTitle { get; set; }
        public string SubmittedCode { get; set; }
        public string OutputResult { get; set; }
        public string ExpectedOutput { get; set; }
    }
}
