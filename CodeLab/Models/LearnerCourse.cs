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
        public string ThumbnailUrl { get; set; }
        public int LessonCount { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsCompleted { get; set; }
        public decimal ProgressPercent { get; set; }
        public List<LearnerLesson> Lessons { get; set; } = new List<LearnerLesson>();
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
