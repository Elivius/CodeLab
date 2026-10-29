# Tutor module: differences from the prototype

The Tutor module follows the business logic of the React prototype (`codelab_prototype`).
This file lists every place where it does something different, and why, so the team can check back.

"Decided by" says who made the call:

- **Edwin**: Edwin asked for it.
- **Built-in**: added while building and accepted by Edwin. It is open to change if the team disagrees.

Last updated: 22 Oct 2026.

## 1. Course status and editing

| # | Prototype | CodeLab | Why | Decided by |
|---|---|---|---|---|
| 1.1 | Lessons, quizzes and challenges can be added, edited and deleted on **any** course, including Published and Locked ones. | Course details and lessons can be changed **only while the course is a Draft**. Pending Review, Published and Locked courses are read-only for the tutor. | An admin approved a specific version. Changing it afterwards means learners see content nobody reviewed. | Built-in |
| 1.2 | "Submit for review" works on a Draft with no lessons. | A course needs **at least one lesson** before it can be submitted. | Stops empty courses reaching the admin queue. | Built-in |
| 1.3 | Course title, description, category and difficulty can only be edited inside the guided builder session. There is no way back to edit them later. | They can be edited any time while the course is a Draft, from the Course builder page. | Tutors need to fix details after an admin asks for changes. | Built-in |
| 1.4 | Rejection: course goes back to Draft with the admin's reason; resubmitting clears it. | **Same.** No separate "Rejected" status. | Matches the prototype. | Edwin |

The status check runs in the pages, in the business layer (`CourseManager`, `LessonManager`) and in the SQL (`AND Status = @Draft`). There is no extra database column; it uses the existing `Courses.Status`.

## 2. Who can author

| # | Prototype | CodeLab | Why | Decided by |
|---|---|---|---|---|
| 2.1 | The guided course builder opens for any tutor, whatever their application status. | Creating or changing courses and lessons needs an **Approved** tutor application. Unapproved tutors see a locked page linking to their application. | The proposal says tutors are vetted by an admin before teaching. | Built-in |
| 2.2 | Only tutors author. | **Admins** can also use the Tutor pages (treated as approved). | Lets an admin fix content. | Built-in |
| 2.3 | "Update Teaching Credentials" re-submits the application and sets the tutor back to **Pending**. | Editing the profile (name, bio, specialty, portfolio, photo) does **not** need re-approval. | Edwin asked for this. | Edwin |

## 3. Course fields

| # | Prototype | CodeLab | Why | Decided by |
|---|---|---|---|---|
| 3.1 | Description must be at least 15 characters. | Description is required; no minimum length. Title max 200 and description max 4,000 characters. | Max lengths match the database columns. | Built-in |
| 3.2 | Categories: Frontend Engineering, Backend Systems, Computer Science, Cloud Architecture, Systems & WebAssembly, Programming Fundamentals. | Categories: Web Development, Frontend Engineering, Backend Development, Programming Fundamentals, Computer Science, Databases & SQL, Python, C# & .NET, Mobile Development, Data Science (`Models/CourseOptions.cs`). | The list was rewritten to fit the course topics in the seed data. **Agree the final list with the team.** | Built-in |
| 3.3 | Course cover image exists only in mock data; there is no upload. | Tutors can **upload a cover** (JPG, PNG or WebP, max 2 MB, checked by file signature). Uses the existing `Courses.ThumbnailUrl` column. | Course cards need a picture. | Built-in |

## 4. Lessons

