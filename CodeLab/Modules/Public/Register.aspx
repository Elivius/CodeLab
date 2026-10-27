<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="CodeLab.Modules.Public.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-card cl-auth cl-register">
        <h1>Create your account</h1>

        <%-- Plain links, so switching tabs never posts the form (which would clear the password boxes) --%>
        <nav class="cl-tabs" aria-label="Account type">
            <a id="lnkLearnerTab" runat="server" href="~/Modules/Public/Register?role=learner">Learner</a>
            <a id="lnkTutorTab" runat="server" href="~/Modules/Public/Register?role=tutor">Tutor</a>
        </nav>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

        <div class="mb-3">
            <asp:Label ID="lblFullName" runat="server" AssociatedControlID="txtFullName" CssClass="form-label" Text="Full name" />
            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="120" />
            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName"
                ErrorMessage="Full name is required." CssClass="text-danger" Display="Dynamic" />
        </div>

        <div class="mb-3">
            <asp:Label ID="lblUsername" runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="50" />
            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername"
                ErrorMessage="Username is required." CssClass="text-danger" Display="Dynamic" />
            <asp:CustomValidator ID="cvUsernameTaken" runat="server" ControlToValidate="txtUsername"
                OnServerValidate="cvUsernameTaken_ServerValidate"
                ErrorMessage="That username is already taken." CssClass="text-danger" Display="Dynamic" />
        </div>

        <div class="mb-3">
            <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="255" TextMode="Email" />
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                ErrorMessage="Email is required." CssClass="text-danger" Display="Dynamic" />
            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                ErrorMessage="Please enter a valid email address." CssClass="text-danger" Display="Dynamic" />
            <asp:CustomValidator ID="cvEmailTaken" runat="server" ControlToValidate="txtEmail"
                OnServerValidate="cvEmailTaken_ServerValidate"
                ErrorMessage="That email is already registered." CssClass="text-danger" Display="Dynamic" />
        </div>

        <div class="mb-3">
            <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
            <div class="form-text">At least 8 characters, including a number.</div>
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                ErrorMessage="Password is required." CssClass="text-danger" Display="Dynamic" />
            <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword"
                ErrorMessage="Password must be at least 8 characters and contain at least one number." CssClass="text-danger" Display="Dynamic" />
        </div>

        <div class="mb-4">
            <asp:Label ID="lblConfirm" runat="server" AssociatedControlID="txtConfirm" CssClass="form-label" Text="Confirm password" />
            <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvConfirm" runat="server" ControlToValidate="txtConfirm"
                ErrorMessage="Please confirm your password." CssClass="text-danger" Display="Dynamic" />
            <asp:CompareValidator ID="cmpConfirm" runat="server" ControlToValidate="txtConfirm" ControlToCompare="txtPassword"
                ErrorMessage="Passwords do not match." CssClass="text-danger" Display="Dynamic" />
        </div>

        <asp:Button ID="btnRegister" runat="server" Text="Create account" CssClass="btn btn-primary" OnClick="btnRegister_Click" />

        <p class="cl-auth-switch">Already have an account? <a runat="server" href="~/Modules/Public/Login">Log in</a></p>
    </main>

</asp:Content>
