<%@ Page Title="Course Curriculum" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Curriculum.aspx.cs" Inherits="CodeLab.Modules.Learner.Curriculum" %>
<asp:Content ID="CurriculumHead" ContentPlaceHolderID="HeadContent" runat="server">
<link rel="stylesheet" href="<%= ResolveUrl("~/Content/LearnerCurriculum.css") %>" />
</asp:Content>
<asp:Content ID="CurriculumMain" ContentPlaceHolderID="MainContent" runat="server">
<main class="cc-wrap">
 <a class="cc-back" href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">&larr; Courses &amp; Curriculum</a>
 <asp:Label ID="lblError" runat="server" Visible="false" CssClass="cc-error" />
 <asp:Panel ID="pnlCurriculum" runat="server">
  <header class="cc-hero"><span class="cc-eyebrow">COURSE CURRICULUM</span><h1><asp:Literal ID="litCourse" runat="server" /></h1><p>Learn each topic, complete its module quiz, and review your mid-term and final assessments.</p><div class="cc-hero-actions"><asp:Button ID="btnEnroll" runat="server" Text="Enrol in Course" CssClass="cc-primary" OnClick="btnEnroll_Click" /><asp:Button ID="btnUnenroll" runat="server" Text="Unenrol" CssClass="cc-secondary" OnClick="btnUnenroll_Click" OnClientClick="return confirm(&quot;Unenrol? Your lesson completion history for this course will be removed.&quot;);" /><asp:Literal ID="litEnrollment" runat="server" /></div>
   <asp:Panel ID="pnlProgress" runat="server" CssClass="cc-progress" Visible="false">
    <div class="cc-progress-top"><div><strong>Learning progress</strong><small><asp:Literal ID="litProgressDetail" runat="server" /></small></div><span class="cc-progress-value"><asp:Literal ID="litProgressPercent" runat="server" />%</span></div>
    <div class="cc-progress-track" role="progressbar" aria-label="Lesson completion" aria-valuemin="0" aria-valuemax="100"><asp:Literal ID="litProgressFill" runat="server" /></div>
    <span class="cc-progress-caption">Based on completed lessons. Quizzes and assessments will be counted when their attempt system is ready.</span>
   </asp:Panel>
  </header>
  <asp:Repeater ID="rptModules" runat="server" OnItemDataBound="rptModules_ItemDataBound">
   <ItemTemplate>
    <section class="cc-module">
      <div class="cc-module-heading"><div class="cc-number"><%# Eval("OrderIndex") %></div><div><small>MODULE <%# Eval("OrderIndex") %></small><h2><%#: Eval("Title") %></h2><p><%#: Eval("Description") %></p></div></div>
      <div class="cc-module-body">
        <asp:Repeater ID="rptLessons" runat="server" OnItemCommand="rptLessons_ItemCommand"><ItemTemplate>
          <div class="cc-item cc-lesson">
            <span class="cc-item-icon">&#128214;</span>
            <div class="cc-lesson-main">
              <details class="cc-lesson-details">
                <summary><strong><%#: Eval("Title") %></strong><small><%# Eval("DurationMinutes") %> min &bull; <%# (bool)Eval("IsCompleted") ? "Completed" : "Not completed" %></small></summary>
                <asp:Panel runat="server" Visible='<%# IsEnrolled %>' CssClass="cc-lesson-expanded">
                  <p class="cc-lesson-content"><%#: Eval("Content") %></p>
                  <asp:Panel runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("CodeSnippet"))) %>'>
                    <pre><code><%#: Eval("CodeSnippet") %></code></pre>
                  </asp:Panel>
                  <asp:HyperLink runat="server" Visible='<%# SafeVideoUrl(Eval("VideoUrl")) != "" %>' NavigateUrl='<%# SafeVideoUrl(Eval("VideoUrl")) %>' Target="_blank" Text="Open lesson video" />
                  <asp:LinkButton runat="server" CssClass="cc-complete" CommandName="ToggleLesson" CommandArgument='<%# Eval("Id") %>' CausesValidation="false" Text='<%# (bool)Eval("IsCompleted") ? "Mark Incomplete" : "Mark Completed" %>' />
                </asp:Panel>
                <asp:Panel runat="server" Visible='<%# !IsEnrolled %>' CssClass="cc-locked">Enrol in this course to access the lesson.</asp:Panel>
              </details>
            </div>
            <span class="cc-status"><%# (bool)Eval("IsCompleted") ? "Completed" : (IsEnrolled ? "In progress" : "Locked") %></span>
          </div>
        </ItemTemplate></asp:Repeater>
        <asp:Repeater ID="rptQuizzes" runat="server"><ItemTemplate>
          <div class="cc-item cc-quiz"><span class="cc-item-icon">&#10003;</span><div><strong><%#: Eval("Title") %></strong><small>Module quiz &bull; Pass mark: <%# Eval("PassMark") %>%</small></div><span class="cc-status">Quiz · <%# Eval("QuestionCount") %> questions</span></div>
        </ItemTemplate></asp:Repeater>
      </div>
    </section>
    <asp:Repeater ID="rptAssessments" runat="server"><ItemTemplate>
      <section class="cc-assessment">
        <span class="cc-exam-icon">&#127891;</span>
        <div><small><%# Convert.ToString(Eval("AssessmentType")) == "Final" ? "CUMULATIVE FINAL" : "MID-TERM" %></small>
        <h2><%#: Eval("Title") %></h2><p><%# Convert.ToString(Eval("AssessmentType")) == "Final" ? "Covers all modules in this course" : "Covers modules completed so far" %></p>
        <span class="cc-meta"><%# Eval("TimeLimitMinutes") %> minutes &bull; <%# Eval("MaxAttempts") %> attempts &bull; Pass mark <%# Eval("PassMark") %>% &bull; <%# Eval("QuestionCount") %> questions</span></div>
        <span class="cc-exam-label">Assessment</span>
      </section>
    </ItemTemplate></asp:Repeater>
   </ItemTemplate>
  </asp:Repeater>
  <asp:Panel ID="pnlNoModules" runat="server" Visible="false" CssClass="cc-empty">No curriculum modules have been added to this course.</asp:Panel>
  <p class="cc-notice">Enrol to unlock lesson content. Expand lessons to study and mark them completed. The progress bar tracks lessons only; quiz and exam attempts are not yet available.</p>
 </asp:Panel>
</main>
</asp:Content>
