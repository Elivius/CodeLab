<%@ Page Title="Discussion Forum"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="DiscussionForum.aspx.cs"
    Inherits="CodeLab.Modules.Learner.DiscussionForum" %>

<asp:Content ID="Head"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <link href="<%= ResolveUrl("~/Content/LearnerDashboard.css") %>?v=3"
        rel="stylesheet" />

    <link href="<%= ResolveUrl("~/Content/LearnerForum.css") %>?v=3"
        rel="stylesheet" />
</asp:Content>

<asp:Content ID="Main"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="ld-wrap lf-wrap">

        <!-- Navigation icons -->
        <svg class="ld-symbols"
            xmlns="http://www.w3.org/2000/svg"
            aria-hidden="true"
            focusable="false">

            <symbol id="lf-nav-dashboard" viewBox="0 0 24 24">
                <path d="m12 3 1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3ZM19 17l.7 1.3L21 19l-1.3.7L19 21l-.7-1.3L17 19l1.3-.7L19 17ZM4 16l.7 1.3L6 18l-1.3.7L4 20l-.7-1.3L2 18l1.3-.7L4 16Z" />
            </symbol>

            <symbol id="lf-nav-book" viewBox="0 0 24 24">
                <path d="M12 6c-2-1.7-5.6-2.1-9-1.2v14c3.4-.9 7-.5 9 1.2 2-1.7 5.6-2.1 9-1.2v-14c-3.4-.9-7-.5-9 1.2ZM12 6v14" />
            </symbol>

            <symbol id="lf-nav-terminal" viewBox="0 0 24 24">
                <path d="m4 6 6 6-6 6M12 18h8" />
            </symbol>

            <symbol id="lf-nav-message" viewBox="0 0 24 24">
                <path d="M5 4h14a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H9l-6 3V6a2 2 0 0 1 2-2Z" />
            </symbol>

            <symbol id="lf-nav-trophy" viewBox="0 0 24 24">
                <path d="M7 3h10v7a5 5 0 0 1-10 0V3ZM7 5H3v3a4 4 0 0 0 4 4M17 5h4v3a4 4 0 0 1-4 4M12 15v4M8 21h8M9 19h6" />
            </symbol>

            <symbol id="lf-nav-user" viewBox="0 0 24 24">
                <circle cx="12" cy="8" r="4" />
                <path d="M4 21a8 8 0 0 1 16 0" />
            </symbol>
        </svg>

        <!-- Shared learner navigation -->
        <nav class="ld-tabs" aria-label="Learner navigation">

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Dashboard.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lf-nav-dashboard" />
                </svg>
                Dashboard
            </a>

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lf-nav-book" />
                </svg>
                Courses &amp; Curriculum
            </a>

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Workspace.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lf-nav-terminal" />
                </svg>
                CodeLab Workspace
            </a>

            <a class="ld-tab selected"
                aria-current="page"
                href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lf-nav-message" />
                </svg>
                Discussion Forum
            </a>

            <span class="ld-tab disabled"
                aria-disabled="true"
                title="Coming in a later phase">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lf-nav-trophy" />
                </svg>
                Leaderboard &amp; Badges
            </span>

            <span class="ld-tab disabled"
                aria-disabled="true"
                title="Coming in a later phase">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#lf-nav-user" />
                </svg>
                Profile &amp; Security
            </span>

            <span class="ld-identity">
                <span class="ld-avatar">
                    <asp:Literal ID="litNavInitial"
                        runat="server"
                        Mode="Encode" />
                </span>

                <span class="ld-user-meta">
                    <b>
                        <asp:Literal ID="litNavName"
                            runat="server"
                            Mode="Encode" />
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

        <!-- Compact page header -->
        <header class="lf-page-header">
            <div>
                <h1>Discussion Forum</h1>
                <p>
                    Ask questions and share ideas with learners and tutors in any published course.
                </p>
            </div>

            <asp:Panel ID="pnlCreateAction" runat="server">
                <button type="button"
                    class="lf-button lf-button-primary"
                    data-forum-open="lfCreateDialog">
                    <span aria-hidden="true">+</span>
                    New Discussion
                </button>
            </asp:Panel>
        </header>

        <nav class="lf-section-nav" aria-label="Discussion navigation">
            <a class="is-active"
                aria-current="page"
                href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                Discussion Forum
            </a>

            <a href="<%= ResolveUrl("~/Modules/Learner/MyDiscussions.aspx") %>">
                My Discussions
            </a>
        </nav>

        <!-- Page-level feedback -->
        <asp:UpdatePanel ID="upForumFeedback"
            runat="server"
            UpdateMode="Always">

            <ContentTemplate>
                <asp:Panel ID="pnlError"
                    runat="server"
                    Visible="false"
                    CssClass="lf-message lf-message-error"
                    role="alert">

                    <asp:Literal ID="litError"
                        runat="server"
                        Mode="Encode" />
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <!-- No accessible enrolled courses -->
        <asp:Panel ID="pnlNoCourses"
            runat="server"
            Visible="false"
            CssClass="lf-empty">

            <h2>No course discussions yet</h2>

            <p>
                Discussions are grouped by course. They will appear here once a course
                has been published.
            </p>

            <a class="lf-button lf-button-primary"
                href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">
                Explore Courses
            </a>
        </asp:Panel>

        <nav class="lf-section-nav" aria-label="Discussion categories">
            <a class="is-active" href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">Course Discussions</a>
        </nav>
        <asp:Panel ID="pnlForum" runat="server">

            <!-- Outside results panel to retain search focus -->
            <asp:Panel ID="pnlFilters"
                runat="server"
                CssClass="lf-filters"
                DefaultButton="btnSearch">

                <div class="lf-filter-course">
                    <asp:Label runat="server"
                        AssociatedControlID="ddlCourse"
                        Text="Discussion area" />

                    <asp:DropDownList ID="ddlCourse"
                        runat="server"
                        CssClass="lf-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
                </div>

                <div class="lf-filter-search">
                    <asp:Label runat="server"
                        AssociatedControlID="txtSearch"
                        Text="Search discussions" />

                    <asp:TextBox ID="txtSearch"
                        runat="server"
                        ClientIDMode="Static"
                        CssClass="lf-input"
                        MaxLength="200"
                        autocomplete="off"
                        placeholder="Search titles, content, authors or courses..." />
                </div>

                <asp:Button ID="btnClearFilters"
                    runat="server"
                    Text="Clear"
                    CssClass="lf-button lf-button-secondary"
                    CausesValidation="false"
                    OnClick="btnClearFilters_Click" />

                <div hidden>
                    <asp:Button ID="btnSearch"
                        runat="server"
                        ClientIDMode="Static"
                        Text="Search"
                        UseSubmitBehavior="false"
                        CausesValidation="false"
                        OnClick="btnSearch_Click" />
                </div>
            </asp:Panel>

            <asp:UpdatePanel ID="upForumResults"
                runat="server"
                UpdateMode="Conditional">

                <ContentTemplate>

                    <asp:Panel ID="pnlEmpty"
                        runat="server"
                        Visible="false"
                        CssClass="lf-empty">

                        <h2>No discussions found</h2>
                        <p>
                            Try another search or course, or start
                            a new discussion.
                        </p>
                    </asp:Panel>

                    <div class="lf-post-list">
                        <asp:Repeater ID="rptPosts" runat="server">
                            <ItemTemplate>

                                <article class="lf-post-card">
                                    <div class="lf-post-meta">
                                        <div class="lf-author">
                                            <span class="lf-avatar"
                                                aria-hidden="true">
                                                <%#: GetInitial(Eval("AuthorName")) %>
                                            </span>

                                            <div class="lf-author-info">
                                                <div class="lf-author-heading">
                                                    <strong>
                                                        <%#: Eval("AuthorName") %>
                                                    </strong>

                                                    <span class="lf-role">
                                                        <%#: Eval("AuthorRole") %>
                                                    </span>

                                                    <asp:PlaceHolder
                                                        runat="server"
                                                        Visible='<%# IsCurrentLearner(Eval("AuthorId")) %>'>
                                                        <span class="lf-you">You</span>
                                                    </asp:PlaceHolder>
                                                </div>

                                                <div class="lf-author-date">
                                                    <span>
                                                        @<%#: Eval("AuthorUsername") %>
                                                    </span>

                                                    <span aria-hidden="true">·</span>

                                                    <time datetime='<%# FormatUtc(Eval("CreatedAt")) %>'>
                                                        <%#: FormatDate(Eval("CreatedAt")) %>
                                                    </time>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="lf-post-labels">
                                            <span class="lf-course-label">
                                                <%#: Eval("CourseTitle") %>
                                            </span>

                                            <asp:PlaceHolder
                                                runat="server"
                                                Visible='<%# Convert.ToBoolean(Eval("HasTutorReply")) %>'>
                                                <span class="lf-tutor-label">
                                                    Tutor Replied
                                                </span>
                                            </asp:PlaceHolder>
                                        </div>
                                    </div>

                                    <h2 class="lf-post-title">
                                        <a href='<%# GetDiscussionUrl(Eval("Id")) %>'>
                                            <%#: Eval("Title") %>
                                        </a>
                                    </h2>

                                    <p class="lf-post-preview"><%#: GetPreview(Eval("Content")) %></p>

                                    <footer class="lf-post-footer">
                                        <div class="lf-post-stats">
                                            <span>
                                                <%#: FormatReplyCount(Eval("ReplyCount")) %>
                                            </span>

                                            <span class="lf-last-activity">
                                                Latest activity:
                                                <%#: FormatDate(Eval("LastActivityAt")) %>
                                            </span>
                                        </div>

                                        <a class="lf-text-link"
                                            href='<%# GetDiscussionUrl(Eval("Id")) %>'>
                                            Read Discussion
                                            <span aria-hidden="true">&rarr;</span>
                                        </a>
                                    </footer>
                                </article>

                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:Panel ID="pnlPagination"
                        runat="server"
                        Visible="false"
                        CssClass="lf-pagination"
                        role="navigation"
                        aria-label="Discussion pages">

                        <asp:Button ID="btnPrevious"
                            runat="server"
                            Text="Previous"
                            CssClass="lf-button lf-button-secondary"
                            CausesValidation="false"
                            OnClick="btnPrevious_Click" />

                        <span class="lf-page-number" aria-live="polite">
                            <asp:Literal ID="litPageNumber"
                                runat="server"
                                Mode="Encode" />
                        </span>

                        <asp:Button ID="btnNext"
                            runat="server"
                            Text="Next"
                            CssClass="lf-button lf-button-secondary"
                            CausesValidation="false"
                            OnClick="btnNext_Click" />
                    </asp:Panel>

                </ContentTemplate>

                <Triggers>
                    <asp:AsyncPostBackTrigger
                        ControlID="btnSearch"
                        EventName="Click" />

                    <asp:AsyncPostBackTrigger
                        ControlID="ddlCourse"
                        EventName="SelectedIndexChanged" />

                    <asp:PostBackTrigger ControlID="btnClearFilters" />
                </Triggers>
            </asp:UpdatePanel>

            <p class="lf-time-note">
                Dates are shown in Malaysia time (UTC+8).
            </p>
        </asp:Panel>

        <!-- New Discussion popup -->
        <dialog id="lfCreateDialog"
            class="lf-dialog"
            aria-labelledby="lfCreateTitle">

            <div class="lf-dialog-header">
                <div>
                    <h2 id="lfCreateTitle">New Discussion</h2>
                    <p>Ask a question or share something useful.</p>
                </div>

                <button type="button"
                    class="lf-close-button"
                    data-forum-close="lfCreateDialog"
                    aria-label="Close new discussion">
                    &times;
                </button>
            </div>

            <div class="lf-dialog-body">

                <asp:Panel ID="pnlCreateError"
                    runat="server"
                    Visible="false"
                    CssClass="lf-message lf-message-error"
                    role="alert">

                    <asp:Literal ID="litCreateError"
                        runat="server"
                        Mode="Encode" />
                </asp:Panel>

                <div class="lf-field">
                    <asp:Label runat="server"
                        AssociatedControlID="ddlPostCourse"
                        Text="Discussion area" />

                    <asp:DropDownList ID="ddlPostCourse"
                        runat="server"
                        CssClass="lf-select" />
                </div>

                <div class="lf-field">
                    <asp:Label runat="server"
                        AssociatedControlID="txtPostTitle"
                        Text="Discussion title" />

                    <asp:TextBox ID="txtPostTitle"
                        runat="server"
                        CssClass="lf-input"
                        MaxLength="250"
                        ValidateRequestMode="Disabled"
                        placeholder="What would you like to discuss?"
                        aria-describedby="lfTitleHelp" />

                    <small id="lfTitleHelp">
                        Maximum 250 characters.
                    </small>
                </div>

                <div class="lf-field">
                    <asp:Label runat="server"
                        AssociatedControlID="txtPostContent"
                        Text="Content" />

                    <asp:TextBox ID="txtPostContent"
                        runat="server"
                        CssClass="lf-textarea"
                        TextMode="MultiLine"
                        Rows="8"
                        ValidateRequestMode="Disabled"
                        placeholder="Describe your question, include code, or share your ideas..."
                        aria-describedby="lfContentHelp" />

                    <small id="lfContentHelp">
                        Plain text and code are supported.
                        Maximum 20,000 characters.
                    </small>
                </div>

                <p class="lf-posting-as">
                    Posting as
                    <strong>
                        <asp:Literal ID="litPostingName"
                            runat="server"
                            Mode="Encode" />
                    </strong>
                </p>
            </div>

            <div class="lf-dialog-footer">
                <button type="button"
                    class="lf-button lf-button-secondary"
                    data-forum-close="lfCreateDialog">
                    Cancel
                </button>

                <asp:Button ID="btnPublish"
                    runat="server"
                    Text="Publish Discussion"
                    CssClass="lf-button lf-button-primary"
                    CausesValidation="false"
                    OnClick="btnPublish_Click" />
            </div>
        </dialog>
    </main>

    <script src="<%= ResolveUrl("~/Scripts/LearnerForum.js") %>?v=1"></script>
</asp:Content>