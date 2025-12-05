using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UzunTec.WordTexMax.Services;

public class LatexImporter
{
    public void ImportLatex(string filePath, RichTextBox richTextBox)
    {
        string latexContent = File.ReadAllText(filePath);

        // Extract document body (between \begin{document} and \end{document})
        var bodyMatch = Regex.Match(latexContent, @"\\begin\{document\}(.*?)\\end\{document\}",
            RegexOptions.Singleline);

        string body = bodyMatch.Success ? bodyMatch.Groups[1].Value : latexContent;

        // Convert LaTeX to HTML
        string html = ConvertLatexToHtml(body);

        // Load HTML into RichTextBox
        richTextBox.Clear();

        // RichTextBox can't load HTML directly, so we need to use a workaround
        // We'll parse the HTML and apply formatting manually
        ParseHtmlToRichTextBox(html, richTextBox);
    }

    private string ConvertLatexToHtml(string latex)
    {
        var html = new StringBuilder();
        html.AppendLine("<html><body>");

        // Remove comments
        latex = Regex.Replace(latex, @"%.*$", "", RegexOptions.Multiline);

        // Process line by line for block elements
        var lines = latex.Split('\n');
        bool inItemize = false;
        bool inEnumerate = false;
        bool inListing = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            // Code listings
            if (trimmed.StartsWith("\\begin{lstlisting}") || trimmed.StartsWith("\\begin{verbatim}"))
            {
                html.AppendLine("<pre style='font-family:monospace;background-color:#f0f0f0;'>");
                inListing = true;
                continue;
            }
            if (trimmed.StartsWith("\\end{lstlisting}") || trimmed.StartsWith("\\end{verbatim}"))
            {
                html.AppendLine("</pre>");
                inListing = false;
                continue;
            }

            if (inListing)
            {
                html.AppendLine(System.Web.HttpUtility.HtmlEncode(line));
                continue;
            }

            // Lists
            if (trimmed.StartsWith("\\begin{itemize}"))
            {
                html.AppendLine("<ul>");
                inItemize = true;
                continue;
            }
            if (trimmed.StartsWith("\\end{itemize}"))
            {
                html.AppendLine("</ul>");
                inItemize = false;
                continue;
            }
            if (trimmed.StartsWith("\\begin{enumerate}"))
            {
                html.AppendLine("<ol>");
                inEnumerate = true;
                continue;
            }
            if (trimmed.StartsWith("\\end{enumerate}"))
            {
                html.AppendLine("</ol>");
                inEnumerate = false;
                continue;
            }

            // List items
            if (trimmed.StartsWith("\\item"))
            {
                var itemText = trimmed.Substring(5).Trim();
                itemText = ProcessInlineLatex(itemText);
                html.AppendLine($"<li>{itemText}</li>");
                continue;
            }

            // Sections
            if (trimmed.StartsWith("\\section{"))
            {
                var title = ExtractBraceContent(trimmed, "\\section");
                html.AppendLine($"<h1>{ProcessInlineLatex(title)}</h1>");
                continue;
            }
            if (trimmed.StartsWith("\\subsection{"))
            {
                var title = ExtractBraceContent(trimmed, "\\subsection");
                html.AppendLine($"<h2>{ProcessInlineLatex(title)}</h2>");
                continue;
            }
            if (trimmed.StartsWith("\\subsubsection{"))
            {
                var title = ExtractBraceContent(trimmed, "\\subsubsection");
                html.AppendLine($"<h3>{ProcessInlineLatex(title)}</h3>");
                continue;
            }

            // Skip common preamble commands
            if (trimmed.StartsWith("\\") && (trimmed.Contains("title") || trimmed.Contains("author") ||
                trimmed.Contains("date") || trimmed.Contains("maketitle")))
            {
                continue;
            }

            // Regular paragraph
            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("\\"))
            {
                html.AppendLine($"<p>{ProcessInlineLatex(trimmed)}</p>");
            }
        }

        if (inItemize) html.AppendLine("</ul>");
        if (inEnumerate) html.AppendLine("</ol>");

        html.AppendLine("</body></html>");
        return html.ToString();
    }

    private string ProcessInlineLatex(string text)
    {
        // Bold
        text = Regex.Replace(text, @"\\textbf\{([^}]+)\}", "<b>$1</b>");

        // Italic
        text = Regex.Replace(text, @"\\textit\{([^}]+)\}", "<i>$1</i>");
        text = Regex.Replace(text, @"\\emph\{([^}]+)\}", "<i>$1</i>");

        // Underline
        text = Regex.Replace(text, @"\\underline\{([^}]+)\}", "<u>$1</u>");

        // Strikeout
        text = Regex.Replace(text, @"\\sout\{([^}]+)\}", "<s>$1</s>");

        // Escape special characters
        text = System.Web.HttpUtility.HtmlEncode(text);

        // Restore HTML tags we just added
        text = text.Replace("&lt;b&gt;", "<b>").Replace("&lt;/b&gt;", "</b>");
        text = text.Replace("&lt;i&gt;", "<i>").Replace("&lt;/i&gt;", "</i>");
        text = text.Replace("&lt;u&gt;", "<u>").Replace("&lt;/u&gt;", "</u>");
        text = text.Replace("&lt;s&gt;", "<s>").Replace("&lt;/s&gt;", "</s>");

        return text;
    }

    private string ExtractBraceContent(string text, string command)
    {
        var match = Regex.Match(text, command + @"\{([^}]+)\}");
        return match.Success ? match.Groups[1].Value : "";
    }

    private void ParseHtmlToRichTextBox(string html, RichTextBox richTextBox)
    {
        // Simple HTML parser for RichTextBox
        // Remove HTML tags and apply formatting

        var doc = new System.Xml.XmlDocument();
        try
        {
            doc.LoadXml(html);
            ProcessHtmlNode(doc.DocumentElement, richTextBox);
        }
        catch
        {
            // If XML parsing fails, try simple regex approach
            ParseHtmlSimple(html, richTextBox);
        }
    }

    private void ProcessHtmlNode(System.Xml.XmlNode? node, RichTextBox richTextBox)
    {
        if (node == null) return;

        foreach (System.Xml.XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == System.Xml.XmlNodeType.Text)
            {
                richTextBox.AppendText(child.Value ?? "");
            }
            else if (child.NodeType == System.Xml.XmlNodeType.Element)
            {
                int start = richTextBox.TextLength;

                switch (child.Name.ToLower())
                {
                    case "h1":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start,
                            fontSize: 18, bold: true);
                        richTextBox.AppendText(Environment.NewLine);
                        break;

                    case "h2":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start,
                            fontSize: 16, bold: true);
                        richTextBox.AppendText(Environment.NewLine);
                        break;

                    case "h3":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start,
                            fontSize: 14, bold: true);
                        richTextBox.AppendText(Environment.NewLine);
                        break;

                    case "b":
                    case "strong":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start, bold: true);
                        break;

                    case "i":
                    case "em":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start, italic: true);
                        break;

                    case "u":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start, underline: true);
                        break;

                    case "s":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start, strikeout: true);
                        break;

                    case "pre":
                        ProcessHtmlNode(child, richTextBox);
                        ApplyFormatting(richTextBox, start, richTextBox.TextLength - start,
                            fontFamily: "Courier New", fontSize: 10);
                        richTextBox.SelectionBackColor = Color.FromArgb(240, 240, 240);
                        richTextBox.AppendText(Environment.NewLine);
                        break;

                    case "p":
                        ProcessHtmlNode(child, richTextBox);
                        richTextBox.AppendText(Environment.NewLine);
                        break;

                    case "li":
                        richTextBox.AppendText("• ");
                        ProcessHtmlNode(child, richTextBox);
                        richTextBox.AppendText(Environment.NewLine);
                        break;

                    default:
                        ProcessHtmlNode(child, richTextBox);
                        break;
                }
            }
        }
    }

    private void ParseHtmlSimple(string html, RichTextBox richTextBox)
    {
        // Fallback: simple text extraction
        var text = Regex.Replace(html, "<[^>]+>", "");
        text = System.Web.HttpUtility.HtmlDecode(text);
        richTextBox.Text = text;
    }

    private void ApplyFormatting(RichTextBox rtb, int start, int length,
        bool? bold = null, bool? italic = null, bool? underline = null, bool? strikeout = null,
        float? fontSize = null, string? fontFamily = null)
    {
        if (length <= 0) return;

        rtb.Select(start, length);
        var currentFont = rtb.SelectionFont ?? rtb.Font;

        FontStyle style = currentFont.Style;
        if (bold == true) style |= FontStyle.Bold;
        if (italic == true) style |= FontStyle.Italic;
        if (underline == true) style |= FontStyle.Underline;
        if (strikeout == true) style |= FontStyle.Strikeout;

        float size = fontSize ?? currentFont.Size;
        string family = fontFamily ?? currentFont.FontFamily.Name;

        rtb.SelectionFont = new Font(family, size, style);
    }
}
