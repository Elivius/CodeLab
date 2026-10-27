<%@ Page Title="Analytics" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="Analytics.aspx.cs" Inherits="CodeLab.Modules.Tutor.Analytics" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>
<%@ Import Namespace="CodeLab.Models" %>

<asp:Content ID="AnalyticsContent" ContentPlaceHolderID="TutorContent" runat="server">

    <%-- ===== Header: title and course filter ===== --%>
    <div class="cl-builder-header">
        <div class="cl-builder-heading">
            <nav class="cl-crumbs" aria-label="Breadcrumb">
                <a href="<%: Master.GetPageUrl("Dashboard") %>">Dashboard</a>
                <i data-lucide="chevron-right" class="cl-icon" aria-hidden="true"></i>
                <span>Analytics</span>
            </nav>
            <h1>Assessment analytics</h1>
            <p><asp:Literal ID="litSubheading" runat="server" Mode="Encode" /></p>
        </div>
        <asp:PlaceHolder ID="phFilter" runat="server" Visible="false">
            <div class="cl-analytics-filter">
                <label for="courseFilter"><i data-lucide="filter" class="cl-icon" aria-hidden="true"></i>Course</label>
                <select id="courseFilter" class="form-select" data-base-url="<%: Master.GetPageUrl("Analytics") %>">
                    <asp:Literal ID="litCourseOptions" runat="server" Mode="PassThrough" />
                </select>
            </div>
        </asp:PlaceHolder>
    </div>

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
            <h2>Analytics are locked</h2>
            <p>You can see learner analytics once an administrator approves your tutor application.</p>
            <a class="btn btn-primary" href="<%: Master.GetPageUrl("Profile") %>">View my application</a>
        </section>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phMain" runat="server" Visible="false">

        <%-- ===== Shown until learners start using the courses ===== --%>
        <asp:PlaceHolder ID="phNoActivity" runat="server" Visible="false">
            <div class="cl-feedback cl-feedback-info" role="status">
                <span class="cl-builder-icon cl-tone-amber"><i data-lucide="hourglass" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <strong>No learner activity yet</strong>
                    <p>The numbers below fill in as learners enrol, finish lessons, take quizzes and submit code.</p>
                </div>
            </div>
        </asp:PlaceHolder>

        <%-- ===== Summary cards ===== --%>
        <div class="cl-stat-grid cl-analytics-stats">
            <div class="cl-stat-card">
                <span class="cl-stat-icon cl-tone-blue"><i data-lucide="users" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-stat-card-value"><asp:Literal ID="litStudents" runat="server" Mode="Encode" /></div>
                    <div class="cl-stat-card-label">Students</div>
                    <div class="cl-stat-card-note"><asp:Literal ID="litStudentsNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
            <div class="cl-stat-card">
                <span class="cl-stat-icon cl-tone-pink"><i data-lucide="graduation-cap" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-stat-card-value"><asp:Literal ID="litCompletion" runat="server" Mode="Encode" /></div>
                    <div class="cl-stat-card-label">Completion rate</div>
                    <div class="cl-stat-card-note"><asp:Literal ID="litCompletionNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
            <div class="cl-stat-card">
                <span class="cl-stat-icon cl-tone-green"><i data-lucide="trending-up" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-stat-card-value"><asp:Literal ID="litAverageScore" runat="server" Mode="Encode" /></div>
                    <div class="cl-stat-card-label">Avg quiz score</div>
                    <div class="cl-stat-card-note"><asp:Literal ID="litAverageScoreNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
            <div class="cl-stat-card">
                <span class="cl-stat-icon cl-tone-amber"><i data-lucide="circle-check-big" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-stat-card-value"><asp:Literal ID="litPassRate" runat="server" Mode="Encode" /></div>
                    <div class="cl-stat-card-label">Quiz pass rate</div>
                    <div class="cl-stat-card-note"><asp:Literal ID="litPassRateNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
            <div class="cl-stat-card">
                <span class="cl-stat-icon cl-tone-neutral"><i data-lucide="square-terminal" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-stat-card-value"><asp:Literal ID="litSubmissions" runat="server" Mode="Encode" /></div>
                    <div class="cl-stat-card-label">Code submissions</div>
                    <div class="cl-stat-card-note"><asp:Literal ID="litSubmissionsNote" runat="server" Mode="Encode" /></div>
                </div>
            </div>
            <div class="cl-stat-card">
                <span class="cl-stat-icon cl-tone-red"><i data-lucide="message-circle-question" class="cl-icon" aria-hidden="true"></i></span>
                <div>
                    <div class="cl-stat-card-value"><asp:Literal ID="litUnanswered" runat="server" Mode="Encode" /></div>
                    <div class="cl-stat-card-label">Unanswered questions</div>
                    <div class="cl-stat-card-note">In your course discussions</div>
                </div>
            </div>
        </div>

        <%-- ===== Charts ===== --%>
        <div class="cl-analytics-grid">
            <section class="cl-card cl-chart-card cl-span-2">
                <div class="cl-chart-head">
                    <div>
                        <h2><asp:Literal ID="litProgressTitle" runat="server" Mode="Encode" /></h2>
                        <p><asp:Literal ID="litProgressNote" runat="server" Mode="Encode" /></p>
                    </div>
                </div>
                <div class="cl-chart-box"><canvas id="chartProgress" aria-label="Course progress chart" role="img"></canvas></div>
            </section>

            <section class="cl-card cl-chart-card">
                <div class="cl-chart-head">
                    <div><h2>Quiz score distribution</h2><p>How learners scored, across all attempts.</p></div>
                </div>
                <div class="cl-chart-box"><canvas id="chartScores" aria-label="Quiz score distribution chart" role="img"></canvas></div>
            </section>

            <section class="cl-card cl-chart-card cl-span-2">
                <div class="cl-chart-head">
                    <div><h2>Activity, last 8 weeks</h2><p>Quiz attempts and code submissions per week.</p></div>
                </div>
                <div class="cl-chart-box"><canvas id="chartActivity" aria-label="Weekly activity chart" role="img"></canvas></div>
            </section>

            <section class="cl-card cl-chart-card">
                <div class="cl-chart-head">
                    <div><h2>Code Lab results</h2><p>Outcome of every code submission.</p></div>
                </div>
                <div class="cl-chart-box"><canvas id="chartCode" aria-label="Code Lab results chart" role="img"></canvas></div>
            </section>
        </div>

        <%-- ===== Results per quiz ===== --%>
        <section class="cl-card cl-table-card cl-analytics-section">
            <div class="cl-table-card-header">
                <h2>Quiz results</h2>
                <p>One row per course quiz. The pass rate turns green once it reaches the quiz's pass mark.</p>
            </div>
            <asp:Repeater ID="rptQuizzes" runat="server">
                <HeaderTemplate>
                    <div class="table-responsive">
                    <table class="table cl-table">
                        <thead><tr><th>Quiz</th><th>Questions</th><th>Attempts</th><th>Learners</th><th>Avg score</th><th>Pass rate</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="cl-table-title"><%#: Eval("QuizTitle") %><small class="cl-table-sub"><%#: Eval("CourseTitle") %></small></td>
                        <td><%# Eval("QuestionCount") %></td>
                        <td><%# Eval("Attempts") %></td>
                        <td><%# Eval("Learners") %></td>
                        <td><%#: ScoreText((double?)Eval("AverageScore")) %></td>
                        <td><%# PassRateHtml((TutorAnalytics.QuizStatRow)Container.DataItem) %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></div></FooterTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="phNoQuizzes" runat="server" Visible="false">
                <div class="cl-empty cl-empty-sm">
                    <p>None of these courses has a quiz yet.</p>
                </div>
            </asp:PlaceHolder>
        </section>

        <%-- ===== Results per coding challenge ===== --%>
        <section class="cl-card cl-table-card cl-analytics-section">
            <div class="cl-table-card-header">
                <h2>Coding challenge results</h2>
                <p>A learner counts as solved once one of their submissions passes.</p>
            </div>
            <asp:Repeater ID="rptChallenges" runat="server">
                <HeaderTemplate>
                    <div class="table-responsive">
                    <table class="table cl-table">
                        <thead><tr><th>Challenge</th><th>Language</th><th>Submissions</th><th>Learners</th><th>Solved</th><th>Solve rate</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="cl-table-title"><%#: Eval("Title") %><small class="cl-table-sub"><%#: Eval("CourseTitle") %></small></td>
                        <td><span class="cl-code-chip"><%#: LanguageName((string)Eval("Language")) %></span></td>
                        <td><%# Eval("Submissions") %></td>
                        <td><%# Eval("Learners") %></td>
                        <td><%# Eval("Solvers") %></td>
                        <td><%# SolveRateHtml((TutorAnalytics.ChallengeStatRow)Container.DataItem) %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></div></FooterTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="phNoChallenges" runat="server" Visible="false">
                <div class="cl-empty cl-empty-sm">
                    <p>None of these courses has a coding challenge yet.</p>
                </div>
            </asp:PlaceHolder>
        </section>

        <%-- ===== Latest quiz attempts and top learners ===== --%>
        <div class="cl-analytics-grid">
            <section class="cl-card cl-table-card cl-span-2">
                <div class="cl-table-card-header">
                    <h2>Recent quiz attempts</h2>
                    <p>The latest 8 attempts.</p>
                </div>
                <asp:Repeater ID="rptAttempts" runat="server">
                    <HeaderTemplate>
                        <div class="table-responsive">
                        <table class="table cl-table">
                            <thead><tr><th>Learner</th><th>Quiz</th><th>Score</th><th>Result</th><th>When</th></tr></thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# LearnerHtml((string)Eval("LearnerName"), (string)Eval("LearnerAvatarUrl")) %></td>
                            <td class="cl-table-title"><%#: Eval("QuizTitle") %><small class="cl-table-sub"><%#: Eval("CourseTitle") %></small></td>
                            <td><strong><%# Eval("ScorePercent") %>%</strong> <small class="text-muted">(<%# Eval("CorrectCount") %>/<%# Eval("TotalQuestions") %>)</small></td>
                            <td><%# (bool)Eval("Passed") ? "<span class=\"cl-badge cl-status cl-badge-success\">Passed</span>" : "<span class=\"cl-badge cl-status cl-badge-danger\">Failed</span>" %></td>
                            <td class="text-muted"><%#: TimeAgo((DateTime)Eval("AttemptedAt")) %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></div></FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="phNoAttempts" runat="server" Visible="false">
                    <div class="cl-empty cl-empty-sm"><p>No quiz attempts yet.</p></div>
                </asp:PlaceHolder>
            </section>

            <section class="cl-card cl-side-list">
                <div class="cl-chart-head">
                    <div><h2>Top learners</h2><p>Best average quiz score.</p></div>
                </div>
                <asp:Repeater ID="rptTopLearners" runat="server">
                    <HeaderTemplate><ol class="cl-rank-list"></HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            <span class="cl-rank"><%# Container.ItemIndex + 1 %></span>
                            <%# LearnerHtml((string)Eval("LearnerName"), (string)Eval("LearnerAvatarUrl")) %>
                            <span class="cl-rank-score">
                                <strong><%#: ScoreText((double)Eval("AverageScore")) %></strong>
                                <small><%# Eval("Attempts") %> attempts · best <%# Eval("BestScore") %>%</small>
                            </span>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ol></FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="phNoTopLearners" runat="server" Visible="false">
                    <div class="cl-empty cl-empty-sm"><p>Appears once learners take a quiz.</p></div>
                </asp:PlaceHolder>
            </section>
        </div>

        <%-- ===== Latest code submissions and unanswered questions ===== --%>
        <div class="cl-analytics-grid">
            <section class="cl-card cl-table-card cl-span-2">
                <div class="cl-table-card-header">
                    <h2>Recent code submissions</h2>
                    <p>The latest 8 submissions in the Code Lab.</p>
                </div>
                <asp:Repeater ID="rptSubmissions" runat="server">
                    <HeaderTemplate>
                        <div class="table-responsive">
                        <table class="table cl-table">
                            <thead><tr><th>Learner</th><th>Challenge</th><th>Language</th><th>Result</th><th>When</th></tr></thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# LearnerHtml((string)Eval("LearnerName"), (string)Eval("LearnerAvatarUrl")) %></td>
                            <td class="cl-table-title"><%#: Eval("ChallengeTitle") %></td>
                            <td><span class="cl-code-chip"><%#: LanguageName((string)Eval("Language")) %></span></td>
                            <td><%# SubmissionStatusHtml((string)Eval("Status")) %></td>
                            <td class="text-muted"><%#: TimeAgo((DateTime)Eval("SubmittedAt")) %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></table></div></FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="phNoSubmissions" runat="server" Visible="false">
                    <div class="cl-empty cl-empty-sm"><p>No code submissions yet.</p></div>
                </asp:PlaceHolder>
            </section>

            <section class="cl-card cl-side-list">
                <div class="cl-chart-head">
                    <div><h2>Unanswered questions</h2><p>Discussion posts with no reply yet.</p></div>
                </div>
                <asp:Repeater ID="rptUnanswered" runat="server">
                    <HeaderTemplate><ul class="cl-question-feed"></HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            <span class="cl-builder-icon cl-tone-red"><i data-lucide="message-circle-question" class="cl-icon" aria-hidden="true"></i></span>
                            <div>
                                <strong><%#: Eval("Title") %></strong>
                                <small><%#: Eval("AuthorName") %> · <%#: Eval("CourseTitle") %> · <%#: TimeAgo((DateTime)Eval("CreatedAt")) %></small>
                            </div>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ul></FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder ID="phAllAnswered" runat="server" Visible="false">
                    <div class="cl-all-clear">
                        <span class="cl-builder-icon cl-tone-green"><i data-lucide="circle-check" class="cl-icon" aria-hidden="true"></i></span>
                        <span>Every question has a reply.</span>
                    </div>
                </asp:PlaceHolder>
            </section>
        </div>

        <%-- Chart data for the script below (encoded JSON) --%>
        <script type="application/json" id="analyticsData"><asp:Literal ID="litChartJson" runat="server" Mode="PassThrough" /></script>
    </asp:PlaceHolder>

    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.1/chart.umd.min.js"></script>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // Course filter: reload the page for the chosen course
            var filter = document.getElementById('courseFilter');
            if (filter) {
                filter.addEventListener('change', function () {
                    var base = filter.getAttribute('data-base-url');
                    window.location.href = filter.value ? base + '?courseId=' + encodeURIComponent(filter.value) : base;
                });
            }

            var source = document.getElementById('analyticsData');
            if (!source || !window.Chart) {
                return;
            }

            var data = JSON.parse(source.textContent);
            var charts = [];

            // Colours follow the light / dark theme
            function themeColors() {
                var style = getComputedStyle(document.documentElement);
                var dark = document.documentElement.getAttribute('data-theme') === 'dark';
                return {
                    text: style.getPropertyValue('--cl-text-soft').trim() || (dark ? '#D1D5DB' : '#4B5563'),
                    grid: dark ? 'rgba(255,255,255,0.08)' : 'rgba(45,52,54,0.08)',
                    surface: style.getPropertyValue('--cl-surface').trim() || (dark ? '#1F2937' : '#FFFFFF')
                };
            }

            var palette = {
                pink: '#C9527D', coral: '#FFA2A4', blue: '#3B82F6', green: '#10B981',
                amber: '#F59E0B', red: '#F43F5E', grey: '#9CA3AF'
            };

            // A chart with nothing to show gets a short message instead of an empty axis
            function showEmpty(canvas, message) {
                var box = canvas.parentNode;
                box.classList.add('is-empty');
                box.innerHTML = '<div class="cl-chart-empty"><i data-lucide="chart-no-axes-column" class="cl-icon" aria-hidden="true"></i><span>'
                    + message + '</span></div>';
                if (window.lucide) { window.lucide.createIcons(); }
            }

            function sum(values) {
                return values.reduce(function (a, b) { return a + b; }, 0);
            }

            function build() {
                charts.forEach(function (chart) { chart.destroy(); });
                charts = [];

                var c = themeColors();
                Chart.defaults.color = c.text;
                Chart.defaults.font.family = 'Inter, system-ui, sans-serif';
                Chart.defaults.borderColor = c.grid;

                var axis = { grid: { color: c.grid }, ticks: { precision: 0 }, beginAtZero: true };
                var legend = { position: 'bottom', labels: { usePointStyle: true, boxWidth: 8, padding: 16 } };

                // 1. Course progress, or lesson drop-off for one course
                var progressCanvas = document.getElementById('chartProgress');
                if (progressCanvas) {
                    if (data.lessons) {
                        if (data.lessons.labels.length === 0) {
                            showEmpty(progressCanvas, 'This course has no lessons yet.');
                        } else {
                            charts.push(new Chart(progressCanvas, {
                                type: 'bar',
                                data: {
                                    labels: data.lessons.labels,
                                    datasets: [{ label: 'Learners who finished the lesson', data: data.lessons.values,
                                        backgroundColor: palette.pink, borderRadius: 8, maxBarThickness: 48 }]
                                },
                                options: { maintainAspectRatio: false, plugins: { legend: { display: false } },
                                    scales: { x: { grid: { display: false } }, y: axis } }
                            }));
                        }
                    } else if (data.courses.labels.length === 0) {
                        showEmpty(progressCanvas, 'Create a course to see its progress here.');
                    } else {
                        charts.push(new Chart(progressCanvas, {
                            type: 'bar',
                            data: {
                                labels: data.courses.labels,
                                datasets: [
                                    { label: 'Enrolled', data: data.courses.enrolled, backgroundColor: palette.blue, borderRadius: 8, maxBarThickness: 40 },
                                    { label: 'Completed', data: data.courses.completed, backgroundColor: palette.green, borderRadius: 8, maxBarThickness: 40 }
                                ]
                            },
                            options: { maintainAspectRatio: false, plugins: { legend: legend },
                                scales: { x: { grid: { display: false } }, y: axis } }
                        }));
                    }
                }

                // 2. Score distribution
                var scoreCanvas = document.getElementById('chartScores');
                if (scoreCanvas) {
                    if (sum(data.scores) === 0) {
                        showEmpty(scoreCanvas, 'No quiz attempts yet.');
                    } else {
                        charts.push(new Chart(scoreCanvas, {
                            type: 'doughnut',
                            data: {
                                labels: ['0–49%', '50–74%', '75–89%', '90–100%'],
                                datasets: [{ data: data.scores, backgroundColor: [palette.red, palette.amber, palette.blue, palette.green],
                                    borderColor: c.surface, borderWidth: 3 }]
                            },
                            options: { maintainAspectRatio: false, cutout: '62%', plugins: { legend: legend } }
                        }));
                    }
                }

                // 3. Weekly activity
                var activityCanvas = document.getElementById('chartActivity');
                if (activityCanvas) {
                    if (sum(data.activity.quiz) + sum(data.activity.code) === 0) {
                        showEmpty(activityCanvas, 'No quiz attempts or code submissions in the last 8 weeks.');
                    } else {
                        charts.push(new Chart(activityCanvas, {
                            type: 'line',
                            data: {
                                labels: data.activity.labels,
                                datasets: [
                                    { label: 'Quiz attempts', data: data.activity.quiz, borderColor: palette.pink, backgroundColor: 'rgba(201,82,125,0.12)',
                                        fill: true, tension: 0.35, pointRadius: 3 },
                                    { label: 'Code submissions', data: data.activity.code, borderColor: palette.blue, backgroundColor: 'rgba(59,130,246,0.10)',
                                        fill: true, tension: 0.35, pointRadius: 3 }
                                ]
                            },
                            options: { maintainAspectRatio: false, interaction: { mode: 'index', intersect: false },
                                plugins: { legend: legend }, scales: { x: { grid: { display: false } }, y: axis } }
                        }));
                    }
                }

                // 4. Code Lab results
                var codeCanvas = document.getElementById('chartCode');
                if (codeCanvas) {
                    if (sum(data.code) === 0) {
                        showEmpty(codeCanvas, 'No code submissions yet.');
                    } else {
                        charts.push(new Chart(codeCanvas, {
                            type: 'doughnut',
                            data: {
                                labels: ['Passed', 'Failed', 'Error'],
                                datasets: [{ data: data.code, backgroundColor: [palette.green, palette.amber, palette.red],
                                    borderColor: c.surface, borderWidth: 3 }]
                            },
                            options: { maintainAspectRatio: false, cutout: '62%', plugins: { legend: legend } }
                        }));
                    }
                }
            }

            build();

            // Redraw in the new colours when the theme is switched
            new MutationObserver(function () {
                if (charts.length) { build(); }
            }).observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
        });
    </script>

</asp:Content>
