using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.BLL
{
    /// <summary>
    /// Learner results for the signed-in tutor's quizzes and coding challenges: who attempted them,
    /// each attempt's score, and which answers were right or wrong. Read-only; it never changes data.
    /// </summary>
    public class LearnerResultsManager
    {
        private readonly LearnerResultsRepository _repository = new LearnerResultsRepository();
        private readonly CourseManager _courseManager = new CourseManager();

        // ---------- Quizzes ----------

        public OperationResult<List<QuizResultSummary>> GetQuizzes()
        {
            return Run("GetQuizzes", "The quizzes could not be loaded. Please try again.",
                tutorId => _repository.GetQuizSummaries(tutorId, 0));
        }

        /// <summary>
        /// One quiz's results, or a failure if it is not one of the tutor's quizzes.
        /// </summary>
        public OperationResult<QuizResultSummary> GetQuiz(int quizId)
        {
            if (quizId <= 0)
            {
                return OperationResult<QuizResultSummary>.Fail("Quiz not found.");
            }

            OperationResult<List<QuizResultSummary>> result = Run("GetQuiz", "The quiz could not be loaded. Please try again.",
                tutorId => _repository.GetQuizSummaries(tutorId, quizId));
            if (!result.Success)
            {
                return OperationResult<QuizResultSummary>.Fail(result.Message);
            }

            return result.Data.Count == 0
                ? OperationResult<QuizResultSummary>.Fail("Quiz not found, or it is not in one of your published courses.")
                : OperationResult<QuizResultSummary>.Ok(result.Data[0]);
        }

        public OperationResult<List<QuizAttemptRow>> GetQuizAttempts(int quizId)
        {
            return Run("GetQuizAttempts", "The attempts could not be loaded. Please try again.",
                tutorId => _repository.GetQuizAttempts(tutorId, quizId));
        }

        public OperationResult<QuizAttemptDetail> GetQuizAttempt(int attemptId)
        {
            if (attemptId <= 0)
            {
                return OperationResult<QuizAttemptDetail>.Fail("Attempt not found.");
            }

            OperationResult<QuizAttemptDetail> result = Run("GetQuizAttempt", "The attempt could not be loaded. Please try again.",
                tutorId => _repository.GetQuizAttempt(tutorId, attemptId));
            if (result.Success && result.Data == null)
            {
                return OperationResult<QuizAttemptDetail>.Fail("Attempt not found, or it is not in one of your published courses.");
            }

            return result;
        }

        // ---------- Coding challenges ----------

        public OperationResult<List<ChallengeResultSummary>> GetChallenges()
        {
            return Run("GetChallenges", "The coding challenges could not be loaded. Please try again.",
                tutorId => _repository.GetChallengeSummaries(tutorId, 0));
        }

        public OperationResult<ChallengeResultSummary> GetChallenge(int challengeId)
        {
            if (challengeId <= 0)
            {
                return OperationResult<ChallengeResultSummary>.Fail("Coding challenge not found.");
            }

            OperationResult<List<ChallengeResultSummary>> result = Run("GetChallenge", "The coding challenge could not be loaded. Please try again.",
                tutorId => _repository.GetChallengeSummaries(tutorId, challengeId));
            if (!result.Success)
            {
                return OperationResult<ChallengeResultSummary>.Fail(result.Message);
            }

            return result.Data.Count == 0
                ? OperationResult<ChallengeResultSummary>.Fail("Coding challenge not found, or it is not in one of your published courses.")
                : OperationResult<ChallengeResultSummary>.Ok(result.Data[0]);
        }

        public OperationResult<List<ChallengeSubmissionRow>> GetSubmissions(int challengeId)
        {
            return Run("GetSubmissions", "The submissions could not be loaded. Please try again.",
                tutorId => _repository.GetSubmissions(tutorId, challengeId));
        }

        public OperationResult<ChallengeSubmissionDetail> GetSubmission(int submissionId)
        {
            if (submissionId <= 0)
            {
                return OperationResult<ChallengeSubmissionDetail>.Fail("Submission not found.");
            }

            OperationResult<ChallengeSubmissionDetail> result = Run("GetSubmission", "The submission could not be loaded. Please try again.",
                tutorId => _repository.GetSubmission(tutorId, submissionId));
            if (result.Success && result.Data == null)
            {
                return OperationResult<ChallengeSubmissionDetail>.Fail("Submission not found, or it is not in one of your published courses.");
            }

            return result;
        }

        // Checks the tutor may view results (an approved tutor or an Admin), then runs the query for the signed-in tutor
        private OperationResult<T> Run<T>(string operation, string failMessage, Func<int, T> load)
        {
            OperationResult authorization = _courseManager.CheckCanAuthor();
            if (!authorization.Success)
            {
                return OperationResult<T>.Fail(authorization.Message);
            }

            try
            {
                return OperationResult<T>.Ok(load(SessionHelper.GetCurrentUserID()));
            }
            catch (Exception exception)
            {
                LogHelper.LogError("LearnerResultsManager." + operation + " failure", exception);
                return OperationResult<T>.Fail(failMessage);
            }
        }
    }
}
