-- CodeLab curriculum upgrade: non-destructive and rerunnable on SQL Server LocalDB
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.CourseModules', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.CourseModules (
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseModules PRIMARY KEY,
  CourseId INT NOT NULL CONSTRAINT FK_CourseModules_Courses REFERENCES dbo.Courses(Id) ON DELETE CASCADE,
  Title NVARCHAR(200) NOT NULL,
  Description NVARCHAR(MAX) NULL,
  OrderIndex INT NOT NULL,
  CONSTRAINT UQ_CourseModules_CourseOrder UNIQUE(CourseId, OrderIndex)
 );
END;

IF COL_LENGTH(N'dbo.Lessons', N'ModuleId') IS NULL
 ALTER TABLE dbo.Lessons ADD ModuleId INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Lessons_CourseModules')
 ALTER TABLE dbo.Lessons ADD CONSTRAINT FK_Lessons_CourseModules FOREIGN KEY(ModuleId) REFERENCES dbo.CourseModules(Id);

IF COL_LENGTH(N'dbo.Quizzes', N'ModuleId') IS NULL
 ALTER TABLE dbo.Quizzes ADD ModuleId INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Quizzes_CourseModules')
 ALTER TABLE dbo.Quizzes ADD CONSTRAINT FK_Quizzes_CourseModules FOREIGN KEY(ModuleId) REFERENCES dbo.CourseModules(Id);

IF OBJECT_ID(N'dbo.CourseAssessments', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.CourseAssessments (
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourseAssessments PRIMARY KEY,
  CourseId INT NOT NULL CONSTRAINT FK_CourseAssessments_Courses REFERENCES dbo.Courses(Id) ON DELETE CASCADE,
  QuizId INT NOT NULL CONSTRAINT FK_CourseAssessments_Quizzes REFERENCES dbo.Quizzes(Id),
  Title NVARCHAR(200) NOT NULL,
  AssessmentType NVARCHAR(20) NOT NULL CONSTRAINT CK_CourseAssessments_Type CHECK(AssessmentType IN(N'Midterm',N'Final')),
  AfterModuleOrder INT NOT NULL,
  TimeLimitMinutes INT NOT NULL CONSTRAINT DF_CourseAssessments_TimeLimit DEFAULT(45),
  MaxAttempts INT NOT NULL CONSTRAINT DF_CourseAssessments_MaxAttempts DEFAULT(2),
  WeightPercent INT NOT NULL CONSTRAINT DF_CourseAssessments_Weight DEFAULT(0),
  CONSTRAINT CK_CourseAssessments_Time CHECK(TimeLimitMinutes > 0),
  CONSTRAINT CK_CourseAssessments_Attempts CHECK(MaxAttempts > 0),
  CONSTRAINT CK_CourseAssessments_Weight CHECK(WeightPercent BETWEEN 0 AND 100),
  CONSTRAINT UQ_CourseAssessments_CourseType UNIQUE(CourseId, AssessmentType),
  CONSTRAINT UQ_CourseAssessments_Quiz UNIQUE(QuizId)
 );
END;

-- Store the learner's per-question answers for assessments (QuizAttempts holds score).
IF OBJECT_ID(N'dbo.AssessmentResponses', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.AssessmentResponses (
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AssessmentResponses PRIMARY KEY,
  AssessmentId INT NOT NULL CONSTRAINT FK_AssessmentResponses_Assessments REFERENCES dbo.CourseAssessments(Id) ON DELETE CASCADE,
  QuizAttemptId INT NOT NULL CONSTRAINT FK_AssessmentResponses_QuizAttempts REFERENCES dbo.QuizAttempts(Id) ON DELETE CASCADE,
  QuestionId INT NOT NULL CONSTRAINT FK_AssessmentResponses_Questions REFERENCES dbo.QuizQuestions(Id),
  SelectedOptionIndex INT NOT NULL,
  CONSTRAINT UQ_AssessmentResponses_AttemptQuestion UNIQUE(QuizAttemptId,QuestionId)
 );
END;

COMMIT TRANSACTION;
PRINT N'Curriculum schema migration completed.';
END TRY
BEGIN CATCH
 IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
GO
