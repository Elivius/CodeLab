<%@ Page Title="Database Backups" Language="C#" MasterPageFile="~/Modules/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Backups.aspx.cs" Inherits="CodeLab.Modules.Admin.Backups" %>
<%@ Import Namespace="CodeLab.Helpers" %>

<asp:Content ID="BackupsContent" ContentPlaceHolderID="AdminContent" runat="server">

    <section class="cl-admin-backup-hero">
        <div>
            <h2><i data-lucide="hard-drive" class="cl-icon" aria-hidden="true"></i> Back up the database</h2>
            <p>Saves a full copy of the database as a .bak file, with a SHA-256 checksum so you can confirm a download is intact.</p>
        </div>
        <asp:Button ID="btnBackup" runat="server" Text="Run backup now" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnBackup_Click" />
    </section>

    <asp:Label ID="lblMessage" runat="server" Visible="false" />

    <section class="cl-card cl-admin-panel">
        <div class="cl-admin-panel-head">
            <div>
                <h2>Stored backups</h2>
                <p>Newest first. A backup holds every account's data, including password hashes, so keep downloads private.</p>
            </div>
        </div>

        <asp:Repeater ID="rptBackups" runat="server" OnItemCommand="rptBackups_ItemCommand">
            <HeaderTemplate>
                <div class="table-responsive">
                    <table class="table cl-admin-table">
                        <thead>
                            <tr>
                                <th scope="col">Backup</th>
                                <th scope="col">Size</th>
                                <th scope="col">Created</th>
                                <th scope="col">SHA-256</th>
                                <th scope="col" class="text-end">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><strong><%#: Eval("Name") %></strong></td>
                    <td class="text-nowrap"><%#: Eval("SizeText") %></td>
                    <td class="text-nowrap"><%#: TutorDisplayHelper.LocalDateTime((DateTime)Eval("CreatedAt")) %></td>
                    <td><span class="cl-admin-checksum"><%#: (string)Eval("Sha256") ?? "Not recorded" %></span></td>
                    <td>
                        <div class="cl-admin-row-actions">
                            <asp:LinkButton runat="server" CssClass="btn cl-btn-outline btn-sm" CausesValidation="false"
                                CommandName="Download" CommandArgument='<%# Eval("Name") %>'>
                                <i data-lucide="download" class="cl-icon" aria-hidden="true"></i> Download
                            </asp:LinkButton>
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
                <p>There are no backups yet. Run one to create the first.</p>
            </div>
        </asp:PlaceHolder>
    </section>

</asp:Content>
