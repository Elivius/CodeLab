<%@ Page Title="Discussion Details"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="DiscussionDetails.aspx.cs"
    Inherits="CodeLab.Modules.Learner.DiscussionDetails" %>

<asp:Content ID="Head"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <link href="<%= ResolveUrl("~/Content/LearnerDashboard.css") %>"
        rel="stylesheet" />

    <link href="<%= ResolveUrl("~/Content/LearnerForum.css") %>?v=1"
        rel="stylesheet" />
</asp:Content>

<asp:Content ID="Main"
    ContentPlaceHolderID="MainContent"
    runat="server">

<main class="ld-wrap lf-wrap">

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
        aria-current="location"
        href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
        <svg class="ld-icon" aria-hidden="true">
            <use href="#lf-nav-message" />
        </svg>
        Discussion Forum
    </a>

    <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Leaderboard") %>">
        <svg class="ld-icon" aria-hidden="true">
            <use href="#lf-nav-trophy" />
        </svg>
        Leaderboard &amp; Badges
    </a>

    <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Profile") %>">
        <svg class="ld-icon" aria-hidden="true">
            <use href="#lf-nav-user" />
        </svg>
        My Profile
    </a>

    <span class="ld-identity">
        <span class="ld-avatar">
            <asp:Literal ID="litNavInitial"
                runat="server" Mode="Encode" />
        </span>

        <span class="ld-user-meta">
            <b>
                <asp:Literal ID="litNavName"
                    runat="server" Mode="Encode" />
            </b>
            <small>
                Level
                <asp:Literal ID="litNavLevel"
                    runat="server" Mode="Encode" />
                &bull;
                <asp:Literal ID="litNavPoints"
                    runat="server" Mode="Encode" />
                XP
            </small>
        </span>
    </span>
