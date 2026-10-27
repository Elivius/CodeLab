using System;
using System.Collections.Generic;
using CodeLab.DAL;
using CodeLab.Models;

namespace CodeLab.BLL
{
    public class LearnerCourseManager
    {
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
