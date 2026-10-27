using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CodeLab.Helpers
{
    /// <summary>
    /// Turns the Markdown a tutor writes into safe HTML. All text is HTML-encoded first, so no tag,
    /// script or attribute a tutor types can ever run; only the formatting listed below is added.
    ///
    /// Supported: # ## ### headings, **bold**, *italic* or _italic_, `inline code`, ```fenced code```,
    /// - bullet lists, 1. numbered lists, &gt; quotes, --- rules and [links](https://example.com).
    /// Use this anywhere a lesson's content is shown (tutor preview, learner lesson page).
    /// </summary>
    public static class MarkdownHelper
    {
        private static readonly Regex Heading = new Regex(@"^(#{1,3})\s+(.+)$", RegexOptions.Compiled);
        private static readonly Regex Rule = new Regex(@"^(-{3,}|\*{3,})$", RegexOptions.Compiled);
        private static readonly Regex Bullet = new Regex(@"^[-*]\s+(.+)$", RegexOptions.Compiled);
        private static readonly Regex Numbered = new Regex(@"^\d+[.)]\s+(.+)$", RegexOptions.Compiled);
        private static readonly Regex Quote = new Regex(@"^>\s?(.*)$", RegexOptions.Compiled);
        private static readonly Regex CodeSpan = new Regex(@"`([^`\n]+)`", RegexOptions.Compiled);
        private static readonly Regex Bold = new Regex(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
        private static readonly Regex StarItalic = new Regex(@"(?<!\*)\*(?!\*|\s)(.+?)(?<!\s|\*)\*(?!\*)", RegexOptions.Compiled);
        private static readonly Regex UnderscoreItalic = new Regex(@"(?<!\w)_(?!\s)(.+?)(?<!\s)_(?!\w)", RegexOptions.Compiled);
        private static readonly Regex Link = new Regex(@"\[([^\]\n]+)\]\((https?://[^\s)]+)\)", RegexOptions.Compiled);
        private static readonly Regex LanguageName = new Regex(@"^[a-z0-9+#-]{1,20}$", RegexOptions.Compiled);

        /// <summary>
        /// Returns safe HTML for the Markdown text, or an empty string if there is none.
        /// </summary>
        public static string ToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            string[] lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            StringBuilder html = new StringBuilder();
            List<string> paragraph = new List<string>();
            List<string> quote = new List<string>();
            string listTag = null;
            bool inCode = false;

            foreach (string rawLine in lines)
            {
                if (inCode)
                {
                    if (rawLine.TrimEnd().StartsWith("```", StringComparison.Ordinal))
                    {
                        html.Append("</code></pre>");
                        inCode = false;
                    }
                    else
                    {
                        html.Append(HttpUtility.HtmlEncode(rawLine)).Append('\n');
                    }

                    continue;
                }

                string line = rawLine.TrimEnd();
                string trimmed = line.TrimStart();

                if (trimmed.StartsWith("```", StringComparison.Ordinal))
                {
                    FlushAll(html, paragraph, quote, ref listTag);
                    string language = trimmed.Substring(3).Trim().ToLowerInvariant();
                    html.Append("<pre class=\"cl-code\"><code");
                    if (LanguageName.IsMatch(language))
                    {
                        html.Append(" class=\"language-").Append(language).Append('"');
                    }

                    html.Append('>');
                    inCode = true;
                    continue;
                }

                if (trimmed.Length == 0)
                {
                    FlushAll(html, paragraph, quote, ref listTag);
                    continue;
                }

                Match match;
                if ((match = Heading.Match(trimmed)).Success)
                {
                    FlushAll(html, paragraph, quote, ref listTag);
                    // Page titles use h1 and h2, so lesson headings start at h3
                    int level = match.Groups[1].Length + 2;
                    html.Append("<h").Append(level).Append('>').Append(Inline(match.Groups[2].Value)).Append("</h").Append(level).Append('>');
                }
                else if (Rule.IsMatch(trimmed))
                {
                    FlushAll(html, paragraph, quote, ref listTag);
                    html.Append("<hr />");
                }
                else if ((match = Bullet.Match(trimmed)).Success)
                {
                    AddListItem(html, paragraph, quote, ref listTag, "ul", match.Groups[1].Value);
                }
                else if ((match = Numbered.Match(trimmed)).Success)
                {
                    AddListItem(html, paragraph, quote, ref listTag, "ol", match.Groups[1].Value);
                }
                else if ((match = Quote.Match(trimmed)).Success)
                {
                    FlushParagraph(html, paragraph);
                    FlushList(html, ref listTag);
                    quote.Add(match.Groups[1].Value);
                }
                else
                {
                    FlushQuote(html, quote);
                    FlushList(html, ref listTag);
                    paragraph.Add(trimmed);
                }
            }

            if (inCode)
            {
                html.Append("</code></pre>");
            }

            FlushAll(html, paragraph, quote, ref listTag);
            return html.ToString();
        }

        /// <summary>
        /// Returns safe HTML for a code snippet shown in a highlighted block.
        /// </summary>
        public static string CodeBlockToHtml(string code, string language)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return string.Empty;
            }

            string languageClass = !string.IsNullOrEmpty(language) && LanguageName.IsMatch(language)
                ? " class=\"language-" + language + "\""
                : string.Empty;

            return "<pre class=\"cl-code\"><code" + languageClass + ">" + HttpUtility.HtmlEncode(code.Replace("\r\n", "\n")) + "</code></pre>";
        }

        private static void AddListItem(StringBuilder html, List<string> paragraph, List<string> quote,
            ref string listTag, string tag, string text)
        {
            FlushParagraph(html, paragraph);
            FlushQuote(html, quote);

            if (listTag != tag)
            {
                FlushList(html, ref listTag);
                html.Append('<').Append(tag).Append('>');
                listTag = tag;
            }

            html.Append("<li>").Append(Inline(text)).Append("</li>");
        }

        private static void FlushAll(StringBuilder html, List<string> paragraph, List<string> quote, ref string listTag)
        {
            FlushParagraph(html, paragraph);
            FlushQuote(html, quote);
            FlushList(html, ref listTag);
        }

        private static void FlushParagraph(StringBuilder html, List<string> paragraph)
        {
            if (paragraph.Count == 0)
            {
                return;
            }

            // Line breaks the tutor typed are kept
            html.Append("<p>").Append(string.Join("<br />", paragraph.ConvertAll(Inline))).Append("</p>");
            paragraph.Clear();
        }

        private static void FlushQuote(StringBuilder html, List<string> quote)
        {
            if (quote.Count == 0)
            {
                return;
            }

            html.Append("<blockquote>").Append(string.Join("<br />", quote.ConvertAll(Inline))).Append("</blockquote>");
            quote.Clear();
        }

        private static void FlushList(StringBuilder html, ref string listTag)
        {
            if (listTag != null)
            {
                html.Append("</").Append(listTag).Append('>');
                listTag = null;
            }
        }

        // Formats one line of text: encodes everything first, then adds the supported formatting
        private static string Inline(string text)
        {
            string encoded = HttpUtility.HtmlEncode(text.Replace("\u0001", string.Empty));

            // Code spans are set aside so their contents are not formatted
            List<string> codeSpans = new List<string>();
            encoded = CodeSpan.Replace(encoded, m =>
            {
                codeSpans.Add("<code>" + m.Groups[1].Value + "</code>");
                return "\u0001" + (codeSpans.Count - 1) + "\u0001";
            });

            // Links go first so the address inside is not touched by the italic rules
            List<string> links = new List<string>();
            encoded = Link.Replace(encoded, m =>
            {
                links.Add("<a href=\"" + m.Groups[2].Value + "\" target=\"_blank\" rel=\"noopener noreferrer\">" + m.Groups[1].Value + "</a>");
                return "\u0002" + (links.Count - 1) + "\u0002";
            });

            encoded = Bold.Replace(encoded, "<strong>$1</strong>");
            encoded = StarItalic.Replace(encoded, "<em>$1</em>");
            encoded = UnderscoreItalic.Replace(encoded, "<em>$1</em>");

            encoded = Regex.Replace(encoded, "\u0002(\\d+)\u0002", m => links[int.Parse(m.Groups[1].Value)]);
            encoded = Regex.Replace(encoded, "\u0001(\\d+)\u0001", m => codeSpans[int.Parse(m.Groups[1].Value)]);
            return encoded;
        }
    }
}
