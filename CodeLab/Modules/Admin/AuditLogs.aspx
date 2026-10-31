<%@ Page Title="Audit Logs" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="AuditLogs.aspx.cs" Inherits="CodeLab.Modules.Admin.AuditLogs" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="AdminContent" runat="server">

    <section class="cl-card cl-admin-panel">
        <div class="cl-admin-panel-head">
            <div>
                <h2>System &amp; security audit trail</h2>
                <p>The 100 most recent activities, newest first.</p>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

        <div class="table-responsive">
            <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="false" CssClass="table cl-admin-table"
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
        </div>
    </section>

</asp:Content>
