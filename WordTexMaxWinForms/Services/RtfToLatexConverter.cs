using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UzunTec.WordTexMax.Services;

public class RtfToLatexConverter
{
    public string ConvertToLatex(RichTextBox richTextBox)
    {
        var sb = new StringBuilder();
        
        // LaTeX Preamble
        sb.AppendLine("\\documentclass{article}");
        sb.AppendLine("\\usepackage[utf8]{inputenc}");
        sb.AppendLine("\\usepackage{listings}");
        sb.AppendLine("\\usepackage{xcolor}");
        sb.AppendLine("\\usepackage{ulem}");
        sb.AppendLine("\\usepackage{geometry}");
        sb.AppendLine("\\geometry{a4paper, margin=1in}");
        sb.AppendLine();
        sb.AppendLine("\\title{Generated Document}");
        sb.AppendLine("\\author{WordLatex User}");
        sb.AppendLine("\\date{\\today}");
        sb.AppendLine();
        sb.AppendLine("\\begin{document}");
        sb.AppendLine("\\maketitle");
        sb.AppendLine();

        // Parse content
        string plainText = richTextBox.Text;
        if (string.IsNullOrEmpty(plainText))
        {
            sb.AppendLine("\\end{document}");
            return sb.ToString();
        }

        // Process character by character with formatting
        int pos = 0;
        while (pos < plainText.Length)
        {
            richTextBox.Select(pos, 1);
            char currentChar = plainText[pos];
            
            Font? font = richTextBox.SelectionFont;
            Color backColor = richTextBox.SelectionBackColor;
            bool isBullet = richTextBox.SelectionBullet;

            if (currentChar == '\n')
            {
                sb.AppendLine();
                sb.AppendLine();
                pos++;
                continue;
            }

            // Detect formatting and wrap accordingly
            string charStr = EscapeLatexSpecialChars(currentChar.ToString());
            
            if (font != null)
            {
                // Check for code block (Courier New + gray background)
                if (font.Name == "Courier New" && backColor.R == 240)
                {
                    // Find end of code block
                    int codeStart = pos;
                    while (pos < plainText.Length && 
                           richTextBox.SelectionFont?.Name == "Courier New" &&
                           richTextBox.SelectionBackColor.R == 240)
                    {
                        pos++;
                        if (pos < plainText.Length)
                            richTextBox.Select(pos, 1);
                    }
                    string codeText = plainText.Substring(codeStart, pos - codeStart);
                    sb.AppendLine("\\begin{lstlisting}");
                    sb.AppendLine(codeText.Trim());
                    sb.AppendLine("\\end{lstlisting}");
                    continue;
                }

                // Heading detection (by font size)
                if (font.Size >= 14)
                {
                    int headingStart = pos;
                    float headingSize = font.Size;
                    while (pos < plainText.Length && 
                           richTextBox.SelectionFont?.Size == headingSize &&
                           plainText[pos] != '\n')
                    {
                        pos++;
                        if (pos < plainText.Length)
                            richTextBox.Select(pos, 1);
                    }
                    string headingText = plainText.Substring(headingStart, pos - headingStart);
                    string command = headingSize >= 18 ? "section" : 
                                   headingSize >= 16 ? "subsection" : "subsubsection";
                    sb.AppendLine($"\\{command}{{{EscapeLatexSpecialChars(headingText)}}}");
                    continue;
                }

                // Apply inline formatting
                bool bold = font.Bold;
                bool italic = font.Italic;
                bool underline = font.Underline;
                bool strikeout = font.Strikeout;

                if (bold) sb.Append("\\textbf{");
                if (italic) sb.Append("\\textit{");
                if (underline) sb.Append("\\underline{");
                if (strikeout) sb.Append("\\sout{");
                
                sb.Append(charStr);
                
                if (strikeout) sb.Append("}");
                if (underline) sb.Append("}");
                if (italic) sb.Append("}");
                if (bold) sb.Append("}");
            }
            else
            {
                sb.Append(charStr);
            }

            pos++;
        }

        sb.AppendLine();
        sb.AppendLine("\\end{document}");
        
        return sb.ToString();
    }

    private string EscapeLatexSpecialChars(string text)
    {
        return text
            .Replace("\\", "\\textbackslash{}")
            .Replace("&", "\\&")
            .Replace("%", "\\%")
            .Replace("$", "\\$")
            .Replace("#", "\\#")
            .Replace("_", "\\_")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("~", "\\textasciitilde{}")
            .Replace("^", "\\textasciicircum{}");
    }
    
    public string ConvertToLatexContent(RichTextBox richTextBox)
    {
        // Same as ConvertToLatex but without preamble and document structure
        var sb = new StringBuilder();
        
        string plainText = richTextBox.Text;
        if (string.IsNullOrEmpty(plainText))
        {
            return "";
        }

        // Process character by character with formatting (same logic as ConvertToLatex)
        int pos = 0;
        while (pos < plainText.Length)
        {
            richTextBox.Select(pos, 1);
            char currentChar = plainText[pos];
            
            Font? font = richTextBox.SelectionFont;
            Color backColor = richTextBox.SelectionBackColor;

            if (currentChar == '\n')
            {
                sb.AppendLine();
                sb.AppendLine();
                pos++;
                continue;
            }

            string charStr = EscapeLatexSpecialChars(currentChar.ToString());
            
            if (font != null)
            {
                // Code block
                if (font.Name == "Courier New" && backColor.R == 240)
                {
                    int codeStart = pos;
                    while (pos < plainText.Length && 
                           richTextBox.SelectionFont?.Name == "Courier New" &&
                           richTextBox.SelectionBackColor.R == 240)
                    {
                        pos++;
                        if (pos < plainText.Length)
                            richTextBox.Select(pos, 1);
                    }
                    string codeText = plainText.Substring(codeStart, pos - codeStart);
                    sb.AppendLine("\\begin{lstlisting}");
                    sb.AppendLine(codeText.Trim());
                    sb.AppendLine("\\end{lstlisting}");
                    continue;
                }

                // Headings
                if (font.Size >= 14)
                {
                    int headingStart = pos;
                    float headingSize = font.Size;
                    while (pos < plainText.Length && 
                           richTextBox.SelectionFont?.Size == headingSize &&
                           plainText[pos] != '\n')
                    {
                        pos++;
                        if (pos < plainText.Length)
                            richTextBox.Select(pos, 1);
                    }
                    string headingText = plainText.Substring(headingStart, pos - headingStart);
                    string command = headingSize >= 18 ? "section" : 
                                   headingSize >= 16 ? "subsection" : "subsubsection";
                    sb.AppendLine($"\\{command}{{{EscapeLatexSpecialChars(headingText)}}}");
                    continue;
                }

                // Inline formatting
                bool bold = font.Bold;
                bool italic = font.Italic;
                bool underline = font.Underline;
                bool strikeout = font.Strikeout;

                if (bold) sb.Append("\\textbf{");
                if (italic) sb.Append("\\textit{");
                if (underline) sb.Append("\\underline{");
                if (strikeout) sb.Append("\\sout{");
                
                sb.Append(charStr);
                
                if (strikeout) sb.Append("}");
                if (underline) sb.Append("}");
                if (italic) sb.Append("}");
                if (bold) sb.Append("}");
            }
            else
            {
                sb.Append(charStr);
            }

            pos++;
        }
        
        return sb.ToString();
    }
}
