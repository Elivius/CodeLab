<%@ Page Title="Quiz" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="QuizEditor.aspx.cs" Inherits="CodeLab.Modules.Tutor.QuizEditor" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>
<%@ Import Namespace="CodeLab.Models" %>

<asp:Content ID="QuizEditorContent" ContentPlaceHolderID="TutorContent" runat="server">

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
                <span>Quiz</span>
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
        <%= Master.BuilderSteps(3, CourseId) %>
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
            <p>You can add quizzes once an administrator approves your tutor application.</p>
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
            <asp:HyperLink ID="hlNotFoundBack" runat="server" NavigateUrl="~/Modules/Tutor/QuizEditor" CssClass="btn btn-primary"
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
                        <i data-lucide="<%# (bool)Eval("HasItem") ? "list-checks" : "list-plus" %>" class="cl-icon" aria-hidden="true"></i>
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
                <p>Create a course first, then add its quiz.</p>
                <a class="btn btn-primary" href="<%: Master.GetPageUrl("CourseEditor") %>">Create a course</a>
            </section>
        </asp:PlaceHolder>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phMain" runat="server" Visible="false">

        <asp:PlaceHolder ID="phReadOnlyNote" runat="server" Visible="false">
            <div class="cl-feedback cl-feedback-info" role="status">
                <span class="cl-builder-icon cl-tone-amber"><i data-lucide="lock-keyhole" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <strong>The quiz is read-only</strong>
                    <p><asp:Literal ID="litReadOnlyText" runat="server" Mode="Encode" /></p>
                </div>
            </div>
        </asp:PlaceHolder>

        <div class="cl-builder cl-quiz-builder<%: CanEdit ? "" : " is-single" %>">
            <div class="cl-builder-main">

                <%-- ===== Quiz details: create the quiz, or change its title, pass mark and instructions ===== --%>
                <asp:PlaceHolder ID="phDetailsForm" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card cl-form">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-pink"><i data-lucide="clipboard-list" class="cl-icon" aria-hidden="true"></i></span>
                            <div>
                                <h2><asp:Literal ID="litDetailsHeading" runat="server" Mode="Encode" /></h2>
                                <p><asp:Literal ID="litDetailsNote" runat="server" Mode="Encode" /></p>
                            </div>
                        </div>

                        <asp:ValidationSummary ID="vsQuiz" runat="server" ValidationGroup="Quiz" CssClass="cl-alert cl-alert-danger"
                            HeaderText="Please fix the following:" DisplayMode="BulletList" />

                        <div class="cl-quiz-details-row">
                            <div class="cl-field cl-grow">
                                <asp:Label ID="lblQuizTitle" runat="server" AssociatedControlID="txtQuizTitle" CssClass="form-label" Text="Quiz title" />
                                <asp:TextBox ID="txtQuizTitle" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="150"
                                    placeholder="e.g. Python basics check" />
                                <asp:RequiredFieldValidator ID="rfvQuizTitle" runat="server" ControlToValidate="txtQuizTitle"
                                    ErrorMessage="The quiz title is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Quiz" />
                            </div>
                            <div class="cl-field cl-passmark-field">
                                <asp:Label ID="lblPassMark" runat="server" AssociatedControlID="txtPassMark" CssClass="form-label" Text="Pass mark" />
                                <div class="cl-input-suffix">
                                    <asp:TextBox ID="txtPassMark" runat="server" CssClass="form-control" TextMode="Number" min="0" max="100" />
                                    <span aria-hidden="true">%</span>
                                </div>
                                <asp:RequiredFieldValidator ID="rfvPassMark" runat="server" ControlToValidate="txtPassMark"
                                    ErrorMessage="The pass mark is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Quiz" />
                                <asp:RangeValidator ID="rvPassMark" runat="server" ControlToValidate="txtPassMark" Type="Integer" MinimumValue="0" MaximumValue="100"
                                    ErrorMessage="The pass mark must be between 0 and 100." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Quiz" />
                            </div>
                        </div>

                        <div class="cl-field">
                            <asp:Label ID="lblInstructions" runat="server" AssociatedControlID="txtInstructions" CssClass="form-label" Text="Instructions (optional)" />
                            <asp:TextBox ID="txtInstructions" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="3"
                                MaxLength="2000" placeholder="e.g. Answer every question. You can retake the quiz as often as you like." />
                        </div>

                        <div class="cl-card-actions">
                            <asp:LinkButton ID="btnDeleteQuiz" runat="server" CssClass="cl-link-danger cl-card-actions-left" CausesValidation="false"
                                OnClick="btnDeleteQuiz_Click"
                                OnClientClick="return confirm('Delete this quiz and all of its questions? This cannot be undone.');">
                                <i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i>Delete quiz
                            </asp:LinkButton>
                            <asp:Button ID="btnSaveQuiz" runat="server" CssClass="btn btn-primary" ValidationGroup="Quiz" OnClick="btnSaveQuiz_Click" />
                        </div>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== Quiz details, read-only (course is not a Draft) ===== --%>
                <asp:PlaceHolder ID="phDetailsReadOnly" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-pink"><i data-lucide="clipboard-list" class="cl-icon" aria-hidden="true"></i></span>
                            <div>
                                <h2><asp:Literal ID="litRoTitle" runat="server" Mode="Encode" /></h2>
                                <p>Pass mark <asp:Literal ID="litRoPassMark" runat="server" Mode="Encode" />%</p>
                            </div>
                        </div>
                        <p class="cl-builder-note cl-prose"><asp:Literal ID="litRoInstructions" runat="server" Mode="Encode" /></p>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== Course has no quiz and cannot be changed ===== --%>
                <asp:PlaceHolder ID="phNoQuiz" runat="server" Visible="false">
                    <section class="cl-card cl-state-card">
                        <h2>No quiz</h2>
                        <p>This course was submitted without a quiz.</p>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== The questions ===== --%>
                <asp:PlaceHolder ID="phQuestions" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-blue"><i data-lucide="message-circle-question" class="cl-icon" aria-hidden="true"></i></span>
                            <div>
                                <h2>Questions</h2>
                                <p><asp:Literal ID="litQuestionCount" runat="server" Mode="Encode" /></p>
                            </div>
                        </div>

                        <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="rptQuestions_ItemCommand">
                            <HeaderTemplate><ol class="cl-question-list"></HeaderTemplate>
                            <ItemTemplate>
                                <li class="cl-question<%# IsSelected((QuizQuestion)Container.DataItem) ? " is-selected" : "" %>">
                                    <div class="cl-question-head">
                                        <span class="cl-lesson-number"><%# Eval("OrderIndex") %></span>
                                        <div class="cl-question-text">
                                            <strong><%#: Eval("QuestionText") %></strong>
                                            <small><%#: (string)Eval("QuestionType") == QuizQuestion.TrueFalse ? "True / False" : "Multiple choice" %></small>
                                        </div>
                                        <%# EditLinkHtml((QuizQuestion)Container.DataItem) %>
                                        <asp:LinkButton ID="btnDeleteQuestion" runat="server" CssClass="cl-lesson-delete" CommandName="delete"
                                            CommandArgument='<%# Eval("Id") %>' CausesValidation="false" Visible="<%# CanEdit %>"
                                            OnClientClick="return confirm('Delete this question? This cannot be undone.');" ToolTip="Delete question">
                                            <i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i><span class="visually-hidden">Delete question</span>
                                        </asp:LinkButton>
                                    </div>
                                    <%# OptionsHtml((QuizQuestion)Container.DataItem) %>
                                    <%# ExplanationHtml((QuizQuestion)Container.DataItem) %>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate></ol></FooterTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder ID="phNoQuestions" runat="server" Visible="false">
                            <div class="cl-syllabus-empty">
                                <svg viewBox="0 0 120 90" aria-hidden="true">
                                    <rect x="20" y="14" width="80" height="62" rx="10" fill="currentColor" opacity=".08" />
                                    <circle cx="38" cy="34" r="5" fill="currentColor" opacity=".3" />
                                    <rect x="50" y="31" width="36" height="6" rx="3" fill="currentColor" opacity=".25" />
                                    <circle cx="38" cy="54" r="5" fill="#C9527D" />
                                    <rect x="50" y="51" width="28" height="6" rx="3" fill="currentColor" opacity=".25" />
                                </svg>
                                <strong>No questions yet</strong>
                                <span><%: CanEdit ? "Write the first one in the panel on the right." : "This quiz has no questions." %></span>
                            </div>
                        </asp:PlaceHolder>
                    </section>
                </asp:PlaceHolder>
            </div>

            <aside class="cl-builder-aside">
                <%-- ===== Before the quiz exists: how quizzes work ===== --%>
                <asp:PlaceHolder ID="phHelp" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-blue"><i data-lucide="lightbulb" class="cl-icon" aria-hidden="true"></i></span>
                            <div><h2>How the quiz works</h2><p>Optional, but it helps learners check what they learned.</p></div>
                        </div>
                        <ol class="cl-roadmap">
                            <li class="is-current"><strong>One quiz per course</strong><span>It holds all of the course's questions.</span></li>
                            <li><strong>Add questions</strong><span>Multiple choice (2 to 4 options) or True / False.</span></li>
                            <li><strong>Set a pass mark</strong><span>Learners pass when their score reaches it.</span></li>
                            <li><strong>Submit the course</strong><span>A quiz needs at least one question first.</span></li>
                        </ol>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== Write or edit a question ===== --%>
                <asp:PlaceHolder ID="phQuestionForm" runat="server" Visible="false">
                    <section class="cl-card cl-builder-card cl-form" id="questionForm">
                        <div class="cl-builder-card-head">
                            <span class="cl-builder-icon cl-tone-green"><i data-lucide="pencil-line" class="cl-icon" aria-hidden="true"></i></span>
                            <div>
                                <h2><asp:Literal ID="litQuestionFormTitle" runat="server" Mode="Encode" /></h2>
                                <p>Mark the correct answer with the circle.</p>
                            </div>
                        </div>

                        <asp:ValidationSummary ID="vsQuestion" runat="server" ValidationGroup="Question" CssClass="cl-alert cl-alert-danger"
                            HeaderText="Please fix the following:" DisplayMode="BulletList" />

                        <div class="cl-field">
                            <asp:Label ID="lblQuestion" runat="server" AssociatedControlID="txtQuestion" CssClass="form-label" Text="Question" />
                            <asp:TextBox ID="txtQuestion" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="3"
                                MaxLength="1000" placeholder="e.g. Which keyword defines a function in Python?" />
                            <asp:RequiredFieldValidator ID="rfvQuestion" runat="server" ControlToValidate="txtQuestion"
                                ErrorMessage="The question is required." CssClass="cl-field-error" Display="Dynamic" ValidationGroup="Question" />
                        </div>

                        <div class="cl-field">
                            <span class="form-label d-block">Type</span>
                            <div class="cl-segmented" role="radiogroup" aria-label="Question type">
                                <asp:RadioButton ID="rbMultiple" runat="server" GroupName="QuestionType" Text="Multiple choice" />
                                <asp:RadioButton ID="rbTrueFalse" runat="server" GroupName="QuestionType" Text="True / False" />
                            </div>
                        </div>

                        <div class="cl-field">
                            <span class="form-label d-block">Answers</span>
                            <div class="cl-options" id="answerOptions">
                                <div class="cl-option">
                                    <asp:RadioButton ID="rbCorrect1" runat="server" GroupName="CorrectAnswer" ToolTip="Correct answer" />
                                    <asp:TextBox ID="txtOption1" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="200"
                                        placeholder="Option A" aria-label="Option A" />
                                </div>
                                <div class="cl-option">
                                    <asp:RadioButton ID="rbCorrect2" runat="server" GroupName="CorrectAnswer" ToolTip="Correct answer" />
                                    <asp:TextBox ID="txtOption2" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="200"
                                        placeholder="Option B" aria-label="Option B" />
                                </div>
                                <div class="cl-option" data-mc-only>
                                    <asp:RadioButton ID="rbCorrect3" runat="server" GroupName="CorrectAnswer" ToolTip="Correct answer" />
                                    <asp:TextBox ID="txtOption3" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="200"
                                        placeholder="Option C (optional)" aria-label="Option C" />
                                </div>
                                <div class="cl-option" data-mc-only>
                                    <asp:RadioButton ID="rbCorrect4" runat="server" GroupName="CorrectAnswer" ToolTip="Correct answer" />
                                    <asp:TextBox ID="txtOption4" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="200"
                                        placeholder="Option D (optional)" aria-label="Option D" />
                                </div>
                            </div>
                        </div>

                        <div class="cl-field">
                            <asp:Label ID="lblExplanation" runat="server" AssociatedControlID="txtExplanation" CssClass="form-label" Text="Explanation (optional)" />
                            <asp:TextBox ID="txtExplanation" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="2"
                                MaxLength="1000" placeholder="Shown to learners after they answer." />
                        </div>

                        <div class="cl-card-actions">
                            <asp:HyperLink ID="hlCancelQuestion" runat="server" CssClass="btn cl-btn-outline" Text="Cancel" />
                            <asp:Button ID="btnSaveQuestion" runat="server" CssClass="btn btn-primary" ValidationGroup="Question" OnClick="btnSaveQuestion_Click" />
                        </div>
                    </section>
                </asp:PlaceHolder>
            </aside>
        </div>

        <%-- Back / next step --%>
        <%= StepNavHtml %>
    </asp:PlaceHolder>

    <%-- Question form: True / False fills in the two answers and hides the other two --%>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var trueFalse = document.getElementById('<%= rbTrueFalse.ClientID %>');
            var multiple = document.getElementById('<%= rbMultiple.ClientID %>');
            if (!trueFalse || !multiple) {
                return;
            }

            var options = document.getElementById('answerOptions');
            var first = document.getElementById('<%= txtOption1.ClientID %>');
            var second = document.getElementById('<%= txtOption2.ClientID %>');
            var saved = { first: first.value, second: second.value };

            function apply() {
                var isTrueFalse = trueFalse.checked;
                options.classList.toggle('is-true-false', isTrueFalse);
                if (isTrueFalse) {
                    if (!first.readOnly) {
                        saved = { first: first.value, second: second.value };
                    }
                    first.value = 'True';
                    second.value = 'False';
                } else if (first.readOnly) {
                    first.value = saved.first === 'True' ? '' : saved.first;
                    second.value = saved.second === 'False' ? '' : saved.second;
                }
                first.readOnly = isTrueFalse;
                second.readOnly = isTrueFalse;

                // Only True or False can be correct now
                var third = document.getElementById('<%= rbCorrect3.ClientID %>');
                var fourth = document.getElementById('<%= rbCorrect4.ClientID %>');
                if (isTrueFalse && (third.checked || fourth.checked)) {
                    document.getElementById('<%= rbCorrect1.ClientID %>').checked = true;
                }
            }

            trueFalse.addEventListener('change', apply);
            multiple.addEventListener('change', apply);
            apply();
        });
    </script>

</asp:Content>
