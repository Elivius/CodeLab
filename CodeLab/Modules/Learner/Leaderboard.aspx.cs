using System;
using System.Globalization;
using System.Collections.Generic;
using System.Web.UI;
using CodeLab.BLL;
using CodeLab.Helpers;
using CodeLab.Models;

namespace CodeLab.Modules.Learner
{
    public partial class Leaderboard : Page
    {
        protected bool ByLevel { get { return Request.QueryString["sort"] == "level"; } }
        protected string BadgeFilter { get { string value=Request.QueryString["badges"] ?? "all"; return Array.IndexOf(new[]{"all","unlocked","locked","coding","curriculum","quiz","xp"},value)>=0 ? value : "all"; } }
        protected bool ShowBadge(BadgeProgress badge) { return BadgeFilter=="all" || (BadgeFilter=="unlocked" && badge.Earned) || (BadgeFilter=="locked" && !badge.Earned) || BadgeFilter==badge.Category; }
        protected string FilterUrl(string sort, string badges) { return ResolveUrl("~/Modules/Learner/Leaderboard") + "?sort=" + sort + "&badges=" + badges; }
        protected List<BadgeProgress> VisibleBadges { get; private set; } = new List<BadgeProgress>();
        protected int BadgePage { get; private set; } = 1;
        protected int BadgePages { get; private set; } = 1;
        protected int MatchingBadges { get; private set; }
        protected string BadgePageUrl(int page)
        {
            return FilterUrl(ByLevel ? "level" : "xp", BadgeFilter) + "&page=" + Board.Page + "&badgePage=" + page + "#badges";
        }
        protected LeaderboardData Board { get; private set; }
        protected string ErrorMessage { get; private set; }
        protected string LearnerName { get { return SessionHelper.GetCurrentFullName() ?? "Learner"; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                Response.Redirect(ResolveUrl("~/Modules/Public/Login"), false);
                Context.ApplicationInstance.CompleteRequest(); return;
            }
            if (!SessionHelper.IsUserInRole("Learner"))
            {
                Response.StatusCode=403; ErrorMessage="This page is available to learners."; return;
            }
            int page;
            if (!int.TryParse(Request.QueryString["page"], out page)) page=1;
            var result = new LeaderboardManager().GetPage(page, ByLevel);
            if (result.Success)
            {
                Board=result.Data;
                var matching=Board.Badges.FindAll(ShowBadge);
                MatchingBadges=matching.Count;
                BadgePages=Math.Max(1,(matching.Count+7)/8);
                int requested;
                if (!int.TryParse(Request.QueryString["badgePage"],out requested)) requested=1;
                BadgePage=Math.Max(1,Math.Min(BadgePages,requested));
                int start=(BadgePage-1)*8;
                VisibleBadges=matching.GetRange(start,Math.Min(8,matching.Count-start));
            }
            else ErrorMessage=result.Message;
        }
        protected string Avatar(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (url.StartsWith("~/") && !url.Contains("\\")) return ResolveUrl(url);
            if (url.StartsWith("/") && !url.StartsWith("//") && !url.Contains("\\")) return url;
            Uri parsed;
            return Uri.TryCreate(url, UriKind.Absolute, out parsed) &&
                (parsed.Scheme == "https" || parsed.Scheme == "http") ? url : null;
        }
        protected string Initial(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "?" : StringInfo.GetNextTextElement(name.Trim()).ToUpperInvariant();
        }
        protected string PageUrl(int page)
        {
            return ResolveUrl("~/Modules/Learner/Leaderboard") + "?page=" + page.ToString(CultureInfo.InvariantCulture) + "&sort=" + (ByLevel ? "level" : "xp") + "&badges=" + BadgeFilter + "&badgePage=" + BadgePage + "#rankings";
        }
    }
}
