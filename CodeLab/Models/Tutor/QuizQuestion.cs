using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// One question of a quiz, as stored in the QuizQuestions table.
    /// </summary>
    public class QuizQuestion
    {
        public const string MultipleChoice = "Multiple Choice";
        public const string TrueFalse = "True-False";

        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }

        // MultipleChoice or TrueFalse
        public string QuestionType { get; set; }

        // The answers to choose from. Stored in the OptionsJson column as a JSON array.
        public List<string> Options { get; set; } = new List<string>();

        // Position of the right answer in Options, starting at 0
        public int CorrectOptionIndex { get; set; }

        // Shown to learners after they answer; empty if the tutor left it out
        public string Explanation { get; set; }

        // Position in the quiz, starting at 1
        public int OrderIndex { get; set; }

        public string CorrectAnswer
        {
            get
            {
                return CorrectOptionIndex >= 0 && CorrectOptionIndex < Options.Count ? Options[CorrectOptionIndex] : string.Empty;
            }
        }
    }
}
