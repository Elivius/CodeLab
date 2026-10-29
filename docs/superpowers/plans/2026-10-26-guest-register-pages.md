# Guest Pages & Register Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let guests browse published courses (landing, catalogue, details) and register as a Learner or Tutor (tutor application included), with Login sending each role to its home.

**Architecture:** ASP.NET Web Forms, 3 layers. Pages call `*Manager` (BLL), which calls `*Repository` (DAL, the only place with SQL). Course reading goes into Edwin's `CourseRepository`/`CourseManager`; account creation into `UserRepository`/`UserManager`. One user control renders course cards for both the landing page and the catalogue.

**Tech Stack:** .NET Framework 4.7.2 Web Forms, C# 7.3, ADO.NET, SQL Server LocalDB, Bootstrap 5, Lucide icons.

**Spec:** Proposal sections 5.2.1 Guest and 5.2.3 Tutor (pasted in chat), plus the decisions agreed on 2026-10-26, summarised under Global Constraints.

## How the phases work

Each task below is one **phase**: small enough to build and check in Visual Studio before the next starts. After every phase:

1. In Visual Studio, click **Reload** when it says the project was changed outside the editor (new pages are added to `CodeLab.csproj` by hand).
2. Open each new `.aspx`/`.ascx` from the phase and press **Ctrl+S** once, so Visual Studio regenerates its `.designer.cs`.
3. **Build > Rebuild Solution** must finish with 0 errors, then press **Ctrl+F5** and run the phase's checks.
4. Commit, then start the next phase.

There is no test project in the solution, so each phase is verified with the browser checks listed in it.

## Global Constraints

