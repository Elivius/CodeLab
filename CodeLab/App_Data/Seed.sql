-- Development seed data. Safe to re-run: every row is only inserted if it is missing.
-- Never run this on a real deployment: the passwords below are public.
--
-- Test accounts (one per role):
--   admin   / Admin@123
--   tutor   / Tutor@123
--   learner / Learner@123
--
-- Password hashes are produced by PasswordHelper.HashPassword.
-- Rows refer to each other by looking up names, never by hard-coded Id numbers.

SET NOCOUNT ON;

-- =====================================
-- Users
-- =====================================
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
    INSERT INTO dbo.Users (FullName, Username, Email, PasswordHash, Role, Status)
    VALUES (N'System Administrator', N'admin', N'admin@codelab.local',
            N'PBKDF2$100000$OBc+sqcmvyOf3s03wTz3zg==$za1bHIPChNws30L5gLcbVmZJUR+yTbACUOrJeiHLN8A=', N'Admin', N'Active');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'tutor')
    INSERT INTO dbo.Users (FullName, Username, Email, PasswordHash, Role, Status)
    VALUES (N'Dr. Alan Turing', N'tutor', N'tutor@codelab.local',
            N'PBKDF2$100000$FEu2YWRbd+EGiSvsO+FwuA==$P6uWyc8nseayvQ65rCeGe/YVYk18mtndKpFBZqK8K0k=', N'Tutor', N'Active');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'learner')
    INSERT INTO dbo.Users (FullName, Username, Email, PasswordHash, Role, Status)
    VALUES (N'Sarah Connor', N'learner', N'learner@codelab.local',
            N'PBKDF2$100000$37zQ2kfdZ148fOYGIfD+iA==$w7QKt8ThVv0X/Ex8yqMapnaL9rHduxn/AtQG1106MTg=', N'Learner', N'Active');

DECLARE @TutorId INT = (SELECT Id FROM dbo.Users WHERE Username = N'tutor');
DECLARE @LearnerId INT = (SELECT Id FROM dbo.Users WHERE Username = N'learner');

-- =====================================
-- Tutor profile (already approved, so the demo tutor can publish)
-- =====================================
IF NOT EXISTS (SELECT 1 FROM dbo.TutorProfiles WHERE UserId = @TutorId)
    INSERT INTO dbo.TutorProfiles (UserId, Qualifications, PortfolioUrl, CurriculumPitch, Bio, Specialty, Status, ApprovedDate)
    VALUES (@TutorId, N'Ph.D. in Computer Science', N'https://portfolio.codelab.local/turing',
            N'A hands-on algorithms course: complexity, sorting, graphs and dynamic programming, each with coding challenges.',
            N'Father of modern computing and algorithm design.', N'Algorithms & Logic', N'Approved', SYSUTCDATETIME());

-- =====================================
-- Courses (the three featured on the landing page)
-- =====================================
IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE Title = N'Modern TypeScript & Scalable React Patterns')
    INSERT INTO dbo.Courses (Title, Description, Category, Difficulty, InstructorId, Status)
    VALUES (N'Modern TypeScript & Scalable React Patterns',
            N'Master enterprise-grade TypeScript type gymnastics, React 18 concurrent rendering, custom hooks, and scalable state management.',
            N'Frontend Engineering', N'Intermediate', @TutorId, N'Published');

IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE Title = N'Algorithms & Discrete Math for Developers')
    INSERT INTO dbo.Courses (Title, Description, Category, Difficulty, InstructorId, Status)
    VALUES (N'Algorithms & Discrete Math for Developers',
            N'Explore rigorous algorithmic principles, asymptotic complexity, graph theory, dynamic programming, and proof techniques.',
            N'Computer Science', N'Advanced', @TutorId, N'Published');

IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE Title = N'Python Essentials: From Zero to Automated Scripts')
    INSERT INTO dbo.Courses (Title, Description, Category, Difficulty, InstructorId, Status)
    VALUES (N'Python Essentials: From Zero to Automated Scripts',
            N'Comprehensive introduction to Python programming syntax, idiomatic data structures, file I/O operations, and task automation.',
            N'Programming Fundamentals', N'Beginner', @TutorId, N'Published');

DECLARE @TypeScriptCourseId INT = (SELECT Id FROM dbo.Courses WHERE Title = N'Modern TypeScript & Scalable React Patterns');

-- =====================================
-- Lesson
-- =====================================
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId = @TypeScriptCourseId AND Title = N'Introduction to TypeScript Types')
    INSERT INTO dbo.Lessons (CourseId, Title, Content, CodeSnippet, CodeLanguage, OrderIndex)
    VALUES (@TypeScriptCourseId, N'Introduction to TypeScript Types',
            N'Learn how type annotations make your web applications robust and bug-free.',
            N'const greeting: string = "Hello, CodeLab!";' + CHAR(10) + N'console.log(greeting);',
            N'typescript', 1);

