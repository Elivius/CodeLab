# CodeLab: Landing, Register and Login explained

This document explains the code behind the landing page, the public catalogue, registration (learner and tutor) and login. Paths are relative to the `CodeLab/` project folder.

## 1. How the code is organised

The project uses three layers. Each page only talks to the layer below it.

| Layer | Folder | Job |
|---|---|---|
| Page (UI) | `*.aspx` and `*.aspx.cs` | Shows controls, reads what the user typed, shows messages. No SQL. |
| BLL (business logic) | `BLL/` | Rules and validation. Returns an `OperationResult`. |
| DAL (data access) | `DAL/` | SQL only, with parameterised queries. |

Supporting pieces: `Models/` (plain data classes), `Helpers/` (passwords, session, logging).

`OperationResult` (`Models/OperationResult.cs`) is how a manager reports back. It holds `Success`, a `Message` and, for `OperationResult<T>`, the `Data`. A page checks `Success` and either continues or shows `Message`.

---

## 2. Landing page

**Files:** `Default.aspx`, `Default.aspx.cs`, `UserControls/CourseGrid.ascx` (+ `.cs`), `Site.Master` (+ `.cs`)

### What the page shows
`Default.aspx` is made of sections:
1. **Hero**: headline, two buttons (Start Learning Free goes to `Register?role=learner`; Browse Course Catalogue scrolls to `#courses`) and four feature cards.
2. **Featured Courses** (`id="courses"`): three course cards and a "View all catalogue (n)" link to the public catalogue.
3. **Sandbox panel**: marketing text and a "Create Learner Account" button.
4. **About** (`id="about"`): value cards, headline stats and a call-to-action bar with **Register Free** and **Teach on CodeLab** (`Register?role=tutor`).

The headline stats (45,000+ learners and so on) are fixed text, not database figures.

A small script at the bottom scrolls to the `#courses` or `#about` section once the page has loaded, because the navbar links arrive as `/#courses`.

### The code-behind (`Default.aspx.cs`)
```csharp
featured = courseManager.GetFeatured(3);      // 3 random Published courses
courseGrid.Courses = featured.Success ? featured.Data : null;
catalogue = courseManager.GetCatalogue();      // all Published courses
litCourseCount.Text = catalogue.Success ? catalogue.Data.Count.ToString() : "0";
```
The page asks `CourseManager` for data and hands it to the `CourseGrid` control. It never touches the database itself.

### Where the courses come from
- `CourseManager.GetCatalogue()` calls `CourseRepository.GetPublished()`. The SQL selects courses whose `Status = 'Published'`, newest first, with their lesson count. If anything throws, the error is logged and the page gets "Courses could not be loaded."
- `CourseManager.GetFeatured(count)` takes that list, shuffles it in memory (`OrderBy(c => Guid.NewGuid())`) and takes `count`. That is why the three courses change on every refresh. The code marks this as a known shortcut: past a few hundred courses it should move to `ORDER BY NEWID()` in SQL.

### The `CourseGrid` user control
A reusable control used by both the landing page and the catalogue.
- `CourseGrid.ascx.cs` has a `Courses` property. In `OnPreRender` it binds the repeater if the list has items; otherwise it shows "New courses are on the way."
- `CourseGrid.ascx` renders one card per course: cover image (or a `</>` placeholder), difficulty badge, category, title, description, lesson count and an "Explore Syllabus" link to `Modules/Public/CourseDetails?id=...`.
- Output uses `<%#: ... %>`, which HTML-encodes values so a course title cannot inject markup.

### Site.Master (shared layout)
Holds the navbar and footer for every public page. In `Page_Init` it does something important: **a signed-in user is redirected away from any guest page** (the landing page, Login, Register, the public catalogue) to their role's home page. Terms, Privacy and Contact are exempt so the footer links always work.

---

## 3. Public catalogue and course details

**Files:** `Modules/Public/Courses.aspx(.cs)`, `Modules/Public/CourseDetails.aspx(.cs)`

