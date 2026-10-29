<%@ Page Title="Learner Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="CodeLab.Modules.Learner.Dashboard" %>
<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" href="<%= ResolveUrl("~/Content/LearnerDashboard.css") %>" />
<style>
.jump-grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:24px}.jump-card{background:var(--cl-surface,#fff);border:1px solid #ffe5ec;border-radius:24px;padding:20px;box-shadow:0 2px 3px #00000008;min-width:0}.jump-top{display:flex;gap:16px}.jump-image{position:relative;flex:0 0 112px;height:112px;border-radius:16px;overflow:hidden;background:linear-gradient(135deg,#172033,#c9527d)}.jump-image img{width:100%;height:100%;object-fit:cover}.jump-image span{position:absolute;bottom:0;left:0;right:0;padding:8px;background:#0008;color:#fff;font-size:10px;font-weight:800}.jump-body{min-width:0}.jump-category{background:#ffe5ec;color:#d64578;border-radius:5px;padding:2px 8px;font-size:10px;font-weight:700;text-transform:uppercase}.jump-body h3{font-size:14px;margin:6px 0;font-weight:700}.jump-description{display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;color:#8493a8;font-size:12px;margin:0 0 8px}.jump-meta{font-size:11px;color:#8493a8;margin:0}.jump-meta strong{color:#c9527d}.jump-progress{display:flex;justify-content:space-between;margin-top:16px;padding-top:10px;border-top:1px solid #f1f5f9;font-size:11px;color:#8493a8}.jump-progress strong{color:#d64578}.jump-card progress{display:block;width:100%;height:8px;margin:8px 0 16px;border:0;border-radius:6px;overflow:hidden;accent-color:#d64578}.jump-card progress::-webkit-progress-bar{background:#fff0f5}.jump-card progress::-webkit-progress-value{background:linear-gradient(90deg,#bd386a,#ff9ea6)}.jump-continue{display:block;text-align:center;padding:10px;background:linear-gradient(90deg,#b93b69,#d64578);border-radius:12px;color:white;font-size:12px;font-weight:700;text-decoration:none;box-shadow:0 3px 5px #c9527d22}.jump-continue:hover{color:white;filter:brightness(.95)}[data-theme=dark] .jump-card{background:#0f172a;border-color:#334155}@media(max-width:900px){.jump-grid{grid-template-columns:1fr}}@media(max-width:480px){.jump-image{flex-basis:80px;height:100px}}
</style></asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
<main class="ld-wrap">
  <!-- Local SVG symbol library; no external icon package required. -->
  <svg class="ld-symbols" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
    <symbol id="ld-sparkles" viewBox="0 0 24 24"><path d="m12 3 1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3ZM19 17l.7 1.3L21 19l-1.3.7L19 21l-.7-1.3L17 19l1.3-.7L19 17ZM4 16l.7 1.3L6 18l-1.3.7L4 20l-.7-1.3L2 18l1.3-.7L4 16Z"/></symbol>
    <symbol id="ld-book" viewBox="0 0 24 24"><path d="M12 6c-2-1.7-5.6-2.1-9-1.2v14c3.4-.9 7-.5 9 1.2 2-1.7 5.6-2.1 9-1.2v-14c-3.4-.9-7-.5-9 1.2ZM12 6v14"/></symbol>
    <symbol id="ld-terminal" viewBox="0 0 24 24"><path d="m4 6 6 6-6 6M12 18h8"/></symbol>
    <symbol id="ld-message" viewBox="0 0 24 24"><path d="M5 4h14a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H9l-6 3V6a2 2 0 0 1 2-2Z"/></symbol>
    <symbol id="ld-trophy" viewBox="0 0 24 24"><path d="M7 3h10v7a5 5 0 0 1-10 0V3ZM7 5H3v3a4 4 0 0 0 4 4M17 5h4v3a4 4 0 0 1-4 4M12 15v4M8 21h8M9 19h6"/></symbol>
    <symbol id="ld-user" viewBox="0 0 24 24"><circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/></symbol>
    <symbol id="ld-flame" viewBox="0 0 24 24"><path d="M12 22c4.2 0 7-3.2 7-7.2 0-3.2-1.8-5.7-4-7.8.1 2-1 3.2-2 3.8C12 7.6 11.2 5 9 2 9 7 5 9.3 5 14.8 5 18.8 7.8 22 12 22Z"/></symbol>
    <symbol id="ld-zap" viewBox="0 0 24 24"><path d="m13 2-9 12h7l-1 8 10-13h-7l0-7Z"/></symbol>
    <symbol id="ld-award" viewBox="0 0 24 24"><circle cx="12" cy="8" r="5"/><path d="m8.5 12-2 9L12 18l5.5 3-2-9"/></symbol>
  </svg>
  <nav class="ld-tabs" aria-label="Learner navigation">
    <a class="ld-tab selected" aria-current="page" href="<%= ResolveUrl("~/Modules/Learner/Dashboard.aspx") %>"><svg class="ld-icon"><use href="#ld-sparkles" /></svg>Dashboard</a>
    <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>"><svg class="ld-icon"><use href="#ld-book" /></svg>Courses &amp; Curriculum</a>
    <a class="ld-tab"
        href="<%= ResolveUrl("~/Modules/Learner/Workspace.aspx") %>">
        <svg class="ld-icon" aria-hidden="true">
            <use href="#ld-terminal" />
        </svg>
        CodeLab Workspace
    </a>
    <a class="ld-tab"
        href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
        <svg class="ld-icon" aria-hidden="true">
            <use href="#ld-message" />
        </svg>
        Discussion Forum
    </a>   
    <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Leaderboard") %>"><svg class="ld-icon"><use href="#ld-trophy" /></svg>Leaderboard &amp; Badges</a>
    <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Profile") %>"><svg class="ld-icon"><use href="#ld-user" /></svg>My Profile</a>
    <span class="ld-identity"><span class="ld-avatar"><% if(!string.IsNullOrEmpty(((CodeLab.SiteMaster)Master).LearnerAvatarUrl)){ %><img class="ld-identity-photo" src="<%: ((CodeLab.SiteMaster)Master).LearnerAvatarUrl %>" alt="" referrerpolicy="no-referrer" onerror="this.remove()" /><% } %><asp:Literal ID="litInitial" runat="server" /></span><span class="ld-user-meta"><b><asp:Literal ID="litNavName" runat="server" /></b><small>Level <asp:Literal ID="litLevel" runat="server" /> &bull; <asp:Literal ID="litNavPoints" runat="server" /> XP</small></span></span>
  </nav>

  <asp:Label ID="lblError" runat="server" Visible="false" CssClass="alert alert-warning d-block mt-3" />
  <section class="ld-hero">
    <div class="ld-intro">
      <div class="ld-kickers">
        <span class="ld-pill">LEARNER DASHBOARD</span>
        <span class="ld-streak"><svg class="ld-icon"><use href="#ld-flame" /></svg><asp:Literal ID="litStreak" runat="server" /> Day Active Streak</span>
      </div>
      <h1>Welcome back, <asp:Literal ID="litName" runat="server" />!</h1>
      <p>You are enrolled in <asp:Literal ID="litEnrolledIntro" runat="server" /> accredited courses. Track your curriculum progress, complete lessons, and code in the sandbox.</p>
    </div>
    <div class="ld-hero-stats">
      <div class="ld-mini"><svg class="ld-stat-icon"><use href="#ld-trophy" /></svg><strong><asp:Literal ID="litPoints" runat="server" /></strong><small>POINTS (XP)</small></div>
      <div class="ld-mini"><svg class="ld-stat-icon orange"><use href="#ld-sparkles" /></svg><strong>Lvl <span id="ld-level-copy"></span></strong><small id="ld-level-title">CURRENT LEVEL</small></div>
      <div class="ld-mini"><svg class="ld-stat-icon green"><use href="#ld-award" /></svg><strong><asp:Literal ID="litBadges" runat="server" /></strong><small>BADGES</small></div>
    </div>
  </section>
  <section class="ld-milestone">
    <div class="ld-milestone-top">
      <div class="ld-milestone-icon"><svg class="ld-icon"><use href="#ld-zap" /></svg></div>
      <div><h2>Milestone Progression to Level <asp:Literal ID="litNextLevel" runat="server" /></h2><p id="ld-milestone-description">Progress toward your next level.</p></div>
      <strong id="ld-progress-text"></strong>
    </div>
    <div class="ld-progress"><div id="ld-progress-fill"></div></div>
  </section>
  <section class="ld-shortcuts">
    <a href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>" class="ld-shortcut"><span class="ld-shortcut-icon pink"><svg class="ld-icon"><use href="#ld-book" /></svg></span><h3>My Courses</h3><p><asp:Literal ID="litEnrolled" runat="server" /> enrolled courses</p></a>
    <a href="<%= ResolveUrl("~/Modules/Learner/Workspace.aspx") %>"
        class="ld-shortcut">
        <span class="ld-shortcut-icon purple">
            <svg class="ld-icon" aria-hidden="true">
                <use href="#ld-terminal" />
            </svg>
        </span>
        <h3>CodeLab Workspace</h3>
        <p>Practise coding challenges</p>
    </a>
    <a class="ld-shortcut"
        href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
        <span class="ld-shortcut-icon blue">
            <svg class="ld-icon" aria-hidden="true">
                <use href="#ld-message" />
            </svg>
        </span>
        <h3>Discussion Forum</h3>
        <p>Q&amp;A with tutors &amp; peers</p>
    </a>
     <a class="ld-shortcut" href="<%= ResolveUrl("~/Modules/Learner/Leaderboard") %>"><span class="ld-shortcut-icon amber"><svg class="ld-icon"><use href="#ld-trophy" /></svg></span><h3>Leaderboard &amp; Badges</h3><p><asp:Literal ID="litBadgesShortcut" runat="server" /> badges</p></a>
  </section>
  <section class="ld-jump">
    <div class="ld-section-heading"><h2><svg class="ld-icon"><use href="#ld-terminal" /></svg> Jump Back In</h2><a href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">View all curriculum &rarr;</a></div>
        <% if(ResumeCourses.Length > 0){ %><div class="jump-grid">
    <% for(int i=0;i<Math.Min(2,ResumeCourses.Length);i++){var course=ResumeCourses[i];var thumbnail=ResumeImage(course["ThumbnailUrl"]); %>
    <article class="jump-card"><div class="jump-top"><div class="jump-image"><% if(thumbnail!=""){ %><img src="<%: thumbnail %>" alt="" onerror="this.hidden=true" /><% } %><span>LESSON</span></div><div class="jump-body"><span class="jump-category"><%: course["Category"] %></span><h3><%: course["LessonTitle"] %></h3><p class="jump-description"><%: course["LessonContent"] %></p><p class="jump-meta"><%: course["DurationMinutes"] %> mins · <%: course["Title"] %> · By <strong><%: course["Instructor"] %></strong></p></div></div><div class="jump-progress"><span>Course Progress</span><strong><%: Convert.ToDecimal(course["ProgressPercent"]).ToString("0") %>%</strong></div><progress max="100" value="<%: Convert.ToDecimal(course["ProgressPercent"]).ToString(System.Globalization.CultureInfo.InvariantCulture) %>" aria-label="Course progress"></progress><a class="jump-continue" href='<%= ResolveUrl("~/Modules/Learner/CourseDetails") %>?id=<%: course["Id"] %>#lesson-<%: course["LessonId"] %>'><svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true" style="vertical-align:middle;margin-right:6px;fill:none;stroke:currentColor;stroke-width:1.8"><circle cx="12" cy="12" r="9"/><path d="m10 8 6 4-6 4Z" style="fill:currentColor;stroke:none"/></svg>Continue Lesson</a></article>
    <% } %></div><% }else{ %><div class="ld-empty-card">No unfinished lessons right now. Browse the curriculum to start your next course.</div><% } %>  </section>
  <section class="ld-learning">
    <div class="ld-section-heading"><h2><svg class="ld-icon"><use href="#ld-book" /></svg> Enrolled Courses &amp; Progression (<asp:Literal ID="litEnrolledHeading" runat="server" />)</h2><a href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">Browse All Courses</a></div>
    <div class="ld-course-grid">
      <asp:Repeater ID="rptCourses" runat="server"><ItemTemplate>
        <article class="ld-course">
          <div class="ld-course-top"><span class="ld-course-category"><%#: Eval("Category") %></span><span class="ld-course-difficulty"><%#: Eval("Difficulty") %></span></div>
          <h3><%#: Eval("Title") %></h3>
          <div class="ld-course-progress"><div class="ld-course-progress-label"><span>Course progress</span><b><%# Convert.ToDecimal(Eval("ProgressPercent")).ToString("0") %>%</b></div><div class="ld-progress"><div style='width:<%# Convert.ToDecimal(Eval("ProgressPercent")).ToString("0", System.Globalization.CultureInfo.InvariantCulture) %>%'></div></div></div>
        </article>
      </ItemTemplate></asp:Repeater>
    </div>
    <asp:Panel ID="pnlNoCourses" runat="server" Visible="false" CssClass="ld-empty-card">You haven't enrolled in a course yet. Browse available courses to get started.</asp:Panel>
  </section>
  <div hidden aria-hidden="true">
    <span id="ld-level-source"><asp:Literal ID="litLevelTitle" runat="server" /></span>
    <span id="ld-milestone-source"><asp:Literal ID="litMilestone" runat="server" /></span>
  </div>
</main>
<script>
(function () {
    function val(id) {
        var el = document.getElementById(id);
        return el ? (el.textContent || '').trim() : '';
    }
    var identity = document.querySelector('.ld-identity');
    var match = identity ? identity.textContent.match(/Level\s+(\d+)/) : null;
    var levelCopy = document.getElementById('ld-level-copy');
    if (match && levelCopy) levelCopy.textContent = match[1];
    var title = val('ld-level-source');
    if (title) document.getElementById('ld-level-title').textContent = title;
    var pointsElement = document.querySelector('.ld-mini strong');
    var points = Number(((pointsElement && pointsElement.textContent) || '0').replace(/,/g, ''));
    var goal = Number(val('ld-milestone-source').replace(/,/g, ''));
    if (goal > 0 && isFinite(points)) {
        var pct = Math.max(0, Math.min(100, points / goal * 100));
        document.getElementById('ld-progress-fill').style.width = pct + '%';
        document.getElementById('ld-progress-text').textContent = points.toLocaleString() + ' / ' + goal.toLocaleString() + ' XP (' + Math.round(pct) + '%)';
        document.getElementById('ld-milestone-description').textContent = 'Earn ' + Math.max(0, goal - points).toLocaleString() + ' more XP to reach the next tier.';
    }
})();
</script>
</asp:Content>
