<%@ Page Title="Course Review" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="CourseReview.aspx.cs" Inherits="CodeLab.Modules.Admin.CourseReview" %>
<%@ Import Namespace="CodeLab.Helpers" %>
<%@ Import Namespace="CodeLab.Models" %>

<asp:Content ID="ReviewContent" ContentPlaceHolderID="AdminContent" runat="server">

    <a runat="server" href="~/Modules/Admin/Courses" class="cl-admin-back">
        <i data-lucide="arrow-left" class="cl-icon" aria-hidden="true"></i>All courses
    </a>

    <asp:Label ID="lblMessage" runat="server" Visible="false" />

    <asp:PlaceHolder ID="phCourse" runat="server">
        <div class="row g-4">
            <%-- ===== The course ===== --%>
            <div class="col-lg-8">
                <section class="cl-card cl-admin-panel">
                    <asp:Image ID="imgCover" runat="server" CssClass="cl-admin-cover" GenerateEmptyAlternateText="true" Visible="false" />

                    <div class="cl-admin-applicant">
                        <div>
                            <h2><asp:Literal ID="litTitle" runat="server" Mode="Encode" /></h2>
                            <span class="cl-admin-meta"><asp:Literal ID="litSubtitle" runat="server" Mode="Encode" /></span>
                        </div>
                        <asp:Label ID="lblStatus" runat="server" />
                    </div>

                    <dl class="cl-admin-facts">
                        <dt>Tutor</dt>
                        <dd><asp:Literal ID="litTutor" runat="server" Mode="Encode" /></dd>
                        <dt>Email</dt>
                        <dd><asp:Literal ID="litTutorEmail" runat="server" Mode="Encode" /></dd>
                        <dt>Students</dt>
                        <dd><asp:Literal ID="litStudents" runat="server" Mode="Encode" /></dd>
                        <dt>Updated</dt>
                        <dd><asp:Literal ID="litUpdated" runat="server" Mode="Encode" /></dd>
                    </dl>

                    <div class="cl-admin-section">
                        <h3>Description</h3>
                        <p class="cl-admin-prose"><asp:Literal ID="litDescription" runat="server" Mode="Encode" /></p>
                    </div>

                    <div class="cl-admin-section">
                        <h3>Lessons (<asp:Literal ID="litLessonCount" runat="server" Mode="Encode" />)</h3>

                        <%-- Bound on every request, so the lesson text is not repeated in the page's view state.
                             The content is written in Markdown, and MarkdownHelper returns safe HTML. --%>
                        <asp:Repeater ID="rptLessons" runat="server" EnableViewState="false">
                            <ItemTemplate>
                                <details class="cl-admin-lesson">
                                    <summary>
                                        <%# Eval("OrderIndex") %>. <%#: Eval("Title") %>
                                        <span class="cl-admin-meta"><%#: LessonFlags((Lesson)Container.DataItem) %></span>
                                    </summary>
                                    <div class="cl-admin-lesson-body">
                                        <%# MarkdownHelper.ToHtml((string)Eval("Content")) %>
                                        <%# MarkdownHelper.CodeBlockToHtml((string)Eval("CodeSnippet"), (string)Eval("CodeLanguage")) %>
                                        <div class="cl-admin-lesson-links">
                                            <asp:HyperLink runat="server" CssClass="btn cl-btn-outline btn-sm" Target="_blank" rel="noopener noreferrer"
                                                NavigateUrl='<%# Eval("VideoUrl") %>' Visible='<%# IsWebLink((string)Eval("VideoUrl")) %>'>
                                                <i data-lucide="video" class="cl-icon" aria-hidden="true"></i> Watch video
                                            </asp:HyperLink>
                                            <asp:HyperLink runat="server" CssClass="btn cl-btn-outline btn-sm" Target="_blank"
                                                NavigateUrl='<%# Eval("AttachmentUrl") %>' Visible='<%# (bool)Eval("HasAttachment") %>'>
                                                <i data-lucide="paperclip" class="cl-icon" aria-hidden="true"></i> <%#: Eval("AttachmentName") %>
                                            </asp:HyperLink>
                                        </div>
                                    </div>
                                </details>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder ID="phNoLessons" runat="server" Visible="false">
                            <p class="cl-admin-meta">This course has no lessons yet.</p>
                        </asp:PlaceHolder>
                    </div>

                    <div class="cl-admin-section">
                        <h3>Quiz</h3>
                        <p class="cl-admin-meta"><asp:Literal ID="litQuiz" runat="server" Mode="Encode" /></p>

                        <asp:PlaceHolder ID="phQuizInstructions" runat="server" Visible="false">
                            <p class="cl-admin-prose"><asp:Literal ID="litQuizInstructions" runat="server" Mode="Encode" EnableViewState="false" /></p>
                        </asp:PlaceHolder>

                        <%-- Bound on every request, like the lessons, so the answers are not repeated in the page's view state --%>
                        <asp:Repeater ID="rptQuestions" runat="server" EnableViewState="false">
                            <ItemTemplate>
                                <details class="cl-admin-lesson">
                                    <summary>
                                        <%# Eval("OrderIndex") %>. <%#: Eval("QuestionText") %>
                                        <span class="cl-admin-meta"><%#: (string)Eval("QuestionType") == QuizQuestion.TrueFalse ? "True / False" : "Multiple choice" %></span>
                                    </summary>
                                    <div class="cl-admin-lesson-body">
                                        <%# OptionsHtml((QuizQuestion)Container.DataItem) %>
                                        <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace((string)Eval("Explanation")) %>'>
                                            <p class="cl-admin-answer-note"><strong>Explanation:</strong> <%#: Eval("Explanation") %></p>
                                        </asp:PlaceHolder>
                                    </div>
                                </details>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div class="cl-admin-section">
                        <h3>Coding challenge</h3>
                        <p class="cl-admin-meta"><asp:Literal ID="litChallenge" runat="server" Mode="Encode" /></p>

                        <%-- MarkdownHelper returns safe HTML, so these literals pass it straight through.
                             They are set on every request, so the text is not repeated in the page's view state. --%>
                        <asp:PlaceHolder ID="phChallenge" runat="server" Visible="false">
                            <details class="cl-admin-lesson">
                                <summary>Problem and code</summary>
                                <div class="cl-admin-lesson-body">
                                    <asp:Literal ID="litChallengeDescription" runat="server" Mode="PassThrough" EnableViewState="false" />
                                    <asp:PlaceHolder ID="phStarterCode" runat="server" Visible="false">
                                        <h4>Starter code</h4>
                                        <asp:Literal ID="litStarterCode" runat="server" Mode="PassThrough" EnableViewState="false" />
                                    </asp:PlaceHolder>
                                    <h4>Expected output</h4>
                                    <asp:Literal ID="litExpectedOutput" runat="server" Mode="PassThrough" EnableViewState="false" />
                                </div>
                            </details>
                        </asp:PlaceHolder>
                    </div>
                </section>
            </div>

            <%-- ===== Decision ===== --%>
            <div class="col-lg-4">
                <section id="decision" class="cl-card cl-admin-panel cl-admin-decision">
                    <h2 class="cl-admin-panel-title">Decision</h2>

                    <p class="cl-admin-meta"><asp:Literal ID="litDecisionNote" runat="server" Mode="Encode" /></p>

                    <asp:PlaceHolder ID="phNote" runat="server" Visible="false">
                        <p class="cl-admin-reason"><strong><asp:Literal ID="litNoteLabel" runat="server" Mode="Encode" /></strong> <asp:Literal ID="litNote" runat="server" Mode="Encode" /></p>
                    </asp:PlaceHolder>

                    <asp:Button ID="btnApprove" runat="server" Text="Approve and publish" CssClass="btn btn-success w-100"
                        CausesValidation="false" OnClick="btnApprove_Click"
                        OnClientClick="return confirm('Publish this course? Learners can enrol straight away.');" />

                    <asp:Button ID="btnUnlock" runat="server" Text="Unlock and publish again" CssClass="btn btn-success w-100 mt-2"
                        CausesValidation="false" OnClick="btnUnlock_Click"
                        OnClientClick="return confirm('Unlock this course? It will be published again.');" />

                    <asp:PlaceHolder ID="phReasonForm" runat="server">
                        <hr />
                        <asp:Label ID="lblReason" runat="server" AssociatedControlID="txtReason" CssClass="form-label" />
                        <%-- ValidateRequestMode="Disabled" lets the admin type code such as List<int> in the reason.
                             That is safe because the reason is only ever shown as encoded text. --%>
                        <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control"
                            ValidateRequestMode="Disabled" placeholder="The tutor will see this." />
                        <asp:RequiredFieldValidator ID="rfvReason" runat="server" ControlToValidate="txtReason" ValidationGroup="Reason"
                            ErrorMessage="Please give a reason. The tutor will see it." CssClass="text-danger small" Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="revReason" runat="server" ControlToValidate="txtReason" ValidationGroup="Reason"
                            ValidationExpression="^[\s\S]{10,500}$" ErrorMessage="The reason must be 10 to 500 characters."
                            CssClass="text-danger small" Display="Dynamic" />
                        <asp:Button ID="btnReject" runat="server" Text="Send back to tutor" CssClass="btn btn-outline-danger w-100 mt-2"
                            ValidationGroup="Reason" OnClick="btnReject_Click" />
                        <asp:Button ID="btnLock" runat="server" Text="Lock course" CssClass="btn btn-outline-danger w-100 mt-2"
                            ValidationGroup="Reason" OnClick="btnLock_Click" />
                    </asp:PlaceHolder>

                    <hr />
                    <asp:Button ID="btnDelete" runat="server" Text="Delete course" CssClass="btn btn-link cl-admin-delete w-100"
                        CausesValidation="false" OnClick="btnDelete_Click" />
                </section>
            </div>
        </div>
    </asp:PlaceHolder>

</asp:Content>
