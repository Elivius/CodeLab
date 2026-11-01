<%@ Page Title="User" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="UserEdit.aspx.cs" Inherits="CodeLab.Modules.Admin.UserEdit" %>

<%-- With no ?id= this page creates an account. With ?id= it edits that account. --%>
<asp:Content ID="UserEditContent" ContentPlaceHolderID="AdminContent" runat="server">

    <a runat="server" href="~/Modules/Admin/Users" class="cl-admin-back">
        <i data-lucide="arrow-left" class="cl-icon" aria-hidden="true"></i>All users
    </a>

    <asp:Label ID="lblMessage" runat="server" Visible="false" />

    <asp:PlaceHolder ID="phUser" runat="server">
        <div class="row g-4">
            <%-- ===== Details ===== --%>
            <div class="col-lg-8">
                <section class="cl-card cl-admin-panel">
                    <h2 class="cl-admin-panel-title"><asp:Literal ID="litHeading" runat="server" Mode="Encode" /></h2>

                    <%-- ValidateRequestMode="Disabled" lets the validators below answer text such as <b> with their own
                         message, instead of ASP.NET stopping the page. The rules reject it, so it is never saved. --%>
                    <div class="row g-3">
                        <div class="col-md-6">
                            <asp:Label ID="lblFullName" runat="server" AssociatedControlID="txtFullName" CssClass="form-label" Text="Full name" />
                            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="120" ValidateRequestMode="Disabled" />
                            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ValidationGroup="User"
                                ErrorMessage="Full name is required." CssClass="text-danger small" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revFullName" runat="server" ControlToValidate="txtFullName" ValidationGroup="User"
                                ErrorMessage="Use at least 2 characters: letters, spaces and . ' - / only." CssClass="text-danger small" Display="Dynamic" />
                        </div>

                        <div class="col-md-6">
                            <asp:Label ID="lblUsername" runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="50" ValidateRequestMode="Disabled" />
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" ValidationGroup="User"
                                ErrorMessage="Username is required." CssClass="text-danger small" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revUsername" runat="server" ControlToValidate="txtUsername" ValidationGroup="User"
                                ErrorMessage="Use 3 to 50 letters, numbers or underscores." CssClass="text-danger small" Display="Dynamic" />
                            <asp:CustomValidator ID="cvUsernameTaken" runat="server" ControlToValidate="txtUsername" ValidationGroup="User"
                                OnServerValidate="cvUsernameTaken_ServerValidate"
                                ErrorMessage="That username is already taken." CssClass="text-danger small" Display="Dynamic" />
                        </div>

                        <div class="col-md-6">
                            <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="255" TextMode="Email" ValidateRequestMode="Disabled" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="User"
                                ErrorMessage="Email is required." CssClass="text-danger small" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="User"
                                ErrorMessage="Please enter a valid email address." CssClass="text-danger small" Display="Dynamic" />
                            <asp:CustomValidator ID="cvEmailTaken" runat="server" ControlToValidate="txtEmail" ValidationGroup="User"
                                OnServerValidate="cvEmailTaken_ServerValidate"
                                ErrorMessage="That email is already registered." CssClass="text-danger small" Display="Dynamic" />
                        </div>

                        <div class="col-md-6">
                            <asp:Label ID="lblRole" runat="server" AssociatedControlID="ddlRole" CssClass="form-label" Text="Role" />
                            <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Learner" Value="Learner" />
                                <asp:ListItem Text="Tutor" Value="Tutor" />
                                <asp:ListItem Text="Admin" Value="Admin" />
                            </asp:DropDownList>
                            <span class="cl-admin-meta"><asp:Literal ID="litRoleNote" runat="server" Mode="Encode" /></span>
                        </div>

                        <asp:PlaceHolder ID="phNewAccount" runat="server">
                            <div class="col-md-6">
                                <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
                                <%-- A password can contain text such as <a. That is safe because it is only ever hashed, never shown. --%>
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="new-password" ValidateRequestMode="Disabled" />
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ValidationGroup="User"
                                    ErrorMessage="Password is required." CssClass="text-danger small" Display="Dynamic" />
                                <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" ValidationGroup="User"
                                    ErrorMessage="Password must be at least 8 characters and include a letter, a number and a symbol." CssClass="text-danger small" Display="Dynamic" />
                            </div>

                            <div class="col-md-6">
                                <asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label" Text="Confirm password" />
                                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="new-password" ValidateRequestMode="Disabled" />
                                <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ValidationGroup="User"
                                    ErrorMessage="Please confirm the password." CssClass="text-danger small" Display="Dynamic" />
                                <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" ValidationGroup="User"
                                    ErrorMessage="Passwords do not match." CssClass="text-danger small" Display="Dynamic" />
                            </div>

                            <div class="col-12">
                                <span class="cl-admin-meta">CodeLab does not send email, so give the user this password yourself.</span>
                            </div>
                        </asp:PlaceHolder>
                    </div>

                    <div class="cl-admin-form-actions">
                        <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="User" OnClick="btnSave_Click" />
                    </div>
                </section>
            </div>

            <%-- ===== Account status and actions: only for an existing account ===== --%>
            <asp:PlaceHolder ID="phAccount" runat="server">
                <div class="col-lg-4">
                    <section class="cl-card cl-admin-panel">
                        <h2 class="cl-admin-panel-title">Account</h2>

                        <dl class="cl-admin-facts">
                            <dt>Status</dt>
                            <dd><asp:Label ID="lblStatus" runat="server" /></dd>
                            <dt>Joined</dt>
                            <dd><asp:Literal ID="litJoined" runat="server" Mode="Encode" /></dd>
                            <dt>Last login</dt>
                            <dd><asp:Literal ID="litLastLogin" runat="server" Mode="Encode" /></dd>
                            <dt>Courses</dt>
                            <dd><asp:Literal ID="litCourses" runat="server" Mode="Encode" /></dd>
                        </dl>

                        <p class="cl-admin-meta"><asp:Literal ID="litAccountNote" runat="server" Mode="Encode" /></p>

                        <asp:Button ID="btnSuspend" runat="server" Text="Suspend account" CssClass="btn btn-outline-danger w-100"
                            CausesValidation="false" OnClick="btnSuspend_Click"
                            OnClientClick="return confirm('Suspend this account? The user is logged out on their next click and cannot log in.');" />
                        <asp:Button ID="btnReactivate" runat="server" Text="Reactivate account" CssClass="btn btn-success w-100"
                            CausesValidation="false" OnClick="btnReactivate_Click"
                            OnClientClick="return confirm('Reactivate this account? The user can log in again.');" />
                        <asp:Button ID="btnDelete" runat="server" Text="Delete account" CssClass="btn btn-link cl-admin-delete w-100 mt-2"
                            CausesValidation="false" OnClick="btnDelete_Click"
                            OnClientClick="return confirm('Delete this account permanently?');" />
                    </section>
                </div>
            </asp:PlaceHolder>
        </div>
    </asp:PlaceHolder>

</asp:Content>