- **Courses**: calls `GetCatalogue()` and passes the result to `CourseGrid`. The same data and the same control as the landing page, just all courses.
- **CourseDetails**: reads `?id=` from the address.
  - A missing, non-numeric or zero id never reaches the database; it gets "Course not found."
  - `CourseManager.GetPublicCourse(id)` loads the course and returns the same "Course not found." message whether it does not exist or is not Published. This means nobody can discover draft course ids by guessing.
  - On success it shows title, category, description, a difficulty badge and the cover image if there is one. A lock box says "Enrol now to access", with Register and Log in buttons for guests.

---

## 4. Register

**Files:** `Modules/Public/Register.aspx(.cs)`, `BLL/UserManager.cs`, `BLL/Tutor/TutorManager.cs`, `DAL/UserRepository.cs`, `Helpers/PasswordHelper.cs`, `BLL/AuditManager.cs`

### The page
- **Two tabs, one page.** The tabs are plain links (`?role=learner` or `?role=tutor`), not buttons, so switching never submits the form. The role is read from the query string in `Register.aspx.cs` (`IsTutor`).
- **Learner:** one section with full name, username, email, password and confirm password.
- **Tutor:** three steps in the same page.
  - Step 1 Account: the same five fields.
  - Step 2 Background: "About you" (bio) and qualifications.
  - Step 3 Teaching: specialty, portfolio link and a sample curriculum pitch.
  - The tutor fields sit inside `phTutorFields`, which is only visible for tutors. Validators inside a hidden placeholder do not run, so learners are never asked for them.
- **Steps are client-side.** `clShowRegisterStep`, `clNextRegisterStep` and `clValidateRegister` (the script at the bottom of the page) show one step at a time and call `Page_ClientValidate(group)` for the current step. Each step has its own validation group (`Account`, `Background`, `Teaching`). Nothing is posted until **Create account**, so the password boxes keep their values between steps.
- **Create account** has `CausesValidation="false"` plus `OnClientClick="return clValidateRegister();"`. The script checks every step and sends the user back to the first step that still has a problem.

### Validation: two layers
1. **Browser (validators in the `.aspx`):**
   - Required fields: "Full name is required.", "Username is required.", "Email is required.", "Password is required.", "Please confirm your password."
   - Email format and password strength: `RegularExpressionValidator`s. Their patterns are set in `Page_Init` from `UserManager.EmailPattern` and `UserManager.PasswordPattern`, so there is **one copy of each rule**, shared with the server.
   - Confirm password: `CompareValidator` against the password box ("Passwords do not match.").
   - Portfolio link: pattern and message come from `TutorManager.PortfolioUrlPattern` and `PortfolioUrlMessage`.
   - Username and email already taken: `CustomValidator`s whose server handlers call `IsUsernameTaken` and `IsEmailTaken`. These need the database, so they only run when the form is posted.
2. **Server (`UserManager.Register`):** the same rules again, because a browser check can be bypassed. This is the check that actually protects the system.

### What the rules are
| Field | Rule |
|---|---|
| Full name | Required, 120 characters at most |
| Username | Required, 50 characters at most, not already used |
| Email | Required, 255 at most, matches `EmailPattern`, not already used |
| Password | At least 8 characters with a letter, a number and a symbol (`PasswordPattern`) |
| Confirm | Must equal the password |
| Specialty (tutor) | Required, 100 at most |
| Bio, Qualifications, Pitch (tutor) | Required, 2000 each at most |
| Portfolio link (tutor) | Required, valid `https://` address, 500 at most |

### What happens on "Create account" (`btnRegister_Click`)
1. `Page.Validate("Account")` (and `Background` and `Teaching` for tutors). If anything is invalid, stop.
2. Build a `User` (full name, username, email, role) and, for tutors, a `TutorProfile` from the application boxes.
3. Call `UserManager.Register(account, password, confirm, application)`.
4. On failure, show the message in the red alert (HTML-encoded). On success, redirect to `Login?registered=learner` or `Login?registered=tutor`.

