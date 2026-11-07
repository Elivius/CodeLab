# Admin Operating Steps

One "Operating Steps" block for each page of the Admin user manual. The page numbers match the diagrams in `CodeLab_Admin_Guidance_Master.drawio`. Page 5.21 is a placeholder and has no steps.

---

## 5.1 Admin Dashboard (Overview)

Operating Steps

Step 1: Read the Platform Figures
Look at the three tiles on the right of the top row. Accounts shows how many accounts are active, split into learners, tutors and admins, and how many joined this month. Courses shows the total number of courses and how many are published, in review, locked and in draft. Database shows its size in MB, the number of tables, how long a test query took and when the last backup was made. Click "Last backup" to open the Database backups page.

Step 2: Act on the Review Banner
The coloured banner on the left of the top row says how many items need your review. Click "Review 2 tutor applications" or "Review 1 course" to go straight to that queue. When nothing is waiting, the banner turns quiet and says "Nothing needs your review".

Step 3: Review Pending Tutor Applications
The "Pending tutor applications" panel lists up to five applicants, oldest first, with their name, specialty and how long ago they applied. Click Review to open an applicant's dossier, or "Review all" to see the full queue.

Step 4: Review Courses That Need Action
The "Course moderation" panel lists courses that are waiting for review or locked, with the tutor, the category and a status badge (Pending Review or Locked). Click Review to open a course, or "All courses" for the full list.

Step 5: Check Recent Users
The "Recent users" panel shows the newest seven accounts with their email address and a role badge (Learner, Tutor or Admin). Click Edit to change an account, or "Manage all users" to open the full directory.

Step 6: Move Between Sections and Sign Out
Use the tab bar to open Overview, Users, Course moderation, Tutor applications, Audit logs or Database backups. A number bubble on Course moderation or Tutor applications shows how many items are waiting. The moon button switches between light and dark mode, and the account menu at the top right is where you sign out.

---

## 5.2 User Management (Users Directory)

Operating Steps

Step 1: Search for an Account
Type a name, username or email address in the search box and click Search. The list shows only the matching accounts, newest first. A Clear link appears next to the box so you can remove the search.

Step 2: Filter by Role and Status
Use the role chips (All, Learners, Tutors, Admins) and the status chips (Any status, Active, Suspended). Each chip shows how many accounts it holds, and the filters work together with your search.

Step 3: Read the Table
Each row shows the user's name, @username and email, their role, whether the account is Active or Suspended, when they joined and when they last logged in ("Never" if they have not logged in yet). Your own account is marked "You".

Step 4: Use the Row Actions
Edit opens the user's page. The next icon suspends an active account or reactivates a suspended one, after a confirmation. The bin deletes an account permanently after a confirmation, but only if the account has no activity. A greyed-out bin means the account has activity, so suspend it instead. Your own row has Edit only.

Step 5: Create a New User
Click "Create user" at the top right to open the create form (see 5.4).

---

## 5.3 User Management (Search With No Results)

Operating Steps

Step 1: Run a Search
Type text that matches nobody and click Search. The text stays in the search box so you can correct it.

Step 2: Read the Empty State
The list is replaced by the message "No users match your search." and every filter chip shows (0).

Step 3: Clear the Search
Click Clear to remove the search and return to the full list, or edit the text and search again.

---

## 5.4 Create User Account

Operating Steps

Step 1: Open the Form
From the Users page, click Create user. The "All users" link at the top left returns to the directory without saving.

Step 2: Enter the Name and Username
The full name needs at least 2 characters, using letters, spaces and . ' - / only. The username needs 3 to 50 letters, numbers or underscores, and it must not already be taken. The username cannot be changed after the account is created.

Step 3: Enter the Email
Enter a valid email address that is not already registered. CodeLab does not send any email to it.

Step 4: Choose the Role
Pick Learner, Tutor or Admin. A new tutor still has to apply to teach from their own Profile page and be approved before they can create courses.

Step 5: Set the Password
Enter a password of 8 to 128 characters with a letter, a number and a symbol, and type it again in Confirm password. Because no email is sent, give the password to the user yourself.

Step 6: Create the Account
Click Create user. The account starts as Active and its page opens with the message "Account created. Give the user their password so they can log in." If something is wrong, a message appears next to the field, for example "That username is already taken."

---

## 5.5 Edit User (Active Account)

Operating Steps

Step 1: Open the Account
Click Edit on the Users page. The Account panel on the right shows the status, when the user joined, their last login and how many courses they teach.

Step 2: Edit the Details
Change the full name or the email. The username is locked. The email must not belong to another account.

Step 3: Change the Role
Pick a different role in the Role box. The user is logged out on their next click and signs in again with the new role. You cannot change your own role, and a tutor who teaches courses cannot change role.

Step 4: Save the Changes
Click Save changes. A green message confirms it and the change is written to the audit log. If nothing changed, the message says "There were no changes to save."

Step 5: Suspend the Account
Click Suspend account and confirm. The user is logged out on their next click and cannot log in until you reactivate the account.

