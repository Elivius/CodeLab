<%@ Page Title="Courses & Curriculum" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Courses.aspx.cs" Inherits="CodeLab.Modules.Learner.Courses" %>
<asp:Content ID="CourseHead" ContentPlaceHolderID="HeadContent" runat="server">
<link rel="stylesheet" href="<%= ResolveUrl("~/Content/LearnerCourses.css") %>" />
</asp:Content>
<asp:Content ID="CourseMain" ContentPlaceHolderID="MainContent" runat="server">
<main class="lc-wrap">
  <nav class="lc-nav" aria-label="Learner navigation">
    <a href="<%= ResolveUrl("~/Modules/Learner/Dashboard.aspx") %>"><span aria-hidden="true">&#10022;</span> Dashboard</a>
    <a class="active" aria-current="page" href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>"><span aria-hidden="true">&#9783;</span> Courses &amp; Curriculum</a>
    <span class="lc-disabled">CodeLab Workspace</span><span class="lc-disabled">Discussion Forum</span>
    <span class="lc-disabled">Leaderboard &amp; Badges</span><span class="lc-disabled">Profile &amp; Security</span>
    <span class="lc-identity"><asp:Literal ID="litIdentity" runat="server" /></span>
  </nav>
  <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="lc-message" />
  <section class="lc-toolbar">
    <div class="lc-switch">
      <asp:LinkButton ID="btnEnrolled" runat="server" CausesValidation="false" OnClick="btnEnrolled_Click" CssClass="lc-switch-button">My Enrolled Courses (<asp:Literal ID="litEnrolledCount" runat="server" />)</asp:LinkButton>
      <asp:LinkButton ID="btnExplore" runat="server" CausesValidation="false" OnClick="btnExplore_Click" CssClass="lc-switch-button">Explore Course Catalogue (<asp:Literal ID="litPublishedCount" runat="server" />)</asp:LinkButton>
    </div>
    <div class="lc-filters">
      <asp:DropDownList ID="ddlCategory" runat="server" CssClass="lc-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" aria-label="Filter by category" />
      <asp:TextBox ID="txtSearch" runat="server" CssClass="lc-search" placeholder="Search courses..." MaxLength="200" />
      <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="lc-search-button" OnClick="Filter_Changed" />
    </div>
  </section>
  <asp:HiddenField ID="hdnMode" runat="server" Value="enrolled" />
  <section class="lc-grid">
    <asp:Repeater ID="rptCourses" runat="server">
      <ItemTemplate>
        <article class="lc-card">
          <div class="lc-thumb">
            <asp:Image ID="imgCourse" runat="server" ImageUrl='<%# CourseImage(Eval("ThumbnailUrl")) %>' AlternateText='<%# Eval("Title") %>' />
            <div class="lc-chips"><span class="lc-category"><%#: Eval("Category") %></span><span class="lc-difficulty"><%#: Eval("Difficulty") %></span></div>
            <asp:Panel runat="server" Visible='<%# Eval("IsCompleted") %>' CssClass="lc-completed">Completed</asp:Panel>
          </div>
          <div class="lc-card-body">
            <h2><%#: Eval("Title") %></h2>
            <p class="lc-description"><%#: Eval("Description") %></p>
            <p class="lc-instructor">Instructor: <%#: Eval("Instructor") %> &bull; <%# Eval("LessonCount") %> lessons</p>
            <asp:Panel runat="server" Visible='<%# Eval("IsEnrolled") %>' CssClass="lc-card-progress">
              <div class="lc-progress-label"><span>Progress</span><b><%# Eval("ProgressPercent","{0:0}") %>%</b></div>
              <div class="lc-progress"><span style='width:<%# Eval("ProgressPercent","{0:0}") %>%'></span></div>
            </asp:Panel>
            <a class="lc-cta" href='<%# ResolveUrl("~/Modules/Learner/Curriculum.aspx?id=" + Eval("Id")) %>'>
              <%# (bool)Eval("IsEnrolled") ? "Open Curriculum" : "View Curriculum &amp; Enrol" %>
            </a>
          </div>
        </article>
      </ItemTemplate>
    </asp:Repeater>
  </section>
  <asp:Panel ID="pnlEmpty" runat="server" CssClass="lc-empty" Visible="false">
    <div class="lc-empty-icon">&#9783;</div>
    <h2>No courses match your filter criteria</h2>
    <p>Try exploring the course catalogue or changing your search filters.</p>
  </asp:Panel>
</main>
</asp:Content>
