<%@ Page Title="Master Programming on Your Terms" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CodeLab._Default" %>
<%@ Register Src="~/UserControls/CourseGrid.ascx" TagPrefix="cl" TagName="CourseGrid" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-landing">

        <%-- ===== Hero ===== --%>
        <section class="cl-hero">
            <span class="cl-pill">
                <i data-lucide="sparkles" class="cl-icon" aria-hidden="true"></i>
                Interactive Computer Science Education
            </span>

            <h1>Master Programming<br /><span class="cl-gradient-text">on Your Terms</span></h1>

            <p class="cl-hero-lead">
                Explore interactive lessons, write and execute code in our zero-setup browser sandbox,
                and unlock complete curriculums upon registration.
            </p>

            <div class="d-flex flex-wrap justify-content-center gap-3 mb-5">
                <a runat="server" href="~/Modules/Public/Register?role=learner" class="btn btn-primary cl-btn-lg">Start Learning Free</a>
                <a href="#courses" class="btn cl-btn-outline cl-btn-lg">
                    Browse Course Catalogue
                    <i data-lucide="arrow-right" class="cl-icon ms-1" aria-hidden="true"></i>
                </a>
            </div>

            <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-4 g-4 pt-3">
                <div class="col">
                    <div class="cl-card cl-feature">
                        <span class="cl-icon-tile"><i data-lucide="book-open" class="cl-icon" aria-hidden="true"></i></span>
                        <h3>Structured Lessons</h3>
                        <p>Step-by-step curriculum guided by verified tech mentors.</p>
                    </div>
                </div>
                <div class="col">
                    <div class="cl-card cl-feature">
                        <span class="cl-icon-tile"><i data-lucide="code" class="cl-icon" aria-hidden="true"></i></span>
                        <h3>Live Sandbox</h3>
                        <p>Write and test your code directly in the in-browser runtime.</p>
                    </div>
                </div>
                <div class="col">
                    <div class="cl-card cl-feature">
                        <span class="cl-icon-tile"><i data-lucide="trophy" class="cl-icon" aria-hidden="true"></i></span>
                        <h3>Gamified Progress</h3>
                        <p>Earn points, unlock milestones, and level up your skills.</p>
                    </div>
                </div>
                <div class="col">
                    <div class="cl-card cl-feature">
                        <span class="cl-icon-tile"><i data-lucide="users" class="cl-icon" aria-hidden="true"></i></span>
                        <h3>Community Support</h3>
                        <p>Join active discussion boards to collaborate with peers.</p>
                    </div>
                </div>
            </div>
        </section>

        <%-- ===== Featured courses: 3 random Published courses, different on every load ===== --%>
        <section id="courses" class="cl-section">
            <div class="d-flex flex-wrap align-items-end justify-content-between gap-2 mb-4">
                <div>
                    <span class="cl-eyebrow">Curated Curriculum</span>
                    <h2 class="cl-section-title">Featured Courses</h2>
                </div>
                <a runat="server" href="~/Modules/Public/Courses" class="cl-link-arrow">
                    View all catalogue (<asp:Literal ID="litCourseCount" runat="server" Text="0" />)
                    <i data-lucide="arrow-right" class="cl-icon ms-1" aria-hidden="true"></i>
                </a>
            </div>

            <cl:CourseGrid ID="courseGrid" runat="server" />
        </section>

        <%-- ===== Sandbox panel ===== --%>
        <section class="cl-section">
            <div class="cl-dark-panel">
                <span class="cl-pill cl-pill-dark">
                    <i data-lucide="terminal" class="cl-icon" aria-hidden="true"></i>
                    Zero-Toolchain Code Execution
                </span>
                <h2>Write, Test, and Debug in Real-Time</h2>
                <p class="cl-lead mb-5">
                    Every lesson comes equipped with an automated test suite. Complete exercises directly
                    in your browser without spending hours configuring local environments.
                </p>

                <div class="row row-cols-1 row-cols-md-3 g-3">
                    <div class="col">
                        <div class="cl-step">
                            <span class="cl-step-label">01 &bull; Browser Sandboxes</span>
                            Instant runtime initialization for Python, JavaScript, TypeScript, and Go.
                        </div>
                    </div>
                    <div class="col">
                        <div class="cl-step">
                            <span class="cl-step-label">02 &bull; Automated Tests</span>
                            Run unit test suites against your implementations with immediate feedback.
                        </div>
                    </div>
                    <div class="col">
                        <div class="cl-step">
                            <span class="cl-step-label">03 &bull; Verified Pedagogy</span>
                            Curated and moderated content by verified engineering practitioners.
                        </div>
                    </div>
                </div>

                <div class="cl-panel-footer">
                    <span>Ready to unlock your interactive coding playground?</span>
                    <a runat="server" href="~/Modules/Public/Register?role=learner" class="btn btn-primary">Create Learner Account</a>
                </div>
            </div>
        </section>

        <%-- ===== About ===== --%>
        <section id="about" class="cl-section">
            <div class="cl-card cl-about">
                <span class="cl-pill">
                    <i data-lucide="shield" class="cl-icon" aria-hidden="true"></i>
                    ABOUT CODELAB
                </span>
                <h2>Built for Aspiring Engineers, Backed by Industry Mentors</h2>
                <p class="cl-lead mb-5">
                    CodeLab bridges the gap between passive video tutorials and production software engineering.
                    We pair peer-reviewed curricula authored by verified educators with an instantaneous,
                    browser-based runtime environment.
                </p>

                <div class="row row-cols-1 row-cols-md-3 g-4">
                    <div class="col">
                        <div class="cl-value">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="shield-check" class="cl-icon" aria-hidden="true"></i></span>
                            <h3>Rigorous Moderation</h3>
                            <p>No unverified content. Every course syllabus, lesson exercise, and starter repo undergoes administrative quality audit before publication.</p>
                        </div>
                    </div>
                    <div class="col">
                        <div class="cl-value">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="laptop" class="cl-icon" aria-hidden="true"></i></span>
                            <h3>Zero-Setup Sandboxes</h3>
                            <p>Eliminate local toolchain setup blockers. Execute code, run automated unit test suites, and inspect errors directly in the browser.</p>
                        </div>
                    </div>
                    <div class="col">
                        <div class="cl-value">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="award" class="cl-icon" aria-hidden="true"></i></span>
                            <h3>Verified Credentials</h3>
                            <p>Validate your competence through milestone-driven projects and shareable certificates recognized across engineering teams.</p>
                        </div>
                    </div>
                </div>

                <div class="row row-cols-2 row-cols-md-4 g-4 cl-stats">
                    <div class="col">
                        <div class="cl-stat-value">45,000+</div>
                        <div class="cl-stat-label">Active Learners</div>
                    </div>
                    <div class="col">
                        <div class="cl-stat-value cl-accent">98.4%</div>
                        <div class="cl-stat-label">Satisfaction Rate</div>
                    </div>
                    <div class="col">
                        <div class="cl-stat-value">5+</div>
                        <div class="cl-stat-label">Audited Curriculums</div>
                    </div>
                    <div class="col">
                        <div class="cl-stat-value cl-accent">100%</div>
                        <div class="cl-stat-label">In-Browser Execution</div>
                    </div>
                </div>

                <div class="cl-dark-panel cl-cta-bar">
                    <div>
                        <h3>Start Your Software Engineering Journey</h3>
                        <p>Register in less than 30 seconds to unlock complete curriculums, interactive code sandboxes, and quizzes.</p>
                    </div>
                    <div class="d-flex flex-wrap gap-2">
                        <a runat="server" href="~/Modules/Public/Register?role=learner" class="btn btn-primary cl-btn-lg">Register Free</a>
                        <a runat="server" href="~/Modules/Public/Register?role=tutor" class="btn cl-btn-outline cl-btn-lg">Teach on CodeLab</a>
                    </div>
                </div>
            </div>
        </section>

    </main>

</asp:Content>