</nav>

        <div class="lf-detail-navigation">
            <asp:HyperLink ID="lnkBack"
                runat="server"
                CssClass="lf-button lf-button-secondary"
                NavigateUrl="~/Modules/Learner/DiscussionForum.aspx"
                Text="← Back to Discussion Forum" />
        </div>

        <asp:Panel ID="pnlError"
            runat="server"
            Visible="false"
            CssClass="lf-message lf-message-error"
            role="alert">

            <asp:Literal ID="litError"
                runat="server"
                Mode="Encode" />
        </asp:Panel>

        <asp:Panel ID="pnlSuccess"
            runat="server"
            Visible="false"
            CssClass="lf-message lf-message-success"
            role="status">

            <asp:Literal ID="litSuccess"
                runat="server"
                Mode="Encode" />
        </asp:Panel>

        <asp:Panel ID="pnlDiscussion"
            runat="server"
            Visible="false">

            <!-- Original discussion -->
            <article class="lf-post-card lf-detail-post">
                <div class="lf-post-meta">
                    <div class="lf-author">
                        <span class="lf-avatar" aria-hidden="true">
                            <asp:Literal ID="litAuthorInitial"
                                runat="server"
                                Mode="Encode" />
                        </span>

                        <div class="lf-author-info">
                            <div class="lf-author-heading">
                                <strong>
                                    <% if(currentPost!=null && currentPost.AuthorRole=="Tutor") { %><%: currentPost.AuthorName %><% } %>
                                    <asp:Literal ID="litAuthorName"
                                        runat="server"
                                        Mode="Encode" />
                                </strong>

                                <% if(currentPost != null && !string.IsNullOrEmpty(currentPost.AuthorBadgeName)) { %><span class="cl-earned-badge tier-<%: currentPost.AuthorBadgeTier %>" title="Featured earned badge">&#9733; <%: currentPost.AuthorBadgeName %></span><% } %>
                                <span class="lf-role">
                                    <asp:Literal ID="litAuthorRole"
                                        runat="server"
                                        Mode="Encode" />
                                </span>

                                <asp:PlaceHolder ID="phYou"
                                    runat="server"
                                    Visible="false">
                                    <span class="lf-you">You</span>
                                </asp:PlaceHolder>
                            </div>

                            <div class="lf-author-date">
                                <span>
                                    @<asp:Literal ID="litAuthorUsername"
                                        runat="server"
                                        Mode="Encode" />
                                </span>

                                <span aria-hidden="true">·</span>

                                <span>
                                    <asp:Literal ID="litCreatedAt"
                                        runat="server"
                                        Mode="Encode" />
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="lf-post-labels">
                        <span class="lf-course-label">
                            <asp:Literal ID="litCourseTitle"
                                runat="server"
                                Mode="Encode" />
                        </span>
                    </div>
                </div>

                <h1 class="lf-detail-title">
                    <asp:Literal ID="litTitle"
                        runat="server"
                        Mode="Encode" />
                </h1>

                <div class="lf-post-content"><asp:Literal ID="litContent" runat="server" Mode="Encode" /></div>

                <asp:Panel ID="pnlAuthorActions"
                    runat="server"
                    Visible="false"
                    CssClass="lf-author-actions">

                    <button type="button"
                        class="lf-button lf-button-secondary"
                        data-forum-open="lfEditDialog">
                        Edit Post
                    </button>

                    <button type="button"
                        class="lf-button lf-button-danger"
                        data-forum-open="lfDeleteDialog">
                        Delete Post
                    </button>
                </asp:Panel>
            </article>

            <!-- Replies -->
            <section class="lf-conversation"
                aria-labelledby="lfConversationHeading">

                <div class="lf-conversation-heading">
                    <h2 id="lfConversationHeading">
                        Conversation
                        (<asp:Literal ID="litReplyCount"
                            runat="server"
                            Mode="Encode"
                            Text="0" />)
                    </h2>

                    <span>Oldest replies first</span>
                </div>

                <asp:Panel ID="pnlNoReplies"
                    runat="server"
                    Visible="false"
                    CssClass="lf-empty">

                    <h2>No replies yet</h2>
                    <p>
                        Be the first to share an answer or join
                        the conversation.
                    </p>
                </asp:Panel>

                <div class="lf-reply-list">
                    <asp:Repeater ID="rptReplies" runat="server">
                        <ItemTemplate>

                            <article
                                id='reply-<%# Eval("Id") %>'
                                class='<%# GetReplyCssClass(Eval("AuthorId"), Eval("AuthorRole")) %>'>

                                <div class="lf-author">
                                    <span class="lf-avatar"
                                        aria-hidden="true">
                                        <%#: GetInitial(Eval("AuthorName")) %>
                                    </span>

                                    <div class="lf-author-info">
                                        <div class="lf-author-heading">
                                            <strong>
                                                <asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("AuthorRole")) == "Tutor" %>'><%#: Eval("AuthorName") %></asp:PlaceHolder><asp:PlaceHolder runat="server" Visible='<%# Convert.ToString(Eval("AuthorRole")) != "Tutor" %>'><%#: Eval("AuthorName") %></asp:PlaceHolder><asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrEmpty(Eval("AuthorBadgeName") as string) %>'><span class='cl-earned-badge tier-<%#: Eval("AuthorBadgeTier") %>' title="Featured earned badge">&#9733; <%#: Eval("AuthorBadgeName") %></span></asp:PlaceHolder>
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

                                <div class="lf-reply-content"><%#: Eval("Content") %></div>
                            </article>

                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlPagination"
                    runat="server"
                    Visible="false"
                    CssClass="lf-pagination"
                    role="navigation"
                    aria-label="Reply pages">

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
            </section>

            <!-- Reply composer -->
            <section class="lf-post-card lf-reply-composer"
                aria-labelledby="lfReplyHeading">

                <h2 id="lfReplyHeading">Write a Reply</h2>

                <p class="lf-composer-intro">
                    Replying as
                    <strong>
                        <asp:Literal ID="litReplyingName"
                            runat="server"
                            Mode="Encode" />
                    </strong>
                </p>

                <asp:Panel ID="pnlReplyError"
                    runat="server"
                    Visible="false"
                    CssClass="lf-message lf-message-error"
                    role="alert">

                    <asp:Literal ID="litReplyError"
                        runat="server"
                        Mode="Encode" />
                </asp:Panel>

                <div class="lf-field">
                    <asp:Label runat="server"
                        AssociatedControlID="txtReply"
                        Text="Your reply" />

                    <asp:TextBox ID="txtReply"
                        runat="server"
                        CssClass="lf-textarea"
                        TextMode="MultiLine"
                        Rows="5"
                        ValidateRequestMode="Disabled"
                        placeholder="Share your answer, code, or follow-up question..."
                        aria-describedby="lfReplyHelp" />

                    <small id="lfReplyHelp">
                        Plain text and code are supported.
                        Maximum 10,000 characters.
                    </small>
                </div>

                <div class="lf-composer-actions">
                    <asp:Button ID="btnPostReply"
                        runat="server"
                        Text="Post Reply"
                        CssClass="lf-button lf-button-primary"
                        CausesValidation="false"
                        OnClick="btnPostReply_Click" />
                </div>
            </section>

            <p class="lf-time-note">
                Dates are shown in Malaysia time (UTC+8).
            </p>
        </asp:Panel>

        <!-- Only rendered for the post author -->
        <asp:PlaceHolder ID="phAuthorDialogs"
            runat="server"
            Visible="false">

            <dialog id="lfEditDialog"
                class="lf-dialog"
                aria-labelledby="lfEditTitle">

                <div class="lf-dialog-header">
                    <div>
                        <h2 id="lfEditTitle">Edit Discussion</h2>
                        <p>Update your title or discussion content.</p>
                    </div>

                    <button type="button"
                        class="lf-close-button"
                        data-forum-close="lfEditDialog"
                        aria-label="Close edit discussion">
                        &times;
                    </button>
                </div>

                <div class="lf-dialog-body">
                    <asp:Panel ID="pnlEditError"
                        runat="server"
                        Visible="false"
                        CssClass="lf-message lf-message-error"
                        role="alert">

                        <asp:Literal ID="litEditError"
                            runat="server"
                            Mode="Encode" />
                    </asp:Panel>

                    <div class="lf-field">
                        <asp:Label runat="server"
                            AssociatedControlID="txtEditTitle"
                            Text="Discussion title" />

                        <asp:TextBox ID="txtEditTitle"
                            runat="server"
                            CssClass="lf-input"
                            MaxLength="250"
                            ValidateRequestMode="Disabled" />
                    </div>

                    <div class="lf-field">
                        <asp:Label runat="server"
                            AssociatedControlID="txtEditContent"
                            Text="Content" />

                        <asp:TextBox ID="txtEditContent"
                            runat="server"
                            CssClass="lf-textarea"
                            TextMode="MultiLine"
                            Rows="8"
                            ValidateRequestMode="Disabled"
                            aria-describedby="lfEditHelp" />

                        <small id="lfEditHelp">
                            Maximum 20,000 characters.
                        </small>
                    </div>
                </div>

                <div class="lf-dialog-footer">
                    <button type="button"
                        class="lf-button lf-button-secondary"
                        data-forum-close="lfEditDialog">
                        Cancel
                    </button>

                    <asp:Button ID="btnSavePost"
                        runat="server"
                        Text="Save Changes"
                        CssClass="lf-button lf-button-primary"
                        CausesValidation="false"
                        OnClick="btnSavePost_Click" />
                </div>
            </dialog>

            <dialog id="lfDeleteDialog"
                class="lf-dialog"
                aria-labelledby="lfDeleteTitle"
                aria-describedby="lfDeleteDescription">

                <div class="lf-dialog-header">
                    <h2 id="lfDeleteTitle">Delete Discussion?</h2>

                    <button type="button"
                        class="lf-close-button"
                        data-forum-close="lfDeleteDialog"
                        aria-label="Close delete confirmation">
                        &times;
                    </button>
                </div>

                <div class="lf-dialog-body">
                    <asp:Panel ID="pnlDeleteError"
                        runat="server"
                        Visible="false"
                        CssClass="lf-message lf-message-error"
                        role="alert">

                        <asp:Literal ID="litDeleteError"
                            runat="server"
                            Mode="Encode" />
                    </asp:Panel>

                    <p id="lfDeleteDescription">
                        This will permanently delete your discussion
                        and all its replies. This action cannot be undone.
                    </p>
                </div>

                <div class="lf-dialog-footer">
                    <button type="button"
                        class="lf-button lf-button-secondary"
                        data-forum-close="lfDeleteDialog"
                        autofocus>
                        Cancel
                    </button>

                    <asp:Button ID="btnDeletePost"
                        runat="server"
                        Text="Delete Discussion"
                        CssClass="lf-button lf-button-danger"
                        CausesValidation="false"
                        OnClick="btnDeletePost_Click" />
                </div>
            </dialog>
        </asp:PlaceHolder>
    </main>

    <script src="<%= ResolveUrl("~/Scripts/LearnerForum.js") %>?v=1"></script>
</asp:Content>