Step 6: Delete the Account
Delete account appears only when the account has no activity, which in practice means an account created by mistake. It asks for a confirmation and then removes the account permanently.

---

## 5.6 Edit User (Suspended Account)

Operating Steps

Step 1: Recognise a Suspended Account
The status badge in the Account panel shows Suspended in red. The user cannot log in. If they try, they see "This account has been suspended. Please contact an administrator."

Step 2: Edit the Details if Needed
You can still change the name, the email or the role and save.

Step 3: Reactivate the Account
Click Reactivate account and confirm. The status returns to Active and the user can log in again.

Step 4: Know Why There Is No Delete
An account with activity cannot be deleted, so the Delete button is not shown and a note explains why.

---

## 5.7 Edit User (Your Own Account)

Operating Steps

Step 1: Open Your Own Account
Click Edit on the row marked "You" in the Users directory.

Step 2: Edit Your Name and Email
Your full name and email can be changed and saved like any other account.

Step 3: Know the Role Is Locked
The Role box is greyed out and says "You cannot change your own role."

Step 4: Know There Is No Suspend or Delete
The Account panel says "This is your account, so you cannot suspend or delete it." This keeps at least one active admin in the system. Ask another admin if your account needs one of these changes.

---

## 5.8 Edit User (Tutor With Courses)

Operating Steps

Step 1: Check the Courses Taught
The Account panel shows how many courses the tutor teaches, for example "4 taught".

Step 2: Understand the Role Rule
The note under Role says "This role cannot change while the tutor teaches courses." If you save a different role, the change is refused and the message says the courses would be left without a tutor and must be deleted first.

Step 3: Understand Why There Is No Delete
A tutor with courses has activity, so the account cannot be deleted and the note says to suspend it instead.

Step 4: Suspend if Needed
Click Suspend account and confirm. The tutor is logged out on their next click and cannot log in.

---

## 5.9 Course Moderation (Courses Directory)

Operating Steps

Step 1: Filter by Status
Use the chips All, Pending, Published, Locked and Draft. Each chip shows how many courses it holds.

Step 2: Search for a Course
Search by course title, tutor name or tutor email, then click Search. Clear removes the search.

Step 3: Read the Table
Each row shows the title with its difficulty and lesson count, the tutor and their email, the category, a status badge and the number of enrolled students. A locked course shows its lock reason under the badge, and a draft that was sent back shows your feedback.

Step 4: Use the Row Actions
Review opens a course that is waiting for review, and View opens any other course. The green tick approves and publishes a pending course after a confirmation. The lock icon on a published course opens its review page, where you type a reason. The open-lock icon on a locked course unlocks it after a confirmation. The bin deletes the course after a confirmation that says how many enrolled students lose access. A draft has View and the bin only.

---

## 5.10 Course Moderation (Locked Courses)

Operating Steps

Step 1: Open the Locked Filter
Click Locked. Only locked courses are listed.

Step 2: Read the Lock Reason
The red Locked badge has "Reason: ..." under it. Learners cannot open a locked course, and it does not appear in the learner catalog.

Step 3: Inspect or Unlock
View opens the course. The open-lock icon unlocks it after a confirmation, and the course is published again.

---

## 5.11 Course Review (Pending Review)

Operating Steps

Step 1: Open the Course
Open a pending course from the Overview, from the "Review 1 course" button or from the Courses list. The badge says Pending Review, and "Updated" shows when the tutor submitted it.

Step 2: Review the Content
Read the description, open each lesson, check the quiz questions and their correct answers, and read the coding challenge.

Step 3: Approve the Course
Click "Approve and publish" and confirm. The course is published at once and learners can enrol straight away.

Step 4: Send the Course Back
To ask for changes, type your feedback in "Feedback for the tutor", using 10 to 500 characters, and click "Send back to tutor". The course returns to Draft, and the tutor reads your feedback in the Course builder, edits the course and submits it again.

Step 5: Delete the Course if Needed
Delete course removes the course permanently. Use it only when the course should not be resubmitted.

---

## 5.12 Course Review (Published Course)

Operating Steps

Step 1: Open the Course
Click View on a course in the list. The "All courses" link at the top returns to the list without changing anything.

Step 2: Read the Course Details
Check the cover, title, category, difficulty and status. The grey box shows the tutor, their email, how many students are enrolled and when the course was last updated. The description is below it.

Step 3: Check the Content
Open each lesson to see whether it has a video, an attachment or code. The quiz lists every question with the correct answer marked, and the coding challenge shows its description, starter code and expected output.

Step 4: Lock the Course if It Breaks the Rules
In the Decision panel, type a reason of 10 to 500 characters and click Lock course. Learners can no longer open the course, and the tutor sees your reason.

Step 5: Delete the Course if Needed
Click Delete course and read the confirmation, which says how many students lose access. Deleting removes the course with its lessons, quiz, challenge and discussions, and deletes its uploaded images and attachments.

---

## 5.13 Course Review (Locked Course)

