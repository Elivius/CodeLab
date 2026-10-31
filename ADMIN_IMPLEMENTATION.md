# Admin Module: Implementation Plan

This plan builds the Admin module from the prototype screens (Overview, Users, Course Moderation, Tutor Applications, Audit Logs, Database Backups).

It is split into **phases**. Each phase is one branch and one pull request that builds, works on localhost and can be merged to `main` on its own. Review and merge a phase before the next one starts.

The first two phases come first because the rest of the team is waiting on them:

- A new tutor stays **Pending** until an admin approves them, and Pending tutors cannot create courses (see `TUTOR_DECISIONS.md` 2.1).
- A submitted course stays **Pending Review** until an admin approves it, and learners only see Published courses.

Today nobody can do either, so only the seeded tutor and courses work.

| Phase | Branch | What it delivers | Status |
|---|---|---|---|
| 0 | `fix/session-authorization-redirection` | Signed-in users are sent from guest pages to their home page | Done |
| 1 | `feat/admin-module` | Admin pages layout + approve or reject tutor applications | Done |
| 2 | `feat/course-moderation` | Review, approve, reject, lock, unlock and delete courses | Done |
| 3 | `feat/course-moderation` | List, search, create, edit, suspend and delete users | Built, in review |
| 4 | `feat/admin-4-overview-audit` | Stat cards, Overview tab, searchable audit logs | |
| 5 | `feat/admin-5-backups` | Database backups with download (optional) | |

---

## 1. Changes from the prototype

The prototype was made for a cloud deployment. CodeLab runs on localhost with LocalDB, so some parts would only show invented numbers. Those are removed or replaced with real ones.

| Prototype | CodeLab | Why |
|---|---|---|
| All six tabs on one page | **One page per tab**. A shared `Admin.Master` holds the header, stat cards and tab bar, so it still looks like tabs. | Each phase adds one page, so each pull request stays small. Each tab gets its own URL. |
| "Server Status" card (Online, 99.98%, CPU, memory, asia-southeast1) | **"Needs your review" card**: pending tutor applications and pending courses, each linking to its tab. | There is no cloud server to measure. The review queue is what an admin acts on. |
| "Database Health" card (latency, connections 42/100, disk 3.8 GB / 10 GB) | Real numbers: database file size, time of a test query, number of tables. | LocalDB has no connection pool limit or disk quota to show. |
| "Node Cluster: pg-prod-primary" label | Removed. | There is no cluster. |
| "+14% this month" | Real: new users this month, from `Users.JoinedDate`. | |
| Blue banner "System operating normally..." | Shown only when work is waiting: "2 tutor applications and 1 course are waiting for review." | A banner that never changes gets ignored. |
| Refresh button | Removed. | Every page is built fresh by the server; reloading is the refresh. |
| "Admin View" dropdown, "Dashboard / Database" links | The existing top bar (name, Log out) plus one "Admin Dashboard" link. | One top bar for the whole site. |
| Users: Delete any user | **Delete only users with no activity.** Everyone else gets **Suspend / Reactivate**. | The schema blocks deleting a user who has courses, enrollments, posts, quiz attempts, submissions or audit entries. Logging in and registering both create audit entries, so in practice only an account an admin created by mistake (and nobody has used) can be deleted. |
| Suspend only changes a label | **Suspending logs the person out** on their next request. Changing someone's role does the same, so they log in again with the new role. | Today the login cookie keeps working after a status change. |
| Reject a tutor or course | **A reason is required, and the tutor sees it.** | Courses already have `Courses.RejectionReason`. Tutors need one new column, `TutorProfiles.RejectionReason` (Phase 1). |
| Audit logs: Client IP column | Removed. | On localhost every IP is `::1`. |
| Audit logs: invented events (`FAILED_AUTHENTICATION_BURST`, `PRIVILEGE_ELEVATION_ATTEMPT`) | Only real events. A new real one, `LOGIN_FAILED`, is added in Phase 4. | |
| Audit logs: Category column | Kept, but **worked out from the action name** (for example, every `DELETE_...` is Deletion). | No schema change. |
| Audit logs: "Inspect" link | Removed; the Details column shows the full text. | There is nothing more to show. |
| Backups: encrypted cloud bucket, 30-day retention | A real `.bak` file in `App_Data/Backups`, a SHA-256 checksum, and a Download button. **No restore.** | Restoring would replace the database every teammate's page is using. |

