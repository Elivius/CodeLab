<%@ Page Title="Course Builder" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="CourseEditor.aspx.cs" Inherits="CodeLab.Modules.Tutor.CourseEditor" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>

<asp:Content ID="CourseEditorContent" ContentPlaceHolderID="TutorContent" runat="server">

    <%-- ===== Header: breadcrumb, title, status ===== --%>
    <div class="cl-builder-header">
        <div class="cl-builder-heading">
            <nav class="cl-crumbs" aria-label="Breadcrumb">
                <a href="<%: Master.GetPageUrl("Dashboard") %>">Dashboard</a>
                <i data-lucide="chevron-right" class="cl-icon" aria-hidden="true"></i>
                <span>Course builder</span>
            </nav>
            <h1><asp:Literal ID="litHeading" runat="server" Mode="Encode" /></h1>
            <p><asp:Literal ID="litSubheading" runat="server" Mode="Encode" /></p>
        </div>
        <asp:Label ID="lblCourseStatus" runat="server" CssClass="cl-badge cl-status cl-badge-lg" Visible="false" />
    </div>

    <%-- ===== Progress: Details > Lessons > Quiz > Challenge > Submit ===== --%>
    <% if (phForm.Visible || phSummary.Visible) { %>
        <%= Master.BuilderSteps(IsSubmitStep ? 5 : 1, CourseId) %>
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
            <p>You can create courses once an administrator approves your tutor application.</p>
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
            <asp:HyperLink ID="hlNotFoundBack" runat="server" NavigateUrl="~/Modules/Tutor/Dashboard" CssClass="btn btn-primary"
                Text="Back to dashboard" />
        </section>
    </asp:PlaceHolder>

    <%-- ===== Admin feedback on a course sent back to Draft (Details and Review steps) ===== --%>
    <asp:PlaceHolder ID="phRejected" runat="server" Visible="false">
        <div class="cl-feedback" role="status">
            <span class="cl-builder-icon cl-tone-red"><i data-lucide="message-square-warning" class="cl-icon" aria-hidden="true"></i></span>
            <div>
                <strong>Changes requested by an administrator</strong>
                <p><asp:Literal ID="litRejectedReason" runat="server" Mode="Encode" /></p>
            </div>
        </div>
    </asp:PlaceHolder>

    <%-- ===== Course summary: step 5 (Review & submit), or a course that can no longer be edited ===== --%>
    <asp:PlaceHolder ID="phSummary" runat="server" Visible="false">
        <div class="cl-builder">
            <div class="cl-builder-main">
                <section class="cl-card cl-readonly-card">
                    <div class="cl-readonly-cover <%: CoverToneCss %>">
                        <% if (CoverSrc != null) { %>
                            <img src="<%: CoverSrc %>" alt="Course cover" />
                        <% } else { %>
                            <span class="cl-course-card-letter" aria-hidden="true"><%: PlaceholderLetter %></span>
                        <% } %>
                    </div>
                    <div class="cl-readonly-body">
                        <h2><asp:Literal ID="litReadOnlyTitle" runat="server" Mode="Encode" /></h2>
                        <div class="cl-readonly-facts">
                            <span><i data-lucide="tag" class="cl-icon" aria-hidden="true"></i><asp:Literal ID="litReadOnlyCategory" runat="server" Mode="Encode" /></span>
                            <span><i data-lucide="gauge" class="cl-icon" aria-hidden="true"></i><asp:Literal ID="litReadOnlyDifficulty" runat="server" Mode="Encode" /></span>
                            <span><i data-lucide="notebook-text" class="cl-icon" aria-hidden="true"></i><%: LessonCount == 1 ? "1 lesson" : LessonCount + " lessons" %></span>
                        </div>
                        <h3 class="cl-builder-label">About this course</h3>
                        <p class="cl-prose"><asp:Literal ID="litReadOnlyDescription" runat="server" Mode="Encode" /></p>
                    </div>
                </section>

                <%-- Step 5: everything the course contains, each linking to its step --%>
                <asp:PlaceHolder ID="phChecklist" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-green"><i data-lucide="list-checks" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Course content</h2><p>Check everything is ready. Click an item to go back and change it.</p></div>
                        </div>
                        <ul class="cl-readiness">
                            <%= DetailsReadinessHtml %>
                            <%= LessonsReadinessHtml %>
                            <%= QuizReadinessHtml %>
                            <%= ChallengeReadinessHtml %>
                        </ul>
                    </section>
                </asp:PlaceHolder>
            </div>

            <aside class="cl-builder-aside">
                <%-- Step 5 for a Draft: send it to an administrator --%>
                <asp:PlaceHolder ID="phSubmit" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card" id="submit">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-pink"><i data-lucide="send" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Submit for review</h2><p>An admin checks every course before learners see it.</p></div>
                        </div>
                        <p class="cl-builder-note">You cannot edit the course while it is being reviewed. If the admin asks for changes, it comes back to you as a draft.</p>
                        <asp:Label ID="lblSubmitBlocked" runat="server" CssClass="cl-submit-blocked" Visible="false" />
                        <asp:Button ID="btnSubmitReview" runat="server" Text="Submit for review" CssClass="btn btn-primary w-100 mt-3"
                            CausesValidation="false" OnClick="btnSubmitReview_Click"
                            OnClientClick="return confirm('Submit this course for review? You cannot edit it while an administrator reviews it.');" />
                    </section>
                </asp:PlaceHolder>

                <%-- A course that is not a Draft --%>
                <asp:PlaceHolder ID="phEditingClosed" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-amber"><i data-lucide="shield-check" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>Editing is closed</h2><p>This course is read-only right now.</p></div>
                        </div>
                        <p class="cl-builder-note"><asp:Literal ID="litReadOnlyNote" runat="server" Mode="Encode" /></p>
                        <asp:HyperLink ID="hlBackToDashboard" runat="server" NavigateUrl="~/Modules/Tutor/Dashboard" CssClass="btn cl-btn-outline w-100 mt-3"
                            Text="Back to dashboard" />
                    </section>
                </asp:PlaceHolder>
            </aside>
        </div>
    </asp:PlaceHolder>

    <%-- ===== Create / edit a Draft (including one sent back with admin feedback) ===== --%>
    <asp:PlaceHolder ID="phForm" runat="server" Visible="false">
        <div class="cl-builder">
            <div class="cl-builder-main">
                <asp:ValidationSummary ID="vsCourse" runat="server" ValidationGroup="Course" CssClass="cl-alert cl-alert-danger"
                    HeaderText="Please fix the following:" DisplayMode="BulletList" />

                <%-- ValidateRequestMode="Disabled" lets tutors type code such as List<int>. That is safe
                     because course text is only ever shown as encoded text, never as HTML. --%>

                <%-- Basics --%>
                <section class="cl-card cl-builder-card cl-form">
                    <div class="cl-builder-card-head">
                        <span class="cl-builder-icon cl-tone-pink"><i data-lucide="type" class="cl-icon" aria-hidden="true"></i></span>
                        <div><h2>The basics</h2><p>Learners see these first on the course catalogue.</p></div>
                    </div>

                    <div class="cl-field">
                        <div class="cl-field-label">
                            <asp:Label ID="lblTitle" runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Course title" />
                            <span class="cl-counter" data-counter-for="<%= txtTitle.ClientID %>" data-max="200"></span>
                        </div>
                        <asp:TextBox ID="txtTitle" runat="server" ValidateRequestMode="Disabled" CssClass="form-control cl-input-lg" MaxLength="200"
                            placeholder="e.g. Python Essentials: From Zero to Automated Scripts" />
                        <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle"
                            ErrorMessage="Title is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Course" />
                    </div>

                    <div class="cl-field-row">
                        <div class="cl-field">
                            <asp:Label ID="lblCategory" runat="server" AssociatedControlID="ddlCategory" CssClass="form-label" Text="Category" />
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
                            <asp:RequiredFieldValidator ID="rfvCategory" runat="server" ControlToValidate="ddlCategory" InitialValue=""
                                ErrorMessage="Choose a category." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Course" />
                        </div>
                        <div class="cl-field">
                            <asp:Label ID="lblDifficulty" runat="server" AssociatedControlID="ddlDifficulty" CssClass="form-label" Text="Difficulty" />
                            <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select" />
                            <asp:RequiredFieldValidator ID="rfvDifficulty" runat="server" ControlToValidate="ddlDifficulty" InitialValue=""
                                ErrorMessage="Choose a difficulty." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Course" />
                        </div>
                    </div>
                </section>

                <%-- Description --%>
                <section class="cl-card cl-builder-card cl-form">
                    <div class="cl-builder-card-head">
                        <span class="cl-builder-icon cl-tone-blue"><i data-lucide="align-left" class="cl-icon" aria-hidden="true"></i></span>
                        <div><h2>Description</h2><p>What will learners be able to do afterwards, and who is the course for?</p></div>
                    </div>

                    <div class="cl-field">
                        <div class="cl-field-label">
                            <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription" CssClass="form-label" Text="About this course" />
                            <span class="cl-counter" data-counter-for="<%= txtDescription.ClientID %>"></span>
                        </div>
                        <asp:TextBox ID="txtDescription" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="8"
                            placeholder="e.g. Learn Python step by step, from variables to writing scripts that automate everyday tasks. No experience needed." />
                        <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription"
                            ErrorMessage="Description is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Course" />
                    </div>
                </section>

                <%-- Cover image --%>
                <section class="cl-card cl-builder-card">
                    <div class="cl-builder-card-head">
                        <span class="cl-builder-icon cl-tone-amber"><i data-lucide="image" class="cl-icon" aria-hidden="true"></i></span>
                        <div><h2>Cover image</h2><p>Optional. Without one, a coloured cover with the first letter is shown.</p></div>
                    </div>

                    <div class="cl-dropzone" id="coverDrop">
                        <div class="cl-dropzone-preview <%: CoverToneCss %>" id="coverPreview">
                            <% if (CoverSrc != null) { %>
                                <img src="<%: CoverSrc %>" alt="Course cover" />
                            <% } else { %>
                                <span class="cl-course-card-letter" aria-hidden="true"><%: PlaceholderLetter %></span>
                            <% } %>
                        </div>
                        <div class="cl-dropzone-text">
                            <span class="cl-dropzone-icon"><i data-lucide="upload-cloud" class="cl-icon" aria-hidden="true"></i></span>
                            <strong>Drop an image here, or <span class="cl-dropzone-link">browse</span></strong>
                            <span id="coverHelp">JPG, PNG or WebP, up to 2 MB. 16:9 works best (e.g. 1280 &times; 720).</span>
                            <span class="cl-dropzone-file" id="coverFileName"></span>
                        </div>
                        <asp:Label ID="lblCover" runat="server" AssociatedControlID="fuCover" CssClass="visually-hidden" Text="Upload a cover image" />
                        <asp:FileUpload ID="fuCover" runat="server" CssClass="cl-dropzone-input"
                            accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" aria-describedby="coverHelp" />
                    </div>
                    <div id="coverClientError" class="cl-field-error" role="alert"></div>

                    <asp:PlaceHolder ID="phRemoveCover" runat="server" Visible="false">
                        <%-- A Remove button that ticks a hidden checkbox; the cover is removed when the course is saved --%>
                        <div class="cl-remove-toggle mt-3">
                            <asp:CheckBox ID="chkRemoveCover" runat="server" />
                            <label for="<%= chkRemoveCover.ClientID %>" class="cl-remove-btn">
                                <span class="cl-remove-off"><i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i>Remove cover</span>
                                <span class="cl-remove-on"><i data-lucide="undo-2" class="cl-icon" aria-hidden="true"></i>Undo</span>
                            </label>
                            <span class="cl-remove-note">The cover will be removed when you save.</span>
                        </div>
                    </asp:PlaceHolder>
                </section>

                <%-- Save bar: stays at the bottom of the screen while scrolling --%>
                <div class="cl-action-bar">
                    <span class="cl-action-bar-note">
                        <i data-lucide="lock-keyhole" class="cl-icon" aria-hidden="true"></i>
                        Drafts are private. Only you can see them.
                    </span>
                    <div class="cl-action-bar-buttons">
                        <asp:HyperLink ID="hlCancel" runat="server" NavigateUrl="~/Modules/Tutor/Dashboard" CssClass="btn cl-btn-outline" Text="Cancel" />
                        <asp:Button ID="btnSave" runat="server" CssClass="btn cl-btn-outline" Text="Save" ValidationGroup="Course" OnClick="btnSave_Click" />
                        <%-- Saves, then moves on to step 2 (Lessons) --%>
                        <asp:Button ID="btnSaveContinue" runat="server" CssClass="btn btn-primary" ValidationGroup="Course" OnClick="btnSaveContinue_Click" />
                    </div>
                </div>
            </div>

            <aside class="cl-builder-aside">
                <%-- Live preview of the catalogue card --%>
                <section class="cl-card cl-builder-card">
                    <div class="cl-builder-card-head">
                        <span class="cl-builder-icon cl-tone-green"><i data-lucide="eye" class="cl-icon" aria-hidden="true"></i></span>
                        <div><h2>Catalogue preview</h2><p>Updates as you type.</p></div>
                    </div>

                    <article class="cl-course-card cl-preview-card" aria-label="Preview of the course card">
                        <div class="cl-course-card-cover <%: CoverToneCss %>" id="previewCover">
                            <% if (CoverSrc != null) { %>
                                <img src="<%: CoverSrc %>" alt="" />
                            <% } else { %>
                                <span class="cl-course-card-letter" id="previewLetter"><%: PlaceholderLetter %></span>
                            <% } %>
                        </div>
                        <div class="cl-course-card-body">
                            <div class="cl-course-card-tags">
                                <span class="cl-badge cl-badge-neutral" id="previewDifficulty">Difficulty</span>
                                <span class="cl-course-card-category" id="previewCategory">Category</span>
                            </div>
                            <h3 id="previewTitle">Your course title</h3>
                            <p class="cl-preview-desc" id="previewDescription">Your description appears here.</p>
                            <div class="cl-course-card-meta">
                                <span><i data-lucide="notebook-text" class="cl-icon" aria-hidden="true"></i><%: LessonCount == 1 ? "1 lesson" : LessonCount + " lessons" %></span>
                                <span><i data-lucide="user-round" class="cl-icon" aria-hidden="true"></i><%: Master.TutorName %></span>
                            </div>
                        </div>
                    </article>
                </section>

                <%-- What happens next (new course) --%>
                <asp:PlaceHolder ID="phNextSteps" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-blue"><i data-lucide="route" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>What happens next</h2><p>Four short steps to a published course.</p></div>
                        </div>
                        <ol class="cl-roadmap">
                            <li class="is-current"><strong>Save as a draft</strong><span>Only you can see it.</span></li>
                            <li><strong>Add lessons</strong><span>Notes, code examples and videos.</span></li>
                            <li><strong>Add a quiz and a challenge</strong><span>Optional, but learners love them.</span></li>
                            <li><strong>Submit for review</strong><span>An admin checks it, then it goes live.</span></li>
                        </ol>
                    </section>
                </asp:PlaceHolder>
            </aside>
        </div>
    </asp:PlaceHolder>

    <%-- Back / next step (step 5, and courses that can no longer be edited) --%>
    <%= StepNavHtml %>

    <%-- Live preview, character counters, and the cover check. The server checks the cover again in CourseManager. --%>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var title = document.getElementById('<%= txtTitle.ClientID %>');
            if (!title) {
                return;
            }

            var description = document.getElementById('<%= txtDescription.ClientID %>');
            var category = document.getElementById('<%= ddlCategory.ClientID %>');
            var difficulty = document.getElementById('<%= ddlDifficulty.ClientID %>');
            var input = document.getElementById('<%= fuCover.ClientID %>');
            var removeCover = document.getElementById('<%= chkRemoveCover.ClientID %>');
            var drop = document.getElementById('coverDrop');
            var error = document.getElementById('coverClientError');
            var fileName = document.getElementById('coverFileName');
            var previewCover = document.getElementById('previewCover');
            var dropPreview = document.getElementById('coverPreview');
            var difficultyCss = { Beginner: 'cl-badge-success', Intermediate: 'cl-badge-info', Advanced: 'cl-badge-danger' };
            var savedCover = previewCover.innerHTML;

            function setText(id, value, fallback) {
                document.getElementById(id).textContent = value.trim() || fallback;
            }

            function letter() {
                var text = title.value.trim();
                return text ? text.charAt(0).toUpperCase() : 'C';
            }

            function updatePreview() {
                setText('previewTitle', title.value, 'Your course title');
                setText('previewDescription', description.value, 'Your description appears here.');
                setText('previewCategory', category.value, 'Category');

                var badge = document.getElementById('previewDifficulty');
                badge.textContent = difficulty.value || 'Difficulty';
                badge.className = 'cl-badge ' + (difficultyCss[difficulty.value] || 'cl-badge-neutral');

                document.querySelectorAll('#previewCover .cl-course-card-letter, #coverPreview .cl-course-card-letter').forEach(function (el) {
                    el.textContent = letter();
                });
            }

            function showCover(html) {
                previewCover.innerHTML = html;
                dropPreview.innerHTML = html;
                updatePreview();
            }

            function placeholder() {
                return '<span class="cl-course-card-letter" aria-hidden="true"></span>';
            }

            [title, description, category, difficulty].forEach(function (el) {
                el.addEventListener('input', updatePreview);
                el.addEventListener('change', updatePreview);
            });

            // Character counters next to the labels
            document.querySelectorAll('.cl-counter').forEach(function (counter) {
                var field = document.getElementById(counter.getAttribute('data-counter-for'));
                var max = counter.getAttribute('data-max');
                function count() {
                    counter.textContent = field.value.length + (max ? ' / ' + max : ' characters');
                }
                field.addEventListener('input', count);
                count();
            });

            // Cover: highlight while a file is dragged over, then check and preview it
            ['dragenter', 'dragover'].forEach(function (name) {
                drop.addEventListener(name, function () { drop.classList.add('is-dragging'); });
            });
            ['dragleave', 'drop'].forEach(function (name) {
                drop.addEventListener(name, function () { drop.classList.remove('is-dragging'); });
            });

            input.addEventListener('change', function () {
                var file = input.files && input.files[0];
                error.textContent = '';
                fileName.textContent = '';
                if (!file) {
                    showCover(savedCover);
                    return;
                }

                var message = '';
                if (!/\.(jpe?g|png|webp)$/i.test(file.name)) {
                    message = 'The cover image must be a JPG, PNG or WebP image.';
                } else if (file.size > <%= CodeLab.BLL.CourseManager.MaxCoverBytes %>) {
                    message = 'The cover image must be 2 MB or smaller.';
                }

                if (message) {
                    error.textContent = message;
                    input.value = '';
                    showCover(savedCover);
                    return;
                }

                fileName.textContent = file.name;
                if (removeCover) {
                    removeCover.checked = false;
                }

                var img = '<img src="' + URL.createObjectURL(file) + '" alt="" />';
                showCover(img);
            });

            if (removeCover) {
                removeCover.addEventListener('change', function () {
                    showCover(removeCover.checked ? placeholder() : savedCover);
                });
            }

            updatePreview();
        });
    </script>

</asp:Content>
