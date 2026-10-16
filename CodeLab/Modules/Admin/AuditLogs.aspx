<%@ Page Title="Audit Logs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AuditLogs.aspx.cs" Inherits="CodeLab.Modules.Admin.AuditLogs" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main class="mt-4">
        <h1>Audit Logs</h1>
        <p>The 100 most recent activities, newest first.</p>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

        <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="false" CssClass="table table-striped"
            GridLines="None" EmptyDataText="No activity has been recorded yet.">
            <Columns>
                <asp:TemplateField HeaderText="Time">
                    <ItemTemplate>
                        <%# ((DateTime)Eval("Timestamp")).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Username" HeaderText="Username" />
                <asp:BoundField DataField="FullName" HeaderText="Full Name" />
                <asp:BoundField DataField="Action" HeaderText="Action" />
                <asp:BoundField DataField="Details" HeaderText="Details" />
            </Columns>
        </asp:GridView>
    </main>

</asp:Content>
