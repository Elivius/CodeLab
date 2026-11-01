<%@ Page Title="Contact us" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="CodeLab.Modules.Public.Contact" %>

<asp:Content ID="ContactHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/Legal.css") %>" />
</asp:Content>

<asp:Content ID="ContactContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-legal cl-contact">
        <header class="cl-legal-head">
            <h1>Contact us</h1>
            <p class="cl-legal-intro">Pick what you need and your email app opens with the subject filled in. You can also write to us directly.</p>
            <a class="cl-contact-email" href="<%: MailLink(null) %>"><%: SupportEmail %></a>
        </header>

        <ul class="cl-contact-list">
            <li class="cl-contact-item">
                <h2>Help with my account</h2>
                <p>Trouble logging in, a suspended account, or details you cannot change yourself.</p>
                <p class="cl-legal-gist">Include your username.</p>
                <a class="btn cl-btn-outline" href="<%: MailLink("Account help") %>">Email us</a>
            </li>
            <li class="cl-contact-item">
                <h2>Tutor application</h2>
                <p>Questions about your application or a rejection reason.</p>
                <p class="cl-legal-gist">Include your username.</p>
                <a class="btn cl-btn-outline" href="<%: MailLink("Tutor application") %>">Email us</a>
            </li>
            <li class="cl-contact-item">
                <h2>Report a course or post</h2>
                <p>Something wrong, unsafe or copied from someone else.</p>
                <p class="cl-legal-gist">Include a link to the page.</p>
                <a class="btn cl-btn-outline" href="<%: MailLink("Report a course or post") %>">Email us</a>
            </li>
            <li class="cl-contact-item">
                <h2>Privacy request</h2>
                <p>Ask for a copy of your information, a correction, or to close your account.</p>
                <p class="cl-legal-gist">Send it from your account's email address.</p>
                <a class="btn cl-btn-outline" href="<%: MailLink("Privacy request") %>">Email us</a>
            </li>
            <li class="cl-contact-item">
                <h2>Something else</h2>
                <p>Feedback, ideas, or a bug you found.</p>
                <p class="cl-legal-gist">For a bug, tell us what you were doing.</p>
                <a class="btn cl-btn-outline" href="<%: MailLink("Question about CodeLab") %>">Email us</a>
            </li>
        </ul>
    </main>

</asp:Content>
