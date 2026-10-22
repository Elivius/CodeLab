-- CodeLab database schema (SQL Server LocalDB).
-- The design and diagram are explained in DATABASE_SCHEMA.md at the repository root.
--
-- Safe to re-run: each table is only created if it does not exist yet.
-- That also means editing a table here does NOT change an existing database:
-- after changing this file, run App_Data\SetupDatabase.cmd again to rebuild.
--
-- Conventions: primary key "Id", foreign keys "<Entity>Id", every constraint named,
-- and every timestamp stored in UTC (SYSUTCDATETIME), matching DateTime.UtcNow in C#.

-- =====================================
-- 1. Users & authentication
-- =====================================
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FullName     NVARCHAR(120)     NOT NULL,
        Username     NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        Email        NVARCHAR(255)     NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
        PasswordHash NVARCHAR(255)     NOT NULL,
        Role         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Users_Role DEFAULT (N'Learner')
                                                CONSTRAINT CK_Users_Role CHECK (Role IN (N'Learner', N'Tutor', N'Admin')),
        -- Whether the account may log in at all. Tutor approval is TutorProfiles.Status.
        Status       NVARCHAR(20)      NOT NULL CONSTRAINT DF_Users_Status DEFAULT (N'Active')
                                                CONSTRAINT CK_Users_Status CHECK (Status IN (N'Active', N'Suspended', N'Pending')),
        AvatarUrl    NVARCHAR(500)     NULL,
        JoinedDate   DATETIME2         NOT NULL CONSTRAINT DF_Users_JoinedDate DEFAULT (SYSUTCDATETIME()),
        LastLogin    DATETIME2         NULL
    );
END
GO

