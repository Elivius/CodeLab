<%@ Page Title="Coding Challenge" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="ChallengeEditor.aspx.cs" Inherits="CodeLab.Modules.Tutor.ChallengeEditor" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>

<asp:Content ID="ChallengeEditorContent" ContentPlaceHolderID="TutorContent" runat="server">

    <%-- ===== Header: breadcrumb, course title, back link ===== --%>
    <div class="cl-builder-header">
        <div class="cl-builder-heading">
            <nav class="cl-crumbs" aria-label="Breadcrumb">
                <a href="<%: Master.GetPageUrl("Dashboard") %>">Dashboard</a>
                <i data-lucide="chevron-right" class="cl-icon" aria-hidden="true"></i>
                <% if (phMain.Visible) { %>
                    <a href="<%: Master.GetPageUrl("CourseEditor") + "?id=" + CourseId %>">Course builder</a>
                    <i data-lucide="chevron-right" class="cl-icon" aria-hidden="true"></i>
                <% } %>
                <span>Coding challenge</span>
            </nav>
            <h1><asp:Literal ID="litHeading" runat="server" Mode="Encode" /></h1>
            <p><asp:Literal ID="litSubheading" runat="server" Mode="Encode" /></p>
        </div>
        <asp:HyperLink ID="hlBackToCourse" runat="server" CssClass="btn cl-btn-outline" Visible="false">
            <i data-lucide="arrow-left" class="cl-icon me-1" aria-hidden="true"></i>Course details
        </asp:HyperLink>
    </div>

    <%-- ===== Progress: Details > Lessons > Quiz > Challenge > Submit ===== --%>
    <% if (phMain.Visible) { %>
        <%= Master.BuilderSteps(4, CourseId) %>
    <% } %>

    <asp:Label ID="lblSuccess" runat="server" CssClass="cl-alert cl-alert-success" Visible="false" />
    <asp:Label ID="lblMessage" runat="server" CssClass="cl-alert cl-alert-danger" Visible="false" />

    <%-- ===== Not approved yet ===== --%>
    <asp:PlaceHolder ID="phLocked" runat="server" Visible="false">
        <section class="cl-card cl-state-card">
            <svg class="cl-state-art" viewBox="0 0 200 140" aria-hidden="true">
                <defs><linearGradient id="lockGrad" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#C9527D" /><stop offset="1" stop-color="#FFA2A4" /></linearGradient></defs>
                <circle cx="100" cy="70" r="62" fill="url(#lockGrad)" opacity=".12" />
                <rect x="66" y="62" width="68" height="52" rx="12" fill="url(#lockGrad)" />
                <path d="M80 62V48a20 20 0 0 1 40 0v14" fill="none" stroke="url(#lockGrad)" stroke-width="9" stroke-linecap="round" />
                <circle cx="100" cy="84" r="7" fill="#fff" /><rect x="97" y="86" width="6" height="14" rx="3" fill="#fff" />
            </svg>
            <h2>Course authoring is locked</h2>
            <p>You can add coding challenges once an administrator approves your tutor application.</p>
            <asp:HyperLink ID="hlLockedProfile" runat="server" NavigateUrl="~/Modules/Tutor/Profile" CssClass="btn btn-primary"
                Text="View my application" />
        </section>
    </asp:PlaceHolder>

    <%-- ===== Course link is wrong, or the course belongs to someone else ===== --%>
    <asp:PlaceHolder ID="phNotFound" runat="server" Visible="false">
        <section class="cl-card cl-state-card">
            <svg class="cl-state-art" viewBox="0 0 200 140" aria-hidden="true">
                <defs><linearGradient id="nfGrad" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#C9527D" /><stop offset="1" stop-color="#FFA2A4" /></linearGradient></defs>
                <circle cx="100" cy="70" r="62" fill="url(#nfGrad)" opacity=".12" />
                <circle cx="92" cy="64" r="28" fill="none" stroke="url(#nfGrad)" stroke-width="9" />
                <path d="M113 85l22 22" stroke="url(#nfGrad)" stroke-width="11" stroke-linecap="round" />
                <path d="M83 55l18 18M101 55L83 73" stroke="#C9527D" stroke-width="5" stroke-linecap="round" />
            </svg>
            <h2>Course not found</h2>
            <p>It may have been removed, or it may not be yours.</p>
            <asp:HyperLink ID="hlNotFoundBack" runat="server" NavigateUrl="~/Modules/Tutor/ChallengeEditor" CssClass="btn btn-primary"
                Text="Choose a course" />
        </section>
    </asp:PlaceHolder>

    <%-- ===== No course in the link (opened from the sidebar): choose one ===== --%>
    <asp:PlaceHolder ID="phPicker" runat="server" Visible="false">
        <asp:Repeater ID="rptPickCourses" runat="server">
            <HeaderTemplate><div class="cl-picker"></HeaderTemplate>
            <ItemTemplate>
                <a class="cl-card cl-picker-item" href="<%#: Eval("Url") %>">
                    <span class="cl-builder-icon <%# (bool)Eval("HasItem") ? "cl-tone-green" : "cl-tone-neutral" %>">
                        <i data-lucide="<%# (bool)Eval("HasItem") ? "square-terminal" : "square-plus" %>" class="cl-icon" aria-hidden="true"></i>
                    </span>
                    <span class="cl-picker-body">
                        <strong><%#: Eval("Title") %></strong>
                        <small><%#: Eval("Note") %></small>
                    </span>
                    <span class="cl-badge cl-status <%# Eval("StatusCss") %>"><%#: Eval("Status") %></span>
                    <i data-lucide="chevron-right" class="cl-icon cl-picker-arrow" aria-hidden="true"></i>
                </a>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phNoCourses" runat="server" Visible="false">
            <section class="cl-card cl-state-card">
                <h2>No courses yet</h2>
                <p>Create a course first, then add its coding challenge.</p>
                <a class="btn btn-primary" href="<%: Master.GetPageUrl("CourseEditor") %>">Create a course</a>
            </section>
        </asp:PlaceHolder>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phMain" runat="server" Visible="false">

        <asp:PlaceHolder ID="phReadOnlyNote" runat="server" Visible="false">
            <div class="cl-feedback cl-feedback-info" role="status">
                <span class="cl-builder-icon cl-tone-amber"><i data-lucide="lock-keyhole" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <strong>The coding challenge is read-only</strong>
                    <p><asp:Literal ID="litReadOnlyText" runat="server" Mode="Encode" /></p>
                </div>
            </div>
        </asp:PlaceHolder>

        <%-- ===== Create or edit the challenge (Draft course) ===== --%>
        <asp:PlaceHolder ID="phForm" runat="server" Visible="false">
            <div class="cl-builder">
                <div class="cl-builder-main">
                    <asp:ValidationSummary ID="vsChallenge" runat="server" ValidationGroup="Challenge" CssClass="cl-alert cl-alert-danger"
                        HeaderText="Please fix the following:" DisplayMode="BulletList" />

                    <%-- ValidateRequestMode="Disabled" lets tutors type code. That is safe because the text is only
                         ever shown encoded, and the problem statement goes through MarkdownHelper. --%>

                    <%-- The basics --%>
                    <section class="cl-card cl-builder-card cl-form">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-pink"><i data-lucide="flag" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>The challenge</h2><p>One hands-on coding task for the whole course.</p></div>
                        </div>

                        <div class="cl-field">
                            <asp:Label ID="lblTitle" runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Title" />
                            <asp:TextBox ID="txtTitle" runat="server" ValidateRequestMode="Disabled" CssClass="form-control cl-input-lg" MaxLength="200"
                                placeholder="e.g. FizzBuzz from 1 to 15" />
                            <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle"
                                ErrorMessage="The title is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Challenge" />
                        </div>

                        <div class="cl-field-row cl-field-row-3">
                            <div class="cl-field">
                                <asp:Label ID="lblDifficulty" runat="server" AssociatedControlID="ddlDifficulty" CssClass="form-label" Text="Difficulty" />
                                <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select" />
                            </div>
                            <div class="cl-field">
                                <asp:Label ID="lblLanguage" runat="server" AssociatedControlID="ddlLanguage" CssClass="form-label" Text="Language" />
                                <asp:DropDownList ID="ddlLanguage" runat="server" CssClass="form-select" />
                                <asp:RequiredFieldValidator ID="rfvLanguage" runat="server" ControlToValidate="ddlLanguage" InitialValue=""
                                    ErrorMessage="Choose the programming language." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Challenge" />
                            </div>
                            <div class="cl-field">
                                <asp:Label ID="lblPoints" runat="server" AssociatedControlID="txtPoints" CssClass="form-label" Text="Points reward" />
                                <div class="cl-input-suffix">
                                    <asp:TextBox ID="txtPoints" runat="server" CssClass="form-control" TextMode="Number" min="0" max="1000" />
                                    <span aria-hidden="true">pts</span>
                                </div>
                                <asp:RequiredFieldValidator ID="rfvPoints" runat="server" ControlToValidate="txtPoints"
                                    ErrorMessage="Points are required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Challenge" />
                                <asp:RangeValidator ID="rvPoints" runat="server" ControlToValidate="txtPoints" Type="Integer" MinimumValue="0" MaximumValue="1000"
                                    ErrorMessage="Points must be between 0 and 1000." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Challenge" />
                            </div>
                        </div>
                    </section>

                    <%-- Problem statement (Markdown) --%>
                    <section class="cl-card cl-builder-card cl-form">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-blue"><i data-lucide="file-text" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Problem statement</h2><p>Explain the task, the input and what the program should print.</p></div>
                        </div>

                        <div class="cl-writer" data-mode="write">
                            <div class="cl-writer-bar">
                                <div class="cl-tabs" role="tablist" aria-label="Editor mode">
                                    <button type="button" class="cl-tab is-active" role="tab" aria-selected="true" data-writer-tab="write">
                                        <i data-lucide="pencil-line" class="cl-icon" aria-hidden="true"></i>Write
                                    </button>
                                    <%-- Preview asks the server for the safe HTML, without reloading the page --%>
                                    <asp:LinkButton ID="btnPreview" runat="server" CssClass="cl-tab" role="tab" aria-selected="false" data-writer-tab="preview"
                                        CausesValidation="false" OnClick="btnPreview_Click">
                                        <i data-lucide="eye" class="cl-icon" aria-hidden="true"></i>Preview
                                    </asp:LinkButton>
                                </div>
                                <div class="cl-md-toolbar" role="toolbar" aria-label="Formatting">
                                    <button type="button" data-md="bold" title="Bold"><i data-lucide="bold" class="cl-icon" aria-hidden="true"></i></button>
                                    <button type="button" data-md="italic" title="Italic"><i data-lucide="italic" class="cl-icon" aria-hidden="true"></i></button>
                                    <button type="button" data-md="heading" title="Heading"><i data-lucide="heading-2" class="cl-icon" aria-hidden="true"></i></button>
                                    <span class="cl-md-divider" aria-hidden="true"></span>
                                    <button type="button" data-md="ul" title="Bullet list"><i data-lucide="list" class="cl-icon" aria-hidden="true"></i></button>
                                    <button type="button" data-md="ol" title="Numbered list"><i data-lucide="list-ordered" class="cl-icon" aria-hidden="true"></i></button>
                                    <span class="cl-md-divider" aria-hidden="true"></span>
                                    <button type="button" data-md="code" title="Inline code"><i data-lucide="code" class="cl-icon" aria-hidden="true"></i></button>
                                    <button type="button" data-md="codeblock" title="Code block"><i data-lucide="square-code" class="cl-icon" aria-hidden="true"></i></button>
                                </div>
                            </div>

                            <div class="cl-writer-pane cl-writer-write">
                                <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription" CssClass="visually-hidden" Text="Problem statement" />
                                <asp:TextBox ID="txtDescription" runat="server" ValidateRequestMode="Disabled" CssClass="form-control cl-md-input cl-md-input-sm"
                                    TextMode="MultiLine" Rows="9"
                                    placeholder="e.g. Print the numbers 1 to 15. For multiples of 3 print Fizz, for multiples of 5 print Buzz, and for both print FizzBuzz." />
                            </div>

                            <div class="cl-writer-pane cl-writer-preview">
                                <asp:UpdatePanel ID="upPreview" runat="server" UpdateMode="Conditional">
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btnPreview" EventName="Click" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <asp:PlaceHolder ID="phPreview" runat="server" Visible="false">
                                            <div class="cl-markdown"><asp:Literal ID="litPreview" runat="server" Mode="PassThrough" /></div>
                                        </asp:PlaceHolder>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>

                            <div class="cl-writer-foot">
                                <span><i data-lucide="file-text" class="cl-icon" aria-hidden="true"></i>Markdown supported</span>
                                <span data-word-count>0 words</span>
                            </div>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription"
                            ErrorMessage="The problem statement is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Challenge" />
                    </section>

                    <%-- Starter code and expected output --%>
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-green"><i data-lucide="square-terminal" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Code</h2><p>Learners start from the starter code. Their program's output is compared with the expected output.</p></div>
                        </div>

                        <div class="cl-field">
                            <span class="form-label d-block">Starter code <small class="text-muted">(optional)</small></span>
                            <div class="cl-ide">
                                <div class="cl-ide-bar">
                                    <span class="cl-ide-dots" aria-hidden="true"><i></i><i></i><i></i></span>
                                    <span class="cl-ide-label" id="starterLanguage">main</span>
                                </div>
                                <asp:Label ID="lblStarterCode" runat="server" AssociatedControlID="txtStarterCode" CssClass="visually-hidden" Text="Starter code" />
                                <asp:TextBox ID="txtStarterCode" runat="server" ValidateRequestMode="Disabled" CssClass="cl-ide-input"
                                    TextMode="MultiLine" Rows="10" spellcheck="false"
                                    placeholder="// The code learners start from" />
                            </div>
                        </div>

                        <div class="cl-field">
                            <span class="form-label d-block">Expected output</span>
                            <div class="cl-ide cl-ide-output">
                                <div class="cl-ide-bar">
                                    <span class="cl-ide-label"><i data-lucide="terminal" class="cl-icon" aria-hidden="true"></i>Output</span>
                                </div>
                                <asp:Label ID="lblExpectedOutput" runat="server" AssociatedControlID="txtExpectedOutput" CssClass="visually-hidden" Text="Expected output" />
                                <asp:TextBox ID="txtExpectedOutput" runat="server" ValidateRequestMode="Disabled" CssClass="cl-ide-input"
                                    TextMode="MultiLine" Rows="6" spellcheck="false"
                                    placeholder="Exactly what a correct program prints, e.g. one value per line" />
                            </div>
                            <asp:RequiredFieldValidator ID="rfvExpectedOutput" runat="server" ControlToValidate="txtExpectedOutput"
                                ErrorMessage="The expected output is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Challenge" />
                        </div>
                    </section>

                    <%-- Save bar: stays at the bottom of the screen while scrolling --%>
                    <div class="cl-action-bar">
                        <span class="cl-action-bar-note">
                            <i data-lucide="info" class="cl-icon" aria-hidden="true"></i>
                            One coding challenge per course.
                        </span>
                        <div class="cl-action-bar-buttons">
                            <asp:HyperLink ID="hlCancel" runat="server" CssClass="btn cl-btn-outline" Text="Cancel" />
                            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Challenge" OnClick="btnSave_Click" />
                        </div>
                    </div>
                </div>

                <aside class="cl-builder-aside">
                    <%-- How the challenge appears in the Code Lab list --%>
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-green"><i data-lucide="eye" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Code Lab preview</h2><p>Updates as you type.</p></div>
                        </div>
                        <div class="cl-challenge-card">
                            <div class="cl-challenge-card-top">
                                <span class="cl-badge cl-badge-neutral" id="previewDifficulty">Beginner</span>
                                <span class="cl-challenge-points"><i data-lucide="trophy" class="cl-icon" aria-hidden="true"></i><span id="previewPoints">50</span> pts</span>
                            </div>
                            <h3 id="previewTitle">Your challenge title</h3>
                            <span class="cl-challenge-language"><i data-lucide="code" class="cl-icon" aria-hidden="true"></i><span id="previewLanguage">Language</span></span>
                        </div>
                    </section>

                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-amber"><i data-lucide="lightbulb" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Tips</h2><p>Make it easy to check.</p></div>
                        </div>
                        <ul class="cl-checklist">
                            <li>Say exactly what the program should print, one value per line.</li>
                            <li>Keep the expected output short and exact, including capital letters.</li>
                            <li>Use the starter code to set up anything learners shouldn't have to write.</li>
                        </ul>
                        <asp:PlaceHolder ID="phDelete" runat="server" Visible="false">
                            <div class="cl-card-actions">
                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="cl-link-danger cl-card-actions-left" CausesValidation="false"
                                    OnClick="btnDelete_Click"
                                    OnClientClick="return confirm('Delete this coding challenge? This cannot be undone.');">
                                    <i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i>Delete challenge
                                </asp:LinkButton>
                            </div>
                        </asp:PlaceHolder>
                    </section>
                </aside>
            </div>
        </asp:PlaceHolder>

        <%-- ===== Read-only (course is not a Draft) ===== --%>
        <asp:PlaceHolder ID="phReadOnly" runat="server" Visible="false">
            <section class="cl-card cl-builder-card cl-challenge-readonly">
                <div class="cl-challenge-card-top">
                    <span class="cl-badge cl-badge-neutral"><asp:Literal ID="litRoDifficulty" runat="server" Mode="Encode" /></span>
                    <span class="cl-challenge-points"><i data-lucide="trophy" class="cl-icon" aria-hidden="true"></i><asp:Literal ID="litRoPoints" runat="server" Mode="Encode" /> pts</span>
                </div>
                <h2><asp:Literal ID="litRoTitle" runat="server" Mode="Encode" /></h2>
                <span class="cl-challenge-language"><i data-lucide="code" class="cl-icon" aria-hidden="true"></i><asp:Literal ID="litRoLanguage" runat="server" Mode="Encode" /></span>

                <h3 class="cl-builder-label">Problem statement</h3>
                <div class="cl-markdown"><asp:Literal ID="litRoDescription" runat="server" Mode="PassThrough" /></div>

                <h3 class="cl-builder-label">Starter code</h3>
                <asp:Literal ID="litRoStarterCode" runat="server" Mode="PassThrough" />

                <h3 class="cl-builder-label">Expected output</h3>
                <pre class="cl-code cl-output"><asp:Literal ID="litRoExpectedOutput" runat="server" Mode="Encode" /></pre>
            </section>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phNoChallenge" runat="server" Visible="false">
            <section class="cl-card cl-state-card">
                <h2>No coding challenge</h2>
                <p>This course was submitted without a coding challenge.</p>
            </section>
        </asp:PlaceHolder>
    </asp:PlaceHolder>

    <%-- Syntax colours for code in the preview; the writer, preview tab and code boxes come from tutor-writer.js --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/tutor-writer.js") %>"></script>

    <script>
        // Read-only page: colour the starter code
        document.addEventListener('DOMContentLoaded', function () {
            if (window.hljs) {
                document.querySelectorAll('.cl-challenge-readonly pre code').forEach(function (block) {
                    window.hljs.highlightElement(block);
                });
            }

            var title = document.getElementById('<%= txtTitle.ClientID %>');
            if (!title) {
                return;
            }

            // Live preview of the Code Lab card
            var difficulty = document.getElementById('<%= ddlDifficulty.ClientID %>');
            var language = document.getElementById('<%= ddlLanguage.ClientID %>');
            var points = document.getElementById('<%= txtPoints.ClientID %>');
            var difficultyCss = { Beginner: 'cl-badge-success', Intermediate: 'cl-badge-info', Advanced: 'cl-badge-danger' };

            function update() {
                document.getElementById('previewTitle').textContent = title.value.trim() || 'Your challenge title';
                var badge = document.getElementById('previewDifficulty');
                badge.textContent = difficulty.value;
                badge.className = 'cl-badge ' + (difficultyCss[difficulty.value] || 'cl-badge-neutral');
                var languageName = language.value ? language.options[language.selectedIndex].text : 'Language';
                document.getElementById('previewLanguage').textContent = languageName;
                document.getElementById('starterLanguage').textContent = language.value ? languageName : 'main';
                document.getElementById('previewPoints').textContent = points.value || '0';
            }

            [title, difficulty, language, points].forEach(function (el) {
                el.addEventListener('input', update);
                el.addEventListener('change', update);
            });
            update();
        });
    </script>

</asp:Content>