-- =====================================
-- Quiz and question
-- =====================================
IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE Title = N'TypeScript Fundamentals Quiz')
    INSERT INTO dbo.Quizzes (CourseId, Title, PassMark, CreatedByUserId, Instructions)
    VALUES (@TypeScriptCourseId, N'TypeScript Fundamentals Quiz', 80, @TutorId,
            N'Test your knowledge on static typing and basic interfaces.');

DECLARE @QuizId INT = (SELECT Id FROM dbo.Quizzes WHERE Title = N'TypeScript Fundamentals Quiz');

IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId = @QuizId AND OrderIndex = 1)
    INSERT INTO dbo.QuizQuestions (QuizId, QuestionText, OptionsJson, CorrectOptionIndex, Explanation, OrderIndex)
    VALUES (@QuizId,
            N'Which keyword is used to declare a block-scoped variable in modern JavaScript/TypeScript?',
            N'["var", "let", "global", "dim"]', 1,
            N'The "let" keyword defines block-scoped variables in modern ECMAScript.', 1);

-- =====================================
-- Learner gamification stats
-- =====================================
IF NOT EXISTS (SELECT 1 FROM dbo.LearnerStats WHERE LearnerId = @LearnerId)
    INSERT INTO dbo.LearnerStats (LearnerId, AchievementPoints, CurrentLevel, LevelTitle, NextMilestonePoints, StreakDays)
    VALUES (@LearnerId, 1250, 4, N'Code Voyager', 2000, 5);
GO

-- Expanded curriculum lessons and quizzes
GO
-- CodeLab: expanded development curriculum seed (safe to run repeatedly).
-- Targets the existing Schema.sql. Does not delete or overwrite existing courses, lessons or quizzes.
-- Intended for local/demo database only. Run against CodeLabDb Database.mdf.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @TutorId INT = (SELECT TOP (1) Id FROM dbo.Users WHERE Username=N'tutor');
IF @TutorId IS NULL THROW 51010, 'Seed tutor user missing. Run original Seed.sql first.', 1;
DECLARE @CourseId INT, @QuizId INT;
-- Course 1: Modern TypeScript & Scalable React Patterns
SET @CourseId = (SELECT TOP (1) Id FROM dbo.Courses WHERE Title=N'Modern TypeScript & Scalable React Patterns' ORDER BY Id);
IF @CourseId IS NULL THROW 51011, 'Expected seeded course missing. Run original Seed.sql first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Introduction to TypeScript Types')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Introduction to TypeScript Types',N'TypeScript checks types before code runs. Use string, number and boolean annotations when they clarify intent. Type inference usually removes the need for repeated annotations. Try changing the age variable to text and observe the compiler error.',N'const learner: string = "Sarah";
const age: number = 21;
const active: boolean = true;
console.log(`${learner} (${age}): ${active}`);',N'typescript',1,15);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Interfaces and Reusable Types')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Interfaces and Reusable Types',N'Interfaces describe object shapes. Optional properties use a question mark. A type alias can describe a union such as a status. Use these contracts for component props and API responses.',N'interface Course {
  id: number;
  title: string;
  duration?: number;
}
type Status = "draft" | "published";
const course: Course = { id: 1, title: "TypeScript" };',N'typescript',2,20);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Generics and Type-Safe Functions')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Generics and Type-Safe Functions',N'A generic function preserves information about its input type. It avoids using any and keeps return values predictable. Practice writing a function that returns the first item in a list.',N'function first<T>(items: T[]): T | undefined {
  return items[0];
}
console.log(first<number>([3, 5, 8]));',N'typescript',3,22);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'React Components and Typed Props')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'React Components and Typed Props',N'React components receive props. Define a TypeScript interface for those props so the editor can detect missing fields. Keep components focused and reusable.',N'type WelcomeProps = { name: string; level: number };
function Welcome({ name, level }: WelcomeProps) {
  return <h2>Hello {name}, level {level}</h2>;
}',N'typescript',4,25);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'State, Effects and Custom Hooks')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'State, Effects and Custom Hooks',N'useState stores local UI state. useEffect synchronizes a component with external systems. Extract repeated stateful logic into a custom hook. Do not put derived values into state unless necessary.',N'import { useState } from "react";
function Counter() {
  const [count, setCount] = useState(0);
  return <button onClick={() => setCount(c => c + 1)}>{count}</button>;
}',N'typescript',5,28);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Performance and Accessible UI')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Performance and Accessible UI',N'Measure before optimizing. Memoization can help expensive computations but is not needed everywhere. Use semantic elements, keyboard-accessible controls and meaningful labels.',N'const courses = items.filter(item => item.published);
// Prefer semantic buttons and clear accessible labels.
<button aria-label="Open course details">Open</button>',N'typescript',6,20);
IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'TypeScript Fundamentals Quiz')
    INSERT INTO dbo.Quizzes (CourseId,Title,PassMark,CreatedByUserId,Instructions)
    VALUES (@CourseId,N'TypeScript Fundamentals Quiz',75,@TutorId,N'Answer all four questions. You need at least 75 percent to pass.');
