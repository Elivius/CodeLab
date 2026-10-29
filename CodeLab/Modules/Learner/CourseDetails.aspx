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
</asp:Content>
