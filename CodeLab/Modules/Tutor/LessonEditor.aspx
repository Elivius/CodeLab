<%@ Page Title="Lessons" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="LessonEditor.aspx.cs" Inherits="CodeLab.Modules.Tutor.LessonEditor" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>
<%@ Import Namespace="CodeLab.Models" %>

<asp:Content ID="LessonEditorContent" ContentPlaceHolderID="TutorContent" runat="server">

    <%-- ===== Header: breadcrumb, course title, back link ===== --%>
    <div class="cl-builder-header">
        <div class="cl-builder-heading">
            <nav class="cl-crumbs" aria-label="Breadcrumb">
                <a href="<%: Master.GetPageUrl("Dashboard") %>">Dashboard</a>
                <i data-lucide="chevron-right" class="cl-icon" aria-hidden="true"></i>
                <% if (CourseId > 0 && phMain.Visible) { %>
                    <a href="<%: Master.GetPageUrl("CourseEditor") + "?id=" + CourseId %>">Course builder</a>
                    <i data-lucide="chevron-right" class="cl-icon" aria-hidden="true"></i>
                <% } %>
                <span>Lessons</span>
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
        <%= Master.BuilderSteps(2, CourseId) %>
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
            <p>You can add lessons once an administrator approves your tutor application.</p>
            <asp:HyperLink ID="hlLockedProfile" runat="server" NavigateUrl="~/Modules/Tutor/Profile" CssClass="btn btn-primary"
                Text="View my application" />
        </section>
    </asp:PlaceHolder>

    <%-- ===== No course in the link, or it belongs to someone else ===== --%>
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
            <p>Open a course from your dashboard to manage its lessons.</p>
            <asp:HyperLink ID="hlNotFoundBack" runat="server" NavigateUrl="~/Modules/Tutor/Dashboard" CssClass="btn btn-primary"
                Text="Back to dashboard" />
        </section>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phMain" runat="server" Visible="false">

        <asp:PlaceHolder ID="phReadOnlyNote" runat="server" Visible="false">
            <div class="cl-feedback cl-feedback-info" role="status">
                <span class="cl-builder-icon cl-tone-amber"><i data-lucide="lock-keyhole" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <strong>Lessons are read-only</strong>
                    <p><asp:Literal ID="litReadOnlyText" runat="server" Mode="Encode" /></p>
                </div>
            </div>
        </asp:PlaceHolder>

        <div class="cl-lesson-builder<%: CanEdit ? "" : " is-readonly" %>">

            <%-- ===== Syllabus: the lessons in order ===== --%>
            <aside class="cl-card cl-syllabus-card">
                <div class="cl-syllabus-head">
                    <div>
                        <h2>Syllabus</h2>
                        <p><asp:Literal ID="litLessonCount" runat="server" Mode="Encode" /></p>
                    </div>
                    <asp:HyperLink ID="hlNewLesson" runat="server" CssClass="cl-icon-btn" Visible="false" ToolTip="New lesson">
                        <i data-lucide="plus" class="cl-icon" aria-hidden="true"></i><span class="visually-hidden">New lesson</span>
                    </asp:HyperLink>
                </div>

                <asp:Repeater ID="rptLessons" runat="server" OnItemCommand="rptLessons_ItemCommand">
                    <HeaderTemplate><ol class="cl-syllabus"></HeaderTemplate>
                    <ItemTemplate>
                        <li class="cl-lesson-item<%# IsSelected((Lesson)Container.DataItem) ? " is-selected" : "" %>">
                            <span class="cl-lesson-number"><%# Eval("OrderIndex") %></span>
                            <div class="cl-lesson-body">
                                <%# LessonTitleHtml((Lesson)Container.DataItem) %>
                                <div class="cl-lesson-meta"><%# LessonMetaHtml((Lesson)Container.DataItem) %></div>
                            </div>
                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="cl-lesson-delete" CommandName="delete"
                                CommandArgument='<%# Eval("Id") %>' CausesValidation="false" Visible="<%# CanEdit %>"
                                OnClientClick="return confirm('Delete this lesson? This cannot be undone.');" ToolTip="Delete lesson">
                                <i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i><span class="visually-hidden">Delete lesson</span>
                            </asp:LinkButton>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ol></FooterTemplate>
                </asp:Repeater>

                <asp:PlaceHolder ID="phNoLessons" runat="server" Visible="false">
                    <div class="cl-syllabus-empty">
                        <svg viewBox="0 0 120 90" aria-hidden="true">
                            <rect x="22" y="14" width="76" height="62" rx="10" fill="currentColor" opacity=".08" />
                            <rect x="34" y="28" width="40" height="6" rx="3" fill="currentColor" opacity=".35" />
                            <rect x="34" y="42" width="52" height="5" rx="2.5" fill="currentColor" opacity=".2" />
                            <rect x="34" y="53" width="30" height="5" rx="2.5" fill="currentColor" opacity=".2" />
                            <circle cx="92" cy="66" r="14" fill="#C9527D" />
                            <path d="M92 59v14M85 66h14" stroke="#fff" stroke-width="3" stroke-linecap="round" />
                        </svg>
                        <strong>No lessons yet</strong>
                        <span>Learners go through lessons in the order shown here.</span>
                    </div>
                </asp:PlaceHolder>

                <asp:HyperLink ID="hlSubmitHint" runat="server" CssClass="cl-syllabus-footer" Visible="false">
                    <i data-lucide="send" class="cl-icon" aria-hidden="true"></i>Ready? Submit the course for review
                </asp:HyperLink>
            </aside>

            <%-- ===== Lesson form ===== --%>
            <asp:PlaceHolder ID="phForm" runat="server" Visible="false">
                <div class="cl-builder-main">
                    <asp:ValidationSummary ID="vsLesson" runat="server" ValidationGroup="Lesson" CssClass="cl-alert cl-alert-danger"
                        HeaderText="Please fix the following:" DisplayMode="BulletList" />

                    <%-- ValidateRequestMode="Disabled" lets tutors type code such as List<int>. That is safe
                         because lesson text is only ever shown as encoded text, never as raw HTML. --%>

                    <%-- Title, order and the lesson text --%>
                    <section class="cl-card cl-builder-card cl-form">
                        <div class="cl-lesson-form-head">
                            <span class="cl-eyebrow"><asp:Literal ID="litFormTitle" runat="server" Mode="Encode" /></span>
                            <div class="cl-lesson-title-row">
                                <div class="cl-field cl-grow">
                                    <asp:Label ID="lblTitle" runat="server" AssociatedControlID="txtTitle" CssClass="visually-hidden" Text="Lesson title" />
                                    <asp:TextBox ID="txtTitle" runat="server" ValidateRequestMode="Disabled" CssClass="form-control cl-title-input" MaxLength="200"
                                        placeholder="Lesson title, e.g. Introduction to TypeScript types" />
                                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle"
                                        ErrorMessage="Title is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Lesson" />
                                </div>
                            </div>
                        </div>

                        <div class="cl-writer" id="lessonWriter" data-mode="write">
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
                                    <button type="button" data-md="quote" title="Quote"><i data-lucide="quote" class="cl-icon" aria-hidden="true"></i></button>
                                    <span class="cl-md-divider" aria-hidden="true"></span>
                                    <button type="button" data-md="code" title="Inline code"><i data-lucide="code" class="cl-icon" aria-hidden="true"></i></button>
                                    <button type="button" data-md="codeblock" title="Code block"><i data-lucide="square-code" class="cl-icon" aria-hidden="true"></i></button>
                                    <button type="button" data-md="link" title="Link"><i data-lucide="link" class="cl-icon" aria-hidden="true"></i></button>
                                </div>
                            </div>

                            <div class="cl-writer-pane cl-writer-write">
                                <asp:Label ID="lblContent" runat="server" AssociatedControlID="txtContent" CssClass="visually-hidden" Text="Lesson content" />
                                <asp:TextBox ID="txtContent" runat="server" ValidateRequestMode="Disabled" CssClass="form-control cl-md-input"
                                    TextMode="MultiLine" Rows="16"
                                    placeholder="Explain the topic in Markdown. Use the buttons above for headings, lists, code and links." />
                            </div>

                            <div class="cl-writer-pane cl-writer-preview" id="lessonPreview">
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
                        <asp:RequiredFieldValidator ID="rfvContent" runat="server" ControlToValidate="txtContent"
                            ErrorMessage="Content is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Lesson" />
                    </section>

                    <%-- Code example: looks like a code editor --%>
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-blue"><i data-lucide="square-terminal" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Code example</h2><p>Optional. Shown under the lesson text with syntax colours.</p></div>
                        </div>

                        <div class="cl-ide">
                            <div class="cl-ide-bar">
                                <span class="cl-ide-dots" aria-hidden="true"><i></i><i></i><i></i></span>
                                <asp:Label ID="lblLanguage" runat="server" AssociatedControlID="ddlLanguage" CssClass="visually-hidden" Text="Code language" />
                                <asp:DropDownList ID="ddlLanguage" runat="server" CssClass="cl-ide-select" />
                            </div>
                            <asp:Label ID="lblCode" runat="server" AssociatedControlID="txtCode" CssClass="visually-hidden" Text="Code example" />
                            <asp:TextBox ID="txtCode" runat="server" ValidateRequestMode="Disabled" CssClass="cl-ide-input"
                                TextMode="MultiLine" Rows="10" spellcheck="false"
                                placeholder="const greeting: string = &quot;Hello, CodeLab!&quot;;" />
                        </div>
                    </section>

                    <%-- Video and attachment --%>
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-amber"><i data-lucide="clapperboard" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Video and files</h2><p>Optional extras for this lesson.</p></div>
                        </div>

                        <div class="cl-media-grid">
                            <div class="cl-field">
                                <asp:Label ID="lblVideoUrl" runat="server" AssociatedControlID="txtVideoUrl" CssClass="form-label" Text="YouTube video" />
                                <div class="cl-input-icon">
                                    <i data-lucide="video" class="cl-icon" aria-hidden="true"></i>
                                    <asp:TextBox ID="txtVideoUrl" runat="server" CssClass="form-control" MaxLength="500"
                                        placeholder="https://www.youtube.com/watch?v=..." aria-describedby="videoHelp" />
                                </div>
                                <div class="cl-video-thumb" id="videoThumb">
                                    <div class="cl-video-empty">
                                        <i data-lucide="circle-play" class="cl-icon" aria-hidden="true"></i>
                                        <span id="videoHelp">Paste a YouTube link. Learners watch it inside the lesson; nothing is uploaded.</span>
                                    </div>
                                </div>
                            </div>

                            <div class="cl-field">
                                <asp:Label ID="lblAttachment" runat="server" AssociatedControlID="fuAttachment" CssClass="form-label" Text="Attachment" />
                                <div class="cl-dropzone cl-dropzone-stack">
                                    <span class="cl-dropzone-icon"><i data-lucide="paperclip" class="cl-icon" aria-hidden="true"></i></span>
                                    <div class="cl-dropzone-text">
                                        <strong>Drop a file, or <span class="cl-dropzone-link">browse</span></strong>
                                        <span id="attachmentHelp">PDF, ZIP, TXT, DOCX, PPTX, XLSX or an image, up to 5 MB. No video.</span>
                                        <span class="cl-dropzone-file" id="attachmentFileName"></span>
                                    </div>
                                    <asp:FileUpload ID="fuAttachment" runat="server" CssClass="cl-dropzone-input"
                                        accept=".pdf,.zip,.txt,.docx,.pptx,.xlsx,.png,.jpg,.jpeg,.webp" aria-describedby="attachmentHelp" />
                                </div>
                                <div id="attachmentClientError" class="cl-field-error" role="alert"></div>

                                <asp:PlaceHolder ID="phCurrentAttachment" runat="server" Visible="false">
                                    <div class="cl-file-chip">
                                        <span class="cl-builder-icon cl-tone-pink"><i data-lucide="file" class="cl-icon" aria-hidden="true"></i></span>
                                        <div class="cl-file-chip-body">
                                            <small>Current file</small>
                                            <asp:HyperLink ID="hlCurrentAttachment" runat="server" Target="_blank" CssClass="cl-break" />
                                        </div>
                                        <%-- A Remove button that ticks a hidden checkbox; the file is removed when the lesson is saved --%>
                                        <div class="cl-remove-toggle">
                                            <asp:CheckBox ID="chkRemoveAttachment" runat="server" />
                                            <label for="<%= chkRemoveAttachment.ClientID %>" class="cl-remove-btn" title="Remove this file when the lesson is saved">
                                                <span class="cl-remove-off"><i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i>Remove</span>
                                                <span class="cl-remove-on"><i data-lucide="undo-2" class="cl-icon" aria-hidden="true"></i>Undo</span>
                                            </label>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                            </div>
                        </div>
                    </section>

                    <%-- Save bar: stays at the bottom of the screen while scrolling --%>
                    <div class="cl-action-bar">
                        <span class="cl-action-bar-note">
                            <i data-lucide="keyboard" class="cl-icon" aria-hidden="true"></i>
                            Tip: switch to Preview to see the lesson as learners will.
                        </span>
                        <div class="cl-action-bar-buttons">
                            <asp:HyperLink ID="hlCancel" runat="server" CssClass="btn cl-btn-outline" Text="Cancel" />
                            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Lesson" OnClick="btnSave_Click" />
                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>
        </div>
    </asp:PlaceHolder>

    <%-- Syntax colours for code in the preview; the writer, preview tabs and code boxes come from tutor-writer.js --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/tutor-writer.js") %>"></script>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var video = document.getElementById('<%= txtVideoUrl.ClientID %>');
            if (!video) {
                return;
            }

            // ----- YouTube thumbnail -----
            var thumb = document.getElementById('videoThumb');
            var emptyThumb = thumb.innerHTML;

            function youTubeId(url) {
                var match = /(?:youtube\.com\/(?:watch\?(?:.*&)?v=|embed\/|shorts\/|live\/)|youtu\.be\/)([A-Za-z0-9_-]{11})/.exec(url || '');
                return match ? match[1] : null;
            }

            function updateThumb() {
                var value = video.value.trim();
                var id = youTubeId(value);
                if (id) {
                    thumb.innerHTML = '<img src="https://i.ytimg.com/vi/' + id + '/hqdefault.jpg" alt="Video thumbnail" />'
                        + '<span class="cl-video-play" aria-hidden="true"></span>';
                    thumb.classList.add('has-video');
                } else {
                    thumb.innerHTML = value
                        ? '<div class="cl-video-empty is-error"><span>That does not look like a YouTube link.</span></div>'
                        : emptyThumb;
                    thumb.classList.remove('has-video');
                    if (!value && window.lucide) { window.lucide.createIcons(); }
                }
            }
            video.addEventListener('input', updateThumb);
            updateThumb();

            // ----- Attachment: checked in the browser first; the server checks again in LessonManager -----
            var file = document.getElementById('<%= fuAttachment.ClientID %>');
            var error = document.getElementById('attachmentClientError');
            var fileName = document.getElementById('attachmentFileName');
            var drop = file.parentNode;

            ['dragenter', 'dragover'].forEach(function (name) {
                drop.addEventListener(name, function () { drop.classList.add('is-dragging'); });
            });
            ['dragleave', 'drop'].forEach(function (name) {
                drop.addEventListener(name, function () { drop.classList.remove('is-dragging'); });
            });

            file.addEventListener('change', function () {
                var chosen = file.files && file.files[0];
                error.textContent = '';
                fileName.textContent = '';
                if (!chosen) { return; }

                if (!/\.(pdf|zip|txt|docx|pptx|xlsx|png|jpe?g|webp)$/i.test(chosen.name)) {
                    error.textContent = 'Attachments can be PDF, ZIP, TXT, DOCX, PPTX, XLSX or image files (no video).';
                    file.value = '';
                } else if (chosen.size > <%= CodeLab.BLL.LessonManager.MaxAttachmentBytes %>) {
                    error.textContent = 'The attachment must be 5 MB or smaller.';
                    file.value = '';
                } else {
                    fileName.textContent = chosen.name;
                }
            });
        });
    </script>

</asp:Content>
