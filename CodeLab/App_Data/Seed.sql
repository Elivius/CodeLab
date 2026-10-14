-- Development seed data. Safe to re-run: rows are only inserted if missing.
--
-- Test accounts (one per role):
--   admin   / Admin@123
--   tutor   / Tutor@123
--   learner / Learner@123
--
-- Password hashes are produced by PasswordHelper.HashPassword.

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, Role)
    VALUES (N'admin', 'PBKDF2$100000$OBc+sqcmvyOf3s03wTz3zg==$za1bHIPChNws30L5gLcbVmZJUR+yTbACUOrJeiHLN8A=',
            N'System Administrator', N'admin@codelab.local', 'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'tutor')
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, Role)
    VALUES (N'tutor', 'PBKDF2$100000$FEu2YWRbd+EGiSvsO+FwuA==$P6uWyc8nseayvQ65rCeGe/YVYk18mtndKpFBZqK8K0k=',
            N'Demo Tutor', N'tutor@codelab.local', 'Tutor');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'learner')
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, Role)
    VALUES (N'learner', 'PBKDF2$100000$37zQ2kfdZ148fOYGIfD+iA==$w7QKt8ThVv0X/Ex8yqMapnaL9rHduxn/AtQG1106MTg=',
            N'Demo Learner', N'learner@codelab.local', 'Learner');
GO
