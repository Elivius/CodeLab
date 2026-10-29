<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="CodeLab.Modules.Tutor.Dashboard" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>
<%@ Import Namespace="CodeLab.Models" %>

<asp:Content ID="DashboardContent" ContentPlaceHolderID="TutorContent" runat="server">

    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

    <div class="cl-dash">
        <div class="cl-dash-main">

            <%-- ===== Welcome banner ===== --%>
            <section class="cl-hero-banner">
                <svg class="cl-hero-art" viewBox="0 0 320 200" aria-hidden="true">
                    <path d="M240 34l7 19 19 7-19 7-7 19-7-19-19-7 19-7z" fill="rgba(255,255,255,0.55)" />
                    <path d="M286 116l4 11 11 4-11 4-4 11-4-11-11-4 11-4z" fill="rgba(255,255,255,0.4)" />
                    <path d="M196 132l3 8 8 3-8 3-3 8-3-8-8-3 8-3z" fill="rgba(255,255,255,0.35)" />
                    <circle cx="300" cy="30" r="58" fill="none" stroke="rgba(255,255,255,0.14)" stroke-width="22" />
                    <circle cx="250" cy="190" r="70" fill="none" stroke="rgba(255,255,255,0.08)" stroke-width="28" />
                </svg>
                <div class="cl-hero-banner-body">
                    <span class="cl-hero-kicker"><asp:Literal ID="litGreeting" runat="server" Mode="Encode" /></span>
                    <h1><asp:Literal ID="litTutorName" runat="server" Mode="Encode" /></h1>
                    <p><asp:Literal ID="litSummary" runat="server" Mode="Encode" /></p>
                    <div class="cl-hero-actions">
                        <asp:HyperLink ID="hlCreateCourse" runat="server" CssClass="btn cl-btn-light">
                            <i data-lucide="plus" class="cl-icon" aria-hidden="true"></i>Create course
                        </asp:HyperLink>
                        <asp:HyperLink ID="hlProfile" runat="server" NavigateUrl="~/Modules/Tutor/Profile" CssClass="btn cl-btn-ghost-light">
                            <i data-lucide="user" class="cl-icon" aria-hidden="true"></i>My profile
                        </asp:HyperLink>
                    </div>
                </div>
            </section>

            <%-- ===== Headline numbers ===== --%>
            <div class="cl-stat-grid">
                <div class="cl-stat-card">
                    <span class="cl-stat-icon cl-tone-pink"><i data-lucide="book-open" class="cl-icon" aria-hidden="true"></i></span>
                    <div>
                        <div class="cl-stat-card-value"><asp:Literal ID="litCourseCount" runat="server" Text="0" /></div>
                        <div class="cl-stat-card-label">Courses</div>
                        <div class="cl-stat-card-note"><asp:Literal ID="litPublishedNote" runat="server" Mode="Encode" /></div>
                    </div>
                </div>
                <div class="cl-stat-card">
                    <span class="cl-stat-icon cl-tone-blue"><i data-lucide="users" class="cl-icon" aria-hidden="true"></i></span>
                    <div>
                        <div class="cl-stat-card-value"><asp:Literal ID="litStudentCount" runat="server" Text="0" /></div>
                        <div class="cl-stat-card-label">Students</div>
                        <div class="cl-stat-card-note">Enrolled in your courses</div>
                    </div>
                </div>
                <div class="cl-stat-card">
                    <span class="cl-stat-icon cl-tone-amber"><i data-lucide="list-checks" class="cl-icon" aria-hidden="true"></i></span>
                    <div>
                        <div class="cl-stat-card-value"><asp:Literal ID="litQuizCount" runat="server" Text="0" /></div>
                        <div class="cl-stat-card-label">Quizzes</div>
                        <div class="cl-stat-card-note"><asp:Literal ID="litAttemptNote" runat="server" Mode="Encode" /></div>
                    </div>
                </div>
                <div class="cl-stat-card">
                    <span class="cl-stat-icon cl-tone-green"><i data-lucide="target" class="cl-icon" aria-hidden="true"></i></span>
                    <div>
                        <div class="cl-stat-card-value"><asp:Literal ID="litAverageScore" runat="server" Mode="Encode" /></div>
                        <div class="cl-stat-card-label">Average quiz score</div>
                        <div class="cl-stat-card-note"><asp:Literal ID="litPassRateNote" runat="server" Mode="Encode" /></div>
                    </div>
                </div>
            </div>

            <%-- ===== My courses ===== --%>
            <section class="cl-section-block">
                <div class="cl-section-bar">
                    <div>
                        <h2>My courses</h2>
                        <p>Every course you have created and where it is in the review process.</p>
                    </div>
                    <div class="cl-chip-group" role="group" aria-label="Filter courses by status">
                        <button type="button" class="cl-chip is-active" data-filter="all">All <span><asp:Literal ID="litCountAll" runat="server" Text="0" /></span></button>
                        <button type="button" class="cl-chip" data-filter="Draft">Draft <span><asp:Literal ID="litCountDraft" runat="server" Text="0" /></span></button>
                        <button type="button" class="cl-chip" data-filter="Pending Review">In review <span><asp:Literal ID="litCountPending" runat="server" Text="0" /></span></button>
                        <button type="button" class="cl-chip" data-filter="Published">Published <span><asp:Literal ID="litCountPublished" runat="server" Text="0" /></span></button>
                    </div>
                </div>

                <asp:Repeater ID="rptCourses" runat="server">
                    <HeaderTemplate><div class="cl-course-grid" id="courseGrid"></HeaderTemplate>
                    <ItemTemplate>
                        <article class="cl-course-card" data-status="<%#: Eval("Status") %>">
                            <a class="cl-course-card-cover <%# CoverToneCss((TutorCourseSummary)Container.DataItem) %>" href="<%#: CourseUrl((int)Eval("Id")) %>">
                                <%# CoverHtml((TutorCourseSummary)Container.DataItem) %>
                                <span class="cl-badge cl-status cl-course-card-status <%# StatusCss((string)Eval("Status")) %>"><%#: Eval("Status") %></span>
                            </a>
                            <div class="cl-course-card-body">
                                <div class="cl-course-card-tags">
                                    <span class="cl-badge <%# DifficultyCss((string)Eval("Difficulty")) %>"><%#: Eval("Difficulty") %></span>
                                    <span class="cl-course-card-category"><%#: Eval("Category") %></span>
                                </div>
                                <h3><a href="<%#: CourseUrl((int)Eval("Id")) %>"><%#: Eval("Title") %></a></h3>
                                <%# (bool)Eval("HasFeedback") ? "<div class=\"cl-course-card-flag\"><i data-lucide=\"message-square-warning\" class=\"cl-icon\" aria-hidden=\"true\"></i>Changes requested</div>" : "" %>
                                <div class="cl-course-card-meta">
                                    <span><i data-lucide="notebook-text" class="cl-icon" aria-hidden="true"></i><%# Eval("LessonCount") %> lessons</span>
                                    <span><i data-lucide="users" class="cl-icon" aria-hidden="true"></i><%# Eval("StudentCount") %> students</span>
                                </div>
                                <div class="cl-course-card-footer">
                                    <span><%#: "Updated " + ((DateTime)Eval("UpdatedAt")).ToLocalTime().ToString("d MMM yyyy") %></span>
                                    <%# ManageLink((int)Eval("Id")) %>
                                </div>
                            </div>
                        </article>
                    </ItemTemplate>
                    <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>

                <div class="cl-empty cl-card cl-filter-empty" id="filterEmpty" hidden>
                    <span class="cl-icon-tile"><i data-lucide="filter-x" class="cl-icon" aria-hidden="true"></i></span>
                    <h3>No courses here</h3>
                    <p>No course has this status yet.</p>
                </div>

                <asp:PlaceHolder ID="phNoCourses" runat="server" Visible="false">
                    <div class="cl-empty cl-card">
                        <span class="cl-icon-tile"><i data-lucide="book-plus" class="cl-icon" aria-hidden="true"></i></span>
                        <h3>No courses yet</h3>
                        <p>Create your first course, add lessons, then submit it for review.</p>
                    </div>
                </asp:PlaceHolder>
            </section>
        </div>

        <%-- ===== Right column ===== --%>
        <aside class="cl-dash-side">
            <section class="cl-card cl-side-card">
                <div class="cl-side-card-head">
                    <h2>Course pipeline</h2>
                    <span><asp:Literal ID="litPipelineTotal" runat="server" Mode="Encode" /></span>
                </div>
                <asp:Repeater ID="rptPipeline" runat="server">
                    <HeaderTemplate><ul class="cl-pipeline"></HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            <div class="cl-pipeline-row">
                                <span class="cl-pipeline-dot <%# Eval("Tone") %>"></span>
                                <span><%#: Eval("Label") %></span>
                                <strong><%# Eval("Count") %></strong>
                            </div>
                            <div class="cl-pipeline-track"><span class="<%# Eval("Tone") %>" style="width: <%# Eval("Percent") %>%"></span></div>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ul></FooterTemplate>
                </asp:Repeater>
            </section>

            <section class="cl-card cl-side-card">
                <div class="cl-side-card-head">
                    <h2>Needs your attention</h2>
                </div>
                <asp:Repeater ID="rptAttention" runat="server">
                    <HeaderTemplate><ul class="cl-attention"></HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            <a href="<%#: Eval("Url") %>">
                                <span class="cl-attention-icon <%# Eval("Tone") %>"><i data-lucide="<%# Eval("Icon") %>" class="cl-icon" aria-hidden="true"></i></span>
                                <span class="cl-attention-text">
                                    <strong><%#: Eval("Title") %></strong>
                                    <span><%#: Eval("Text") %></span>
                                </span>
                                <i data-lucide="chevron-right" class="cl-icon cl-attention-go" aria-hidden="true"></i>
                            </a>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ul></FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="phAllClear" runat="server" Visible="false">
                    <div class="cl-all-clear">
                        <i data-lucide="party-popper" class="cl-icon" aria-hidden="true"></i>
                        <span>You're all caught up.</span>
                    </div>
                </asp:PlaceHolder>
            </section>
        </aside>
    </div>

    <%-- Status filter for the course cards (runs in the browser; nothing is reloaded) --%>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var grid = document.getElementById('courseGrid');
            var empty = document.getElementById('filterEmpty');
            if (!grid) { return; }

            document.querySelectorAll('.cl-chip[data-filter]').forEach(function (chip) {
                chip.addEventListener('click', function () {
                    var filter = chip.getAttribute('data-filter');
                    document.querySelectorAll('.cl-chip[data-filter]').forEach(function (c) {
                        c.classList.toggle('is-active', c === chip);
                    });

                    var shown = 0;
                    grid.querySelectorAll('.cl-course-card').forEach(function (card) {
                        var match = filter === 'all' || card.getAttribute('data-status') === filter;
                        card.hidden = !match;
                        if (match) { shown++; }
                    });
                    empty.hidden = shown > 0;
                });
            });
        });
    </script>

</asp:Content>
