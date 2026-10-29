using System;
using System.Collections.Generic;
using System.Linq;
using CodeLab.DAL;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public sealed class LearnerChallengeManager
    {
        private readonly LearnerChallengeRepository repository =
            new LearnerChallengeRepository();

        public List<LearnerChallenge> List(
            int learnerId,
            string search = "",
            int courseId = 0,
            string difficulty = "",
            string progress = "")
        {
            if (learnerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid learner ID.",
                    nameof(learnerId));
            }

            if (courseId < 0)
            {
                throw new ArgumentException(
                    "Invalid course ID.",
                    nameof(courseId));
            }

            // The repository restricts results to published courses
            // in which this learner is enrolled.
            IEnumerable<LearnerChallenge> challenges =
                repository.GetChallenges(learnerId);

            string searchText = (search ?? string.Empty).Trim();
            string difficultyFilter = (difficulty ?? string.Empty).Trim();
            string progressFilter = (progress ?? string.Empty).Trim();

            if (courseId > 0)
            {
                challenges = challenges.Where(
                    challenge => challenge.CourseId == courseId);
            }

            if (searchText.Length > 0)
            {
                challenges = challenges.Where(challenge =>
                    ContainsText(challenge.Title, searchText) ||
                    ContainsText(challenge.Description, searchText) ||
                    ContainsText(challenge.CourseTitle, searchText) ||
                    ContainsText(challenge.Language, searchText));
            }

            if (difficultyFilter.Length > 0 &&
                !difficultyFilter.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                challenges = challenges.Where(challenge =>
                    string.Equals(
                        challenge.Difficulty,
                        difficultyFilter,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (progressFilter.Length > 0 &&
                !progressFilter.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase))
            {
                challenges = challenges.Where(challenge =>
                    string.Equals(
                        challenge.ProgressStatus,
                        progressFilter,
                        StringComparison.OrdinalIgnoreCase));
            }

            return challenges.ToList();
        }

        private static bool ContainsText(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(
                       search,
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}