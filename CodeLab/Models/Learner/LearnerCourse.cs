using System;
using System.Collections.Generic;

namespace CodeLab.Models
{
    public class LearnerCourse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Difficulty { get; set; }
        public string Instructor { get; set; }
        public int InstructorId { get; set; }
        public string ThumbnailUrl { get; set; }
        public int LessonCount { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgressPercent { get; set; }
        public List<LearnerLesson> Lessons { get; set; } = new List<LearnerLesson>();
    }

    public sealed class QuizAttemptReview
    {
        public int PassMark { get; set; }
        public List<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    }

    public sealed class QuizAttemptAnswer
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int SelectedOptionIndex { get; set; }
        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; }
        public bool IsCorrect { get { return SelectedOptionIndex == CorrectOptionIndex; } }
    }

    public class LearnerLesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CodeSnippet { get; set; }
        public string CodeLanguage { get; set; }
        public string VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsCompleted { get; set; }
    }
}
