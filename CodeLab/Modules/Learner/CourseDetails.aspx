<%@ Page Title="Course Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CourseDetails.aspx.cs" Inherits="CodeLab.Modules.Learner.CourseDetails" %>
<asp:Content ID="DetailHead" ContentPlaceHolderID="HeadContent" runat="server"><link rel="stylesheet" href="<%= ResolveUrl("~/Content/LearnerCourses.css") %>" /></asp:Content>
<asp:Content ID="DetailMain" ContentPlaceHolderID="MainContent" runat="server">
<main class="lc-wrap lc-detail">
  <a class="lc-back" href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">&larr; Back to Courses &amp; Curriculum</a>
  <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="lc-message" />
  <asp:Panel ID="pnlCourse" runat="server">
    <p><a class="lc-back" href='<%= ResolveUrl("~/Modules/Learner/Curriculum.aspx?id=" + Server.UrlEncode(Request.QueryString["id"])) %>'>View structured curriculum</a></p>
    <header class="lc-detail-hero">
      <div><span class="lc-category"><asp:Literal ID="litCategory" runat="server" /></span>
      <span class="lc-difficulty"><asp:Literal ID="litDifficulty" runat="server" /></span>
      <h1><asp:Literal ID="litTitle" runat="server" /></h1>
      <p><asp:Literal ID="litDescription" runat="server" /></p>
      <p class="lc-instructor">Instructor: <asp:Literal ID="litInstructor" runat="server" /> &bull; <asp:Literal ID="litLessonCount" runat="server" /> lessons</p>
      <asp:Panel ID="pnlProgress" runat="server" CssClass="lc-card-progress">
        <div class="lc-progress-label"><span>Your progress</span><b><asp:Literal ID="litProgress" runat="server" />%</b></div>
        <div class="lc-progress"><span id="progressBar" runat="server"></span></div>
      </asp:Panel>
      <asp:Button ID="btnEnroll" runat="server" Text="Enrol in Course" CssClass="lc-cta lc-action" OnClick="btnEnroll_Click" />
      <asp:Button ID="btnUnenroll" runat="server" Text="Unenrol" CssClass="lc-secondary lc-action" OnClick="btnUnenroll_Click" OnClientClick="return confirm('Unenrol from this course? Your lesson completion history for this course will be removed.');" />
      </div>
    </header>
    <section class="lc-lessons">
      <h2>Course Curriculum</h2>
      <asp:Repeater ID="rptLessons" runat="server" OnItemCommand="rptLessons_ItemCommand">
        <ItemTemplate>
          <article class="lc-lesson" id='lesson-<%# Eval("Id") %>'>
            <div class="lc-lesson-top">
              <div><span class="lc-lesson-index">Lesson <%# Container.ItemIndex + 1 %></span><h3><%#: Eval("Title") %></h3>
              <p><%# Eval("DurationMinutes") %> min &bull; <%#: Eval("CodeLanguage") %></p></div>
              <span class="lc-lesson-status"><%# (bool)Eval("IsCompleted") ? "Completed" : "Not completed" %></span>
            </div>
            <asp:Panel runat="server" Visible='<%# IsEnrolled %>'>
              <details class="lc-lesson-content"><summary>Study Lesson</summary>
                <p class="lc-lesson-text"><%#: Eval("Content") %></p>
                <asp:Panel runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("CodeSnippet"))) %>'><pre><code><%#: Eval("CodeSnippet") %></code></pre></asp:Panel>
                <asp:HyperLink runat="server" Visible='<%# SafeVideoUrl(Eval("VideoUrl")) != "" %>' NavigateUrl='<%# SafeVideoUrl(Eval("VideoUrl")) %>' Target="_blank" Text="Open lesson video" />
              </details>
              <asp:LinkButton runat="server" CssClass="lc-lesson-button" CommandName="ToggleLesson" CommandArgument='<%# Eval("Id") %>' CausesValidation="false"
                Text='<%# (bool)Eval("IsCompleted") ? "Mark as Incomplete" : "Mark as Completed" %>' />
            </asp:Panel>
            <asp:Panel runat="server" Visible='<%# !IsEnrolled %>' CssClass="lc-lesson-locked">Enrol to access this lesson.</asp:Panel>
          </article>
        </ItemTemplate>
      </asp:Repeater>
      <asp:Panel ID="pnlNoLessons" runat="server" Visible="false" CssClass="lc-empty">No lessons have been published for this course yet.</asp:Panel>
    </section>
  </asp:Panel>
</main>
<% if(ShowCompletion){ %>
<style>
.course-done{width:min(480px,calc(100% - 32px));padding:36px 28px;border:1px solid #ffdce7;border-radius:24px;background:var(--cl-surface,#fff);color:var(--cl-text,#0f172a);text-align:center;box-shadow:0 20px 70px #34142633}.course-done::backdrop{background:#0f172a66;backdrop-filter:blur(4px)}.course-done-close{position:absolute;right:14px;top:10px;border:0;background:transparent;color:inherit;font-size:26px;width:32px;height:32px;cursor:pointer}.course-done-icon{display:grid;place-items:center;width:64px;height:64px;margin:0 auto 18px;background:#fff0f5;color:#c9527d;border-radius:50%}.course-done h2{font-size:24px;margin:0 0 14px}.course-done p{line-height:1.6;color:#64748b}.course-done-actions{display:flex;flex-direction:column;gap:10px;margin-top:24px}.course-done-actions a,.course-done-actions button{padding:12px 16px;border-radius:12px;font:inherit;font-weight:600;text-decoration:none}.course-done-actions a{color:#fff;background:linear-gradient(90deg,#b93b69,#d64578)}.course-done-actions button{border:1px solid #e2e8f0;background:transparent;color:inherit}.course-done :focus-visible{outline:3px solid #c9527d;outline-offset:3px}
</style>
<dialog id="courseCompletedDialog" class="course-done" aria-labelledby="courseCompletedTitle" aria-describedby="courseCompletedMessage">
<button type="button" class="course-done-close" aria-label="Close congratulations" onclick="document.getElementById('courseCompletedDialog').close()">&times;</button>
<div class="course-done-icon"><svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path d="M7 3h10v7a5 5 0 0 1-10 0V3ZM7 5H3v3a4 4 0 0 0 4 4M17 5h4v3a4 4 0 0 1-4 4M12 15v5M8 21h8"/></svg></div>
<h2 id="courseCompletedTitle">Congratulations!</h2><p id="courseCompletedMessage">You've completed <strong><%: CompletedCourseTitle %></strong>. Put what you've learned into practice with this course's coding challenges.</p>
<div class="course-done-actions"><a href="<%: ChallengeUrl %>" autofocus>Go to Coding Challenges &rarr;</a><button type="button" onclick="document.getElementById('courseCompletedDialog').close()">Back to Course</button></div>
</dialog>
<script>document.getElementById('courseCompletedDialog').showModal();</script>
<% } %>
</asp:Content>
