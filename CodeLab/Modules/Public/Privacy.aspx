<%@ Page Title="Privacy policy" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Privacy.aspx.cs" Inherits="CodeLab.Modules.Public.Privacy" %>

<asp:Content ID="PrivacyHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="<%: ResolveUrl("~/Content/Legal.css") %>" />
</asp:Content>

<asp:Content ID="PrivacyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-legal">
        <header class="cl-legal-head">
            <h1>Privacy policy</h1>
            <p class="cl-legal-date">Last updated 1 November 2026</p>
            <p class="cl-legal-intro">This page explains what CodeLab stores about you, who can see it and what your choices are. Each section has a short version written as a code comment.</p>
        </header>

        <section id="what-we-collect" class="cl-legal-section">
            <h2>1. What we collect</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Your account, your learning and what you post. No payment details.</p>
            <div class="cl-legal-body">
                <ul>
                    <li><strong>Account details:</strong> your full name, username, email address and profile photo. Your password is stored as a one-way hash, so nobody can read it.</li>
                    <li><strong>Tutor applications:</strong> your specialty, qualifications, portfolio link, bio and curriculum pitch.</li>
                    <li><strong>Learning records:</strong> your enrollments, progress, quiz attempts, submitted code, points, badges and daily streak.</li>
                    <li><strong>Posts:</strong> what you write in discussions.</li>
                    <li><strong>Activity records:</strong> what you do on the site and when, such as logging in, a failed login, or creating a course.</li>
                </ul>
                <p>CodeLab does not take payments, so we hold no card or bank details.</p>
            </div>
        </section>

        <section id="how-we-use-it" class="cl-legal-section">
            <h2>2. How we use it</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>To run your account and your courses. Nothing else.</p>
            <div class="cl-legal-body">
                <p>We use this information to run your account, show your courses and progress, review tutors and courses, and keep the site safe. We do not sell it, and we do not show advertising.</p>
            </div>
        </section>

        <section id="who-can-see-it" class="cl-legal-section">
            <h2>3. Who can see it</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Some of it is visible to other people on CodeLab.</p>
            <div class="cl-legal-body">
                <ul>
                    <li><strong>Other users</strong> can see your name next to your discussion posts. The leaderboard also shows learners' names, points and levels.</li>
                    <li><strong>Tutors</strong> can see the quiz and challenge results of learners on their own courses, with those learners' names.</li>
                    <li><strong>Administrators</strong> can see account details, tutor applications and the activity records.</li>
                </ul>
            </div>
        </section>

        <section id="outside-services" class="cl-legal-section">
            <h2>4. Outside services</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Your challenge code leaves CodeLab. Your name and email do not.</p>
            <div class="cl-legal-body">
                <p>When you run a coding challenge, we send your code to two outside services. We do not send your name or email address with it, so keep personal details out of your code.</p>
                <ul>
                    <li><strong>JDoodle</strong> runs your code.</li>
                    <li><strong>Google Gemini</strong> reviews your code, its output and the challenge text, and writes your feedback.</li>
                </ul>
                <p>Your browser also connects to other sites to load parts of each page: Google Fonts, unpkg and Cloudflare for fonts and page scripts, YouTube for lesson videos, and Unsplash for the ready-made profile photos. Those sites can see your IP address, as any website you visit can.</p>
            </div>
        </section>

        <section id="cookies" class="cl-legal-section">
            <h2>5. Cookies and browser storage</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Two cookies to keep you logged in, and one saved setting. No tracking.</p>
            <div class="cl-legal-body">
                <div class="table-responsive">
                    <table class="table cl-legal-table">
                        <thead>
                            <tr>
                                <th scope="col">Name</th>
                                <th scope="col">What it does</th>
                                <th scope="col">How long it lasts</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td><code>.ASPXAUTH</code></td>
                                <td>Keeps you logged in</td>
                                <td>Until you log out or close your browser, or after 60 minutes without activity</td>
                            </tr>
                            <tr>
                                <td><code>ASP.NET_SessionId</code></td>
                                <td>Remembers who you are from page to page</td>
                                <td>Until you close your browser. Its data on our side ends after 60 minutes without activity</td>
                            </tr>
                            <tr>
                                <td><code>codelab-theme</code></td>
                                <td>Remembers light or dark mode, in your browser's storage</td>
                                <td>Until you clear your browser's data</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <p>CodeLab does not use advertising or analytics cookies. YouTube may set its own cookies when a lesson video loads.</p>
            </div>
        </section>

        <section id="how-long-we-keep-it" class="cl-legal-section">
            <h2>6. How long we keep it</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>As long as your account exists. Backups hold a copy too.</p>
            <div class="cl-legal-body">
                <p>We keep your information for as long as your account exists, and we do not delete it automatically. Administrators can make backups of the database, and each backup holds a copy of everything in it at that time.</p>
            </div>
        </section>

        <section id="your-choices" class="cl-legal-section">
            <h2>7. Your choices</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>Edit your details yourself. Ask us for anything else.</p>
            <div class="cl-legal-body">
                <p>You can change most of your details on your Profile page. To ask for a copy of your information or a correction, <a runat="server" href="~/Modules/Public/Contact">contact us</a>.</p>
                <p>You can also ask us to close your account. An account with activity cannot be fully removed, because courses, results and activity records depend on it. We can suspend it instead, so it can no longer be used.</p>
            </div>
        </section>

        <section id="keeping-it-safe" class="cl-legal-section">
            <h2>8. Keeping it safe</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>We protect your data, but no site is perfectly secure.</p>
            <div class="cl-legal-body">
                <p>Passwords are hashed, the login cookie cannot be read by page scripts, and each role can only open the pages meant for it. No website is perfectly secure, so use a password you do not use anywhere else.</p>
            </div>
        </section>

        <section id="changes" class="cl-legal-section">
            <h2>9. Changes to this policy</h2>
            <p class="cl-legal-gist"><span class="visually-hidden">In short: </span>If this changes, the date at the top changes too.</p>
            <div class="cl-legal-body">
                <p>We may update this policy as CodeLab changes. The date at the top of this page shows the latest version.</p>
            </div>
        </section>

        <p class="cl-legal-end">Questions about your information? <a runat="server" href="~/Modules/Public/Contact">Contact us</a>. See also the <a runat="server" href="~/Modules/Public/Terms">Terms of use</a>.</p>
    </main>

</asp:Content>
