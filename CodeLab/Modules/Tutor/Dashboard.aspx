<%@ Page Title="Tutor Dashboard" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="CodeLab.Modules.Tutor.Dashboard" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>

<asp:Content ID="DashboardContent" ContentPlaceHolderID="TutorContent" runat="server">

    <div class="cl-page-header">
        <div>
            <span class="cl-eyebrow">Tutor Dashboard</span>
            <h1>Welcome back, <asp:Literal ID="litTutorName" runat="server" Mode="Encode" /></h1>
            <p>An overview of your courses, learners and quiz results.</p>
        </div>
        <asp:HyperLink ID="hlCreateCourse" runat="server" CssClass="btn btn-primary">
            <i data-lucide="plus" class="cl-icon me-1" aria-hidden="true"></i>Create Course
        </asp:HyperLink>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

    <%-- ===== Headline numbers ===== --%>
    <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-4 g-3 mb-4">
        <div class="col">
            <div class="cl-card cl-kpi">
                <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="book-open" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-kpi-label">Courses</div>
                    <div class="cl-kpi-value"><asp:Literal ID="litCourseCount" runat="server" Text="0" /></div>
                    <div class="cl-kpi-note"><asp:Literal ID="litPublishedNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="cl-card cl-kpi">
                <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="users" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-kpi-label">Students</div>
                    <div class="cl-kpi-value"><asp:Literal ID="litStudentCount" runat="server" Text="0" /></div>
                    <div class="cl-kpi-note">Learners enrolled in your courses</div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="cl-card cl-kpi">
                <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="list-checks" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-kpi-label">Quizzes</div>
                    <div class="cl-kpi-value"><asp:Literal ID="litQuizCount" runat="server" Text="0" /></div>
                    <div class="cl-kpi-note"><asp:Literal ID="litAttemptNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="cl-card cl-kpi">
                <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="target" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-kpi-label">Average quiz score</div>
                    <div class="cl-kpi-value"><asp:Literal ID="litAverageScore" runat="server" Mode="Encode" /></div>
                    <div class="cl-kpi-note"><asp:Literal ID="litPassRateNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
        </div>
    </div>

    <%-- ===== Course list ===== --%>
    <section class="cl-card cl-table-card">
        <div class="cl-table-card-header">
            <div>
                <h2>My Courses</h2>
                <p>Every course you have created and where it is in the review process.</p>
            </div>
        </div>

        <asp:Repeater ID="rptCourses" runat="server">
            <HeaderTemplate>
                <div class="table-responsive">
                    <table class="table cl-table">
                        <thead>
                            <tr>
                                <th scope="col">Course</th>
                                <th scope="col">Difficulty</th>
                                <th scope="col">Status</th>
                                <th scope="col" class="text-end">Lessons</th>
                                <th scope="col" class="text-end">Students</th>
                                <th scope="col" class="d-none d-xl-table-cell">Last updated</th>
                                <th scope="col"><span class="visually-hidden">Actions</span></th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                            <tr>
                                <td>
                                    <div class="cl-table-title"><%#: Eval("Title") %></div>
                                    <div class="cl-course-category"><%#: Eval("Category") %></div>
                                </td>
                                <td><span class="cl-badge <%# DifficultyCss((string)Eval("Difficulty")) %>"><%#: Eval("Difficulty") %></span></td>
                                <td><span class="cl-badge cl-status <%# StatusCss((string)Eval("Status")) %>"><%#: Eval("Status") %></span></td>
                                <td class="text-end"><%# Eval("LessonCount") %></td>
                                <td class="text-end"><%# Eval("StudentCount") %></td>
                                <td class="text-nowrap d-none d-xl-table-cell"><%#: ((DateTime)Eval("UpdatedAt")).ToLocalTime().ToString("d MMM yyyy") %></td>
                                <td class="text-end text-nowrap"><%# ManageLink((int)Eval("Id")) %></td>
                            </tr>
            </ItemTemplate>
            <FooterTemplate>
                        </tbody>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phNoCourses" runat="server" Visible="false">
            <div class="cl-empty">
                <span class="cl-icon-tile"><i data-lucide="book-plus" class="cl-icon" aria-hidden="true"></i></span>
                <h3>No courses yet</h3>
                <p>Create your first course, add lessons, then submit it for review.</p>
            </div>
        </asp:PlaceHolder>
    </section>

</asp:Content>
