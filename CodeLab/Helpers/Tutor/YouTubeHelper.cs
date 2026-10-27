using System;
using System.Text.RegularExpressions;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Works with YouTube links for lesson videos. Tutors paste a normal YouTube link; the lesson page
    /// shows it with EmbedHtml, which plays the video inside CodeLab. Nothing is uploaded.
    /// </summary>
    public static class YouTubeHelper
    {
        // A YouTube video id is always 11 letters, digits, '-' or '_'
        private static readonly Regex VideoId = new Regex(@"^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled);

        /// <summary>
        /// Reads the video id from the usual YouTube link formats:
        /// youtube.com/watch?v=ID, youtu.be/ID, youtube.com/embed/ID, youtube.com/shorts/ID, youtube.com/live/ID.
        /// Returns false for anything else, including links to other websites.
        /// </summary>
        public static bool TryGetVideoId(string url, out string videoId)
        {
            videoId = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            string text = url.Trim();
            if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                text = "https://" + text;
            }

            Uri uri;
            if (!Uri.TryCreate(text, UriKind.Absolute, out uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            string host = uri.Host.ToLowerInvariant();
            string[] segments = uri.AbsolutePath.Trim('/').Split('/');
            string candidate = null;

            if (host == "youtu.be")
            {
                candidate = segments[0];
            }
            else if (host == "youtube.com" || host == "www.youtube.com" || host == "m.youtube.com"
                || host == "youtube-nocookie.com" || host == "www.youtube-nocookie.com")
            {
                if (segments[0] == "watch")
                {
                    candidate = GetQueryValue(uri.Query, "v");
                }
                else if (segments.Length >= 2 && (segments[0] == "embed" || segments[0] == "shorts" || segments[0] == "live"))
                {
                    candidate = segments[1];
                }
            }

            if (candidate == null || !VideoId.IsMatch(candidate))
            {
                return false;
            }

            videoId = candidate;
            return true;
        }

        /// <summary>
        /// The standard link for a video id, which is what gets stored in Lessons.VideoUrl.
        /// </summary>
        public static string ToWatchUrl(string videoId)
        {
            return "https://www.youtube.com/watch?v=" + videoId;
        }

        /// <summary>
        /// HTML that plays the video inside the page (16:9 player), or an empty string if the link is
        /// not a YouTube video. Uses youtube-nocookie.com, YouTube's privacy-friendly player.
        /// </summary>
        public static string EmbedHtml(string url, string title)
        {
            string videoId;
            if (!TryGetVideoId(url, out videoId))
            {
                return string.Empty;
            }

            string safeTitle = System.Web.HttpUtility.HtmlAttributeEncode(string.IsNullOrWhiteSpace(title) ? "Lesson video" : title);
            return "<div class=\"cl-video\"><iframe src=\"https://www.youtube-nocookie.com/embed/" + videoId + "\""
                + " title=\"" + safeTitle + "\" loading=\"lazy\""
                + " allow=\"accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture\""
                + " referrerpolicy=\"strict-origin-when-cross-origin\" allowfullscreen></iframe></div>";
        }

        private static string GetQueryValue(string query, string name)
        {
            foreach (string pair in query.TrimStart('?').Split('&'))
            {
                int equals = pair.IndexOf('=');
                if (equals > 0 && pair.Substring(0, equals) == name)
                {
                    return Uri.UnescapeDataString(pair.Substring(equals + 1));
                }
            }

            return null;
        }
    }
}
