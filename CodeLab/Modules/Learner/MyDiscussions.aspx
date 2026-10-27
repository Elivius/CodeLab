<%@ Page Title="My Discussions"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="MyDiscussions.aspx.cs"
    Inherits="CodeLab.Modules.Learner.MyDiscussions" %>

<asp:Content ID="Head"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <link href="<%= ResolveUrl("~/Content/LearnerDashboard.css") %>?v=3"
        rel="stylesheet" />

    <link href="<%= ResolveUrl("~/Content/LearnerForum.css") %>?v=2"
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

        <header class="lf-page-header">
            <div>
                <h1>My Discussions</h1>
                <p>
                    Revisit your posts and the conversations
                    you have contributed to.
                </p>
            </div>

            <a class="lf-button lf-button-primary"
                href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                Browse Discussions
            </a>
        </header>

        <nav class="lf-section-nav" aria-label="Discussion navigation">
            <a href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                Discussion Forum
            </a>

            <a class="is-active"
                aria-current="page"
                href="<%= ResolveUrl("~/Modules/Learner/MyDiscussions.aspx") %>">
                My Discussions
            </a>
        </nav>

        <asp:Panel ID="pnlError"
            runat="server"
            Visible="false"
            CssClass="lf-message lf-message-error"
            role="alert">

            <asp:Literal ID="litError"
                runat="server"
                Mode="Encode" />
        </asp:Panel>

        <asp:Panel ID="pnlMyDiscussions"
            runat="server"
            Visible="false">

            <nav class="lf-section-nav"
                aria-label="Your discussion activity">

                <asp:HyperLink ID="lnkMyPosts"
                    runat="server"
                    NavigateUrl="~/Modules/Learner/MyDiscussions.aspx?tab=posts"
                    Text="My Posts" />

                <asp:HyperLink ID="lnkMyReplies"
                    runat="server"
                    NavigateUrl="~/Modules/Learner/MyDiscussions.aspx?tab=replies"
                    Text="My Replies" />
            </nav>

            <!-- My Posts -->
            <asp:Panel ID="pnlPosts"
                runat="server"
                Visible="false">

                <asp:Panel ID="pnlNoPosts"
                    runat="server"
                    Visible="false"
                    CssClass="lf-empty">

                    <h2>No posts to show</h2>
                    <p>
                        Your general posts and posts from courses you can currently
                        access will appear here.
                    </p>

                    <a class="lf-button lf-button-primary"
                        href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                        Go to Discussion Forum
                    </a>
                </asp:Panel>

                <div class="lf-post-list">
                    <asp:Repeater ID="rptPosts" runat="server">
                        <ItemTemplate>

                            <article class="lf-post-card">
                                <div class="lf-post-meta">
                                    <span class="lf-course-label">
                                        <%#: Eval("CourseTitle") %>
                                    </span>

                                    <div class="lf-author-date">
                                        <span>Posted</span>
                                        <time datetime='<%# FormatUtc(Eval("CreatedAt")) %>'>
                                            <%#: FormatDate(Eval("CreatedAt")) %>
                                        </time>
                                    </div>
                                </div>

                                <h2 class="lf-post-title">
                                    <a href='<%# GetPostUrl(Eval("Id")) %>'>
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

                                        <asp:PlaceHolder
                                            runat="server"
                                            Visible='<%# Convert.ToBoolean(Eval("HasTutorReply")) %>'>
                                            <span class="lf-tutor-label">
                                                Tutor Replied
                                            </span>
                                        </asp:PlaceHolder>
                                    </div>

                                    <a class="lf-text-link"
                                        href='<%# GetPostUrl(Eval("Id")) %>'>
                                        Open Discussion
                                        <span aria-hidden="true">&rarr;</span>
                                    </a>
                                </footer>
                            </article>

                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <!-- My Replies -->
            <asp:Panel ID="pnlReplies"
                runat="server"
                Visible="false">

                <asp:Panel ID="pnlNoReplies"
                    runat="server"
                    Visible="false"
                    CssClass="lf-empty">

                    <h2>No replies to show</h2>
                    <p>
                        Your general replies and replies from courses you can currently
                        access will appear here.
                    </p>

                    <a class="lf-button lf-button-primary"
                        href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                        Browse Discussions
                    </a>
                </asp:Panel>

                <div class="lf-post-list">
                    <asp:Repeater ID="rptReplies" runat="server">
                        <ItemTemplate>

                            <article class="lf-post-card">
                                <div class="lf-post-meta">
                                    <span class="lf-course-label">
                                        <%#: Eval("CourseTitle") %>
                                    </span>

                                    <div class="lf-author-date">
                                        <span>Replied</span>
                                        <time datetime='<%# FormatUtc(Eval("CreatedAt")) %>'>
                                            <%#: FormatDate(Eval("CreatedAt")) %>
                                        </time>
                                    </div>
                                </div>

                                <h2 class="lf-post-title">
                                    <a href='<%# GetReplyDiscussionUrl(Eval("PostId")) %>'>
                                        <%#: Eval("PostTitle") %>
                                    </a>
                                </h2>

                                <div class="lf-posting-as">
                                    <strong>Your reply</strong>
                                </div>

                                <div class="lf-post-content"><%#: Eval("Content") %></div>

                                <footer class="lf-post-footer">
                                    <a class="lf-text-link"
                                        href='<%# GetReplyDiscussionUrl(Eval("PostId")) %>'>
                                        Return to Conversation
                                        <span aria-hidden="true">&rarr;</span>
                                    </a>
                                </footer>
                            </article>

                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <!-- Shared pagination for the selected tab -->
            <asp:Panel ID="pnlPagination"
                runat="server"
                Visible="false"
                CssClass="lf-pagination"
                role="navigation"
                aria-label="Your activity pages">

                <asp:Button ID="btnPrevious"
                    runat="server"
                    Text="Previous"
                    CssClass="lf-button lf-button-secondary"
                    CausesValidation="false"
                    OnClick="btnPrevious_Click" />

                <span class="lf-page-number">
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

            <p class="lf-time-note">
                Dates are shown in Malaysia time (UTC+8).
            </p>
        </asp:Panel>
    </main>
</asp:Content>