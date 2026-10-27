using System;

namespace CodeLab.Models
{
    /// <summary>
    /// The allowed values for a course's category and difficulty. The Difficulty values must match
    /// the CHECK constraint on Courses.Difficulty in Schema.sql.
    /// </summary>
    public static class CourseOptions
    {
        public static readonly string[] Categories =
        {
            "Web Development",
            "Frontend Engineering",
            "Backend Development",
            "Programming Fundamentals",
            "Computer Science",
            "Databases & SQL",
            "Python",
            "C# & .NET",
            "Mobile Development",
            "Data Science"
        };

        public static readonly string[] Difficulties = { "Beginner", "Intermediate", "Advanced" };

        public static bool IsValidCategory(string value)
        {
            return Array.IndexOf(Categories, value) >= 0;
        }

        public static bool IsValidDifficulty(string value)
        {
            return Array.IndexOf(Difficulties, value) >= 0;
        }
    }
}
