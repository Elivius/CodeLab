<%@ Page Title="Audit Logs" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="AuditLogs.aspx.cs" Inherits="CodeLab.Modules.Admin.AuditLogs" %>
<%@ Import Namespace="CodeLab.Helpers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="AdminContent" runat="server">

    <section class="cl-card cl-admin-panel">
        <div class="cl-admin-panel-head">
            <div>
                <h2>System &amp; security audit trail</h2>
                <p>Every recorded activity, newest first.</p>
            </div>
            <nav class="cl-admin-filters" aria-label="Filter by category">
                <asp:HyperLink ID="hlCategoryAll" runat="server" Text="All events" />
                <asp:HyperLink ID="hlCategorySecurity" runat="server" Text="Security" />
                <asp:HyperLink ID="hlCategoryAdministrative" runat="server" Text="Administrative" />
                <asp:HyperLink ID="hlCategoryContent" runat="server" Text="Content" />
                <asp:HyperLink ID="hlCategoryDeletion" runat="server" Text="Deletion" />
            </nav>
        </div>

        <%-- ValidateRequestMode="Disabled" lets the admin search for any text. That is safe because the
             search text is only shown encoded and sent to SQL as a parameter. --%>
        <asp:Panel ID="pnlSearch" runat="server" CssClass="cl-admin-search" DefaultButton="btnSearch">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" MaxLength="100" ValidateRequestMode="Disabled"
                placeholder="Search by user, action or details" aria-label="Search audit logs" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn cl-btn-outline" CausesValidation="false" OnClick="btnSearch_Click" />
            <asp:HyperLink ID="hlClearSearch" runat="server" CssClass="btn btn-link" Text="Clear" Visible="false" />
        </asp:Panel>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-danger d-block" Visible="false" />

        <div class="table-responsive">
            <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="false" CssClass="table cl-admin-table" GridLines="None">
                <Columns>
                    <asp:TemplateField HeaderText="Time">
                        <ItemTemplate>
                            <%# ((DateTime)Eval("Timestamp")).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="User">
                        <ItemTemplate>
                            <span class="d-block"><%#: Eval("FullName") %></span>
                            <span class="cl-admin-meta"><%#: Eval("Username") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <span class="cl-admin-action"><%#: Eval("Action") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Category">
                        <ItemTemplate>
                            <span class="cl-admin-badge <%# DisplayHelper.AuditCategoryBadgeCss((string)Eval("Category")) %>"><%#: Eval("Category") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Details" HeaderText="Details" />
                </Columns>
            </asp:GridView>
        </div>

        <div class="cl-admin-pager">
            <span class="cl-admin-meta"><asp:Literal ID="litPageInfo" runat="server" Mode="Encode" /></span>
            <div>
                <asp:HyperLink ID="hlPrevious" runat="server" CssClass="btn cl-btn-outline btn-sm" Text="Previous" />
                <asp:HyperLink ID="hlNext" runat="server" CssClass="btn cl-btn-outline btn-sm" Text="Next" />
            </div>
        </div>
    </section>

</asp:Content>
