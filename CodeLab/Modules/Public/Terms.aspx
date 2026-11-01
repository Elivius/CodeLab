<%@ Page Title="Terms of use" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Terms.aspx.cs" Inherits="CodeLab.Modules.Public.Terms" %>

<asp:Content ID="TermsHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/Legal.css") %>" />
</asp:Content>

<asp:Content ID="TermsContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-legal">
        <header class="cl-legal-head">
            <h1>Terms of use</h1>
            <p class="cl-legal-date">Last updated 1 November 2026</p>
            <p class="cl-legal-intro">These terms apply whenever you use CodeLab. Each section has a short version written as a code comment. The full text is what counts.</p>
        </header>

        <section id="using-codelab" class="cl-legal-section">
            <h2>1. Using CodeLab</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Learn, teach and play fair.</p>
            <div class="cl-legal-body">
                <p>CodeLab is a place to learn programming. Learners take courses, quizzes and coding challenges, and tutors create the courses. By creating an account or using the site, you agree to these terms.</p>
            </div>
        </section>

        <section id="your-account" class="cl-legal-section">
            <h2>2. Your account</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>One person, one account. Keep your password private.</p>
            <div class="cl-legal-body">
                <p>Use your own details when you register, and keep them up to date. Keep your password private, because you are responsible for what happens under your account.</p>
                <p>If you think someone else has used your account, <a runat="server" href="~/Modules/Public/Contact">contact us</a> straight away.</p>
            </div>
        </section>

        <section id="tutors-and-courses" class="cl-legal-section">
            <h2>3. Tutors and courses</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Tutors are reviewed, and so is every course.</p>
            <div class="cl-legal-body">
                <p>An administrator reviews every tutor application before that tutor can create courses. A rejected applicant sees the reason and can update their application and send it again.</p>
                <p>An administrator also reviews every course before learners can see it. They can publish a course, send it back with feedback, lock it, or delete it.</p>
            </div>
        </section>

        <section id="your-content" class="cl-legal-section">
            <h2>4. Your content</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>What you make stays yours. We only need permission to show it.</p>
            <div class="cl-legal-body">
                <p>You keep ownership of the courses, lessons, code and posts you add. You give CodeLab permission to store them and show them to other users, because the site cannot work without that.</p>
                <p>Only add content you have the right to share.</p>
            </div>
        </section>

        <section id="code-you-submit" class="cl-legal-section">
            <h2>5. Code you submit</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Your code runs on other services. Keep secrets out of it.</p>
            <div class="cl-legal-body">
                <p>When you run a coding challenge, your code is sent to outside services that run it and review it. The <a runat="server" href="~/Modules/Public/Privacy#outside-services">Privacy policy</a> lists them.</p>
                <p>Do not put passwords, keys or personal details in your code. Automated feedback can be wrong, so treat it as help and not as a final answer.</p>
            </div>
        </section>

        <section id="acceptable-use" class="cl-legal-section">
            <h2>6. Acceptable use</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Be decent, and do not break things.</p>
            <div class="cl-legal-body">
                <p>Do not:</p>
                <ul>
                    <li>post anything illegal, abusive, misleading or copied from someone else</li>
                    <li>try to reach another person's account or data</li>
                    <li>submit code meant to harm CodeLab or the services it uses</li>
                    <li>use the site to send spam or advertising</li>
                </ul>
            </div>
        </section>

        <section id="suspension" class="cl-legal-section">
            <h2>7. Suspension and removal</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Break these terms and your account or course can be stopped.</p>
            <div class="cl-legal-body">
                <p>An administrator can suspend an account, or lock or delete a course, if these terms are broken. A suspended account is logged out and cannot log in. A locked course can no longer be opened by learners, even if they are enrolled.</p>
                <p>If you think this was a mistake, <a runat="server" href="~/Modules/Public/Contact">contact us</a>.</p>
            </div>
        </section>

        <section id="no-guarantees" class="cl-legal-section">
            <h2>8. No guarantees</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>We do our best, but the site comes as it is.</p>
            <div class="cl-legal-body">
                <p>CodeLab is provided as it is. Courses are for learning and may contain mistakes, and the site may sometimes be unavailable. As far as the law allows, CodeLab is not responsible for any loss that comes from using it.</p>
            </div>
        </section>

        <section id="changes" class="cl-legal-section">
            <h2>9. Changes to these terms</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>If these change, the date at the top changes too.</p>
            <div class="cl-legal-body">
                <p>We may update these terms as CodeLab changes. The date at the top of this page shows the latest version. If you keep using the site after a change, the new terms apply to you.</p>
            </div>
        </section>

        <p class="cl-legal-end">Questions about these terms? <a runat="server" href="~/Modules/Public/Contact">Contact us</a>.</p>
    </main>

</asp:Content>
