<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CourseGrid.ascx.cs" Inherits="CodeLab.UserControls.CourseGrid" %>

<asp:Repeater ID="rptCourses" runat="server" ItemType="CodeLab.Models.Course" EnableViewState="false">
    <HeaderTemplate>
        <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
    </HeaderTemplate>
    <ItemTemplate>
        <div class="col">
            <article class="cl-card cl-course">
                <div class="cl-course-thumb<%# Item.ThumbnailUrl == null ? " cl-thumb-default" : string.Empty %>">
                    <asp:PlaceHolder runat="server" Visible="<%# Item.ThumbnailUrl == null %>">&lt;/&gt;</asp:PlaceHolder>
                    <asp:Image runat="server" Visible="<%# Item.ThumbnailUrl != null %>" ImageUrl="<%# Item.ThumbnailUrl %>"
                        AlternateText="<%# Item.Title %>" CssClass="cl-course-cover" />
                    <span class="cl-level cl-level-<%#: Item.Difficulty.ToLowerInvariant() %>"><%#: Item.Difficulty %></span>
                </div>
                <div class="cl-course-body">
                    <span class="cl-course-category"><%#: Item.Category %></span>
                    <h3><%#: Item.Title %></h3>
                    <p><%#: Item.Description %></p>
                    <div class="cl-course-footer">
                        <span><i data-lucide="book-open" class="cl-icon me-1" aria-hidden="true"></i><%#: LessonText(Item.LessonCount) %></span>
                        <a href="<%#: ResolveUrl("~/Modules/Public/CourseDetails?id=" + Item.Id) %>">Explore Syllabus <i data-lucide="arrow-right" class="cl-icon ms-1" aria-hidden="true"></i></a>
                    </div>
                </div>
            </article>
        </div>
    </ItemTemplate>
    <FooterTemplate>
        </div>
    </FooterTemplate>
</asp:Repeater>

<asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
    <p class="cl-card p-4 mb-0 text-center">New courses are on the way.</p>
</asp:PlaceHolder>
