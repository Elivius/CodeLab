<%@ Page Title="Users" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="CodeLab.Modules.Admin.Users" %>
<%@ Import Namespace="CodeLab.Helpers" %>

<asp:Content ID="UsersContent" ContentPlaceHolderID="AdminContent" runat="server">

    <section class="cl-card cl-admin-panel">
        <div class="cl-admin-panel-head">
            <div>
                <h2>User directory</h2>
                <p>Create accounts, update details and roles, and suspend anyone who breaks the rules.</p>
            </div>
            <asp:HyperLink runat="server" NavigateUrl="~/Modules/Admin/UserEdit" CssClass="btn btn-primary">
                <i data-lucide="plus" class="cl-icon" aria-hidden="true"></i> Create user
            </asp:HyperLink>
        </div>

        <%-- ValidateRequestMode="Disabled" lets the admin search for any text. That is safe because the
             search text is only shown encoded and sent to SQL as a parameter. --%>
        <asp:Panel ID="pnlSearch" runat="server" CssClass="cl-admin-search" DefaultButton="btnSearch">
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" MaxLength="100" ValidateRequestMode="Disabled"
                placeholder="Search by name, username or email" aria-label="Search users" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn cl-btn-outline" CausesValidation="false" OnClick="btnSearch_Click" />
            <asp:HyperLink ID="hlClearSearch" runat="server" CssClass="btn btn-link" Text="Clear" Visible="false" />
        </asp:Panel>

        <div class="cl-admin-filter-row">
            <nav class="cl-admin-filters" aria-label="Filter by role">
                <asp:HyperLink ID="hlRoleAll" runat="server" />
                <asp:HyperLink ID="hlRoleLearner" runat="server" />
                <asp:HyperLink ID="hlRoleTutor" runat="server" />
                <asp:HyperLink ID="hlRoleAdmin" runat="server" />
            </nav>
            <nav class="cl-admin-filters" aria-label="Filter by status">
                <asp:HyperLink ID="hlStatusAll" runat="server" />
                <asp:HyperLink ID="hlStatusActive" runat="server" />
                <asp:HyperLink ID="hlStatusSuspended" runat="server" />
            </nav>
        </div>

        <asp:Label ID="lblMessage" runat="server" Visible="false" />

        <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
            <HeaderTemplate>
                <div class="table-responsive">
                    <table class="table cl-admin-table">
                        <thead>
                            <tr>
                                <th scope="col">User</th>
                                <th scope="col">Role</th>
                                <th scope="col">Status</th>
                                <th scope="col">Joined</th>
                                <th scope="col">Last login</th>
                                <th scope="col" class="text-end">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <strong><%#: Eval("FullName") %></strong>
                        <asp:PlaceHolder runat="server" Visible='<%# IsSelf((int)Eval("Id")) %>'>
                            <span class="cl-admin-badge cl-admin-badge-neutral">You</span>
                        </asp:PlaceHolder>
                        <span class="cl-admin-meta cl-admin-meta-row"><span>@<%#: Eval("Username") %></span><span><%#: Eval("Email") %></span></span>
                    </td>
                    <td><span class="cl-admin-badge <%# DisplayHelper.RoleBadgeCss((string)Eval("Role")) %>"><%#: Eval("Role") %></span></td>
                    <td><span class="cl-admin-badge cl-admin-status <%# DisplayHelper.UserStatusBadgeCss((string)Eval("Status")) %>"><%#: Eval("Status") %></span></td>
                    <td><%#: TutorDisplayHelper.LocalDate((DateTime)Eval("JoinedDate")) %></td>
                    <td><%#: DisplayHelper.LastLogin((DateTime?)Eval("LastLogin")) %></td>
                    <td>
                        <div class="cl-admin-row-actions">
                            <asp:HyperLink runat="server" CssClass="btn cl-btn-outline btn-sm"
                                NavigateUrl='<%# "~/Modules/Admin/UserEdit?id=" + Eval("Id") %>'>
                                <i data-lucide="pencil" class="cl-icon" aria-hidden="true"></i> Edit
                            </asp:HyperLink>
                            <asp:LinkButton runat="server" CssClass="btn btn-outline-warning btn-sm" CausesValidation="false" ToolTip="Suspend" aria-label="Suspend"
                                CommandName="Suspend" CommandArgument='<%# Eval("Id") %>' Visible='<%# (bool)Eval("CanSuspend") && !IsSelf((int)Eval("Id")) %>'
                                OnClientClick="return confirm('Suspend this account? The user is logged out on their next click and cannot log in.');">
                                <i data-lucide="user-x" class="cl-icon" aria-hidden="true"></i>
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" CssClass="btn btn-outline-success btn-sm" CausesValidation="false" ToolTip="Reactivate" aria-label="Reactivate"
                                CommandName="Reactivate" CommandArgument='<%# Eval("Id") %>' Visible='<%# (bool)Eval("CanReactivate") && !IsSelf((int)Eval("Id")) %>'
                                OnClientClick="return confirm('Reactivate this account? The user can log in again.');">
                                <i data-lucide="user-check" class="cl-icon" aria-hidden="true"></i>
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" CssClass="btn btn-outline-danger btn-sm" CausesValidation="false" ToolTip="Delete permanently" aria-label="Delete"
                                CommandName="Delete" CommandArgument='<%# Eval("Id") %>' Visible='<%# !(bool)Eval("HasActivity") && !IsSelf((int)Eval("Id")) %>'
                                OnClientClick="return confirm('Delete this account permanently?');">
                                <i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i>
                            </asp:LinkButton>
                            <%-- A disabled button shows no tooltip of its own, so the outer span carries it --%>
                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("HasActivity") && !IsSelf((int)Eval("Id")) %>'>
                                <span class="d-inline-block cl-admin-disabled-action" title="This account has activity, so it cannot be deleted. Suspend it instead.">
                                    <span class="btn btn-outline-danger btn-sm disabled" aria-disabled="true"><i data-lucide="trash-2" class="cl-icon" aria-hidden="true"></i></span>
                                </span>
                            </asp:PlaceHolder>
                        </div>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                        </tbody>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
            <div class="cl-admin-empty">
                <i data-lucide="inbox" class="cl-icon" aria-hidden="true"></i>
                <p><asp:Literal ID="litEmpty" runat="server" Mode="Encode" /></p>
            </div>
        </asp:PlaceHolder>
    </section>

</asp:Content>
