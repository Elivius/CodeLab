using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// A course's quiz and its questions. Rules:
    /// - A course has at most one quiz, holding all of its questions (team decision, see TUTOR_DECISIONS.md).
    /// - The quiz can only be changed while the course is a Draft.
    /// - A quiz needs at least one question before the course is submitted (checked in CourseManager).
    /// </summary>
    public class QuizManager
    {
        public const int MaxTitleLength = 150;
        public const int MaxInstructionsLength = 2000;
        public const int MaxQuestionLength = 1000;
        public const int MaxOptionLength = 200;
        public const int MaxExplanationLength = 1000;
        public const int MinOptions = 2;
        public const int MaxOptions = 4;

        private readonly QuizRepository _quizRepository = new QuizRepository();
        private readonly CourseManager _courseManager = new CourseManager();

        /// <summary>
        /// Returns the course's quiz with its questions. Data is null when the course has no quiz yet.
        /// Only the course's own tutor can see it.
        /// </summary>
        public OperationResult<Quiz> GetQuiz(int courseId)
        {
            OperationResult<Course> course = _courseManager.GetCourse(courseId);
            if (!course.Success)
            {
                return OperationResult<Quiz>.Fail(course.Message);
            }

            try
            {
                Quiz quiz = _quizRepository.GetByCourse(courseId);
                if (quiz != null)
                {
                    quiz.Questions = _quizRepository.GetQuestions(quiz.Id);
                }

                return OperationResult<Quiz>.Ok(quiz);
            }
            catch (Exception exception)
            {
                LogHelper.LogError("QuizManager.GetQuiz failure", exception);
                return OperationResult<Quiz>.Fail("The quiz could not be loaded. Please try again.");
            }
        }

        /// <summary>
        /// For each of the signed-in tutor's courses that has a quiz: course id → number of questions.
        /// </summary>
        public Dictionary<int, int> GetQuestionCounts()
        {
            try
            {
                return _quizRepository.GetQuestionCountsByTutor(SessionHelper.GetCurrentUserID());
            }
            catch (Exception exception)
            {
                LogHelper.LogError("QuizManager.GetQuestionCounts failure", exception);
                return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Creates the course's quiz. Fails if the course already has one. Data holds the new quiz id.
        /// </summary>
        public OperationResult<int> CreateQuiz(Quiz quiz)
        {
            OperationResult<Course> course = GetEditableCourse(quiz.CourseId);
            if (!course.Success)
            {
                return OperationResult<int>.Fail(course.Message);
            }

            string validationError = CleanAndValidate(quiz);
            if (validationError != null)
            {
                return OperationResult<int>.Fail(validationError);
            }

            OperationResult<Quiz> existing = GetQuiz(quiz.CourseId);
            if (!existing.Success)
            {
                return OperationResult<int>.Fail(existing.Message);
            }

            if (existing.Data != null)
            {
                return OperationResult<int>.Fail("This course already has a quiz. A course can only have one quiz.");
            }

            quiz.CreatedByUserId = SessionHelper.GetCurrentUserID();
            int quizId = _quizRepository.Insert(quiz);
            if (quizId == 0)
            {
                return OperationResult<int>.Fail("A system error occurred while saving the quiz. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "CREATE_QUIZ",
                "Created quiz '" + quiz.Title + "' for course '" + course.Data.Title + "'.");
            return OperationResult<int>.Ok(quizId, "The quiz has been created. Now add its questions.");
        }

        /// <summary>
        /// Saves the quiz's title, pass mark and instructions.
        /// </summary>
        public OperationResult UpdateQuiz(Quiz quiz)
        {
            OperationResult<Quiz> existing = GetEditableQuiz(quiz.CourseId);
            if (!existing.Success)
            {
                return OperationResult.Fail(existing.Message);
            }

            string validationError = CleanAndValidate(quiz);
            if (validationError != null)
            {
                return OperationResult.Fail(validationError);
            }

            quiz.Id = existing.Data.Id;
            if (!_quizRepository.Update(quiz))
            {
                return OperationResult.Fail("A system error occurred while saving the quiz. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "UPDATE_QUIZ",
                "Updated quiz '" + quiz.Title + "'.");
            return OperationResult.Ok("The quiz details have been saved.");
        }

        /// <summary>
        /// Deletes the course's quiz with all of its questions.
        /// </summary>
        public OperationResult DeleteQuiz(int courseId)
        {
            OperationResult<Quiz> existing = GetEditableQuiz(courseId);
            if (!existing.Success)
            {
                return OperationResult.Fail(existing.Message);
            }

            if (!_quizRepository.Delete(courseId, existing.Data.Id))
            {
                return OperationResult.Fail("A system error occurred while deleting the quiz. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "DELETE_QUIZ",
                "Deleted quiz '" + existing.Data.Title + "' and its " + existing.Data.Questions.Count + " questions.");
            return OperationResult.Ok("The quiz has been deleted.");
        }

        /// <summary>
        /// Adds a question after the last one. Data holds the new question id.
        /// </summary>
        public OperationResult<int> AddQuestion(int courseId, QuizQuestion question)
        {
            OperationResult<Quiz> quiz = GetEditableQuiz(courseId);
            if (!quiz.Success)
            {
                return OperationResult<int>.Fail(quiz.Message);
            }

            string validationError = CleanAndValidate(question);
            if (validationError != null)
            {
                return OperationResult<int>.Fail(validationError);
            }

            question.QuizId = quiz.Data.Id;
            int questionId = _quizRepository.InsertQuestion(question);
            if (questionId == 0)
            {
                return OperationResult<int>.Fail("A system error occurred while saving the question. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "CREATE_QUESTION",
                "Added a question to quiz '" + quiz.Data.Title + "'.");
            return OperationResult<int>.Ok(questionId, "The question has been added.");
        }

        /// <summary>
        /// Saves changes to a question; it keeps its position.
        /// </summary>
        public OperationResult UpdateQuestion(int courseId, QuizQuestion question)
        {
            OperationResult<Quiz> quiz = GetEditableQuiz(courseId);
            if (!quiz.Success)
            {
                return OperationResult.Fail(quiz.Message);
            }

            if (!quiz.Data.Questions.Exists(q => q.Id == question.Id))
            {
                return OperationResult.Fail("That question could not be found.");
            }

            string validationError = CleanAndValidate(question);
            if (validationError != null)
            {
                return OperationResult.Fail(validationError);
            }

            question.QuizId = quiz.Data.Id;
            if (!_quizRepository.UpdateQuestion(question))
            {
                return OperationResult.Fail("A system error occurred while saving the question. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "UPDATE_QUESTION",
                "Updated a question in quiz '" + quiz.Data.Title + "'.");
            return OperationResult.Ok("The question has been saved.");
        }

        /// <summary>
        /// Deletes a question; the ones after it move up.
        /// </summary>
        public OperationResult DeleteQuestion(int courseId, int questionId)
        {
            OperationResult<Quiz> quiz = GetEditableQuiz(courseId);
            if (!quiz.Success)
            {
                return OperationResult.Fail(quiz.Message);
            }

            if (!_quizRepository.DeleteQuestion(quiz.Data.Id, questionId))
            {
                return OperationResult.Fail("That question could not be deleted. Please try again.");
            }

            new AuditManager().RecordActivity(SessionHelper.GetCurrentUserID(), "DELETE_QUESTION",
                "Deleted a question from quiz '" + quiz.Data.Title + "'.");
            return OperationResult.Ok("The question has been deleted.");
        }

        // The signed-in tutor's own course, if they are allowed to author and the course is still a Draft
        private OperationResult<Course> GetEditableCourse(int courseId)
        {
            OperationResult authorization = _courseManager.CheckCanAuthor();
            if (!authorization.Success)
            {
                return OperationResult<Course>.Fail(authorization.Message);
            }

            OperationResult<Course> course = _courseManager.GetCourse(courseId);
            if (course.Success && !CourseStatuses.IsEditable(course.Data.Status))
            {
                return OperationResult<Course>.Fail("The quiz can only be changed while the course is a Draft.");
            }

            return course;
        }

        // The quiz of an editable course; fails if the course has no quiz yet
        private OperationResult<Quiz> GetEditableQuiz(int courseId)
        {
            OperationResult<Course> course = GetEditableCourse(courseId);
            if (!course.Success)
            {
                return OperationResult<Quiz>.Fail(course.Message);
            }

            OperationResult<Quiz> quiz = GetQuiz(courseId);
            if (quiz.Success && quiz.Data == null)
            {
                return OperationResult<Quiz>.Fail("Create the quiz first.");
            }

            return quiz;
        }

        // Trims the quiz details, then returns the first problem with them, or null if they are valid
        private static string CleanAndValidate(Quiz quiz)
        {
            quiz.Title = Clean(quiz.Title);
            quiz.Instructions = Clean(quiz.Instructions);

            if (quiz.Title == null)
            {
                return "The quiz title is required.";
            }

            if (quiz.Title.Length > MaxTitleLength)
            {
                return "The quiz title must be " + MaxTitleLength + " characters or fewer.";
            }

            if (quiz.Instructions != null && quiz.Instructions.Length > MaxInstructionsLength)
            {
                return "The instructions must be " + MaxInstructionsLength + " characters or fewer.";
            }

            if (quiz.PassMark < 0 || quiz.PassMark > 100)
            {
                return "The pass mark must be between 0 and 100.";
            }

            return null;
        }

        // Trims the question, drops empty options, then returns the first problem, or null if it is valid.
        // For True/False the options are always "True" and "False".
        private static string CleanAndValidate(QuizQuestion question)
        {
            question.QuestionText = Clean(question.QuestionText);
            question.Explanation = Clean(question.Explanation);

            if (question.QuestionText == null)
            {
                return "The question is required.";
            }

            if (question.QuestionText.Length > MaxQuestionLength)
            {
                return "The question must be " + MaxQuestionLength + " characters or fewer.";
            }

            if (question.Explanation != null && question.Explanation.Length > MaxExplanationLength)
            {
                return "The explanation must be " + MaxExplanationLength + " characters or fewer.";
            }

            if (question.QuestionType == QuizQuestion.TrueFalse)
            {
                question.Options = new List<string> { "True", "False" };
                return question.CorrectOptionIndex == 0 || question.CorrectOptionIndex == 1
                    ? null
                    : "Choose whether the answer is True or False.";
            }

            if (question.QuestionType != QuizQuestion.MultipleChoice)
            {
                return "Choose a question type.";
            }

            // The form has one box per option. Empty boxes are skipped, so the correct answer's position
            // is counted among the filled boxes only.
            List<string> filled = new List<string>();
            int correctIndex = -1;
            for (int i = 0; i < question.Options.Count && i < MaxOptions; i++)
            {
                string option = Clean(question.Options[i]);
                if (option == null)
                {
                    continue;
                }

                if (option.Length > MaxOptionLength)
                {
                    return "Each answer option must be " + MaxOptionLength + " characters or fewer.";
                }

                if (i == question.CorrectOptionIndex)
                {
                    correctIndex = filled.Count;
                }

                filled.Add(option);
            }

            if (filled.Count < MinOptions)
            {
                return "Add at least " + MinOptions + " answer options.";
            }

            if (filled.Count != new HashSet<string>(filled, StringComparer.OrdinalIgnoreCase).Count)
            {
                return "Each answer option must be different.";
            }

            if (correctIndex < 0)
            {
                return "Mark which of the filled-in options is the correct answer.";
            }

            question.Options = filled;
            question.CorrectOptionIndex = correctIndex;
            return null;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