| # | Prototype | CodeLab | Why | Decided by |
|---|---|---|---|---|
| 4.1 | Attachments are just a typed file name (nothing is uploaded). | **Real file upload**: PDF, ZIP, TXT, DOCX, PPTX, XLSX or images, max 5 MB, one per lesson. Needed new columns `Lessons.AttachmentUrl` and `Lessons.AttachmentName`. | The prototype could only fake it. | Edwin (schema approved) |
| 4.2 | No video field in the lesson editor (the type has `videoUrl`, but it is unused). | Optional **YouTube link**, embedded in the lesson. Uses the existing `Lessons.VideoUrl` column. | Edwin asked for it. | Edwin |
| 4.3 | Lesson **thumbnail** image and **duration (minutes)** fields. | **Not in the lesson form yet.** The columns exist (`Lessons.ThumbnailUrl`, `Lessons.DurationMinutes`, default 15), so they can be added later without a schema change. | Kept the first version of the form small. | Built-in |
| 4.4 | Lesson content is a plain textarea (the type's comment mentions Markdown, but nothing renders it). | Content is **Markdown**, with a formatting toolbar and a **Preview** that shows the lesson as learners see it (video, text and highlighted code). The Markdown is turned into safe HTML on the server. | Easier to write structured lessons. | Edwin |
| 4.5 | Code languages: TypeScript, JavaScript, Python, Go, Rust, SQL. | JavaScript, TypeScript, Python, C#, SQL, HTML, CSS, Java, C++, PHP, JSON, Bash (`Models/LessonOptions.cs`). No Go or Rust. | Fits the course topics. **Agree the final list with the team.** | Built-in |
| 4.6 | The lesson form has an order number box; a reorder function exists in the store but is never used in the UI. | **No order box.** A new lesson is always added after the last one, and an edited lesson keeps its place. Lessons cannot be reordered. Deleting a lesson closes the gap (1, 2, 3 and so on). | Simpler for tutors. | Edwin |

## 5. Quizzes and coding challenges

Built on the Quiz page (`QuizEditor`) and the Coding challenge page (`ChallengeEditor`).

| # | Prototype | CodeLab | Decided by |
|---|---|---|---|
| 5.1 | A quiz belongs to a course (optionally a lesson). | Same: `Quizzes.CourseId`. Note this differs from the proposal's line "Quiz must be linked to an existing lesson"; mention it in the documentation. | Edwin |
| 5.2 | Pass mark as a percentage. | `PassMark` 0 to 100 (default 80). | Edwin |
| 5.3 | A challenge has several test cases and a fake sandbox. | A single `ExpectedOutput` for now. How learner code is run is **not decided yet**. | Team (open) |
| 5.4 | The guided builder creates **one** quiz and **one** challenge, but the course page and the data model allow **many** of each. | **One quiz (holding all its questions) and one coding challenge per course.** Enforced in `QuizManager` / `ChallengeManager` and in the insert SQL. The schema itself still allows many, so this can be changed later without a database change. | Edwin |
| 5.5 | The quiz and challenge are optional; a course can be submitted without them. | Same: both optional. | Edwin |
| 5.6 | No rule about empty quizzes. | A quiz needs **at least one question** before the course can be submitted (proposal: "quiz must have ≥1 question before publishing"). A quiz with no questions blocks Submit until a question is added or the quiz is deleted. | Proposal |
| 5.7 | Question types: Multiple Choice and True/False; options in a list with a correct index. | Same. Multiple Choice has 2 to 4 options, and each must be different. True/False always uses "True" and "False". The explanation is optional. New questions go at the end; questions are not reordered. | Built-in |
| 5.8 | Quiz and challenge can be changed on any course. | Only while the course is a **Draft** (same rule as lessons, item 1.1). | Built-in |
| 5.9 | Challenge languages: TypeScript, JavaScript, Python, Go, Rust. | Uses the same language list as lesson code (item 4.5). **Agree the list with the team once the code runner is decided.** | Built-in |
| 5.10 | — | The challenge also stores `Difficulty` (defaults to the course's) and `PointsReward` (default 50, 0 to 1000), because the schema has these columns. | Built-in |

## 6. Layout and look only

These change how pages look, not what they do:

- The prototype shows the tutor area as tabs on one page. CodeLab uses separate pages (Dashboard, Course builder, Lessons, Profile) with a sidebar.
- The prototype's guided builder is one long 5-step wizard that saves everything at the end. CodeLab saves each step on its own page (course details first, then lessons), with a progress bar linking the steps.
- A profile dropdown in the top bar with My profile and Sign out.