-- =====================================
-- 2. Tutor profiles
-- =====================================
IF OBJECT_ID('dbo.TutorProfiles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TutorProfiles
    (
        UserId         INT           NOT NULL CONSTRAINT PK_TutorProfiles PRIMARY KEY
                                              CONSTRAINT FK_TutorProfiles_Users REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        Qualifications NVARCHAR(MAX) NOT NULL,
        PortfolioUrl   NVARCHAR(500) NOT NULL,
        -- Sample curriculum pitch from the tutor application (required for new applications, checked in TutorManager)
        CurriculumPitch NVARCHAR(MAX) NULL,
        Bio            NVARCHAR(MAX) NOT NULL,
        Specialty      NVARCHAR(100) NOT NULL,
        -- New tutors wait for an Admin to approve them
        Status         NVARCHAR(20)  NOT NULL CONSTRAINT DF_TutorProfiles_Status DEFAULT (N'Pending')
                                              CONSTRAINT CK_TutorProfiles_Status CHECK (Status IN (N'Approved', N'Pending', N'Rejected')),
        ApprovedDate   DATETIME2     NULL
    );
END
GO

-- =====================================
-- 3. Courses
-- =====================================
IF OBJECT_ID('dbo.Courses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Courses
    (
        Id                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Courses PRIMARY KEY,
        Title                NVARCHAR(200)     NOT NULL,
        Description          NVARCHAR(MAX)     NOT NULL,
        Category             NVARCHAR(80)      NOT NULL,
        Difficulty           NVARCHAR(20)      NOT NULL CONSTRAINT DF_Courses_Difficulty DEFAULT (N'Beginner')
                                                        CONSTRAINT CK_Courses_Difficulty CHECK (Difficulty IN (N'Beginner', N'Intermediate', N'Advanced')),
        InstructorId         INT               NOT NULL CONSTRAINT FK_Courses_Users REFERENCES dbo.Users (Id),
        -- New courses start as drafts and go through admin review before publication
        Status               NVARCHAR(20)      NOT NULL CONSTRAINT DF_Courses_Status DEFAULT (N'Draft')
                                                        CONSTRAINT CK_Courses_Status CHECK (Status IN (N'Draft', N'Pending Review', N'Published', N'Locked')),
        ThumbnailUrl         NVARCHAR(500)     NULL,
        LockReason           NVARCHAR(500)     NULL,
        RejectionReason      NVARCHAR(500)     NULL,
        MaterialsDescription NVARCHAR(MAX)     NULL,
        CreatedAt            DATETIME2         NOT NULL CONSTRAINT DF_Courses_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt            DATETIME2         NOT NULL CONSTRAINT DF_Courses_UpdatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 4. Lessons
-- =====================================
IF OBJECT_ID('dbo.Lessons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Lessons
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Lessons PRIMARY KEY,
        CourseId        INT               NOT NULL CONSTRAINT FK_Lessons_Courses REFERENCES dbo.Courses (Id) ON DELETE CASCADE,
        Title           NVARCHAR(200)     NOT NULL,
        Content         NVARCHAR(MAX)     NOT NULL,
        CodeSnippet     NVARCHAR(MAX)     NULL,
        CodeLanguage    NVARCHAR(50)      NULL CONSTRAINT DF_Lessons_CodeLanguage DEFAULT (N'javascript'),
        ThumbnailUrl    NVARCHAR(500)     NULL,
        VideoUrl        NVARCHAR(500)     NULL,
        OrderIndex      INT               NOT NULL CONSTRAINT DF_Lessons_OrderIndex DEFAULT (1),
        DurationMinutes INT               NOT NULL CONSTRAINT DF_Lessons_DurationMinutes DEFAULT (15),
        CreatedAt       DATETIME2         NOT NULL CONSTRAINT DF_Lessons_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 5. Course enrollments
-- =====================================
IF OBJECT_ID('dbo.CourseEnrollments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourseEnrollments
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseEnrollments PRIMARY KEY,
        LearnerId       INT               NOT NULL CONSTRAINT FK_CourseEnrollments_Users REFERENCES dbo.Users (Id),
        CourseId        INT               NOT NULL CONSTRAINT FK_CourseEnrollments_Courses REFERENCES dbo.Courses (Id) ON DELETE CASCADE,
        EnrolledAt      DATETIME2         NOT NULL CONSTRAINT DF_CourseEnrollments_EnrolledAt DEFAULT (SYSUTCDATETIME()),
        ProgressPercent DECIMAL(5,2)      NOT NULL CONSTRAINT DF_CourseEnrollments_ProgressPercent DEFAULT (0.00),
        IsCompleted     BIT               NOT NULL CONSTRAINT DF_CourseEnrollments_IsCompleted DEFAULT (0),
        CompletedAt     DATETIME2         NULL,
        CONSTRAINT UQ_CourseEnrollments_LearnerCourse UNIQUE (LearnerId, CourseId)
    );
END
GO

-- =====================================
-- 6. Lesson completions
-- =====================================
IF OBJECT_ID('dbo.LessonCompletions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LessonCompletions
    (
        LearnerId   INT       NOT NULL CONSTRAINT FK_LessonCompletions_Users REFERENCES dbo.Users (Id),
        LessonId    INT       NOT NULL CONSTRAINT FK_LessonCompletions_Lessons REFERENCES dbo.Lessons (Id) ON DELETE CASCADE,
        CompletedAt DATETIME2 NOT NULL CONSTRAINT DF_LessonCompletions_CompletedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_LessonCompletions PRIMARY KEY (LearnerId, LessonId)
    );
END
GO

-- =====================================
-- 7. Quizzes
-- =====================================
IF OBJECT_ID('dbo.Quizzes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Quizzes
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Quizzes PRIMARY KEY,
        CourseId        INT               NULL CONSTRAINT FK_Quizzes_Courses REFERENCES dbo.Courses (Id) ON DELETE SET NULL,
        Title           NVARCHAR(150)     NOT NULL,
        PassMark        INT               NOT NULL CONSTRAINT DF_Quizzes_PassMark DEFAULT (80)
                                                   CONSTRAINT CK_Quizzes_PassMark CHECK (PassMark BETWEEN 0 AND 100),
        CreatedByUserId INT               NOT NULL CONSTRAINT FK_Quizzes_Users REFERENCES dbo.Users (Id),
        Instructions    NVARCHAR(MAX)     NULL,
        CreatedAt       DATETIME2         NOT NULL CONSTRAINT DF_Quizzes_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 8. Quiz questions
-- =====================================
IF OBJECT_ID('dbo.QuizQuestions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuizQuestions
    (
        Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuizQuestions PRIMARY KEY,
        QuizId             INT               NOT NULL CONSTRAINT FK_QuizQuestions_Quizzes REFERENCES dbo.Quizzes (Id) ON DELETE CASCADE,
        QuestionText       NVARCHAR(MAX)     NOT NULL,
        QuestionType       NVARCHAR(30)      NOT NULL CONSTRAINT DF_QuizQuestions_QuestionType DEFAULT (N'Multiple Choice')
                                                      CONSTRAINT CK_QuizQuestions_QuestionType CHECK (QuestionType IN (N'Multiple Choice', N'True-False')),
        OptionsJson        NVARCHAR(MAX)     NOT NULL, -- JSON array, e.g. ["Option A", "Option B", "Option C"]
        CorrectOptionIndex INT               NOT NULL CONSTRAINT DF_QuizQuestions_CorrectOptionIndex DEFAULT (0),
        Explanation        NVARCHAR(MAX)     NOT NULL,
        OrderIndex         INT               NOT NULL CONSTRAINT DF_QuizQuestions_OrderIndex DEFAULT (1)
    );
END
GO

-- =====================================
-- 9. Quiz attempts
-- =====================================
IF OBJECT_ID('dbo.QuizAttempts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuizAttempts
    (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuizAttempts PRIMARY KEY,
        LearnerId      INT               NOT NULL CONSTRAINT FK_QuizAttempts_Users REFERENCES dbo.Users (Id),
        QuizId         INT               NOT NULL CONSTRAINT FK_QuizAttempts_Quizzes REFERENCES dbo.Quizzes (Id) ON DELETE CASCADE,
        ScorePercent   INT               NOT NULL,
        Passed         BIT               NOT NULL,
        TotalQuestions INT               NOT NULL,
        CorrectCount   INT               NOT NULL,
        AttemptedAt    DATETIME2         NOT NULL CONSTRAINT DF_QuizAttempts_AttemptedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 10. Coding challenges (Code Lab)
-- =====================================
IF OBJECT_ID('dbo.CodingChallenges', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CodingChallenges
    (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CodingChallenges PRIMARY KEY,
        CourseId       INT               NOT NULL CONSTRAINT FK_CodingChallenges_Courses REFERENCES dbo.Courses (Id) ON DELETE CASCADE,
        Title          NVARCHAR(200)     NOT NULL,
        Description    NVARCHAR(MAX)     NOT NULL,
        Difficulty     NVARCHAR(20)      NOT NULL CONSTRAINT DF_CodingChallenges_Difficulty DEFAULT (N'Beginner')
                                                  CONSTRAINT CK_CodingChallenges_Difficulty CHECK (Difficulty IN (N'Beginner', N'Intermediate', N'Advanced')),
        Language       NVARCHAR(50)      NOT NULL CONSTRAINT DF_CodingChallenges_Language DEFAULT (N'javascript'),
        StarterCode    NVARCHAR(MAX)     NOT NULL,
        ExpectedOutput NVARCHAR(MAX)     NOT NULL,
        PointsReward   INT               NOT NULL CONSTRAINT DF_CodingChallenges_PointsReward DEFAULT (50),
        CreatedAt      DATETIME2         NOT NULL CONSTRAINT DF_CodingChallenges_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 11. Code submissions
-- =====================================
IF OBJECT_ID('dbo.CodeSubmissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CodeSubmissions
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CodeSubmissions PRIMARY KEY,
        LearnerId     INT               NOT NULL CONSTRAINT FK_CodeSubmissions_Users REFERENCES dbo.Users (Id),
        ChallengeId   INT               NOT NULL CONSTRAINT FK_CodeSubmissions_CodingChallenges REFERENCES dbo.CodingChallenges (Id) ON DELETE CASCADE,
        SubmittedCode NVARCHAR(MAX)     NOT NULL,
        Status        NVARCHAR(30)      NOT NULL CONSTRAINT CK_CodeSubmissions_Status CHECK (Status IN (N'Passed', N'Failed', N'Error')),
        OutputResult  NVARCHAR(MAX)     NULL,
        SubmittedAt   DATETIME2         NOT NULL CONSTRAINT DF_CodeSubmissions_SubmittedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 12. Learner stats (gamification)
-- =====================================
IF OBJECT_ID('dbo.LearnerStats', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LearnerStats
    (
        LearnerId           INT          NOT NULL CONSTRAINT PK_LearnerStats PRIMARY KEY
                                                  CONSTRAINT FK_LearnerStats_Users REFERENCES dbo.Users (Id) ON DELETE CASCADE,
        AchievementPoints   INT          NOT NULL CONSTRAINT DF_LearnerStats_AchievementPoints DEFAULT (0),
        CurrentLevel        INT          NOT NULL CONSTRAINT DF_LearnerStats_CurrentLevel DEFAULT (1),
        LevelTitle          NVARCHAR(50) NOT NULL CONSTRAINT DF_LearnerStats_LevelTitle DEFAULT (N'Novice Coder'),
        NextMilestonePoints INT          NOT NULL CONSTRAINT DF_LearnerStats_NextMilestonePoints DEFAULT (500),
        StreakDays          INT          NOT NULL CONSTRAINT DF_LearnerStats_StreakDays DEFAULT (1),
        LastActiveDate      DATE         NOT NULL CONSTRAINT DF_LearnerStats_LastActiveDate DEFAULT (CAST(SYSUTCDATETIME() AS DATE))
    );
END
GO

-- =====================================
-- 13. Discussion posts
-- =====================================
IF OBJECT_ID('dbo.DiscussionPosts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DiscussionPosts
    (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DiscussionPosts PRIMARY KEY,
        CourseId  INT               NOT NULL CONSTRAINT FK_DiscussionPosts_Courses REFERENCES dbo.Courses (Id) ON DELETE CASCADE,
        AuthorId  INT               NOT NULL CONSTRAINT FK_DiscussionPosts_Users REFERENCES dbo.Users (Id),
        Title     NVARCHAR(250)     NOT NULL,
        Content   NVARCHAR(MAX)     NOT NULL,
        CreatedAt DATETIME2         NOT NULL CONSTRAINT DF_DiscussionPosts_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 14. Discussion replies
-- =====================================
IF OBJECT_ID('dbo.DiscussionReplies', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DiscussionReplies
    (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DiscussionReplies PRIMARY KEY,
        PostId    INT               NOT NULL CONSTRAINT FK_DiscussionReplies_DiscussionPosts REFERENCES dbo.DiscussionPosts (Id) ON DELETE CASCADE,
        AuthorId  INT               NOT NULL CONSTRAINT FK_DiscussionReplies_Users REFERENCES dbo.Users (Id),
        Content   NVARCHAR(MAX)     NOT NULL,
        CreatedAt DATETIME2         NOT NULL CONSTRAINT DF_DiscussionReplies_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- 15. Audit logs (business activity shown to Admins)
-- =====================================
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        UserId    INT               NOT NULL CONSTRAINT FK_AuditLogs_Users REFERENCES dbo.Users (Id),
        Action    VARCHAR(50)       NOT NULL, -- e.g. 'CREATE_QUIZ', 'LOGIN', 'LOCK_COURSE'
        Details   NVARCHAR(500)     NULL,
        Timestamp DATETIME2         NOT NULL CONSTRAINT DF_AuditLogs_Timestamp DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs (Timestamp DESC);
END
GO