Operating Steps

Step 1: Read Why the Course Is Locked
The Decision panel says the course is locked, so learners cannot open it, and shows "Locked because:" followed by the reason you or another admin gave.

Step 2: Review the Content
The lessons, quiz and coding challenge are shown on the left in the same way as for any other course.

Step 3: Unlock the Course
Click "Unlock and publish again" and confirm. The course becomes Published and learners can open it again.

Step 4: Delete the Course if Needed
Delete course works in any status, with the same confirmation as before.

---

## 5.14 Course Review (Draft Course)

Operating Steps

Step 1: Recognise a Draft
The badge says Draft, and the Decision panel says "The tutor is still working on this draft. It comes back here when they submit it."

Step 2: Read What Exists So Far
You can open the lessons, quiz and coding challenge that the tutor has written so far.

Step 3: Know the Available Action
The only action is Delete course. Approving and locking are not offered until the tutor submits the course.

Step 4: Recognise a Course You Sent Back
If you sent this course back earlier, the panel also shows "Sent back because:" with your feedback.

---

## 5.15 Tutor Applications

Operating Steps

Step 1: Choose a Queue
Pending is shown first. Switch to Approved, Rejected or All with the chips. Each chip shows how many applications it holds.

Step 2: Read the Applications
Each entry shows the applicant's name, a status badge (Pending, Approved or Rejected), how long ago they applied, their specialty and a short summary of what they want to teach.

Step 3: Open the Dossier
Click Review application to read the full details and make a decision.

Step 4: Decide From the List
On a pending application, Approve approves the tutor after a confirmation, without opening the dossier. Reject opens the dossier, because a rejection needs a written reason.

---

## 5.16 Tutor Application Review

Operating Steps

Step 1: Read the Applicant Details
The top of the dossier shows the applicant's name, specialty and status. Below it are their username, email, when they applied and a portfolio link. The link opens in a new tab, and only web addresses (http or https) can be clicked.

Step 2: Read the Application
Read the About text, the Qualifications and the Sample curriculum pitch to decide whether the applicant is ready to teach.

Step 3: Approve the Applicant
Click Approve tutor and confirm. The tutor can create courses straight away, and the approval is written to the audit log.

Step 4: Reject the Applicant
Type the reason in "Reason for rejecting", using 10 to 500 characters, and click Reject application. The applicant sees the reason on their Profile page, can update their details and can apply again. CodeLab does not send them an email.

---

## 5.17 Tutor Application (Already Decided)

Operating Steps

Step 1: Open a Decided Application
Open an application with the Approved or Rejected filter on the Tutor applications page.

Step 2: Read the Decision
The Decision card shows the result, for example "Approved on 8 Sep 2026. This tutor can create courses." A rejected application shows "Rejected" with the reason you gave and says the applicant can update their details and resubmit.

Step 3: Know That There Are No Buttons
Approve and Reject appear only on a pending application. If another admin decides first, you see the message "This application has already been approved" (or rejected).

---

## 5.18 Audit Logs

Operating Steps

Step 1: Open the Audit Trail
The page lists every recorded activity, newest first. Each row shows the time (as "x minutes ago" and the exact date), the user, the action code, a category badge and the details.

Step 2: Filter by Category
Use the chips All events, Security, Administrative, Content and Deletion. The category is worked out from the action name, for example LOGIN and SUSPEND_USER are Security, APPROVE_COURSE is Administrative and CREATE_COURSE is Content.

Step 3: Search the Log
Type a user, an action or a word from the details and click Search. For example, search "LOGIN_FAILED" to find wrong-password attempts. Clear removes the search.

Step 4: Move Between Pages
Each page shows 25 events. The line below the table says, for example, "Page 1 of 2 (37 events)", and Previous and Next move between pages.

---

## 5.19 Audit Logs (No Results)

Operating Steps

Step 1: Run a Search
Type a word that matches nothing and click Search. Your text stays in the box.

Step 2: Read the Empty Result
The table is replaced by "No activity matches this search." and the line below says "Page 1 of 1 (0 events)".

Step 3: Clear or Change the Search
Click Clear to remove the search text, which keeps the category filter you chose. You can also pick a different category or search for something else.

---

## 5.20 Database Backups

Operating Steps

Step 1: Run a Backup
Click "Run backup now". CodeLab saves a full copy of the database as a .bak file and writes a SHA-256 checksum next to it. A green message confirms the file name.

Step 2: Read the List of Backups
"Stored backups" lists the newest first, with the file name, its size, when it was created and its SHA-256 checksum. The date and time in the file name are in UTC.

Step 3: Download a Backup
Click Download on a row to save the .bak file. Each download is written to the audit log.

Step 4: Check the Download
To confirm a download is intact, calculate its SHA-256 (in PowerShell, `Get-FileHash <file> -Algorithm SHA256`) and compare it with the value in the table.

Step 5: Keep Backups Private
A backup holds every account's data, including password hashes, so store downloads safely. There is no restore button here, because restoring would replace the database that everyone is using.