- Follow CONTRIBUTING.md: SQL only in `DAL/`, rules only in `BLL/`, parameterized queries with explicit `SqlDbType`, `OperationResult`/`OperationResult<T>` from the BLL, friendly URLs (no `.aspx` in links).
- DAL write methods catch `SqlException`, log with `LogHelper.LogError`, return `false`/`0`. DAL read methods let exceptions rise; the BLL logs and returns `Fail`.
- Only `CourseStatuses.Published` courses are ever shown to guests, and only title, description, category, difficulty, thumbnail and lesson count.
- Register rules (proposal): every field required; username and email unique; password at least 8 characters with at least one number; confirm password must match. Added: email format; max lengths full name 120, username 50, email 255.
- Role on Register comes from `?role=` and must be whitelisted: `tutor` makes a Tutor, anything else makes a Learner. Never Admin.
- New accounts: `Users.Status = UserStatuses.Active`. Tutors also get a `TutorProfiles` row with `Status = TutorStatuses.Pending`, in the same transaction. Pending tutors can log in; authoring stays locked (already handled by Edwin's code).
- After Register, redirect to Login (no automatic sign-in). No photo on Register (tutors add it in Edwin's Edit profile).
- Do not edit `Site.Master` (navbar follows Joanne's version).
- Tutor-entered text (course titles, descriptions) is HTML-encoded on every public page.

## Review Focus

- **Role tampering:** `Register?role=Admin`, or a forged post, must create a Learner, never an Admin. Check in Task 3.
- **HTML in course text:** a course title like `<script>alert(1)</script>` must show as plain text on the landing page, catalogue and details. Check in Task 1 and Task 2.
- **Bad course ids:** `CourseDetails?id=abc`, `?id=-1`, a missing id and a Draft course's id all show "Course not found", never an error page. Check in Task 2.
- **Same username, different case:** `TUTOR` must be rejected because `tutor` exists (the database's default collation ignores case). Check in Task 3.
- **No courses or database down:** the landing page and catalogue still load and show "New courses are on the way". Check in Task 1.

---

### Task 1 (Phase 1): Published courses on the landing page

**Files:**
- Modify: `CodeLab/Models/Course.cs` (add property)
- Modify: `CodeLab/DAL/CourseRepository.cs` (add method)
- Modify: `CodeLab/BLL/CourseManager.cs` (add methods)
- Create: `CodeLab/UserControls/CourseGrid.ascx`, `.ascx.cs`, `.ascx.designer.cs`
- Modify: `CodeLab/Default.aspx`, `CodeLab/Default.aspx.cs`
- Modify: `CodeLab/Content/Site.css`
- Modify: `CodeLab/CodeLab.csproj` (CourseGrid entries)

**Interfaces:**
- Produces:
  - `Course.LessonCount` (`int`)
  - `CourseRepository.GetPublished() : List<Course>`
  - `CourseManager.GetCatalogue() : OperationResult<List<Course>>`
  - `CourseManager.GetFeatured(int count) : OperationResult<List<Course>>`
  - `CourseManager.GetPublicCourse(int courseId) : OperationResult<Course>` (used in Task 2)
  - `CodeLab.UserControls.CourseGrid` with `public List<Course> Courses { get; set; }`; registered on pages as `<%@ Register Src="~/UserControls/CourseGrid.ascx" TagPrefix="cl" TagName="CourseGrid" %>`

- [ ] **Step 1: Add `public int LessonCount { get; set; }` to `Course`.** One-line comment: filled only by `GetPublished`.

- [ ] **Step 2: Add `CourseRepository.GetPublished()`.** Selects `Id, Title, Description, Category, Difficulty, ThumbnailUrl, CreatedAt` plus `(SELECT COUNT(*) FROM Lessons l WHERE l.CourseId = c.Id) AS LessonCount` from `Courses c` where `Status = @Published` (`NVarChar, 20` = `CourseStatuses.Published`), `ORDER BY c.CreatedAt DESC`. Read method: no try/catch.

- [ ] **Step 3: Add the three `CourseManager` methods.** No role check (guests). Each wraps the repository call in try/catch, logs with `LogHelper.LogError("CourseManager.<Method> failure", ex)`, and fails with `"Courses could not be loaded. Please try again."`.
  - `GetCatalogue()` returns `GetPublished()`.
  - `GetFeatured(int count)` returns `GetPublished().OrderBy(c => Guid.NewGuid()).Take(count).ToList()` (add `using System.Linq;`) with the comment `// shortcut: shuffles the whole published list in memory, move to ORDER BY NEWID() in SQL past a few hundred courses`.
  - `GetPublicCourse(int courseId)` uses the existing `GetById`. If it returns `null`, or `Status != CourseStatuses.Published`, fail with `"Course not found."` (the same message for both, so draft ids cannot be probed).

- [ ] **Step 4: Create `UserControls/CourseGrid.ascx`.** Namespace `CodeLab.UserControls`.
  - **Data:** a `Repeater` bound to `Courses` in `OnPreRender`. When `Courses` is null or empty, hide it and show a `PlaceHolder` with "New courses are on the way."
  - **Markup:** reuse the existing card markup and classes from `Default.aspx` (`cl-card cl-course`, `cl-course-thumb`, `cl-level cl-level-<difficulty lowercase>`, `cl-course-body`, `cl-course-category`, `cl-course-footer`).
  - **Encoding:** bind every text with `<%#: %>`, so it is HTML-encoded.
  - **Thumbnail:** if `ThumbnailUrl` is set, show an `<img>` (`ResolveUrl`, `alt` = title, class `cl-course-cover`). Otherwise show the default tile `cl-thumb-default` with the text `</>`.
  - **Footer:** shows `1 Lesson` or `N Lessons`. The "Explore Syllabus" link goes to `~/Modules/Public/CourseDetails?id=<Id>`.
  - **csproj:** add `<Content Include="UserControls\CourseGrid.ascx" />`, plus `<Compile>` entries for `.ascx.cs` and `.ascx.designer.cs` with `<DependentUpon>CourseGrid.ascx</DependentUpon>`, matching the `Login.aspx` entries.

- [ ] **Step 5: Wire the landing page.**
  - **Markup:** in `Default.aspx`, replace the three hard-coded `<article>` cards with `<cl:CourseGrid ID="courseGrid" runat="server" />`. Change the "View all catalogue (5)" link to `href="~/Modules/Public/Courses"` (`runat="server"`), with the count in a `Literal` `litCourseCount`.
  - **Code-behind:** in `Default.aspx.cs` `Page_Load`, set `courseGrid.Courses` from `GetFeatured(3)` (null on fail), and set `litCourseCount` to the `GetCatalogue()` count (`0` on fail). Remove the "static mock data" comment.

- [ ] **Step 6: CSS.** In `Site.css`:
  - Delete `.cl-thumb-frontend`, `.cl-thumb-science` and `.cl-thumb-python`. First grep the whole project to confirm nothing else uses them.
  - Add `.cl-thumb-default` (reuse one of the deleted gradients).
  - Add `.cl-course-cover` (`width:100%; height:100%; object-fit:cover`).
  - Clamp `.cl-course-body p` to 3 lines (`display:-webkit-box; -webkit-line-clamp:3; -webkit-box-orient:vertical; overflow:hidden`).

- [ ] **Step 7: Verify.** Rebuild, then press Ctrl+F5 and open `/`. Expected:
  - The 3 seeded courses appear with the default tile and real lesson counts.
  - Refreshing changes their order.
  - The link reads "View all catalogue (3)".
  - Run `UPDATE Courses SET Status='Draft'` in Server Explorer, then reload: "New courses are on the way" shows. Run `UPDATE Courses SET Status='Published'` to restore.
  - Run `UPDATE Courses SET Title=N'<b>x</b>' WHERE Id=1`: the page shows `<b>x</b>` as text. Restore it afterwards by rebuilding the database.

- [ ] **Step 8: Commit.**

```bash
git add CodeLab/Models/Course.cs CodeLab/DAL/CourseRepository.cs CodeLab/BLL/CourseManager.cs CodeLab/UserControls CodeLab/Default.aspx CodeLab/Default.aspx.cs CodeLab/Content/Site.css CodeLab/CodeLab.csproj
git commit -m "feat: show published courses on the landing page"
```

---

### Task 2 (Phase 2): Public catalogue and course details

**Files:**
- Create: `CodeLab/Modules/Public/Courses.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Create: `CodeLab/Modules/Public/CourseDetails.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Modify: `CodeLab/Content/Site.css`
- Modify: `CodeLab/CodeLab.csproj`

**Interfaces:**
- Consumes: `CourseManager.GetCatalogue()`, `CourseManager.GetPublicCourse(int)`, `CourseGrid` (Task 1); `SessionHelper.IsUserInRole(UserRoles.Learner)`.
- Produces: pages at `~/Modules/Public/Courses` and `~/Modules/Public/CourseDetails?id=<int>`.

- [ ] **Step 1: `Courses` page.**
  - Page title "Course Catalogue", namespace `CodeLab.Modules.Public`, `MasterPageFile="~/Site.Master"`.
  - Heading plus `<cl:CourseGrid>`, with `Courses` set from `GetCatalogue()` (null on fail).
  - No search, filter or paging.

- [ ] **Step 2: `CourseDetails` page.**
  - **Course id:** parse `Request.QueryString["id"]` with `int.TryParse`. A missing or invalid id counts as not found and never calls the BLL.
  - **Not found:** on `Fail`, show only a "Course not found" panel with a link back to `~/Modules/Public/Courses`.
  - **Found:** show the thumbnail (or the default tile), title, category, difficulty badge and full description, all encoded (`HttpUtility.HtmlEncode` or `<%#: %>`).
  - **Lock box:** below the course, a locked box: "Enrol now to access lessons and quizzes".
    - For a signed-in Learner, it holds one button, "Open course", linking to `~/Modules/Learner/CourseDetails?id=<id>`.
    - For everyone else, it holds "Register" (`~/Modules/Public/Register?role=learner`) and "Log in" (`~/Modules/Public/Login`).

- [ ] **Step 3: CSS and csproj.**
  - **CSS:** in `Site.css`, add only what the details layout needs (`.cl-course-detail`, `.cl-lock-box`), reusing the existing card and pill styles.
  - **csproj:** add entries for both pages, the same as `Login.aspx`.

- [ ] **Step 4: Verify.** Rebuild, then press Ctrl+F5. Expected:
  - `/Modules/Public/Courses` lists all 3 courses, newest first.
  - Clicking a card opens its details with the Register and Log in buttons.
  - Logged in as `learner`, the box shows "Open course".
  - `?id=abc`, `?id=-1`, no id, and the id of a course set to `Draft` all show "Course not found".

- [ ] **Step 5: Commit.**

```bash
git add CodeLab/Modules/Public/Courses.* CodeLab/Modules/Public/CourseDetails.* CodeLab/Content/Site.css CodeLab/CodeLab.csproj
git commit -m "feat: public course catalogue and course details pages"
```

---

### Task 3 (Phase 3): Learner registration

**Files:**
- Modify: `CodeLab/DAL/UserRepository.cs`
- Modify: `CodeLab/BLL/UserManager.cs`
- Create: `CodeLab/Modules/Public/Register.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Modify: `CodeLab/Modules/Public/Login.aspx`, `Login.aspx.cs`
- Modify: `CodeLab/Default.aspx` (Register links)
- Modify: `CodeLab/Content/Site.css`, `CodeLab/CodeLab.csproj`

**Interfaces:**
- Consumes: `PasswordHelper.HashPassword(string)`, `AuditManager.RecordActivity(int, string, string)`, `UserRoles`, `UserStatuses.Active`.
- Produces:
  - `UserRepository.IsUsernameTaken(string username) : bool`, which matches under the database's case-insensitive collation.
  - `UserRepository.IsEmailTaken(string email) : bool`
  - `UserRepository.Insert(User user, TutorProfile application) : int` returns the new user id, or `0` on failure. `application` is `null` for learners.
  - `UserManager.EmailPattern` and `UserManager.PasswordPattern` (`public const string`). The values are `^[^@\s]+@[^@\s]+\.[^@\s]+$` and `^(?=.*\d).{8,}$`.
  - `UserManager.IsUsernameTaken(string) : bool` and `UserManager.IsEmailTaken(string) : bool`. They return `false` and log if the database fails; `Register` re-checks anyway.
  - `UserManager.Register(User account, string password, string confirmPassword, TutorProfile application) : OperationResult`. `account` carries `FullName`, `Username`, `Email` and `Role`.

- [ ] **Step 1: `UserRepository`.**
  - **The two `Is…Taken` methods:** each runs `SELECT COUNT(*) FROM Users WHERE Username = @Username` (or `Email`). These are read methods, so no try/catch.
  - **`Insert`:** opens one connection and a `SqlTransaction`, then:
    - `INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, Status) OUTPUT INSERTED.Id VALUES (...)`
    - When `application != null`: `INSERT INTO TutorProfiles (UserId, Specialty, Qualifications, PortfolioUrl, Bio, CurriculumPitch, Status) VALUES (..., @Status)`, with `@Status = TutorStatuses.Pending`
    - Commit.
  - **On `SqlException`:** roll back, log `"UserRepository.Insert failure"`, return `0`.

- [ ] **Step 2: `UserManager.Register`.** It checks, in this order, and returns the first failure:
  1. **Clean the input.** Trim full name, username and email. Set the role to `UserRoles.Tutor` only if `account.Role == UserRoles.Tutor`, otherwise `UserRoles.Learner`.
  2. **Required fields:** `"All fields are required."`
  3. **Lengths:** 120, 50 and 255, with messages naming the field.
  4. **Email** matches `EmailPattern`: `"Please enter a valid email address."`
  5. **Password** matches `PasswordPattern`: `"Password must be at least 8 characters and contain at least one number."`
  6. **Confirm password:** `"Passwords do not match."`
  7. **Tutor application** (tutors only): `application` must not be null; set `application.FullName = account.FullName` and run `TutorManager.CleanAndValidate(application)`. Task 4 makes this method `internal`, so in this task, guard it with `if (role == UserRoles.Tutor)` and leave the call for Task 4.
  8. **Uniqueness:** `"That username is already taken."` / `"That email is already registered."`
  9. **Save.** Hash the password, then set `Status = UserStatuses.Active` and call `Insert`.
     - If `Insert` returns `0`, re-check both "taken" methods (someone may have taken the name meanwhile) and return the matching message. Otherwise return `"A system error occurred while creating your account. Please try again."`
     - On success, record the audit entry `REGISTER` with `"Registered as <role>."` and return `Ok("Account created.")`.

  Wrap the repository reads in try/catch, the same as `Authenticate`.

- [ ] **Step 3: `Register` page.**
  - **Basics:** page title "Register", namespace `CodeLab.Modules.Public`, the `cl-card cl-auth` layout from `Login.aspx`.
  - **Tabs:** two links, "Learner" (`?role=learner`) and "Tutor" (`?role=tutor`), with the active one taken from `?role` (case-insensitive; anything else means Learner). This task only builds the Learner form. The Tutor tab link exists, but its fields come in Task 4.
  - **Fields:** `txtFullName` (MaxLength 120), `txtUsername` (50), `txtEmail` (255, `TextMode="Email"`), `txtPassword` and `txtConfirm` (`TextMode="Password"`). Each has a label and `Display="Dynamic"` validators under it:
    - `RequiredFieldValidator` on each field
    - `RegularExpressionValidator` for email and password, with `ValidationExpression` set in `Page_Init` from `UserManager.EmailPattern` and `UserManager.PasswordPattern`
    - `CompareValidator` from confirm to password
    - `CustomValidator` `cvUsernameTaken` and `cvEmailTaken`, whose `ServerValidate` calls `UserManager.IsUsernameTaken` / `IsEmailTaken`
  - **`btnRegister_Click`:** if `Page.IsValid`, call `Register`.
    - On success, `Response.Redirect("~/Modules/Public/Login?registered=" + (isTutor ? "tutor" : "learner"))`.
    - On fail, show `lblMessage` (alert-danger).
  - **`Page_Load`:** a signed-in user is redirected to `~/` (same as `Login.aspx.cs`).
  - **Footer:** "Already have an account? Log in" link.
  - **csproj:** add the entries.

- [ ] **Step 4: `Login` page.**
  - **Message:** when `?registered=learner`, show a green `lblSuccess` with "Account created. Please log in."
  - **Link:** add "No account? Register" (`~/Modules/Public/Register`).

- [ ] **Step 5: Landing links.** In `Default.aspx`, the three Register buttons get `?role=learner`.

- [ ] **Step 6: Verify.** Rebuild, then press Ctrl+F5 and open `/Modules/Public/Register`. Expected:
  - An empty submit shows a message under each field without a page reload.
  - `abc@` gives the email message. `password` (no digit) and `pass1` (too short) give the password message. A mismatched confirm gives "Passwords do not match."
  - Username `TUTOR` gives "That username is already taken." Email `tutor@codelab.local` gives "That email is already registered."
  - `Register?role=Admin` shows the Learner tab and creates a Learner.
  - A valid sign-up lands on Login with the green message. Logging in with the new account works, and a `REGISTER` row shows in Admin > Audit Logs.

- [ ] **Step 7: Commit.**

```bash
git add CodeLab/DAL/UserRepository.cs CodeLab/BLL/UserManager.cs CodeLab/Modules/Public/Register.* CodeLab/Modules/Public/Login.* CodeLab/Default.aspx CodeLab/Content/Site.css CodeLab/CodeLab.csproj
git commit -m "feat: learner registration"
```

---

### Task 4 (Phase 4): Tutor registration with application

**Files:**
- Modify: `CodeLab/BLL/TutorManager.cs` (one word)
- Modify: `CodeLab/BLL/UserManager.cs`
- Modify: `CodeLab/Modules/Public/Register.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Modify: `CodeLab/Modules/Public/Login.aspx.cs`
- Modify: `CodeLab/Default.aspx`, `CodeLab/Content/Site.css`

**Interfaces:**
- Consumes: `UserRepository.Insert(User, TutorProfile)` and `UserManager.Register(...)` (Task 3).
- Produces: `TutorManager.CleanAndValidate(TutorProfile) : string` changes from `private static` to `internal static`.

- [ ] **Step 1: Make `CleanAndValidate` `internal static`** in `TutorManager.cs`. Nothing else in the file changes.

- [ ] **Step 2: Finish the tutor branch of `Register`.**
  - **Validation:** call `TutorManager.CleanAndValidate(application)` and return its message if it isn't null.
  - **Audit:** after a successful insert, also record `SUBMIT_TUTOR_APPLICATION` with `"Submitted a tutor application for review (specialty: " + application.Specialty + ")."`, the same text Edwin's `SubmitApplication` writes.

- [ ] **Step 3: Tutor fields on `Register`.**
  - **Placeholder:** a `PlaceHolder phTutorFields`, `Visible` only on the Tutor tab, headed "Your tutor application". Validators inside it only run when it is visible.
  - **Fields:** `txtSpecialty` (100), `txtQualifications` (multi-line), `txtPortfolioUrl` (500), `txtBio` (multi-line) and `txtPitch` (multi-line). Each has a `RequiredFieldValidator`, and the portfolio link also has a `RegularExpressionValidator` `^https?://\S+$`.
  - **Labels:** match Edwin's Profile page (Specialty, Qualifications, Portfolio link, About you, Sample curriculum pitch).
  - **On submit:** build a `TutorProfile` from the fields when on the Tutor tab, otherwise pass `null`.

- [ ] **Step 4: Login message and landing button.**
  - **Login:** when `?registered=tutor`, show "Account created. Your tutor application is waiting for admin review. Log in to track its status."
  - **Landing:** in `Default.aspx`, add one "Teach on CodeLab" button (`cl-btn-outline`) next to "Register Free" in the final CTA bar, linking to `~/Modules/Public/Register?role=tutor`.

- [ ] **Step 5: Verify.** Rebuild, then press Ctrl+F5. Expected:
  - Register on the Tutor tab with empty tutor fields shows a required message under each one. `portfolio.com` (no http) gives the portfolio message.
  - A valid tutor sign-up goes to Login with the tutor message.
  - Logging in lands on the Tutor Dashboard with the "under review" notice, and authoring links show a lock.
  - Profile shows **Pending approval**, and the timeline is at "Under review".
  - Audit Logs show both `REGISTER` and `SUBMIT_TUTOR_APPLICATION`.
  - Making the `TutorProfiles` insert fail (temporarily set `Specialty` to a 101-character value directly in the code) leaves no new row in `Users`. Revert after checking.

- [ ] **Step 6: Commit.**

```bash
git add CodeLab/BLL/TutorManager.cs CodeLab/BLL/UserManager.cs CodeLab/Modules/Public/Register.* CodeLab/Modules/Public/Login.aspx.cs CodeLab/Default.aspx CodeLab/Content/Site.css
git commit -m "feat: tutor registration with application"
```

---

### Task 5 (Phase 5): Login sends each role home

**Files:**
- Modify: `CodeLab/Modules/Public/Login.aspx.cs`

**Interfaces:**
- Consumes: `UserRoles`, `FormsAuthentication.GetRedirectUrl(string, bool)`.

- [ ] **Step 1: Redirect by role in `btnLogin_Click`.**
  - **Return URL first:** if `Request.QueryString["ReturnUrl"]` is not empty, keep the current `FormsAuthentication.GetRedirectUrl(...)`, which only allows URLs on this site.
  - **Otherwise, by role:**
    - Tutor goes to `~/Modules/Tutor/Dashboard`.
    - Learner goes to `~/Modules/Learner/Dashboard`.
    - Admin goes to `~/Modules/Admin/AuditLogs`.

- [ ] **Step 2: Verify.** Rebuild, then press Ctrl+F5. Expected:
  - `tutor` lands on the Tutor Dashboard. `admin` lands on Audit Logs.
  - `learner` lands on `/Modules/Learner/Dashboard`, which is a 404 until Joanne's branch is merged. That is expected.
  - Opening `/Modules/Admin/AuditLogs` while logged out, then logging in as `admin`, returns to Audit Logs.

- [ ] **Step 3: Commit.**

```bash
git add CodeLab/Modules/Public/Login.aspx.cs
git commit -m "feat: send each role to its home after login"
```
