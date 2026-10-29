<%@ Page Title="Course Catalogue" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Courses.aspx.cs" Inherits="CodeLab.Modules.Public.Courses" %>
<%@ Register Src="~/UserControls/CourseGrid.ascx" TagPrefix="cl" TagName="CourseGrid" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-section">
        <div class="mb-4">
            <span class="cl-eyebrow">Curated Curriculum</span>
            <h1 class="cl-section-title">Course Catalogue</h1>
            <p class="cl-page-intro">Browse every published course. Register to unlock lessons, quizzes and coding challenges.</p>
        </div>

        <cl:CourseGrid ID="courseGrid" runat="server" />
    </main>

</asp:Content>
