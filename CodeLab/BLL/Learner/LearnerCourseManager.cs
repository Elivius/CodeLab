using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class LearnerCourseManager
    {
        private static void AuthorizeReview(int learnerId)
        {
            if (!CodeLab.Helpers.SessionHelper.IsUserInRole("Learner") ||
                CodeLab.Helpers.SessionHelper.GetCurrentUserID() != learnerId || learnerId <= 0)
                throw new InvalidOperationException("Please sign in with your learner account.");
        }

        public void SaveAttemptReview(System.Data.SqlClient.SqlTransaction transaction,
            int learnerId, int courseId, int attemptId, QuizAttemptReview review)
        {
            AuthorizeReview(learnerId);
            ValidateReview(review);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            repo.SaveAttemptReview(transaction, learnerId, courseId, attemptId, serializer.Serialize(review));
        }

        public QuizAttemptReview GetAttemptReview(int learnerId, int courseId, int attemptId)
        {
            AuthorizeReview(learnerId);
            string json = repo.GetAttemptReview(learnerId, courseId, attemptId);
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var review = serializer.Deserialize<QuizAttemptReview>(json);
                ValidateReview(review);
                return review;
            }
            catch (ArgumentException ex)
            {
                CodeLab.Helpers.LogHelper.LogError("Reading quiz attempt review", ex);
                throw new InvalidOperationException("Detailed answers could not be loaded.");
            }
        }

        private static void ValidateReview(QuizAttemptReview review)
        {
            if (review == null || review.PassMark < 0 || review.PassMark > 100 ||
                review.Answers == null || review.Answers.Count == 0)
                throw new ArgumentException("Invalid attempt review.");
            foreach (var answer in review.Answers)
            {
                if (answer == null || answer.Options == null || answer.Options.Count < 2 ||
                    answer.Options.Exists(option => option == null) ||
                    answer.SelectedOptionIndex < 0 || answer.SelectedOptionIndex >= answer.Options.Count ||
                    answer.CorrectOptionIndex < 0 || answer.CorrectOptionIndex >= answer.Options.Count)
                    throw new ArgumentException("Invalid attempt answer.");
            }
        }

        private readonly LearnerCourseRepository repo = new LearnerCourseRepository();
        private static void CheckLearner(int id) { if (id <= 0) throw new ArgumentException("Invalid learner."); }
        public List<LearnerCourse> List(int learnerId, bool enrolled, string search, string category)
        {
            CheckLearner(learnerId);
            return repo.GetCourses(learnerId, enrolled, (search ?? "").Trim(), (category ?? "").Trim());
        }
        public List<string> Categories() { return repo.GetCategories(); }
        public LearnerCourse Details(int learnerId, int courseId)
        {
            CheckLearner(learnerId);
            if (courseId <= 0) throw new ArgumentException("Invalid course.");
            return repo.GetCourse(learnerId, courseId);
        }
        public void Enroll(int learnerId, int courseId)
        {
            CheckLearner(learnerId);
            if (courseId <= 0) throw new ArgumentException("Invalid course.");
            repo.Enroll(learnerId, courseId);
        }
        public void Unenroll(int learnerId, int courseId)
        {
            CheckLearner(learnerId);
            if (courseId <= 0) throw new ArgumentException("Invalid course.");
            repo.Unenroll(learnerId, courseId);
        }
        public void CompleteLesson(int learnerId, int courseId, int lessonId, bool completed)
        {
            CheckLearner(learnerId);
            if (courseId <= 0 || lessonId <= 0) throw new ArgumentException("Invalid lesson.");
            repo.SetLessonCompleted(learnerId, courseId, lessonId, completed);
        }
    }
}
