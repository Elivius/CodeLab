COMBINED COURSE CURRICULUM — CODELAB WEB FORMS

This patch combines course details and curriculum into ONE learner page.
The catalogue now opens Curriculum.aspx?id=<courseId> directly.
Curriculum.aspx displays modules, expandable lesson content, code samples,
lesson completion controls, module quizzes and mid-term/final assessment cards.
Learners can enrol and unenrol without visiting CourseDetails.aspx.

INSTALL
1. Back up your current CodeLab project.
2. Copy these files to matching relative folders inside CodeLab.
3. DO NOT overwrite CodeLab.csproj, Dashboard or Site.Master.
4. Confirm all included files appear in Visual Studio Solution Explorer.
   Existing LearnerCourseManager, LearnerCourseRepository and LearnerCourse
   model from the prior patch must still be present and included in project.
5. Rebuild and log in as Learner. Open Modules/Learner/Courses.aspx.
6. Select a course. You should see the combined curriculum.
7. Test enrol, expand lesson, mark completed, refresh and verify progress.

NO DATABASE MIGRATION REQUIRED if the earlier migration and seed succeeded.
CourseDetails.aspx may remain for old links; the catalogue no longer links to it.
You can remove the legacy page later after checking for other links to it.

NOT IMPLEMENTED YET
- Actual quiz/assessment attempt interface, grading, timers and attempt limits.
- Quiz/assessment-based final course completion calculations.
- Full Visual Studio and LocalDB integration testing.

If you see a CS0246 error, check that your original BLL, DAL and Models
files remain included in CodeLab.csproj. Do not replace your project file.
