<%@ Page Title="My Profile" Language="C#" MasterPageFile="~/Modules/Tutor/Tutor.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="CodeLab.Modules.Tutor.Profile" %>
<%@ MasterType VirtualPath="~/Modules/Tutor/Tutor.Master" %>

<asp:Content ID="ProfileContent" ContentPlaceHolderID="TutorContent" runat="server">

    <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success d-block" Visible="false" />
    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

    <asp:PlaceHolder ID="phProfile" runat="server">

        <%-- ===== Profile header: cover, photo, name, quick facts ===== --%>
        <section class="cl-card cl-profile-hero">
            <div class="cl-profile-cover" aria-hidden="true"></div>

            <div class="cl-profile-hero-body">
                <div class="cl-profile-avatar-wrap">
                    <% if (AvatarSrc != null) { %>
                        <img class="cl-avatar cl-avatar-xl" src="<%: AvatarSrc %>" alt="Your profile photo" />
                    <% } else { %>
                        <span class="cl-avatar cl-avatar-xl" aria-hidden="true"><%: Initials %></span>
                    <% } %>
                    <label class="cl-avatar-camera" for="<%= fuAvatar.ClientID %>" title="Change profile photo">
                        <i data-lucide="camera" class="cl-icon" aria-hidden="true"></i>
                        <span class="visually-hidden">Change profile photo</span>
                    </label>
                </div>

                <div class="cl-profile-identity">
                    <h1><asp:Literal ID="litName" runat="server" Mode="Encode" /></h1>
                    <p class="cl-profile-headline"><asp:Literal ID="litHeadline" runat="server" Mode="Encode" /></p>
                    <div class="cl-profile-meta">
                        <asp:Label ID="lblStatusBadge" runat="server" CssClass="cl-badge cl-status" />
                        <span><i data-lucide="id-card" class="cl-icon" aria-hidden="true"></i><asp:Literal ID="litTutorCode" runat="server" Mode="Encode" /></span>
                        <span><i data-lucide="calendar-days" class="cl-icon" aria-hidden="true"></i><asp:Literal ID="litJoinedShort" runat="server" Mode="Encode" /></span>
                        <asp:HyperLink ID="hlPortfolioMeta" runat="server" Target="_blank" Visible="false">
                            <i data-lucide="external-link" class="cl-icon" aria-hidden="true"></i>Portfolio
                        </asp:HyperLink>
                    </div>
                </div>

                <div class="cl-profile-actions">
                    <asp:HyperLink ID="hlEditProfile" runat="server" NavigateUrl="~/Modules/Tutor/Profile?mode=edit"
                        CssClass="btn cl-btn-outline" Visible="false">
                        <i data-lucide="pencil" class="cl-icon me-1" aria-hidden="true"></i>Edit profile
                    </asp:HyperLink>
                </div>
            </div>

            <asp:PlaceHolder ID="phStats" runat="server" Visible="false">
                <div class="cl-profile-stats">
                    <div class="cl-profile-stat">
                        <span class="cl-profile-stat-value"><asp:Literal ID="litStatCourses" runat="server" Text="0" /></span>
                        <span class="cl-profile-stat-label">Courses</span>
                    </div>
                    <div class="cl-profile-stat">
                        <span class="cl-profile-stat-value"><asp:Literal ID="litStatPublished" runat="server" Text="0" /></span>
                        <span class="cl-profile-stat-label">Published</span>
                    </div>
                    <div class="cl-profile-stat">
                        <span class="cl-profile-stat-value"><asp:Literal ID="litStatStudents" runat="server" Text="0" /></span>
                        <span class="cl-profile-stat-label">Students</span>
                    </div>
                </div>
            </asp:PlaceHolder>
        </section>

        <%-- Photo upload: the camera button opens this picker, and choosing a file uploads it straight away --%>
        <div class="visually-hidden">
            <asp:FileUpload ID="fuAvatar" runat="server" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
            <asp:Button ID="btnUploadAvatar" runat="server" Text="Upload photo" CausesValidation="false"
                OnClick="btnUploadAvatar_Click" TabIndex="-1" />
        </div>
        <div id="avatarClientError" class="alert alert-danger" role="alert" hidden></div>

        <div class="row g-4">
            <div class="col-lg-8">

                <%-- ===== Overview (Pending or Approved, not editing) ===== --%>
                <asp:PlaceHolder ID="phOverview" runat="server">
                    <section class="cl-card cl-panel cl-profile-section">
                        <div class="cl-section-heading">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="user-round" class="cl-icon" aria-hidden="true"></i></span>
                            <h2>About</h2>
                        </div>
                        <p class="cl-prose"><asp:Literal ID="litBio" runat="server" Mode="Encode" /></p>
                    </section>

                    <section class="cl-card cl-panel cl-profile-section">
                        <div class="cl-section-heading">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="presentation" class="cl-icon" aria-hidden="true"></i></span>
                            <h2>Teaching pitch</h2>
                        </div>
                        <blockquote class="cl-pitch"><asp:Literal ID="litPitch" runat="server" Mode="Encode" /></blockquote>
                    </section>

                    <section class="cl-card cl-panel cl-profile-section">
                        <div class="cl-section-heading">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="graduation-cap" class="cl-icon" aria-hidden="true"></i></span>
                            <h2>Qualifications</h2>
                        </div>
                        <p class="cl-prose"><asp:Literal ID="litQualifications" runat="server" Mode="Encode" /></p>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== Administrators have no tutor details ===== --%>
                <asp:PlaceHolder ID="phAdminNote" runat="server" Visible="false">
                    <section class="cl-card cl-panel cl-empty">
                        <span class="cl-icon-tile"><i data-lucide="shield-check" class="cl-icon" aria-hidden="true"></i></span>
                        <h3>Administrator account</h3>
                        <p>Administrators can use every tutor tool without a tutor application.</p>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== Form: apply, resubmit after rejection, or edit an existing profile ===== --%>
                <asp:PlaceHolder ID="phForm" runat="server" Visible="false">
                    <section class="cl-card cl-panel cl-form">
                        <div class="cl-form-header">
                            <h2><asp:Literal ID="litFormTitle" runat="server" Mode="Encode" /></h2>
                            <p><asp:Literal ID="litFormIntro" runat="server" Mode="Encode" /></p>
                        </div>

                        <asp:PlaceHolder ID="phPhotoRow" runat="server" Visible="false">
                            <div class="cl-photo-row">
                                <% if (AvatarSrc != null) { %>
                                    <img class="cl-avatar cl-avatar-md" src="<%: AvatarSrc %>" alt="" />
                                <% } else { %>
                                    <span class="cl-avatar cl-avatar-md" aria-hidden="true"><%: Initials %></span>
                                <% } %>
                                <div class="cl-photo-row-text">
                                    <strong>Profile photo</strong>
                                    <span>JPG, PNG or WebP, up to 2 MB. Saved straight away.</span>
                                </div>
                                <div class="cl-photo-row-actions">
                                    <label class="btn cl-btn-outline btn-sm mb-0" for="<%= fuAvatar.ClientID %>">
                                        <i data-lucide="upload" class="cl-icon me-1" aria-hidden="true"></i>Upload
                                    </label>
                                    <asp:Button ID="btnRemoveAvatar" runat="server" Text="Remove" CssClass="btn btn-link btn-sm cl-link-danger"
                                        CausesValidation="false" OnClientClick="return confirm('Remove your profile photo?');" OnClick="btnRemoveAvatar_Click" />
                                </div>
                            </div>
                        </asp:PlaceHolder>

                        <asp:ValidationSummary ID="vsProfile" runat="server" ValidationGroup="Profile" CssClass="alert alert-danger"
                            HeaderText="Please fix the following:" DisplayMode="BulletList" />

                        <%-- ValidateRequestMode="Disabled" lets tutors type code such as List<int>. That is safe
                             because this page only ever shows these values as encoded text, never as HTML. --%>
                        <div class="row g-3">
                            <div class="col-md-6">
                                <asp:Label ID="lblFullName" runat="server" AssociatedControlID="txtFullName" CssClass="form-label" Text="Full name" />
                                <asp:TextBox ID="txtFullName" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="120" />
                                <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName"
                                    ErrorMessage="Full name is required." CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                            </div>
                            <div class="col-md-6">
                                <asp:Label ID="lblSpecialty" runat="server" AssociatedControlID="txtSpecialty" CssClass="form-label" Text="Specialty" />
                                <asp:TextBox ID="txtSpecialty" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="100"
                                    placeholder="e.g. Web Development, Python, SQL" />
                                <asp:RequiredFieldValidator ID="rfvSpecialty" runat="server" ControlToValidate="txtSpecialty"
                                    ErrorMessage="Specialty is required." CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                            </div>

                            <div class="col-12">
                                <asp:Label ID="lblPortfolioUrl" runat="server" AssociatedControlID="txtPortfolioUrl" CssClass="form-label" Text="Portfolio link" />
                                <asp:TextBox ID="txtPortfolioUrl" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="500"
                                    placeholder="https://github.com/your-name" />
                                <asp:RequiredFieldValidator ID="rfvPortfolioUrl" runat="server" ControlToValidate="txtPortfolioUrl"
                                    ErrorMessage="Portfolio link is required." CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                                <asp:RegularExpressionValidator ID="revPortfolioUrl" runat="server" ControlToValidate="txtPortfolioUrl"
                                    ValidationExpression="^\s*https?://\S+\s*$" ErrorMessage="Portfolio link must start with http:// or https://."
                                    CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                            </div>

                            <div class="col-12">
                                <asp:Label ID="lblBio" runat="server" AssociatedControlID="txtBio" CssClass="form-label" Text="About you" />
                                <asp:TextBox ID="txtBio" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="4"
                                    placeholder="A short introduction learners will see on your profile" />
                                <asp:RequiredFieldValidator ID="rfvBio" runat="server" ControlToValidate="txtBio"
                                    ErrorMessage="About you is required." CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                            </div>

                            <div class="col-12">
                                <asp:Label ID="lblPitch" runat="server" AssociatedControlID="txtPitch" CssClass="form-label" Text="Sample curriculum pitch" />
                                <asp:TextBox ID="txtPitch" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="5"
                                    placeholder="Describe a course you would teach: who it is for, the main topics, and the hands-on work" />
                                <asp:RequiredFieldValidator ID="rfvPitch" runat="server" ControlToValidate="txtPitch"
                                    ErrorMessage="Sample curriculum pitch is required." CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                            </div>

                            <div class="col-12">
                                <asp:Label ID="lblQualifications" runat="server" AssociatedControlID="txtQualifications" CssClass="form-label" Text="Qualifications" />
                                <asp:TextBox ID="txtQualifications" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" TextMode="MultiLine" Rows="3"
                                    placeholder="Degrees, certifications and teaching experience" />
                                <asp:RequiredFieldValidator ID="rfvQualifications" runat="server" ControlToValidate="txtQualifications"
                                    ErrorMessage="Qualifications are required." CssClass="text-danger small" Display="Dynamic" ValidationGroup="Profile" />
                            </div>
                        </div>

                        <div class="cl-form-actions">
                            <asp:HyperLink ID="hlCancelEdit" runat="server" NavigateUrl="~/Modules/Tutor/Profile" CssClass="btn cl-btn-outline"
                                Text="Cancel" Visible="false" />
                            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Profile" OnClick="btnSave_Click" />
                        </div>
                    </section>
                </asp:PlaceHolder>
            </div>

            <div class="col-lg-4">
                <%-- ===== Application status timeline ===== --%>
                <asp:PlaceHolder ID="phTimeline" runat="server">
                    <section class="cl-card cl-panel cl-profile-section">
                        <div class="cl-section-heading">
                            <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="route" class="cl-icon" aria-hidden="true"></i></span>
                            <h2>Application</h2>
                        </div>
                        <asp:Repeater ID="rptTimeline" runat="server">
                            <HeaderTemplate><ol class="cl-timeline"></HeaderTemplate>
                            <ItemTemplate>
                                <li class="<%# Eval("CssClass") %>">
                                    <span class="cl-timeline-dot"><i data-lucide="<%# Eval("Icon") %>" class="cl-icon" aria-hidden="true"></i></span>
                                    <strong><%#: Eval("Title") %></strong>
                                    <span><%#: Eval("Text") %></span>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate></ol></FooterTemplate>
                        </asp:Repeater>
                    </section>
                </asp:PlaceHolder>

                <%-- ===== Account details ===== --%>
                <section class="cl-card cl-panel cl-profile-section">
                    <div class="cl-section-heading">
                        <span class="cl-icon-tile cl-icon-tile-sm"><i data-lucide="contact-round" class="cl-icon" aria-hidden="true"></i></span>
                        <h2>Account</h2>
                    </div>
                    <ul class="cl-info-list">
                        <li>
                            <i data-lucide="at-sign" class="cl-icon" aria-hidden="true"></i>
                            <div><span>Username</span><strong><asp:Literal ID="litUsername" runat="server" Mode="Encode" /></strong></div>
                        </li>
                        <li>
                            <i data-lucide="mail" class="cl-icon" aria-hidden="true"></i>
                            <div><span>Email</span><strong class="cl-break"><asp:Literal ID="litEmail" runat="server" Mode="Encode" /></strong></div>
                        </li>
                        <li>
                            <i data-lucide="sparkles" class="cl-icon" aria-hidden="true"></i>
                            <div><span>Specialty</span><strong><asp:Literal ID="litSpecialty" runat="server" Mode="Encode" /></strong></div>
                        </li>
                        <li>
                            <i data-lucide="link" class="cl-icon" aria-hidden="true"></i>
                            <div><span>Portfolio</span><strong><asp:HyperLink ID="hlPortfolio" runat="server" Target="_blank" CssClass="cl-break" /></strong></div>
                        </li>
                        <li>
                            <i data-lucide="calendar-check" class="cl-icon" aria-hidden="true"></i>
                            <div><span>Member since</span><strong><asp:Literal ID="litJoined" runat="server" Mode="Encode" /></strong></div>
                        </li>
                    </ul>
                </section>
            </div>
        </div>

    </asp:PlaceHolder>

    <%-- Checks the photo in the browser, then uploads it. The server checks again in TutorManager.UpdateAvatar. --%>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var input = document.getElementById('<%= fuAvatar.ClientID %>');
            var uploadButton = document.getElementById('<%= btnUploadAvatar.ClientID %>');
            var error = document.getElementById('avatarClientError');
            if (!input || !uploadButton) {
                return;
            }

            input.addEventListener('change', function () {
                var file = input.files && input.files[0];
                var message = '';

                if (!file) {
                    return;
                } else if (!/\.(jpe?g|png|webp)$/i.test(file.name)) {
                    message = 'The photo must be a JPG, PNG or WebP image.';
                } else if (file.size > <%= CodeLab.BLL.TutorManager.MaxAvatarBytes %>) {
                    message = 'The photo must be 2 MB or smaller.';
                }

                if (message) {
                    error.textContent = message;
                    error.hidden = false;
                    input.value = '';
                    return;
                }

                error.hidden = true;
                document.body.classList.add('cl-uploading');
                uploadButton.click();
            });
        });
    </script>

</asp:Content>
