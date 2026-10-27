CodeLab: Enrollment-gated lessons and progress bar

Copy these 4 files into their matching paths in the existing CodeLab project:
Modules/Learner/Curriculum.aspx
Modules/Learner/Curriculum.aspx.cs
Modules/Learner/Curriculum.aspx.designer.cs
Content/LearnerCurriculum.css

This patch builds on the previous Combined Curriculum patch, which already
includes enrollment and mark-complete functionality and depends on the existing
BLL/LearnerCourseManager.cs and DAL/LearnerCourseRepository.cs.

Behavior:
- Before enrollment: course outline visible, lesson content hidden server-side,
  Enrol button shown, no progress bar.
- After enrollment: expandable lesson content and code samples, completion
  buttons, Unenrol button, lesson progress bar and percentage.
- Completing/uncompleting lessons updates existing LessonCompletions and
  CourseEnrollments tables through LearnerCourseManager; bar recomputes from
  database each request and remains after refresh.
- Progress represents LESSONS ONLY, not quizzes, assessments, or final grade.
- Unenrol removes lesson completions (existing repository behavior).
- No SQL migration required if prior curriculum migration already ran.
- No changes to CodeLab.csproj, Site.Master, Dashboard or other files.

Rebuild and test: open a course, enrol, expand a lesson, mark complete,
refresh, verify percentage. Test two lessons = 33% if six lessons total.

Not compiled in Visual Studio or tested against live LocalDB.
