<%@ Page Title="CodeLab Workspace"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Workspace.aspx.cs"
    Inherits="CodeLab.Modules.Learner.Workspace" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet"
        href="<%= ResolveUrl("~/Content/LearnerDashboard.css") %>?v=3" />
    <link rel="stylesheet"
        href="<%= ResolveUrl("~/Content/LearnerWorkspace.css") %>?v=3" />
<style>.cw-start-button.aspNetDisabled{opacity:.5;cursor:not-allowed;pointer-events:none;box-shadow:none}</style></asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <main class="ld-wrap cw-wrap">

        <svg class="ld-symbols"
            xmlns="http://www.w3.org/2000/svg"
            aria-hidden="true"
            focusable="false">

            <symbol id="cw-sparkles" viewBox="0 0 24 24">
                <path d="m12 3 1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3ZM19 17l.7 1.3L21 19l-1.3.7L19 21l-.7-1.3L17 19l1.3-.7L19 17ZM4 16l.7 1.3L6 18l-1.3.7L4 20l-.7-1.3L2 18l1.3-.7L4 16Z" />
            </symbol>

            <symbol id="cw-book" viewBox="0 0 24 24">
                <path d="M12 6c-2-1.7-5.6-2.1-9-1.2v14c3.4-.9 7-.5 9 1.2 2-1.7 5.6-2.1 9-1.2v-14c-3.4-.9-7-.5-9 1.2ZM12 6v14" />
            </symbol>

            <symbol id="cw-terminal" viewBox="0 0 24 24">
                <path d="m4 6 6 6-6 6M12 18h8" />
            </symbol>

            <symbol id="cw-message" viewBox="0 0 24 24">
                <path d="M5 4h14a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H9l-6 3V6a2 2 0 0 1 2-2Z" />
            </symbol>

            <symbol id="cw-trophy" viewBox="0 0 24 24">
                <path d="M7 3h10v7a5 5 0 0 1-10 0V3ZM7 5H3v3a4 4 0 0 0 4 4M17 5h4v3a4 4 0 0 1-4 4M12 15v4M8 21h8M9 19h6" />
            </symbol>

            <symbol id="cw-user" viewBox="0 0 24 24">
                <circle cx="12" cy="8" r="4" />
                <path d="M4 21a8 8 0 0 1 16 0" />
            </symbol>

            <symbol id="cw-search" viewBox="0 0 24 24">
                <circle cx="10.5" cy="10.5" r="6.5" />
                <path d="m16 16 5 5" />
            </symbol>
        </svg>

        <nav class="ld-tabs" aria-label="Learner navigation">
            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Dashboard.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#cw-sparkles" />
                </svg>
                Dashboard
            </a>

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/Courses.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#cw-book" />
                </svg>
                Courses &amp; Curriculum
            </a>

            <a class="ld-tab selected"
                aria-current="page"
                href="<%= ResolveUrl("~/Modules/Learner/Workspace.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#cw-terminal" />
                </svg>
                CodeLab Workspace
            </a>

            <a class="ld-tab"
                href="<%= ResolveUrl("~/Modules/Learner/DiscussionForum.aspx") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#cw-message" />
                </svg>
                Discussion Forum
            </a>

            <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Leaderboard") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#cw-trophy" />
                </svg>
                Leaderboard &amp; Badges
            </a>

            <a class="ld-tab" href="<%= ResolveUrl("~/Modules/Learner/Profile") %>">
                <svg class="ld-icon" aria-hidden="true">
                    <use href="#cw-user" />
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

        <asp:UpdatePanel ID="upWorkspaceError"
            runat="server"
            UpdateMode="Always">
            <ContentTemplate>
                <asp:Panel ID="pnlError"
                    runat="server"
                    Visible="false"
                    CssClass="cw-error"
                    role="alert">
                    <asp:Literal ID="litError"
                        runat="server" Mode="Encode" />
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <!-- Filters stay outside the results panel to preserve typing focus. -->
        <asp:Panel ID="pnlFilters"
            runat="server"
            CssClass="cw-filter-panel"
            DefaultButton="btnSearch">

            <div hidden>
                <asp:Button ID="btnSearch"
                    runat="server"
                    Text="Search"
                    CausesValidation="false"
                    OnClick="btnSearch_Click"
                    TabIndex="-1"
                    UseSubmitBehavior="false" />
            </div>

            <div class="cw-filter-top">
                <div class="cw-search-group">
                    <div class="cw-search-field">
                        <svg class="ld-icon" aria-hidden="true">
                            <use href="#cw-search" />
                        </svg>

                        <asp:TextBox ID="txtSearch"
                            runat="server"
                            CssClass="cw-input"
                            MaxLength="200"
                            aria-label="Search challenges"
                            placeholder="Search challenges by title, course or language..." />
                    </div>
                </div>

                <asp:DropDownList ID="ddlCourse"
                    runat="server"
                    CssClass="cw-select cw-course-select"
                    aria-label="Filter by enrolled course"
                    AutoPostBack="true"
                    CausesValidation="false"
                    OnSelectedIndexChanged="Filter_Changed">
                    <asp:ListItem Text="All Enrolled Courses" Value="0" />
                </asp:DropDownList>
            </div>

            <div class="cw-filter-bottom">
                <div class="cw-filter-group"
                    role="group"
                    aria-labelledby="cw-difficulty-label">

                    <span id="cw-difficulty-label"
                        class="cw-filter-label">Difficulty:</span>

                    <asp:RadioButtonList ID="ddlDifficulty"
                        runat="server"
                        CssClass="cw-segments"
                        RepeatDirection="Horizontal"
                        RepeatLayout="Flow"
                        AutoPostBack="true"
                        CausesValidation="false"
                        OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Text="All" Value="" Selected="True" />
                        <asp:ListItem Text="Beginner" Value="Beginner" />
                        <asp:ListItem Text="Intermediate" Value="Intermediate" />
                        <asp:ListItem Text="Advanced" Value="Advanced" />
                    </asp:RadioButtonList>
                </div>

                <div class="cw-filter-group"
                    role="group"
                    aria-labelledby="cw-status-label">

                    <span id="cw-status-label"
                        class="cw-filter-label">Status:</span>

                    <asp:RadioButtonList ID="ddlProgress"
                        runat="server"
                        CssClass="cw-segments"
                        RepeatDirection="Horizontal"
                        RepeatLayout="Flow"
                        AutoPostBack="true"
                        CausesValidation="false"
                        OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Text="All" Value="" Selected="True" />
                        <asp:ListItem Text="Not attempted" Value="Not attempted" />
                        <asp:ListItem Text="Attempted" Value="Attempted" />
                        <asp:ListItem Text="Solved" Value="Solved" />
                    </asp:RadioButtonList>
                </div>

                <div class="cw-filter-group">
                    <asp:Label runat="server"
                        AssociatedControlID="ddlLanguage"
                        CssClass="cw-filter-label"
                        Text="Language:" />

                    <asp:DropDownList ID="ddlLanguage"
                        runat="server"
                        CssClass="cw-select cw-language-select"
                        AutoPostBack="true"
                        CausesValidation="false"
                        OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Text="All Languages" Value="" />
                    </asp:DropDownList>
                </div>

                <asp:Button ID="btnClearFilters"
                    runat="server"
                    Text="Clear filters"
                    CssClass="cw-clear-button"
                    CausesValidation="false"
                    OnClick="btnClearFilters_Click" />
            </div>
        </asp:Panel>

        <asp:UpdatePanel ID="upChallengeResults"
            runat="server"
            UpdateMode="Conditional">
            <ContentTemplate>

                <div class="cw-results-bar" role="status">
                    <asp:Literal ID="litResultCount"
                        runat="server" Mode="Encode" />
                </div>

                <div class="cw-grid">
                    <asp:Repeater ID="rptChallenges" runat="server">
                        <ItemTemplate>
                            <article class="cw-card">
                                <div class="cw-card-top">
                                    <span class="cw-course-tag"
                                        title='<%#: Eval("CourseTitle") %>'>
                                        <%#: Eval("CourseTitle") %>
                                    </span>

                                    <span class="cw-reward">
                                        <svg class="ld-icon" aria-hidden="true">
                                            <use href="#cw-trophy" />
                                        </svg>
                                        <%#: Eval("PointsReward") %> XP
                                    </span>
                                </div>

                                <h2 class="cw-card-title">
                                    <%#: Eval("Title") %>
                                </h2>

                                <p class="cw-description">
                                    <%#: Eval("Description") %>
                                </p>

                                <div class="cw-card-meta">
                                    <span class="cw-difficulty"
                                        data-difficulty='<%#: Eval("Difficulty") %>'>
                                        <%#: Eval("Difficulty") %>
                                    </span>

                                    <span class="cw-language">
                                        <%#: Eval("Language") %>
                                    </span>

                                    <span class="cw-attempt-count">
                                        Attempts: <%#: Eval("AttemptCount") %>
                                    </span>
                                </div>

                                <footer class="cw-card-footer">
                                    <div class="cw-status-row">
                                        <span>Attempt status:</span>

                                        <span class='<%#: Eval("StatusCssClass") %>'>
                                            <%#: Eval("ProgressStatus") %>
                                        </span>
                                    </div>

                                    <asp:HyperLink runat="server" Enabled='<%# Convert.ToBoolean(Eval("CanAccess")) %>'
                                        CssClass='<%# Convert.ToBoolean(Eval("IsSolved")) ? "cw-start-button is-solved" : "cw-start-button" %>'
                                        NavigateUrl='<%# ResolveUrl("~/Modules/Learner/Challenge.aspx?id=" + Convert.ToInt32(Eval("Id")).ToString(System.Globalization.CultureInfo.InvariantCulture)) %>'>
                                        <svg class="ld-icon" aria-hidden="true">
                                            <use href="#cw-terminal" />
                                        </svg>
                                        <%#: Eval("ActionText") %>
                                        <span aria-hidden="true">&rarr;</span>
                                    </asp:HyperLink>
                                </footer>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlEmpty"
                    runat="server"
                    Visible="false"
                    CssClass="cw-empty">

                    <span class="cw-empty-icon">
                        <svg class="ld-icon" aria-hidden="true">
                            <use href="#cw-terminal" />
                        </svg>
                    </span>

                    <h2>
                        <asp:Literal ID="litEmptyTitle"
                            runat="server" Mode="Encode" />
                    </h2>

                    <p>
                        <asp:Literal ID="litEmptyMessage"
                            runat="server" Mode="Encode" />
                    </p>
                </asp:Panel>

            </ContentTemplate>

            <Triggers>
                <asp:AsyncPostBackTrigger
                    ControlID="btnSearch"
                    EventName="Click" />
                <asp:AsyncPostBackTrigger
                    ControlID="ddlCourse"
                    EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger
                    ControlID="ddlDifficulty"
                    EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger
                    ControlID="ddlProgress"
                    EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger
                    ControlID="ddlLanguage"
                    EventName="SelectedIndexChanged" />
                <asp:PostBackTrigger ControlID="btnClearFilters" />
            </Triggers>
        </asp:UpdatePanel>

    </main>

    <script>
        (function () {
            function initialiseSearch() {
                var input = document.getElementById(
                    "<%= txtSearch.ClientID %>");

                var trigger = document.getElementById(
                    "<%= btnSearch.ClientID %>");

                var clearButton = document.getElementById(
                    "<%= btnClearFilters.ClientID %>");

                if (!input || !trigger || input.dataset.liveSearchReady) {
                    return;
                }

                input.dataset.liveSearchReady = "true";

                var timer;
                var pending = false;
                var composing = false;
                var clearing = false;

                var manager =
                    Sys.WebForms.PageRequestManager.getInstance();

                function search() {
                    clearTimeout(timer);

                    if (composing || clearing) {
                        return;
                    }

                    if (manager.get_isInAsyncPostBack()) {
                        pending = true;
                        return;
                    }

                    pending = false;
                    trigger.click();
                }

                function scheduleSearch() {
                    clearTimeout(timer);
                    pending = false;

                    if (!composing && !clearing) {
                        timer = setTimeout(search, 400);
                    }
                }

                input.addEventListener("input", scheduleSearch);

                input.addEventListener("compositionstart", function () {
                    composing = true;
                    pending = false;
                    clearTimeout(timer);
                });

                input.addEventListener("compositionend", function () {
                    composing = false;
                    scheduleSearch();
                });

                input.addEventListener("keydown", function (event) {
                    if (event.key === "Enter" &&
                        !composing &&
                        !event.isComposing) {
                        event.preventDefault();
                        search();
                    }
                });

                if (clearButton) {
                    clearButton.addEventListener("click", function () {
                        clearing = true;
                        pending = false;
                        clearTimeout(timer);
                    });
                }

                manager.add_endRequest(function () {
                    if (pending && !clearing) {
                        search();
                    }
                });
            }

            if (document.readyState === "loading") {
                document.addEventListener(
                    "DOMContentLoaded",
                    initialiseSearch);
            } else {
                initialiseSearch();
            }
        })();
    </script>
</asp:Content>