---

## 2. Rules for every phase

### Before starting a phase

```bash
git checkout main
git pull
git checkout -b feat/admin-N-name
```

Always branch from an up-to-date `main`. A branch made from an old `main` will not have the earlier phases.

### Where files go

Admin code gets its own sub-folders, like `Tutor/` and `Discussion/` already do:

```text
CodeLab/
├── Models/Admin/        # Data shapes for admin pages (AdminCourseRow, AdminStats, ...)
├── DAL/Admin/           # All admin SQL
├── BLL/Admin/           # Admin rules: role check, validation, audit entries
├── Modules/Admin/       # Admin pages and Admin.Master
└── Content/Admin.css    # Admin styles (cl-admin-* classes)
```

Add every file through Visual Studio (right-click the folder > Add), so it is listed in `CodeLab.csproj`.

### What every admin feature must do

1. **Check the role in the BLL.** Every public admin BLL method starts with:
   ```csharp
   if (!SessionHelper.IsUserInRole(UserRoles.Admin))
   {
       return OperationResult.Fail("Only administrators can do this.");
   }
   ```
   `Modules/Admin/Web.config` already blocks the folder. This is the second lock.
2. **Write an audit entry** after every change, with the target in the details text, for example `"Approved the tutor application of 'Helena Zhang' (user 7)."`
3. **Guard status changes in the SQL**, for example `UPDATE ... WHERE Id = @Id AND Status = N'Pending'`. If the row was already changed (a double click, or a second admin in another tab), zero rows change and the page says so, instead of applying the change twice.
4. **Use the status constants** (`TutorStatuses`, `CourseStatuses`, `UserStatuses`, `UserRoles`). Never type `"Pending Review"` by hand.
5. **No SQL outside `DAL/`**, and parameters on every query (see `CONTRIBUTING.md`).

### Audit action names

| Action | Phase | Category |
|---|---|---|
| `APPROVE_TUTOR`, `REJECT_TUTOR` | 1 | Administrative |
| `APPROVE_COURSE`, `REJECT_COURSE`, `LOCK_COURSE`, `UNLOCK_COURSE` | 2 | Administrative |
| `DELETE_COURSE` | 2 | Deletion |
| `CREATE_USER`, `UPDATE_USER` | 3 | Administrative |
| `SUSPEND_USER`, `REACTIVATE_USER` | 3 | Security |
| `DELETE_USER` | 3 | Deletion |
| `LOGIN_FAILED` | 4 | Security |
| `CREATE_BACKUP`, `DOWNLOAD_BACKUP` | 5 | Administrative |

### A phase is done when

- [ ] Visual Studio builds it with 0 errors and 0 warnings.
- [ ] Every test in the phase's checklist passes on a freshly rebuilt database.
- [ ] Logged in as `learner` and as `tutor`, the new pages redirect away (blocked).
- [ ] The pull request description lists any change to a teammate's file.

Test accounts from the seed: `admin / Admin@123`, `tutor / Tutor@123`, `learner / Learner@123`.

---

## Phase 0: Redirect fix (done)

**Branch:** `fix/session-authorization-redirection` (already has the changes, not committed yet)

A signed-in user who opens the landing page, Login, Register or the public course pages is sent to their home page. Before this, only tutors were.