SET @QuizId=(SELECT TOP (1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'TypeScript Fundamentals Quiz' ORDER BY Id);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which TypeScript type describes true or false?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Which TypeScript type describes true or false?',N'Multiple Choice',N'["string", "boolean", "number", "object"]',1,N'boolean is the primitive type for true and false.',11);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does an optional interface property use?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'What does an optional interface property use?',N'Multiple Choice',N'["!", "?", "#", "@"]',1,N'A question mark marks an optional property.',12);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Why use a generic function?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Why use a generic function?',N'Multiple Choice',N'["To avoid functions", "To preserve input and output types", "To disable checking", "To force string outputs"]',1,N'Generics retain type information.',13);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which React hook stores local component state?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Which React hook stores local component state?',N'Multiple Choice',N'["useState", "useEffect", "useMemo", "useId"]',0,N'useState holds state across renders.',14);
-- Course 2: Algorithms & Discrete Math for Developers
SET @CourseId = (SELECT TOP (1) Id FROM dbo.Courses WHERE Title=N'Algorithms & Discrete Math for Developers' ORDER BY Id);
IF @CourseId IS NULL THROW 51011, 'Expected seeded course missing. Run original Seed.sql first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Big-O and Growth Rates')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Big-O and Growth Rates',N'Big-O describes how resource use grows as input size increases. A single loop is usually O(n), while nested loops often give O(n squared). Always identify the input size and count dominant operations.',N'def linear_search(items, target):
    for index, value in enumerate(items):
        if value == target:
            return index
    return -1',N'python',1,25);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Recursion and Mathematical Induction')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Recursion and Mathematical Induction',N'A recursive solution needs a base case and a smaller subproblem. Mathematical induction proves a statement by establishing a base case and an inductive step.',N'def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)
print(factorial(5))',N'python',2,28);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Sorting and Binary Search')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Sorting and Binary Search',N'Sorting creates order that binary search can exploit. Binary search repeatedly halves the search interval and takes O(log n) comparisons on a sorted array.',N'def binary_search(a, x):
    lo, hi = 0, len(a) - 1
    while lo <= hi:
        mid = (lo + hi) // 2
        if a[mid] == x: return mid
        if a[mid] < x: lo = mid + 1
        else: hi = mid - 1
    return -1',N'python',3,30);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Graphs and Breadth-First Search')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Graphs and Breadth-First Search',N'A graph contains vertices and edges. Breadth-first search explores neighbours layer by layer. With an adjacency list it runs in O(V + E) time.',N'from collections import deque

def bfs(graph, start):
    seen, queue = {start}, deque([start])
    while queue:
        node = queue.popleft()
        print(node)
        for nxt in graph.get(node, []):
            if nxt not in seen:
                seen.add(nxt)
                queue.append(nxt)',N'python',4,32);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Dynamic Programming')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Dynamic Programming',N'Dynamic programming stores answers to repeated subproblems. Identify a state, transition and base cases. A bottom-up Fibonacci calculation runs in O(n) time.',N'def fibonacci(n):
    if n < 2: return n
    a, b = 0, 1
    for _ in range(2, n + 1):
        a, b = b, a + b
    return b',N'python',5,35);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Proofs, Sets and Logic')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Proofs, Sets and Logic',N'Sets support union, intersection and difference. Logical implication and quantifiers help state algorithm properties precisely. Use a loop invariant to argue that a search remains correct.',N'a = {1, 2, 3}
b = {3, 4, 5}
print(a | b)  # union
print(a & b)  # intersection
print(a - b)  # difference',N'python',6,25);
IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Algorithms and Discrete Math Quiz')
    INSERT INTO dbo.Quizzes (CourseId,Title,PassMark,CreatedByUserId,Instructions)
    VALUES (@CourseId,N'Algorithms and Discrete Math Quiz',75,@TutorId,N'Answer all four questions. You need at least 75 percent to pass.');
