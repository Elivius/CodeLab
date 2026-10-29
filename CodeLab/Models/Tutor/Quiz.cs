using System;
using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// A course's quiz, as stored in the Quizzes table. A course has at most one quiz (see QuizManager).
    /// </summary>
    public class Quiz
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }

        // Score needed to pass, 0 to 100 (percent)
        public int PassMark { get; set; }

        public string Instructions { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Filled by QuizManager.GetQuiz; empty until questions are added
        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
