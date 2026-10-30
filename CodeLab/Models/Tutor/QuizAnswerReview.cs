using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeLab.Models
{
    /// <summary>
    /// The snapshot saved in QuizAttempts.AnswerReviewJson: the pass mark and every question as the learner
    /// saw it, with the option they picked. Stored as JSON, e.g.
    /// {"PassMark":75,"Answers":[{"QuestionText":"...","Options":["a","b"],"SelectedOptionIndex":0,
    ///   "CorrectOptionIndex":1,"Explanation":"...","IsCorrect":false}]}
    /// The quiz-taking page writes it with ToJson; the Tutor review pages read it with Parse.
    /// </summary>
    public class QuizAnswerReview
    {
        // The quiz's pass mark at the time of the attempt
        public int? PassMark { get; set; }

        // One entry per question, in the order the learner saw them
        public List<QuizAnswerItem> Answers { get; set; } = new List<QuizAnswerItem>();

        /// <summary>
        /// Reads AnswerReviewJson. Returns null when there is no snapshot (older attempts) or it cannot be read.
        /// </summary>
        public static QuizAnswerReview Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                QuizAnswerReview review = JsonConvert.DeserializeObject<QuizAnswerReview>(json);
                if (review == null || review.Answers == null)
                {
                    return null;
                }

                review.Answers.RemoveAll(answer => answer == null);
                return review;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// The JSON to store in QuizAttempts.AnswerReviewJson.
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// One question in an attempt's answer snapshot.
    /// </summary>
    public class QuizAnswerItem
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } = new List<string>();

        // The option the learner picked, starting at 0; null or out of range when they skipped it
        public int? SelectedOptionIndex { get; set; }

        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; }
        public bool IsCorrect { get; set; }

        [JsonIgnore]
        public bool IsAnswered
        {
            get { return SelectedOptionIndex.HasValue && SelectedOptionIndex.Value >= 0 && SelectedOptionIndex.Value < Options.Count; }
        }
    }
}
