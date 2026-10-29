<%@ Page Title="Page unavailable" Language="C#" MasterPageFile="~/Site.Master" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.StatusCode = 404;
        Response.TrySkipIisCustomErrors = true;
        Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
    }
</script>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5"><h1>Page unavailable</h1><p>Public tutor profiles are no longer available.</p></div>
</asp:Content>