SET @QuizId=(SELECT TOP (1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Algorithms and Discrete Math Quiz' ORDER BY Id);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is binary search time complexity on a sorted array?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'What is binary search time complexity on a sorted array?',N'Multiple Choice',N'["O(n squared)", "O(log n)", "O(n)", "O(1) always"]',1,N'The search interval is halved each step.',11);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does BFS normally use to visit nodes in layers?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'What does BFS normally use to visit nodes in layers?',N'Multiple Choice',N'["Stack", "Queue", "Heap", "Set only"]',1,N'A queue supports first-in-first-out traversal.',12);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which is the base case for factorial?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Which is the base case for factorial?',N'Multiple Choice',N'["n is always 10", "n <= 1", "n > 100", "No base case"]',1,N'The base case stops recursion.',13);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does dynamic programming store?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'What does dynamic programming store?',N'Multiple Choice',N'["Repeated subproblem results", "Only user input", "Random results", "Only graph edges"]',0,N'Stored subproblem answers avoid repeated work.',14);
-- Course 3: Python Essentials: From Zero to Automated Scripts
SET @CourseId = (SELECT TOP (1) Id FROM dbo.Courses WHERE Title=N'Python Essentials: From Zero to Automated Scripts' ORDER BY Id);
IF @CourseId IS NULL THROW 51011, 'Expected seeded course missing. Run original Seed.sql first.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Getting Started with Python')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Getting Started with Python',N'Python executes statements from top to bottom. Use print to display output and comments to explain intent. Indentation groups statements. Run this example and change the greeting.',N'name = "Sarah"
print("Hello,", name)
print("Welcome to CodeLab")',N'python',1,15);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Variables and Data Types')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Variables and Data Types',N'Variables hold references to values. Common types include int, float, str and bool. Convert text to numbers with int or float after validating input.',N'age = 20
price = 19.50
is_student = True
print(type(age), type(price), type(is_student))',N'python',2,18);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Conditions and Loops')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Conditions and Loops',N'Use if, elif and else to make decisions. A for loop processes sequences, while a while loop repeats while its condition remains true.',N'score = 75
if score >= 50:
    print("Pass")
else:
    print("Try again")
for number in range(1, 4):
    print(number)',N'python',3,22);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Lists and Dictionaries')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Lists and Dictionaries',N'Lists store ordered items. Dictionaries map keys to values. Use append to extend a list and get to retrieve a dictionary value safely.',N'skills = ["Python", "SQL"]
skills.append("Git")
student = {"name": "Sarah", "level": 1}
print(skills)
print(student.get("name"))',N'python',4,24);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Functions and Error Handling')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Functions and Error Handling',N'Functions package reusable behaviour and return results. Handle expected errors with try and except. Avoid broad exception handlers that hide programming mistakes.',N'def calculate_total(price, quantity):
    return price * quantity
try:
    amount = int("3")
    print(calculate_total(12, amount))
except ValueError:
    print("Enter a valid number")',N'python',5,24);
