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
-- Badge catalog only. Awards are recorded on first eligibility check, in UTC.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @Catalog TABLE(Code varchar(50),Name nvarchar(100),Description nvarchar(300),Icon varchar(30),Category varchar(20),Metric varchar(20),Target int,SortOrder int);
INSERT @Catalog VALUES
 ('first-steps',N'First Steps',N'Complete your first lesson.','book','curriculum','lessons',1,1),
 ('dedicated-learner',N'Dedicated Learner',N'Complete 15 lessons.','book','curriculum','lessons',15,2),
 ('quiz-explorer',N'Quiz Explorer',N'Pass your first quiz.','award','quiz','quizzes',1,3),
 ('quiz-scholar',N'Quiz Scholar',N'Pass 10 different quizzes.','award','quiz','quizzes',10,4),
 ('problem-solver',N'Problem Solver',N'Pass your first coding challenge.','terminal','coding','challenges',1,5),
 ('code-explorer',N'Code Explorer',N'Pass 10 different coding challenges.','terminal','coding','challenges',10,6),
 ('rising-star',N'Rising Star',N'Reach 500 XP.','sparkles','xp','points',500,7),
 ('xp-champion',N'XP Champion',N'Reach 3,000 XP.','trophy','xp','points',3000,8),
 ('curriculum-expert',N'Curriculum Expert',N'Complete 25 lessons.','book','curriculum','lessons',25,9),
 ('curriculum-master',N'Curriculum Master',N'Complete 50 lessons.','trophy','curriculum','lessons',50,10),
 ('quiz-expert',N'Quiz Expert',N'Pass 20 different quizzes.','award','quiz','quizzes',20,11),
 ('quiz-master',N'Quiz Master',N'Pass 30 different quizzes.','trophy','quiz','quizzes',30,12),
 ('code-specialist',N'Code Specialist',N'Pass 25 different coding challenges.','terminal','coding','challenges',25,13),
 ('code-master',N'Code Master',N'Pass 50 different coding challenges.','trophy','coding','challenges',50,14),
 ('xp-elite',N'XP Elite',N'Reach 5,000 XP.','zap','xp','points',5000,15),
 ('xp-legend',N'XP Legend',N'Reach 10,000 XP.','flame','xp','points',10000,16),
 ('lesson-veteran',N'Lesson Veteran',N'Complete 75 lessons.','book','curriculum','lessons',75,17),
 ('knowledge-keeper',N'Knowledge Keeper',N'Complete 100 lessons.','award','curriculum','lessons',100,18),
 ('learning-legend',N'Learning Legend',N'Complete 150 lessons.','trophy','curriculum','lessons',150,19),
 ('quiz-veteran',N'Quiz Veteran',N'Pass 40 different quizzes.','award','quiz','quizzes',40,20),
 ('quiz-champion',N'Quiz Champion',N'Pass 50 different quizzes.','zap','quiz','quizzes',50,21),
 ('quiz-legend',N'Quiz Legend',N'Pass 75 different quizzes.','trophy','quiz','quizzes',75,22),
 ('code-veteran',N'Code Veteran',N'Pass 75 different coding challenges.','terminal','coding','challenges',75,23),
 ('code-champion',N'Code Champion',N'Pass 100 different coding challenges.','zap','coding','challenges',100,24),
 ('code-legend',N'Code Legend',N'Pass 150 different coding challenges.','trophy','coding','challenges',150,25),
 ('xp-vanguard',N'XP Vanguard',N'Reach 15,000 XP.','sparkles','xp','points',15000,26),
 ('xp-titan',N'XP Titan',N'Reach 25,000 XP.','flame','xp','points',25000,27),
 ('xp-immortal',N'XP Immortal',N'Reach 50,000 XP.','trophy','xp','points',50000,28)
;
-- Existing awards and their original dates are retained when requirements change.
UPDATE b SET Name=v.Name,Description=v.Description,Icon=v.Icon,Category=v.Category,Metric=v.Metric,Target=v.Target,SortOrder=v.SortOrder FROM dbo.Badges b JOIN @Catalog v ON b.Code=v.Code;
INSERT dbo.Badges(Code,Name,Description,Icon,Category,Metric,Target,SortOrder)
SELECT v.* FROM @Catalog v WHERE NOT EXISTS(SELECT 1 FROM dbo.Badges b WHERE b.Code=v.Code);
COMMIT;
GO
