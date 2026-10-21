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
    INSERT INTO dbo.TutorProfiles (UserId, Qualifications, PortfolioUrl, Bio, Specialty, Status, ApprovedDate)
    VALUES (@TutorId, N'Ph.D. in Computer Science', N'https://portfolio.codelab.local/turing',
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