IF NOT EXISTS (SELECT 1 FROM dbo.Lessons WHERE CourseId=@CourseId AND Title=N'Files and Simple Automation')
    INSERT INTO dbo.Lessons (CourseId,Title,Content,CodeSnippet,CodeLanguage,OrderIndex,DurationMinutes)
    VALUES (@CourseId,N'Files and Simple Automation',N'Use with open to close files automatically. Read and write UTF-8 text. The pathlib library simplifies file paths and is useful for basic automation.',N'from pathlib import Path
path = Path("notes.txt")
path.write_text("Learning Python", encoding="utf-8")
print(path.read_text(encoding="utf-8"))',N'python',6,28);
IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Python Essentials Quiz')
    INSERT INTO dbo.Quizzes (CourseId,Title,PassMark,CreatedByUserId,Instructions)
    VALUES (@CourseId,N'Python Essentials Quiz',75,@TutorId,N'Answer all four questions. You need at least 75 percent to pass.');
SET @QuizId=(SELECT TOP (1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Python Essentials Quiz' ORDER BY Id);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which statement displays text in Python?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Which statement displays text in Python?',N'Multiple Choice',N'["echo()", "print()", "show()", "write()"]',1,N'print writes to standard output.',11);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which type is used for a list of items?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Which type is used for a list of items?',N'Multiple Choice',N'["list", "int", "bool", "float"]',0,N'A list is an ordered collection.',12);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which keyword starts a function definition?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'Which keyword starts a function definition?',N'Multiple Choice',N'["func", "function", "def", "method"]',2,N'Python functions use def.',13);
IF NOT EXISTS (SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does with open(...) help ensure?')
    INSERT INTO dbo.QuizQuestions (QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
    VALUES (@QuizId,N'What does with open(...) help ensure?',N'Multiple Choice',N'["File is automatically closed", "All errors disappear", "File is deleted", "Code is compiled"]',0,N'The context manager closes the file.',14);
COMMIT TRANSACTION;
PRINT 'Curriculum seed completed.';
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;

SELECT c.Title,COUNT(DISTINCT l.Id) AS Lessons,COUNT(DISTINCT q.Id) AS Quizzes
FROM dbo.Courses c LEFT JOIN dbo.Lessons l ON l.CourseId=c.Id
LEFT JOIN dbo.Quizzes q ON q.CourseId=c.Id
WHERE c.Title IN (N'Modern TypeScript & Scalable React Patterns',N'Algorithms & Discrete Math for Developers',N'Python Essentials: From Zero to Automated Scripts')
GROUP BY c.Title ORDER BY c.Title;
GO

-- Module grouping and cumulative assessments
-- Run 01_Migrate_Curriculum.sql and CodeLab_Expanded_Curriculum_Seed.sql first.
-- Idempotent seed: no destructive changes to existing learner progress.
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @CourseId INT, @ModuleId INT, @QuizId INT, @TutorId INT;
SET @TutorId=(SELECT TOP(1) Id FROM dbo.Users WHERE Username=N'tutor');
IF @TutorId IS NULL THROW 51010, 'Seed tutor missing.', 1;
-- Modern TypeScript & Scalable React Patterns
SET @CourseId=(SELECT TOP(1) Id FROM dbo.Courses WHERE Title=N'Modern TypeScript & Scalable React Patterns' ORDER BY Id);
IF @CourseId IS NULL THROW 51011, 'Required course missing.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=1)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'TypeScript Foundations',N'Two guided lessons and a knowledge check.',1);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=1);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (1,2) AND l.ModuleId IS NULL;
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'TypeScript Fundamentals Quiz' ORDER BY Id);
IF @QuizId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE ModuleId=@ModuleId) UPDATE dbo.Quizzes SET ModuleId=@ModuleId WHERE Id=@QuizId;
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=2)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Typed React Components',N'Two guided lessons and a knowledge check.',2);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=2);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (3,4) AND l.ModuleId IS NULL;
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 2 Quiz: Typed React Components')
 INSERT dbo.Quizzes(CourseId,ModuleId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,@ModuleId,N'Module 2 Quiz: Typed React Components',70,@TutorId,N'Answer the module questions.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 2 Quiz: Typed React Components' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which statement correctly declares a typed number?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which statement correctly declares a typed number?',N'Multiple Choice',N'["let count: number = 5", "let count: string = 5", "let count: boolean = 5", "let count: null = 5"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which React feature passes data from a parent component?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which React feature passes data from a parent component?',N'Multiple Choice',N'["props", "reducers only", "effects only", "DOM IDs"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=3)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Scalable Application Patterns',N'Two guided lessons and a knowledge check.',3);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=3);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (5,6) AND l.ModuleId IS NULL;
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 3 Quiz: Scalable Application Patterns')
 INSERT dbo.Quizzes(CourseId,ModuleId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,@ModuleId,N'Module 3 Quiz: Scalable Application Patterns',70,@TutorId,N'Answer the module questions.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 3 Quiz: Scalable Application Patterns' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which hook is intended for synchronizing with an external system?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which hook is intended for synchronizing with an external system?',N'Multiple Choice',N'["useEffect", "useId", "useState", "useRef only"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is an appropriate reason to create a custom hook?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is an appropriate reason to create a custom hook?',N'Multiple Choice',N'["Reuse stateful logic", "Avoid all components", "Replace the browser", "Store passwords in JSX"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Mid-Term Assessment - Modern TypeScript & Scalable React Patterns')
 INSERT dbo.Quizzes(CourseId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,N'Mid-Term Assessment - Modern TypeScript & Scalable React Patterns',70,@TutorId,N'Mid-term: modules 1-2.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Mid-Term Assessment - Modern TypeScript & Scalable React Patterns' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseAssessments WHERE CourseId=@CourseId AND AssessmentType=N'Midterm')
 INSERT dbo.CourseAssessments(CourseId,QuizId,Title,AssessmentType,AfterModuleOrder,TimeLimitMinutes,MaxAttempts,WeightPercent) VALUES(@CourseId,@QuizId,N'Mid-Term Assessment',N'Midterm',2,30,2,40);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which type is used for true/false values in TypeScript?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which type is used for true/false values in TypeScript?',N'Multiple Choice',N'["number", "boolean", "string", "unknown"]',1,N'This question checks understanding of a core concept from the covered modules.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is the purpose of an interface?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is the purpose of an interface?',N'Multiple Choice',N'["Describe an object shape", "Run a browser", "Start a database", "Install packages"]',0,N'This question checks understanding of a core concept from the covered modules.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does useState do in React?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does useState do in React?',N'Multiple Choice',N'["Fetch a web page", "Manage component state", "Create a CSS file", "Compile TypeScript"]',1,N'This question checks understanding of a core concept from the covered modules.',3);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What helps reduce unnecessary expensive calculations?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What helps reduce unnecessary expensive calculations?',N'Multiple Choice',N'["useMemo", "setTimeout", "console.log", "JSON.parse"]',0,N'This question checks understanding of a core concept from the covered modules.',4);
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Final Assessment - Modern TypeScript & Scalable React Patterns')
 INSERT dbo.Quizzes(CourseId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,N'Final Assessment - Modern TypeScript & Scalable React Patterns',70,@TutorId,N'Cumulative: all modules.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Final Assessment - Modern TypeScript & Scalable React Patterns' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseAssessments WHERE CourseId=@CourseId AND AssessmentType=N'Final')
 INSERT dbo.CourseAssessments(CourseId,QuizId,Title,AssessmentType,AfterModuleOrder,TimeLimitMinutes,MaxAttempts,WeightPercent) VALUES(@CourseId,@QuizId,N'Final Assessment',N'Final',3,60,2,60);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which type is used for true/false values in TypeScript?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which type is used for true/false values in TypeScript?',N'Multiple Choice',N'["number", "boolean", "string", "unknown"]',1,N'This question checks understanding of a core concept from the covered modules.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is the purpose of an interface?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is the purpose of an interface?',N'Multiple Choice',N'["Describe an object shape", "Run a browser", "Start a database", "Install packages"]',0,N'This question checks understanding of a core concept from the covered modules.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does useState do in React?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does useState do in React?',N'Multiple Choice',N'["Fetch a web page", "Manage component state", "Create a CSS file", "Compile TypeScript"]',1,N'This question checks understanding of a core concept from the covered modules.',3);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What helps reduce unnecessary expensive calculations?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What helps reduce unnecessary expensive calculations?',N'Multiple Choice',N'["useMemo", "setTimeout", "console.log", "JSON.parse"]',0,N'This question checks understanding of a core concept from the covered modules.',4);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which syntax marks an optional property?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which syntax marks an optional property?',N'Multiple Choice',N'["?", "!", "&", "%"]',0,N'This question checks understanding of a core concept from the covered modules.',5);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does a generic function preserve?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does a generic function preserve?',N'Multiple Choice',N'["Input and output type relationships", "Browser cookies", "CSS inheritance", "File permissions"]',0,N'This question checks understanding of a core concept from the covered modules.',6);
-- Algorithms & Discrete Math for Developers
SET @CourseId=(SELECT TOP(1) Id FROM dbo.Courses WHERE Title=N'Algorithms & Discrete Math for Developers' ORDER BY Id);
IF @CourseId IS NULL THROW 51011, 'Required course missing.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=1)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Complexity and Recursion',N'Two guided lessons and a knowledge check.',1);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=1);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (1,2) AND l.ModuleId IS NULL;
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Algorithms and Discrete Math Quiz' ORDER BY Id);
IF @QuizId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE ModuleId=@ModuleId) UPDATE dbo.Quizzes SET ModuleId=@ModuleId WHERE Id=@QuizId;
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=2)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Searching and Graphs',N'Two guided lessons and a knowledge check.',2);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=2);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (3,4) AND l.ModuleId IS NULL;
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 2 Quiz: Searching and Graphs')
 INSERT dbo.Quizzes(CourseId,ModuleId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,@ModuleId,N'Module 2 Quiz: Searching and Graphs',70,@TutorId,N'Answer the module questions.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 2 Quiz: Searching and Graphs' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which complexity grows fastest as n increases?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which complexity grows fastest as n increases?',N'Multiple Choice',N'["O(log n)", "O(n)", "O(n\u00b2)", "O(1)"]',2,N'Review the corresponding module lesson for the reasoning behind this answer.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which traversal visits nodes layer by layer?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which traversal visits nodes layer by layer?',N'Multiple Choice',N'["Depth-first search", "Breadth-first search", "Binary search", "Selection sort"]',1,N'Review the corresponding module lesson for the reasoning behind this answer.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=3)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Dynamic Programming and Logic',N'Two guided lessons and a knowledge check.',3);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=3);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (5,6) AND l.ModuleId IS NULL;
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 3 Quiz: Dynamic Programming and Logic')
 INSERT dbo.Quizzes(CourseId,ModuleId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,@ModuleId,N'Module 3 Quiz: Dynamic Programming and Logic',70,@TutorId,N'Answer the module questions.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 3 Quiz: Dynamic Programming and Logic' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What must a valid recursive algorithm include?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What must a valid recursive algorithm include?',N'Multiple Choice',N'["A stopping condition", "A database table", "A random value", "A graphics engine"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which approach caches repeated subproblem answers?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which approach caches repeated subproblem answers?',N'Multiple Choice',N'["Dynamic programming", "Bubble sort", "Linear search", "Brute-force repetition"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Mid-Term Assessment - Algorithms & Discrete Math for Developers')
 INSERT dbo.Quizzes(CourseId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,N'Mid-Term Assessment - Algorithms & Discrete Math for Developers',70,@TutorId,N'Mid-term: modules 1-2.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Mid-Term Assessment - Algorithms & Discrete Math for Developers' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseAssessments WHERE CourseId=@CourseId AND AssessmentType=N'Midterm')
 INSERT dbo.CourseAssessments(CourseId,QuizId,Title,AssessmentType,AfterModuleOrder,TimeLimitMinutes,MaxAttempts,WeightPercent) VALUES(@CourseId,@QuizId,N'Mid-Term Assessment',N'Midterm',2,30,2,40);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is the typical time complexity of binary search?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is the typical time complexity of binary search?',N'Multiple Choice',N'["O(n)", "O(log n)", "O(n\u00b2)", "O(2\u207f)"]',1,N'This question checks understanding of a core concept from the covered modules.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is required for binary search?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is required for binary search?',N'Multiple Choice',N'["Sorted input", "Random input", "A queue", "A graph"]',0,N'This question checks understanding of a core concept from the covered modules.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What data structure is used by breadth-first search?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What data structure is used by breadth-first search?',N'Multiple Choice',N'["Stack", "Queue", "Heap", "Tree only"]',1,N'This question checks understanding of a core concept from the covered modules.',3);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which is essential for a recursive function to stop?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which is essential for a recursive function to stop?',N'Multiple Choice',N'["Base case", "Infinite loop", "Global variable", "Hash map"]',0,N'This question checks understanding of a core concept from the covered modules.',4);
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Final Assessment - Algorithms & Discrete Math for Developers')
 INSERT dbo.Quizzes(CourseId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,N'Final Assessment - Algorithms & Discrete Math for Developers',70,@TutorId,N'Cumulative: all modules.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Final Assessment - Algorithms & Discrete Math for Developers' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseAssessments WHERE CourseId=@CourseId AND AssessmentType=N'Final')
 INSERT dbo.CourseAssessments(CourseId,QuizId,Title,AssessmentType,AfterModuleOrder,TimeLimitMinutes,MaxAttempts,WeightPercent) VALUES(@CourseId,@QuizId,N'Final Assessment',N'Final',3,60,2,60);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is the typical time complexity of binary search?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is the typical time complexity of binary search?',N'Multiple Choice',N'["O(n)", "O(log n)", "O(n\u00b2)", "O(2\u207f)"]',1,N'This question checks understanding of a core concept from the covered modules.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is required for binary search?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is required for binary search?',N'Multiple Choice',N'["Sorted input", "Random input", "A queue", "A graph"]',0,N'This question checks understanding of a core concept from the covered modules.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What data structure is used by breadth-first search?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What data structure is used by breadth-first search?',N'Multiple Choice',N'["Stack", "Queue", "Heap", "Tree only"]',1,N'This question checks understanding of a core concept from the covered modules.',3);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which is essential for a recursive function to stop?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which is essential for a recursive function to stop?',N'Multiple Choice',N'["Base case", "Infinite loop", "Global variable", "Hash map"]',0,N'This question checks understanding of a core concept from the covered modules.',4);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is the purpose of dynamic programming?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is the purpose of dynamic programming?',N'Multiple Choice',N'["Reuse solutions to subproblems", "Sort every input", "Avoid all loops", "Remove memory usage"]',0,N'This question checks understanding of a core concept from the covered modules.',5);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which operation returns shared elements of two sets?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which operation returns shared elements of two sets?',N'Multiple Choice',N'["Union", "Intersection", "Difference", "Complement"]',1,N'This question checks understanding of a core concept from the covered modules.',6);