| File | Change |
|---|---|
| `Helpers/SessionHelper.cs` | New `GetHomeUrl(role)`: the one place that decides each role's home page |
| `Site.Master.cs` | `Page_Init` redirects every signed-in role, not only Tutors. The guest links are hidden and the logo links to the home page. |
| `Modules/Public/Login.aspx.cs`, `Register.aspx.cs` | Their own "already signed in" checks are removed (`Site.Master` does it first) |

**To finish:** commit, open the pull request, merge.

---

## Phase 1: Admin layout + Tutor applications

**Branch:** `feat/admin-module`
**Goal:** an admin can approve or reject tutor applications, and a rejected tutor sees why.

### Database

Add one column. Put it in the `CREATE TABLE dbo.TutorProfiles` block, and add this at the end of `Schema.sql` so an existing database gets it when the script is run again:

```sql
-- =====================================
-- Admin: reason shown to a tutor whose application was rejected
-- =====================================
IF COL_LENGTH('dbo.TutorProfiles', 'RejectionReason') IS NULL
    ALTER TABLE dbo.TutorProfiles ADD RejectionReason NVARCHAR(500) NULL;
GO
```

Add one Pending applicant to `Seed.sql` so the queue is not empty on a fresh database.

### New files

| File | Contents |
|---|---|
| `Models/Admin/TutorApplication.cs` | `UserId`, `FullName`, `Username`, `Email`, `Specialty`, `Qualifications`, `PortfolioUrl`, `CurriculumPitch`, `Bio`, `Status`, `RejectionReason`, `AppliedAt` |
| `DAL/Admin/TutorApprovalRepository.cs` | `GetApplications(string status)`, `GetApplication(int userId)`, `CountPending()`, `Approve(int userId)`, `Reject(int userId, string reason)` |
| `BLL/Admin/TutorApprovalManager.cs` | Admin check, reason validation (required, at most 500 characters), audit entries |
| `Modules/Admin/Admin.Master` (+ `.cs`) | Nested in `Site.Master`. Page title, tab bar with a pending-count badge, and an `AdminContent` placeholder. Tabs are added as their pages are built. |
| `Modules/Admin/Dashboard.aspx` | The Overview tab. In this phase it only has the "Pending tutor applications" panel (newest 5, each with a Review button). |
| `Modules/Admin/TutorApplications.aspx` | Filter buttons (Pending, Approved, Rejected, All). One row per application: name, status badge, date applied, specialty, first lines of the bio, Review button. |
| `Modules/Admin/TutorApplication.aspx?userId=` | The full application. An **Approve** button, and a **Reject** box where the reason is required. |
| `Content/Admin.css` | Admin styles, linked from `Admin.Master` |

**`AppliedAt`:** there is no "submitted at" column. Use the time of the latest `SUBMIT_TUTOR_APPLICATION` or `RESUBMIT_TUTOR_APPLICATION` audit entry for that user, and fall back to `Users.JoinedDate` for tutors who applied while registering:

```sql
COALESCE(
    (SELECT MAX(a.Timestamp) FROM AuditLogs a
     WHERE a.UserId = tp.UserId AND a.Action IN ('SUBMIT_TUTOR_APPLICATION', 'RESUBMIT_TUTOR_APPLICATION')),
    u.JoinedDate) AS AppliedAt
```

**Approve and reject SQL**, both guarded on Pending:

```sql
-- Approve
UPDATE TutorProfiles SET Status = @Approved, ApprovedDate = SYSUTCDATETIME(), RejectionReason = NULL
WHERE UserId = @UserId AND Status = @Pending;

-- Reject
UPDATE TutorProfiles SET Status = @Rejected, RejectionReason = @Reason
WHERE UserId = @UserId AND Status = @Pending;
```

### Changed files

