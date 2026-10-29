<%@ Page Title="Courses & Curriculum"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Courses.aspx.cs"
    Inherits="CodeLab.Modules.Learner.Courses" %>

<asp:Content ID="CourseHead"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <link rel="stylesheet"
        href="<%= ResolveUrl("~/Content/LearnerCourses.css") %>?v=3" />

    <link rel="stylesheet"
        href="<%= ResolveUrl("~/Content/LearnerDashboard.css") %>?v=3" />

</asp:Content>

<asp:Content ID="CourseMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="lc-wrap ld-wrap">

        <%-- Same navigation icons as the Dashboard --%>

        <svg class="ld-symbols"
            xmlns="http://www.w3.org/2000/svg"
            aria-hidden="true"
            focusable="false">

            <symbol id="lc-nav-sparkles" viewBox="0 0 24 24">
                <path d="m12 3 1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3ZM19 17l.7 1.3L21 19l-1.3.7L19 21l-.7-1.3L17 19l1.3-.7L19 17ZM4 16l.7 1.3L6 18l-1.3.7L4 20l-.7-1.3L2 18l1.3-.7L4 16Z" />
            </symbol>

            <symbol id="lc-nav-book" viewBox="0 0 24 24">
                <path d="M12 6c-2-1.7-5.6-2.1-9-1.2v14c3.4-.9 7-.5 9 1.2 2-1.7 5.6-2.1 9-1.2v-14c-3.4-.9-7-.5-9 1.2ZM12 6v14" />
            </symbol>

            <symbol id="lc-nav-terminal" viewBox="0 0 24 24">
                <path d="m4 6 6 6-6 6M12 18h8" />
            </symbol>

            <symbol id="lc-nav-message" viewBox="0 0 24 24">
                <path d="M5 4h14a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H9l-6 3V6a2 2 0 0 1 2-2Z" />
            </symbol>

            <symbol id="lc-nav-trophy" viewBox="0 0 24 24">
                <path d="M7 3h10v7a5 5 0 0 1-10 0V3ZM7 5H3v3a4 4 0 0 0 4 4M17 5h4v3a4 4 0 0 1-4 4M12 15v4M8 21h8M9 19h6" />
            </symbol>

            <symbol id="lc-nav-user" viewBox="0 0 24 24">
                <circle cx="12" cy="8" r="4" />
                <path d="M4 21a8 8 0 0 1 16 0" />
            </symbol>

        </svg>

        <%-- Learner navigation --%>

        <nav class="ld-tabs" aria-label="Learner navigation">

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Dashboard.aspx") %>">

                <svg class="ld-icon"
                    aria-hidden="true"
                    focusable="false">
                    <use href="#lc-nav-sparkles" />
                </svg>

                Dashboard
            </a>

            <a class="ld-tab selected"
                aria-current="page"
                href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">

                <svg class="ld-icon"
                    aria-hidden="true"
                    focusable="false">
                    <use href="#lc-nav-book" />
                </svg>

                Courses &amp; Curriculum
            </a>

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Workspace.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lc-nav-terminal" />
                </svg>
                CodeLab Workspace
            </a>

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                <svg class="ld-icon"
                    aria-hidden="true"
                    focusable="false">
                    <use href="#lc-nav-message" />
                </svg>
                Discussion Forum
            </a>

            <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Leaderboard") %>">

                <svg class="ld-icon"
                    aria-hidden="true"
                    focusable="false">
                    <use href="#lc-nav-trophy" />
                </svg>

                Leaderboard &amp; Badges
            </a>

            <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Profile") %>">

                <svg class="ld-icon"
                    aria-hidden="true"
                    focusable="false">
                    <use href="#lc-nav-user" />
                </svg>

                My Profile
            </a>

            <span class="ld-identity">

                <span class="ld-avatar">
                    <asp:Literal ID="litNavInitial"
                        runat="server"
                        Mode="Encode" />
                </span>

                <span class="ld-user-meta">

                    <b>
                        <asp:Literal ID="litIdentity"
                            runat="server" />
                    </b>

                    <small>
                        Level
                        <asp:Literal ID="litNavLevel"
                            runat="server"
                            Mode="Encode" />

                        &bull;

                        <asp:Literal ID="litNavPoints"
                            runat="server"
                            Mode="Encode" />
                        XP
                    </small>

                </span>

            </span>

        </nav>

        <asp:Label ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="lc-message" />

        <%-- Course filters --%>

        <section class="lc-toolbar">

            <div class="lc-switch">

                <asp:LinkButton ID="btnEnrolled"
                    runat="server"
                    Text="My Enrolled Courses"
                    CausesValidation="false"
                    OnClick="btnEnrolled_Click"
                    CssClass="lc-switch-button" />

                <asp:LinkButton ID="btnExplore"
                    runat="server"
                    Text="Explore Course Catalogue"
                    CausesValidation="false"
                    OnClick="btnExplore_Click"
                    CssClass="lc-switch-button" />

            </div>
            <div class="lc-filters">

                <asp:DropDownList ID="ddlCategory"
                    runat="server"
                    CssClass="lc-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="Filter_Changed"
                    aria-label="Filter by category" />

                <asp:TextBox ID="txtSearch"
                    runat="server"
                    CssClass="lc-search"
                    placeholder="Search courses..."
                    aria-label="Search courses"
                    MaxLength="200" />

                <asp:Button ID="btnSearch"
                    runat="server"
                    Text="Search"
                    CssClass="lc-search-button"
                    OnClick="Filter_Changed" />

            </div>

        </section>

        <asp:HiddenField ID="hdnMode"
            runat="server"
            Value="enrolled" />

        <%-- Course cards --%>

        <section class="lc-grid">

            <asp:Repeater ID="rptCourses" runat="server">

                <ItemTemplate>

                    <article class="lc-card">

                        <div class="lc-thumb">

                            <asp:Image ID="imgCourse"
                                runat="server"
                                ImageUrl='<%# CourseImage(Eval("ThumbnailUrl")) %>'
                                AlternateText='<%# Eval("Title") %>' />

                            <div class="lc-chips">

                                <span class="lc-category">
                                    <%#: Eval("Category") %>
                                </span>

                                <span class="lc-difficulty">
                                    <%#: Eval("Difficulty") %>
                                </span>

                            </div>

                            <asp:Panel runat="server"
                                Visible='<%# Eval("IsCompleted") %>'
                                CssClass="lc-completed">

                                Completed

                            </asp:Panel>

                        </div>

                        <div class="lc-card-body">

                            <h2><%#: Eval("Title") %></h2>

                            <p class="lc-description">
                                <%#: Eval("Description") %>
                            </p>

                            <p class="lc-instructor">
                                Instructor: <%#: Eval("Instructor") %>
                                &bull;
                                <%# Eval("LessonCount") %> lessons
                            </p>

                            <asp:Panel runat="server"
                                Visible='<%# Eval("IsEnrolled") %>'
                                CssClass="lc-card-progress">

                                <div class="lc-progress-label">

                                    <span>Progress</span>

                                    <b>
                                        <%# Eval("ProgressPercent", "{0:0}") %>%
                                    </b>

                                </div>

                                <div class="lc-progress">

                                    <span style='width:<%# Eval("ProgressPercent", "{0:0}") %>%'></span>

                                </div>

                            </asp:Panel>

                            <a class="lc-cta"
                                href='<%# ResolveUrl("~/Modules/Learner/Curriculum.aspx?id=" + Eval("Id")) %>'>

                                <%# (bool)Eval("IsEnrolled")
                                    ? "Open Curriculum"
                                    : "View Curriculum &amp; Enrol" %>

                            </a>

                        </div>

                    </article>

                </ItemTemplate>

            </asp:Repeater>

        </section>

        <asp:Panel ID="pnlEmpty"
            runat="server"
            CssClass="lc-empty"
            Visible="false">

            <div class="lc-empty-icon">&#9783;</div>

            <h2>No courses match your filter criteria</h2>

            <p>
                Try exploring the course catalogue or changing your search filters.
            </p>

        </asp:Panel>

    </main>

</asp:Content>