-- Python Essentials: From Zero to Automated Scripts
SET @CourseId=(SELECT TOP(1) Id FROM dbo.Courses WHERE Title=N'Python Essentials: From Zero to Automated Scripts' ORDER BY Id);
IF @CourseId IS NULL THROW 51011, 'Required course missing.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=1)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Python Fundamentals',N'Two guided lessons and a knowledge check.',1);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=1);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (1,2) AND l.ModuleId IS NULL;
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Python Essentials Quiz' ORDER BY Id);
IF @QuizId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE ModuleId=@ModuleId) UPDATE dbo.Quizzes SET ModuleId=@ModuleId WHERE Id=@QuizId;
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=2)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Control Flow and Collections',N'Two guided lessons and a knowledge check.',2);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=2);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (3,4) AND l.ModuleId IS NULL;
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 2 Quiz: Control Flow and Collections')
 INSERT dbo.Quizzes(CourseId,ModuleId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,@ModuleId,N'Module 2 Quiz: Control Flow and Collections',70,@TutorId,N'Answer the module questions.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 2 Quiz: Control Flow and Collections' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What is the result of len([2, 4, 6])?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What is the result of len([2, 4, 6])?',N'Multiple Choice',N'["2", "3", "4", "6"]',1,N'Review the corresponding module lesson for the reasoning behind this answer.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does a Python dictionary store?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does a Python dictionary store?',N'Multiple Choice',N'["Key-value pairs", "Only floating numbers", "Only file names", "Only tuples"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=3)
 INSERT dbo.CourseModules(CourseId,Title,Description,OrderIndex) VALUES(@CourseId,N'Functions and Automation',N'Two guided lessons and a knowledge check.',3);
SET @ModuleId=(SELECT Id FROM dbo.CourseModules WHERE CourseId=@CourseId AND OrderIndex=3);
;WITH OrderedLessons AS (SELECT Id,ROW_NUMBER() OVER(ORDER BY OrderIndex,Id) AS rn FROM dbo.Lessons WHERE CourseId=@CourseId)
UPDATE l SET ModuleId=@ModuleId FROM dbo.Lessons l INNER JOIN OrderedLessons o ON o.Id=l.Id WHERE o.rn IN (5,6) AND l.ModuleId IS NULL;
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 3 Quiz: Functions and Automation')
 INSERT dbo.Quizzes(CourseId,ModuleId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,@ModuleId,N'Module 3 Quiz: Functions and Automation',70,@TutorId,N'Answer the module questions.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Module 3 Quiz: Functions and Automation' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which syntax defines a function?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which syntax defines a function?',N'Multiple Choice',N'["def greet():", "function greet()", "func greet()", "fn greet()"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does a context manager help with when opening files?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does a context manager help with when opening files?',N'Multiple Choice',N'["Closing resources reliably", "Making files public", "Skipping errors", "Encrypting every file"]',0,N'Review the corresponding module lesson for the reasoning behind this answer.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Mid-Term Assessment - Python Essentials: From Zero to Automated Scripts')
 INSERT dbo.Quizzes(CourseId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,N'Mid-Term Assessment - Python Essentials: From Zero to Automated Scripts',70,@TutorId,N'Mid-term: modules 1-2.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Mid-Term Assessment - Python Essentials: From Zero to Automated Scripts' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseAssessments WHERE CourseId=@CourseId AND AssessmentType=N'Midterm')
 INSERT dbo.CourseAssessments(CourseId,QuizId,Title,AssessmentType,AfterModuleOrder,TimeLimitMinutes,MaxAttempts,WeightPercent) VALUES(@CourseId,@QuizId,N'Mid-Term Assessment',N'Midterm',2,30,2,40);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which function displays output in Python?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which function displays output in Python?',N'Multiple Choice',N'["input", "print", "read", "show"]',1,N'This question checks understanding of a core concept from the covered modules.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which type stores an ordered sequence of items?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which type stores an ordered sequence of items?',N'Multiple Choice',N'["dict", "set", "list", "bool"]',2,N'This question checks understanding of a core concept from the covered modules.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which keyword defines a Python function?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which keyword defines a Python function?',N'Multiple Choice',N'["func", "def", "lambda only", "method"]',1,N'This question checks understanding of a core concept from the covered modules.',3);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does a for loop commonly do?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does a for loop commonly do?',N'Multiple Choice',N'["Iterate over items", "Declare a class", "Open a socket", "Catch errors"]',0,N'This question checks understanding of a core concept from the covered modules.',4);
IF NOT EXISTS(SELECT 1 FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Final Assessment - Python Essentials: From Zero to Automated Scripts')
 INSERT dbo.Quizzes(CourseId,Title,PassMark,CreatedByUserId,Instructions) VALUES(@CourseId,N'Final Assessment - Python Essentials: From Zero to Automated Scripts',70,@TutorId,N'Cumulative: all modules.');
SET @QuizId=(SELECT TOP(1) Id FROM dbo.Quizzes WHERE CourseId=@CourseId AND Title=N'Final Assessment - Python Essentials: From Zero to Automated Scripts' ORDER BY Id);
IF NOT EXISTS(SELECT 1 FROM dbo.CourseAssessments WHERE CourseId=@CourseId AND AssessmentType=N'Final')
 INSERT dbo.CourseAssessments(CourseId,QuizId,Title,AssessmentType,AfterModuleOrder,TimeLimitMinutes,MaxAttempts,WeightPercent) VALUES(@CourseId,@QuizId,N'Final Assessment',N'Final',3,60,2,60);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which function displays output in Python?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which function displays output in Python?',N'Multiple Choice',N'["input", "print", "read", "show"]',1,N'This question checks understanding of a core concept from the covered modules.',1);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which type stores an ordered sequence of items?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which type stores an ordered sequence of items?',N'Multiple Choice',N'["dict", "set", "list", "bool"]',2,N'This question checks understanding of a core concept from the covered modules.',2);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which keyword defines a Python function?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which keyword defines a Python function?',N'Multiple Choice',N'["func", "def", "lambda only", "method"]',1,N'This question checks understanding of a core concept from the covered modules.',3);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'What does a for loop commonly do?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'What does a for loop commonly do?',N'Multiple Choice',N'["Iterate over items", "Declare a class", "Open a socket", "Catch errors"]',0,N'This question checks understanding of a core concept from the covered modules.',4);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which statement handles an expected error?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which statement handles an expected error?',N'Multiple Choice',N'["try/except", "if/else", "for/in", "def/return"]',0,N'This question checks understanding of a core concept from the covered modules.',5);
IF NOT EXISTS(SELECT 1 FROM dbo.QuizQuestions WHERE QuizId=@QuizId AND QuestionText=N'Which module provides convenient filesystem paths?')
 INSERT dbo.QuizQuestions(QuizId,QuestionText,QuestionType,OptionsJson,CorrectOptionIndex,Explanation,OrderIndex)
 VALUES(@QuizId,N'Which module provides convenient filesystem paths?',N'Multiple Choice',N'["math", "random", "pathlib", "statistics"]',2,N'This question checks understanding of a core concept from the covered modules.',6);
COMMIT TRANSACTION;
PRINT N'Module, quiz, and assessment data inserted.';
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;
SELECT c.Title, COUNT(DISTINCT m.Id) Modules, COUNT(DISTINCT a.Id) Assessments FROM dbo.Courses c LEFT JOIN dbo.CourseModules m ON m.CourseId=c.Id LEFT JOIN dbo.CourseAssessments a ON a.CourseId=c.Id GROUP BY c.Title;
GO
