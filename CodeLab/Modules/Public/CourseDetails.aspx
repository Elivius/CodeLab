<%@ Page Title="Course Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CourseDetails.aspx.cs" Inherits="CodeLab.Modules.Public.CourseDetails" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-section">
        <a runat="server" href="~/Modules/Public/Courses" class="cl-link-arrow d-inline-block mb-3">
            <i data-lucide="arrow-left" class="cl-icon me-1" aria-hidden="true"></i>
            Back to catalogue
        </a>

        <asp:PlaceHolder ID="phNotFound" runat="server" Visible="false">
            <div class="cl-card p-5 text-center">
                <h1 class="h3"><asp:Literal ID="litNotFound" runat="server" Mode="Encode" /></h1>
                <p class="cl-page-intro mb-4">This course does not exist or is not available yet.</p>
                <a runat="server" href="~/Modules/Public/Courses" class="btn btn-primary">Browse the catalogue</a>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phCourse" runat="server">
            <article class="cl-card cl-course cl-course-detail">
                <div class="cl-course-thumb cl-course-detail-thumb cl-thumb-default">
                    <asp:PlaceHolder ID="phDefaultThumb" runat="server">&lt;/&gt;</asp:PlaceHolder>
                    <asp:Image ID="imgCover" runat="server" CssClass="cl-course-cover" Visible="false" />
                    <span id="spanLevel" runat="server" class="cl-level"></span>
                </div>
                <div class="cl-course-body">
                    <span class="cl-course-category"><asp:Literal ID="litCategory" runat="server" Mode="Encode" /></span>
                    <h1 class="cl-course-detail-title"><asp:Literal ID="litTitle" runat="server" Mode="Encode" /></h1>
                    <p class="cl-course-detail-description"><asp:Literal ID="litDescription" runat="server" Mode="Encode" /></p>
                </div>
            </article>

            <div class="cl-card cl-lock-box">
                <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="lock" class="cl-icon" aria-hidden="true"></i></span>
                <div class="flex-grow-1">
                    <h2>Enrol now to access</h2>
                    <p>Lessons, quizzes and coding challenges unlock once you enrol in this course.</p>
                </div>
                <asp:PlaceHolder ID="phGuestActions" runat="server">
                    <div class="d-flex flex-wrap gap-2">
                        <a runat="server" href="~/Modules/Public/Register?role=learner" class="btn btn-primary">Register</a>
                        <a runat="server" href="~/Modules/Public/Login" class="btn cl-btn-outline">Log in</a>
                    </div>
                </asp:PlaceHolder>
                <asp:HyperLink ID="hlOpenCourse" runat="server" CssClass="btn btn-primary" Text="Open course" Visible="false" />
            </div>
        </asp:PlaceHolder>
    </main>

</asp:Content>