### Inside `UserManager.Register`, step by step
1. Trim the name, username and email.
2. **Never trust the role from the page.** `account.Role = Tutor ? Tutor : Learner`. Anything else becomes a Learner, so nobody can register as an Admin by editing the URL.
3. Required, length, email and password checks, as in the table above.
4. For a tutor, an application must exist, and `TutorManager.CleanAndValidate(application)` checks it. `CleanAndValidate` is `internal` so Register and the tutor Profile page apply exactly the same rules.
5. `GetTakenMessage` checks the username, then the email, against the database.
6. Hash the password with `PasswordHelper.HashPassword`.
7. Set `Status = Active`.
8. `UserRepository.Insert(account, application)`.
9. Record `REGISTER` in the audit log, and for tutors `SUBMIT_TUTOR_APPLICATION` too.
10. Return `OperationResult.Ok("Account created.")`.

### The database insert (`UserRepository.Insert`)
- Inserts the `Users` row and gets the new id back with `OUTPUT INSERTED.Id`.
- For a tutor, it then inserts the `TutorProfiles` row with `Status = Pending`.
- Both inserts run in **one transaction**. Either both are saved or neither is, so there can never be a tutor account with no application.
- If the insert fails (for example another person took the username a moment ago), it returns 0 and `Register` re-checks which value is taken and reports the right message.

### The tutor status rule
A new tutor's **account** is `Active` (so they can log in) but their **application** is `Pending`. The sidebar locks authoring until an admin approves. Setting the user's own status to `Pending` would stop login completely, so that is never done.

---

## 5. Login

**Files:** `Modules/Public/Login.aspx(.cs)`, `BLL/UserManager.cs`, `DAL/UserRepository.cs`, `Helpers/PasswordHelper.cs`, `Helpers/SessionHelper.cs`, `Global.asax(.cs)`, `Web.config`

### The page
Username and password boxes with `RequiredFieldValidator`s, a Log in button, and two alerts: a green one (`lblSuccess`) and a red one (`lblMessage`). The password box has `ValidateRequestMode="Disabled"` so a password containing characters like `<a` is not rejected; that is safe because a password is only hashed, never displayed.

`Page_Load` shows the green messages after registration:
- `?registered=learner` shows "Account created. Please log in."
- `?registered=tutor` shows "Account created. Your tutor application is waiting for admin review. Log in to track its status."
- `?reason=changed` (sent by `Global.asax`) shows "Your account was changed by an administrator. Please log in again."

### What happens on Log in (`btnLogin_Click`)
1. If the page is not valid (blank fields), stop.
2. `UserManager.Authenticate(username, password)`.
3. If it failed, show its message.
4. If it succeeded, `SessionHelper.SignIn(user)`, then redirect.
5. **Redirect rule:** if the address has a `ReturnUrl` (the user was sent to login from a protected page), go back there using `FormsAuthentication.GetRedirectUrl`, which only allows pages of this site. Otherwise go to `SessionHelper.GetHomeUrl(role)`:

| Role | Home page |
|---|---|
| Tutor | `~/Modules/Tutor/Dashboard` |
| Learner | `~/Modules/Learner/Dashboard` |
| Admin | `~/Modules/Admin/Dashboard` |
| Anything else | `~/` |

### `UserManager.Authenticate`, step by step
1. Blank username or password: "Username and password are required."
2. Look up the user by username (`UserRepository.GetByUsername`, a parameterised query). A database error is logged and the user sees a generic "system error" message.
3. **Unknown username:** it still verifies the password against a dummy hash, so a missing user takes as long as a wrong password. The message is the same: "Invalid username or password."
4. **Wrong password:** records `LOGIN_FAILED` in the audit log, and returns the same "Invalid username or password."
5. **Suspended account:** "This account has been suspended. Please contact an administrator."
6. **Any status other than Active:** "This account has not been activated yet."
7. Success: update `LastLogin`, record `LOGIN` in the audit log, return the user.

