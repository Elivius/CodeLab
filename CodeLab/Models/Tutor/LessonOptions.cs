using System.Collections.Generic;

namespace CodeLab.Models
{
    /// <summary>
    /// The programming languages a lesson's code snippet can use. The value is also the class name
    /// the syntax highlighter understands (language-csharp, language-python, ...).
    /// </summary>
    public static class LessonOptions
    {
        public static readonly List<KeyValuePair<string, string>> CodeLanguages = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("javascript", "JavaScript"),
            new KeyValuePair<string, string>("typescript", "TypeScript"),
            new KeyValuePair<string, string>("python", "Python"),
            new KeyValuePair<string, string>("csharp", "C#"),
            new KeyValuePair<string, string>("sql", "SQL"),
            new KeyValuePair<string, string>("html", "HTML"),
            new KeyValuePair<string, string>("css", "CSS"),
            new KeyValuePair<string, string>("java", "Java"),
            new KeyValuePair<string, string>("cpp", "C++"),
            new KeyValuePair<string, string>("php", "PHP"),
            new KeyValuePair<string, string>("json", "JSON"),
            new KeyValuePair<string, string>("bash", "Bash")
        };

        public static bool IsValidLanguage(string value)
        {
            return CodeLanguages.Exists(language => language.Key == value);
        }
    }
}