| File | Change |
|---|---|
| `Modules/Admin/AuditLogs.aspx` | Uses `Admin.Master` and becomes a tab |
| `Helpers/SessionHelper.cs` | `GetHomeUrl`: Admin goes to `~/Modules/Admin/Dashboard` instead of `AuditLogs` |
| `Site.Master` | The admin nav link becomes "Admin Dashboard" |
| `DAL/Tutor/TutorRepository.cs` (**teammate's file**) | `SaveApplication` also sets `RejectionReason = NULL`, so resubmitting clears the old reason |
| `Modules/Tutor/Profile.aspx(.cs)` (**teammate's file**) | When the status is Rejected, show the reason above the resubmit form |

Tell Edwin and Joshua about the two teammate files before merging.

### Tests

- [ ] Register a new tutor. They appear under Pending with today's date, and the tab badge shows 1.
- [ ] Approve them. That tutor can now open the Course builder, and `APPROVE_TUTOR` is in the audit logs.
- [ ] Reject with an empty reason: the page refuses. Reject with a reason: the tutor's Profile page shows it.
- [ ] The rejected tutor resubmits. They are Pending again and the old reason is gone.
- [ ] Open the same application in two tabs and approve in both. The second one says it was already handled.
- [ ] `admin` logs in and lands on the Dashboard.

---

## Phase 2: Course moderation

**Branch:** `feat/admin-module`
**Goal:** an admin can review submitted courses and publish them, send them back with feedback, lock bad ones, and delete spam.

### How course statuses change

```text
Draft ──(tutor submits)──► Pending Review ──(admin approves)──► Published ◄──(admin unlocks)──┐
  ▲                              │                                  │                         │
  └──────(admin rejects, ────────┘                                  └──(admin locks,──► Locked ┘
          reason required)                                                reason required)
```

The tutor side already exists (Edwin): only Drafts can be edited, submitting clears the old rejection reason, and `CourseEditor` already shows the rejection and lock reasons to the tutor.

### New files

| File | Contents |
|---|---|
| `Models/Admin/AdminCourseRow.cs` | `Id`, `Title`, `Difficulty`, `Category`, `Status`, `TutorName`, `TutorEmail`, `LessonCount`, `StudentCount`, `LockReason`, `RejectionReason`, `UpdatedAt` |
| `Models/Admin/CourseReviewDetails.cs` | One course with its lessons, quiz and coding challenge. Not named `CourseReview`, because the page has that name |
| `DAL/Admin/CourseModerationRepository.cs` | `GetCourses(string status, string search)`, `GetReview(int courseId)`, `CountPending()`, `Approve`, `Reject`, `Lock`, `Unlock`, `Delete` |
| `BLL/Admin/CourseModerationManager.cs` | Admin check, reason validation, audit entries, removing uploaded files after a delete |
| `Modules/Admin/Courses.aspx` | Filter buttons (All, Pending, Published, Locked, Draft), a search box (title or tutor), and the table from the prototype. Each row shows only the actions its status allows. |
| `Modules/Admin/CourseReview.aspx?id=` | Course details, cover, description and lessons, with an action panel: Approve, Reject (reason), Lock (reason), Unlock, Delete |

**Which actions each status allows:**

| Status | Approve | Reject | Lock | Unlock | Delete |
|---|---|---|---|---|---|
| Draft | | | | | yes |
| Pending Review | yes | yes | | | yes |
| Published | | | yes | | yes |
| Locked | | | | yes | yes |

Each status change is guarded in the SQL, for example `Approve` is `UPDATE Courses SET Status = @Published, RejectionReason = NULL, UpdatedAt = SYSUTCDATETIME() WHERE Id = @Id AND Status = @PendingReview`.

**Delete needs care.** Deleting a course removes its lessons, enrollments, completions, coding challenge, submissions and discussions (they cascade). But `Quizzes.CourseId` is `ON DELETE SET NULL`, so the quiz would be left behind with no course. Delete in one transaction, quiz first:

```sql
DELETE FROM Quizzes WHERE CourseId = @Id;   -- its questions and attempts cascade
DELETE FROM Courses WHERE Id = @Id;
```

Before deleting, read the cover and lesson attachment paths. After the transaction succeeds, the BLL deletes those files from `~/Uploads`. The confirm box shows how many students are enrolled.

### Changed files

| File | Change |
|---|---|
| `Modules/Admin/Admin.Master` | Adds the "Course Moderation" tab with a pending badge |
| `Modules/Admin/Dashboard.aspx` | Adds the "Course moderation" panel: pending courses and locked courses |

### Tests

- [ ] As `tutor`: create a Draft with one lesson and submit it. As `admin`: it is under Pending with a badge.
- [ ] Approve it. As `learner`, it appears in the course catalogue.
- [ ] Submit another, then reject it with a reason. The tutor's Course builder shows the reason, and the tutor can edit and submit again.
- [ ] Lock a Published course with a reason. It disappears from the learner catalogue, and the tutor sees the lock reason. Unlock it, and it is back.
- [ ] Delete a course that has a quiz. Afterwards `SELECT COUNT(*) FROM Quizzes WHERE CourseId IS NULL` has not gone up, and its cover file is gone from `Uploads`.
- [ ] Approving a course that is already Published does nothing and says so.

---

## Phase 3: Users

**Branch:** `feat/course-moderation`
**Goal:** an admin can find, create and edit users, change roles, and suspend accounts in a way that actually takes effect.

### New files

| File | Contents |
|---|---|
| `Models/Admin/AdminUserRow.cs` | The `User` fields plus `HasActivity` (true if the user has anything that blocks deleting) |
| `DAL/Admin/UserAdminRepository.cs` | `GetUsers(string search)`, `GetById`, `IsEmailTakenByOther`, `UpdateDetails(id, fullName, email, role)`, `Suspend(id)`, `Reactivate(id)`, `Delete(id)`. `HasActivity` is worked out in the shared SELECT. The role and status filters run on the page |
| `BLL/Admin/UserAdminManager.cs` | The rules below, plus audit entries |
| `Modules/Admin/Users.aspx` | Search (name, username or email), role and status filters, and the table from the prototype. Actions: Edit, Suspend or Reactivate, Delete (disabled with an explanation when the user has activity). |
| `Modules/Admin/UserEdit.aspx` | With no `id`: create a user (full name, username, email, role, temporary password). With `?id=`: view and edit (full name, email, role) plus Suspend or Reactivate. |

**Creating a user** reuses what Register already uses: `UserManager.EmailPattern`, `PasswordPattern`, `IsUsernameTaken`, `IsEmailTaken`, `PasswordHelper.HashPassword`, and `UserRepository.Insert(user, null)` (passing `null` means no tutor application). A tutor created by an admin still fills in the tutor application, then an admin approves it like any other.

**`HasActivity`** is true if any of these exist for the user: courses they teach, enrollments, lesson completions, quizzes they created, quiz attempts, code submissions, discussion posts or replies, badges, audit entries. `TutorProfiles` and `LearnerStats` are deleted with the user automatically (cascade), so they do not count. A new table that points at `Users` without a cascade must be added to this check.

### Rules (in `UserAdminManager`)

- An admin cannot suspend, delete or change the role of **their own** account. This also protects the **last active Admin**: whoever makes the change is an active admin, and they stay one, so no separate count is needed.
- A **tutor who still teaches courses** cannot change role, because those courses would be left without a tutor.
- **Delete** only works when `HasActivity` is false. Otherwise the message suggests Suspend.
- Username and email must stay unique, and use the same patterns as Register.

### Suspension and role changes take effect

At the moment, the login cookie keeps working after a status or role change. Add this to `Global.asax.cs` (it runs once per page request, after the session is loaded):

```csharp
// An admin may have suspended this account or changed its role since the cookie was issued
protected void Application_PostAcquireRequestState(object sender, EventArgs e)
{
    if (!Request.IsAuthenticated || Context.Session == null)
    {
        return;
    }

    // Load (Status, Role) for the signed-in user through UserManager.GetAccountState.
    // If the status is not Active, or the role differs from the cookie's role:
    //   SessionHelper.SignOut(), then redirect to ~/Modules/Public/Login?reason=changed
}
```

`Login.aspx` shows a message for `?reason=changed`: "Your account was changed by an administrator. Please log in again." This is one small database query per page, which is fine at this size.

### Changed files

| File | Change |
|---|---|
| `Global.asax.cs` | The check above |
| `BLL/UserManager.cs`, `DAL/UserRepository.cs` | `IsLoginStillValid(userId, role)` for `Global.asax`, reading `GetAccountState(userId)`. If the database fails, the user stays logged in, because a database problem must not sign everyone out |
| `Helpers/SessionHelper.cs` | `GetUserIdFromTicket` |
| `Modules/Public/Login.aspx(.cs)` | The `?reason=changed` message |
| `Modules/Admin/Admin.Master` | Adds the "Users" tab |
| `Modules/Admin/Dashboard.aspx` | Adds the "Recent users" panel (newest 5, with a "Manage all users" link) |

### Tests

- [ ] Create a Learner. They can log in with the temporary password, and `CREATE_USER` is logged.
- [ ] Creating a user with an existing username or email is refused with a clear message.
- [ ] While `learner` is logged in, suspend them in another browser. Their next click sends them to Login with the message, and logging in again says the account is suspended.
- [ ] Reactivate them. They can log in again.
- [ ] Change a logged-in user's role. Their next click logs them out, and after logging in they get the new role's pages.
- [ ] Try to suspend your own account: refused. Try to suspend the only admin: refused.
- [ ] Delete is disabled for `learner` (has activity). A user you just created, who has never logged in, can be deleted.

---

## Phase 4: Overview, stat cards and audit logs

**Branch:** `feat/admin-4-overview-audit`
**Goal:** the Overview tab and the stat cards show real numbers, and the audit logs can be searched and filtered.

### Stat cards

Shown at the top of every admin tab (in `Admin.Master`), as in the prototype:

| Card | Shows |
|---|---|
| Database | Database file size, time of a test query in ms, number of tables |
| Needs your review | Pending tutor applications and pending courses, each linking to its tab |
| Users | Active accounts, new this month, and the count per role (Learners, Tutors, Admins) |
| Courses | Total, plus Published, Pending Review, Locked and Draft counts |

### New files

| File | Contents |
|---|---|
| `Models/Admin/AdminStats.cs` | Every number on the cards and the banner |
| `DAL/Admin/AdminStatsRepository.cs` | One query that returns all the counts. Database size comes from `SELECT SUM(size) * 8 / 1024.0 FROM sys.database_files` (MB). |
| `BLL/Admin/AdminStatsManager.cs` | Admin check. Times a `SELECT 1` with `Stopwatch` for the query time. |
| `Models/Admin/AuditCategories.cs` | The category names and rules (see below) |

### Audit log upgrade

- **Search** across username, full name, action and details.
- **Category filter:** All, Security, Administrative, Content, Deletion. The category is worked out in the SQL from the action name, in one `CASE` in `AuditRepository`:
  - **Deletion:** actions starting `DELETE_` or `REMOVE_`
  - **Security:** `LOGIN`, `LOGOUT`, `LOGIN_FAILED`, `REGISTER`, `SUSPEND_USER`, `REACTIVATE_USER`
  - **Administrative:** `APPROVE_`, `REJECT_`, `LOCK_`, `UNLOCK_`, `CREATE_USER`, `UPDATE_USER`, `CREATE_BACKUP`, `DOWNLOAD_BACKUP`
  - **Content:** everything else (tutors and learners creating, updating and submitting things)
- **Pages of 25 rows** with `OFFSET ... FETCH`, instead of only the latest 100.
- **`LOGIN_FAILED`:** `UserManager.Authenticate` records it when the username exists but the password is wrong. Unknown usernames cannot be recorded, because `AuditLogs.UserId` must point at a real user.

While editing `Authenticate`, also fix the timing gap found in the earlier review: check the password against a dummy hash when the username does not exist, so a wrong username takes as long as a wrong password.

### Changed files

| File | Change |
|---|---|
| `Modules/Admin/Admin.Master(.cs)` | The four cards and the "waiting for review" banner (closable) |
| `Modules/Admin/Dashboard.aspx` | Final layout: Recent users, Pending tutor applications, Course moderation |
| `DAL/AuditRepository.cs`, `BLL/AuditManager.cs` | `Search(text, category, page)` and the total count |
| `Modules/Admin/AuditLogs.aspx(.cs)` | Search box, category buttons, pager, category badge column |
| `BLL/UserManager.cs` | `LOGIN_FAILED` and the dummy-hash check |

### Tests

- [ ] Every card number matches a manual `SELECT COUNT(*)` on the database.
- [ ] Submit a course as `tutor`. The "Needs your review" count and the banner go up by one.
- [ ] A wrong password for `learner` appears in the audit logs under Security as `LOGIN_FAILED`.
- [ ] Searching "course" finds course actions. The Deletion filter shows only `DELETE_` and `REMOVE_` actions.
- [ ] The pager moves through all rows, and the total matches `SELECT COUNT(*) FROM AuditLogs`.

---

## Phase 5: Database backups (optional)

**Branch:** `feat/admin-5-backups`
**Goal:** an admin can make a real backup of the database and download it. Build this only if there is time after Phase 4.

### New files

| File | Contents |
|---|---|
| `DAL/Admin/BackupRepository.cs` | `CreateBackup(string fullPath)` |
| `BLL/Admin/BackupManager.cs` | `Create()`, `List()`, `GetFileForDownload(string name)`, admin check, audit entries |
| `Models/Admin/BackupFile.cs` | `Name`, `SizeBytes`, `CreatedAt`, `Sha256` |
| `Modules/Admin/Backups.aspx` | The dark "Run backup now" banner and the table: name, size, created, short SHA-256, Download button |

**Making a backup.** The database name is the `.mdf` path, so read it with `DB_NAME()`. `BACKUP DATABASE` accepts a variable for it, so there is no SQL built from strings:

```sql
DECLARE @db sysname = DB_NAME();
BACKUP DATABASE @db TO DISK = @Path WITH INIT, CHECKSUM, COPY_ONLY;
```

- Save to `App_Data/Backups/codelab-yyyyMMdd-HHmmss.bak`. LocalDB runs as your Windows user, so it can write there.
- Set `CommandTimeout` to 120 seconds.
- After the backup, compute the SHA-256 of the file and save it next to it as `<name>.sha256`. `List()` reads those files, so no database table is needed.

**Downloading.** Only allow a name that `List()` returned (use `Path.GetFileName`, and require the `.bak` extension and that the file exists in the folder), so nobody can download other files by changing the name. Then send it with `Response.TransmitFile` and `Content-Disposition: attachment`.

### Changed files

| File | Change |
|---|---|
| `.gitignore` | Add `*.bak` and `*.sha256` |
| `Modules/Admin/Admin.Master` | Adds the "Database Backups" tab and the "Backup Database" header button |
| Stat card "Database" | Shows the time of the last backup |

### Tests

- [ ] Run a backup. A `.bak` file appears in `App_Data/Backups`, and `CREATE_BACKUP` is logged.
- [ ] Download it. The file's SHA-256 (in PowerShell, `Get-FileHash file.bak`) matches the one on the page.
- [ ] Changing the download name to `..\Database.mdf` is refused.
- [ ] `git status` does not show the backup files.

---

## Out of scope

- Restoring a backup from the admin pages
- Real server monitoring (CPU, memory, uptime)
- Client IP addresses in the audit logs
- Revoking a copied login cookie after logout (the Phase 3 check covers suspended and changed accounts, which is the case that matters)
- Email notifications to tutors about approval or rejection (they see the result on their Profile and Course builder pages)