The password check comes **before** the status checks. That way a stranger cannot learn that an account is suspended without knowing the password.

An unknown username cannot be written to the audit log, because the audit table needs a real user id.

### Password hashing (`PasswordHelper`)
- Uses PBKDF2 with SHA-256, 100,000 iterations, a random 16-byte salt and a 32-byte hash.
- Stored as `PBKDF2$iterations$saltBase64$hashBase64`. The same password gives a different stored value each time because the salt is random.
- `VerifyPassword` recomputes the hash with the stored salt and compares with a fixed-time loop (`AreEqual`) so the comparison time does not reveal where the first difference is.

### Staying signed in (`SessionHelper.SignIn`)
- Issues an encrypted forms-authentication cookie. The ticket's user data holds `userId|role`. The cookie is `HttpOnly`, so scripts cannot read it.
- Also fills the session (user id, username, full name, role).
- If the session times out before the cookie does, `SessionHelper` rebuilds it from the ticket.
- `Web.config`: forms authentication, `loginUrl="~/Modules/Public/Login"`, 60-minute timeout, sliding expiration.

### Role-based access (`Global.asax.cs`)
- `Application_AuthenticateRequest` runs on every request. It reads the role from the cookie and attaches it to the request. That is how `Modules/Tutor/Web.config` can say `<allow roles="Tutor,Admin" /> <deny users="*" />`: anyone who is not a Tutor or Admin is refused and sent to Login with a `ReturnUrl`. The Learner and Admin folders have their own `Web.config`.
- `Application_PostAcquireRequestState` calls `UserManager.IsLoginStillValid` once per page. If an admin has since suspended or deleted the account, or changed its role, the user is signed out and sent to `Login?reason=changed`. A database error does not sign everybody out; the check lets the request through.

### Logout
`Site.Master.cs` (`btnLogout_Click`) records `LOGOUT`, calls `SessionHelper.SignOut()` (clears the cookie and the session) and redirects to Login. The Tutor layout has its own logout button.

---

## 6. Data used

`Users`: `Id`, `FullName`, `Username`, `Email`, `PasswordHash`, `Role` (Learner, Tutor or Admin), `Status` (Active, Suspended and so on), `AvatarUrl`, `JoinedDate`, `LastLogin`.

`TutorProfiles`: `UserId`, `Specialty`, `Qualifications`, `PortfolioUrl`, `Bio`, `CurriculumPitch`, `Status` (Pending, Approved or Rejected), `ApprovedDate`, `RejectionReason`.

`Courses`: the landing page and catalogue only read rows with `Status = 'Published'`.

Table definitions are in `App_Data/Schema.sql`. Test accounts are in `App_Data/Seed.sql`.

---

## 7. Security points worth mentioning

- **Passwords are hashed**, salted and slow (PBKDF2, 100,000 iterations). Plain text is never stored.
- **Same error for a wrong username and a wrong password**, with matching response time.
- **SQL injection:** every query uses parameters.
- **XSS:** page output is HTML-encoded (`<%#: %>`, `Mode="Encode"`, `Server.HtmlEncode`).
- **The role is enforced on the server.** The page's `?role=` is never trusted; anything but Tutor becomes Learner.
- **Validation runs in the browser and again on the server.**
- **Registration is transactional:** the user and the tutor application are saved together or not at all.
- **Draft course ids cannot be probed** from the public details page.
- **Suspended or changed accounts are signed out** on their next page request.

## 8. Known limits

- Featured courses are shuffled in memory (marked `shortcut:` in the code).
- Username and email duplicate checks run only on the server, so they appear after the form is posted.
- Signed-in users are redirected away from every `Modules/Public` page by `Site.Master`, so the learner and signed-in branches in `CourseDetails.aspx.cs` are never reached from the public page.
- The portfolio check validates the shape of the address, not that the website exists.
