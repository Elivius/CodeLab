<%@ Page Title="Tutor Applications" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="TutorApplications.aspx.cs" Inherits="CodeLab.Modules.Admin.TutorApplications" %>
<%@ Import Namespace="CodeLab.Helpers" %>

<asp:Content ID="ApplicationsContent" ContentPlaceHolderID="AdminContent" runat="server">

    <section class="cl-card cl-admin-panel">
        <div class="cl-admin-panel-head">
            <div>
                <h2>Tutor applications</h2>
                <p>Approve an applicant to unlock course authoring, or reject with a reason they can act on.</p>
            </div>
            <nav class="cl-admin-filters" aria-label="Filter by status">
                <asp:HyperLink ID="hlFilterPending" runat="server" NavigateUrl="~/Modules/Admin/TutorApplications?status=pending" />
                <asp:HyperLink ID="hlFilterApproved" runat="server" NavigateUrl="~/Modules/Admin/TutorApplications?status=approved" />
                <asp:HyperLink ID="hlFilterRejected" runat="server" NavigateUrl="~/Modules/Admin/TutorApplications?status=rejected" />
                <asp:HyperLink ID="hlFilterAll" runat="server" NavigateUrl="~/Modules/Admin/TutorApplications?status=all" />
            </nav>
        </div>

        <asp:Label ID="lblMessage" runat="server" Visible="false" />

        <asp:Repeater ID="rptApplications" runat="server" OnItemCommand="rptApplications_ItemCommand">
            <HeaderTemplate><ul class="cl-admin-apps"></HeaderTemplate>
            <ItemTemplate>
                <li class="cl-admin-app">
                    <div class="cl-admin-app-main">
                        <div class="cl-admin-app-title">
                            <h3><%#: Eval("FullName") %></h3>
                            <span class="cl-admin-badge <%# BadgeCss((string)Eval("Status")) %>"><%#: Eval("Status") %></span>
                            <span class="cl-admin-meta" title="<%#: FullDate((DateTime)Eval("AppliedAt")) %>">
                                Applied <%#: DisplayHelper.TimeAgo((DateTime)Eval("AppliedAt")) %>
                            </span>
                        </div>
                        <p><strong>Specialty:</strong> <%#: Eval("Specialty") %></p>
                        <p><%#: DisplayHelper.Excerpt((string)Eval("Bio"), 220) %></p>
                        <asp:PlaceHolder runat="server" Visible='<%# Eval("RejectionReason") != null %>'>
                            <p class="cl-admin-reason"><strong>Reason given:</strong> <%#: Eval("RejectionReason") %></p>
                        </asp:PlaceHolder>
                    </div>
                    <div class="cl-admin-app-actions">
                        <asp:HyperLink runat="server" CssClass="btn btn-primary btn-sm"
                            NavigateUrl='<%# "~/Modules/Admin/TutorApplication?userId=" + Eval("UserId") %>'>
                            <i data-lucide="eye" class="cl-icon" aria-hidden="true"></i> Review application
                        </asp:HyperLink>
                        <asp:LinkButton runat="server" CssClass="btn btn-success btn-sm" CausesValidation="false"
                            CommandName="Approve" CommandArgument='<%# Eval("UserId") %>' Visible='<%# IsPending((string)Eval("Status")) %>'
                            OnClientClick="return confirm('Approve this tutor? They can start creating courses straight away.');">
                            <i data-lucide="check" class="cl-icon" aria-hidden="true"></i> Approve
                        </asp:LinkButton>
                        <asp:HyperLink runat="server" CssClass="btn btn-outline-danger btn-sm" Visible='<%# IsPending((string)Eval("Status")) %>'
                            NavigateUrl='<%# "~/Modules/Admin/TutorApplication?userId=" + Eval("UserId") + "#decision" %>'>
                            <i data-lucide="x" class="cl-icon" aria-hidden="true"></i> Reject
                        </asp:HyperLink>
                    </div>
                </li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
            <div class="cl-admin-empty">
                <i data-lucide="inbox" class="cl-icon" aria-hidden="true"></i>
                <p><asp:Literal ID="litEmpty" runat="server" Mode="Encode" /></p>
            </div>
        </asp:PlaceHolder>
    </section>

</asp:Content>
