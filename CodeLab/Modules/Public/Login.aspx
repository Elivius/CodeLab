<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CodeLab.Modules.Public.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="cl-card cl-auth">
        <h1>Log in</h1>

        <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success d-block" Visible="false" />
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

        <div class="mb-3">
            <asp:Label ID="lblUsername" runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="50" />
            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername"
                ErrorMessage="Username is required." CssClass="text-danger" Display="Dynamic" />
        </div>

        <div class="mb-3">
            <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                ErrorMessage="Password is required." CssClass="text-danger" Display="Dynamic" />
        </div>

        <asp:Button ID="btnLogin" runat="server" Text="Log in" CssClass="btn btn-primary" OnClick="btnLogin_Click" />

        <p class="cl-auth-switch">No account? <a runat="server" href="~/Modules/Public/Register">Register</a></p>
    </main>

</asp:Content>
