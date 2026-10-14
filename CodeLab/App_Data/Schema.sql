-- CodeLab database schema.
-- Safe to re-run: each table is only created if it does not exist yet.
-- Add the tables for your own module in a new block at the bottom of this file.

-- =====================================
-- Shared: Users
-- =====================================
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username     NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        PasswordHash VARCHAR(200)      NOT NULL,
        FullName     NVARCHAR(100)     NOT NULL,
        Email        NVARCHAR(255)     NULL,
        Role         VARCHAR(20)       NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Tutor', 'Learner')),
        IsActive     BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt    DATETIME2         NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

-- =====================================
-- Shared: AuditLogs
-- =====================================
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        LogID     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        UserID    INT               NOT NULL CONSTRAINT FK_AuditLogs_Users REFERENCES dbo.Users (UserID),
        Action    VARCHAR(50)       NOT NULL,
        Details   NVARCHAR(500)     NULL,
        Timestamp DATETIME2         NOT NULL CONSTRAINT DF_AuditLogs_Timestamp DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_AuditLogs_Timestamp ON dbo.AuditLogs (Timestamp DESC);
END
GO
