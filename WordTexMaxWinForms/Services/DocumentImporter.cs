using System.Text;
using Xceed.Words.NET;
using NPOI.HWPF;
using NPOI.HWPF.UserModel;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UzunTec.WordTexMax.Models;

namespace UzunTec.WordTexMax.Models;

public class DocumentImporter
{
    private readonly LatexImporter latexImporter;
    public DocumentImporter()
    {
        latexImporter = new LatexImporter();
    }
    
    public void ImportDocument(string filePath, RichTextBox richTextBox)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        
        try
        {
            switch (extension)
            {
                case ".rtf":
                    ImportRtf(filePath, richTextBox);
                    break;
                case ".docx":
                    ImportDocx(filePath, richTextBox);
                    break;
                case ".doc":
                    ImportDoc(filePath, richTextBox);
                    break;
                case ".odt":
                    ImportOdt(filePath, richTextBox);
                    break;
                case ".pdf":
                    ImportPdf(filePath, richTextBox);
                    break;
                case ".tex":
                    ImportLatex(filePath, richTextBox);
                    break;
                default:
                    MessageBox.Show($"Unsupported file format: {extension}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error importing file: {ex.Message}", "Import Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportRtf(string filePath, RichTextBox richTextBox)
    {
        richTextBox.LoadFile(filePath, RichTextBoxStreamType.RichText);
    }

    private void ImportDocx(string filePath, RichTextBox richTextBox)
    {
        using var doc = DocX.Load(filePath);
        richTextBox.Clear();
        
        foreach (var paragraph in doc.Paragraphs)
        {
            int startIndex = richTextBox.TextLength;
            richTextBox.AppendText(paragraph.Text + Environment.NewLine);
            
            // Apply formatting from first run (simplified)
            if (paragraph.MagicText.Count > 0)
            {
                var formatting = paragraph.MagicText[0].formatting;
                richTextBox.Select(startIndex, paragraph.Text.Length);
                
                // Apply bold
                if (formatting.Bold.HasValue && formatting.Bold.Value)
                {
                    richTextBox.SelectionFont = new Font(richTextBox.SelectionFont ?? richTextBox.Font, 
                        richTextBox.SelectionFont?.Style | FontStyle.Bold ?? FontStyle.Bold);
                }
                
                // Apply italic
                if (formatting.Italic.HasValue && formatting.Italic.Value)
                {
                    richTextBox.SelectionFont = new Font(richTextBox.SelectionFont ?? richTextBox.Font, 
                        richTextBox.SelectionFont?.Style | FontStyle.Italic ?? FontStyle.Italic);
                }
                
                // Apply underline
                if (formatting.UnderlineStyle.HasValue && formatting.UnderlineStyle.Value != Xceed.Document.NET.UnderlineStyle.none)
                {
                    richTextBox.SelectionFont = new Font(richTextBox.SelectionFont ?? richTextBox.Font, 
                        richTextBox.SelectionFont?.Style | FontStyle.Underline ?? FontStyle.Underline);
                }
                
                // Apply font size
                if (formatting.Size.HasValue)
                {
                    richTextBox.SelectionFont = new Font(richTextBox.SelectionFont?.FontFamily ?? FontFamily.GenericSansSerif, 
                        (float)formatting.Size.Value, richTextBox.SelectionFont?.Style ?? FontStyle.Regular);
                }
            }
        }
        
        richTextBox.Select(0, 0);
    }

    private void ImportDoc(string filePath, RichTextBox richTextBox)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var doc = new HWPFDocument(fs);
        var range = doc.GetRange();
        
        richTextBox.Clear();
        
        // Extract plain text (NPOI has limited formatting support for .doc)
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < range.NumParagraphs; i++)
        {
            var paragraph = range.GetParagraph(i);
            sb.AppendLine(paragraph.Text);
        }
        
        richTextBox.Text = sb.ToString();
        MessageBox.Show("DOC format imported as plain text. Formatting may be lost.", 
            "Import Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ImportOdt(string filePath, RichTextBox richTextBox)
    {
        richTextBox.Clear();
        
        try
        {
            using var archive = System.IO.Compression.ZipFile.OpenRead(filePath);
            var contentEntry = archive.GetEntry("content.xml");
            
            if (contentEntry == null)
            {
                throw new Exception("Invalid ODT file: content.xml not found");
            }
            
            using var stream = contentEntry.Open();
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(stream);
            
            // Define namespaces used in ODT
            var nsmgr = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
            nsmgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            nsmgr.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            
            // Get document body
            var body = xmlDoc.SelectSingleNode("//office:body/office:text", nsmgr);
            if (body == null) return;
            
            // Parse styles
            var styles = ParseOdtStyles(xmlDoc, nsmgr);
            
            // Process paragraphs
            var paragraphs = body.SelectNodes(".//text:p", nsmgr);
            if (paragraphs != null)
            {
                foreach (System.Xml.XmlNode para in paragraphs)
                {
                    ProcessOdtParagraph(para, richTextBox, styles, nsmgr);
                }
            }
            
            richTextBox.Select(0, 0);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse ODT file: {ex.Message}");
        }
    }
    
    private Dictionary<string, OdtStyle> ParseOdtStyles(System.Xml.XmlDocument xmlDoc, System.Xml.XmlNamespaceManager nsmgr)
    {
        var styles = new Dictionary<string, OdtStyle>();
        
        // Parse automatic styles
        var styleNodes = xmlDoc.SelectNodes("//office:automatic-styles/style:style", nsmgr);
        if (styleNodes != null)
        {
            foreach (System.Xml.XmlNode styleNode in styleNodes)
            {
                var styleName = styleNode.Attributes?["style:name"]?.Value;
                if (styleName == null) continue;
                
                var style = new OdtStyle();
                
                // Get text properties
                var textProps = styleNode.SelectSingleNode("style:text-properties", nsmgr);
                if (textProps?.Attributes != null)
                {
                    style.Bold = textProps.Attributes["fo:font-weight"]?.Value == "bold";
                    style.Italic = textProps.Attributes["fo:font-style"]?.Value == "italic";
                    style.Underline = textProps.Attributes["style:text-underline-style"]?.Value != null;
                    
                    var sizeAttr = textProps.Attributes["fo:font-size"]?.Value;
                    if (sizeAttr != null && sizeAttr.EndsWith("pt"))
                    {
                        if (float.TryParse(sizeAttr.Replace("pt", ""), out float size))
                        {
                            style.FontSize = size;
                        }
                    }
                }
                
                styles[styleName] = style;
            }
        }
        
        return styles;
    }
    
    private void ProcessOdtParagraph(System.Xml.XmlNode para, RichTextBox richTextBox, 
        Dictionary<string, OdtStyle> styles, System.Xml.XmlNamespaceManager nsmgr)
    {
        int startIndex = richTextBox.TextLength;
        
        // Get paragraph style
        var paraStyleName = para.Attributes?["text:style-name"]?.Value;
        OdtStyle? paraStyle = null;
        if (paraStyleName != null && styles.ContainsKey(paraStyleName))
        {
            paraStyle = styles[paraStyleName];
        }
        
        // Process text spans
        var hasContent = false;
        foreach (System.Xml.XmlNode child in para.ChildNodes)
        {
            if (child.NodeType == System.Xml.XmlNodeType.Text)
            {
                // Plain text
                richTextBox.AppendText(child.Value ?? "");
                hasContent = true;
            }
            else if (child.Name == "text:span")
            {
                // Formatted text span
                var spanStyleName = child.Attributes?["text:style-name"]?.Value;
                var text = child.InnerText;
                
                int spanStart = richTextBox.TextLength;
                richTextBox.AppendText(text);
                hasContent = true;
                
                // Apply span style
                if (spanStyleName != null && styles.ContainsKey(spanStyleName))
                {
                    ApplyOdtStyle(richTextBox, spanStart, text.Length, styles[spanStyleName]);
                }
            }
            else if (child.Name == "text:s")
            {
                // Space
                var count = child.Attributes?["text:c"]?.Value;
                int spaceCount = 1;
                if (count != null && int.TryParse(count, out int c))
                {
                    spaceCount = c;
                }
                richTextBox.AppendText(new string(' ', spaceCount));
                hasContent = true;
            }
            else if (child.Name == "text:tab")
            {
                richTextBox.AppendText("\t");
                hasContent = true;
            }
        }
        
        // Apply paragraph style to entire paragraph
        if (hasContent && paraStyle != null)
        {
            int length = richTextBox.TextLength - startIndex;
            if (length > 0)
            {
                ApplyOdtStyle(richTextBox, startIndex, length, paraStyle.Value);
            }
        }
        
        richTextBox.AppendText(Environment.NewLine);
    }
    
    private void ApplyOdtStyle(RichTextBox richTextBox, int start, int length, OdtStyle style)
    {
        richTextBox.Select(start, length);
        
        var currentFont = richTextBox.SelectionFont ?? richTextBox.Font;
        FontStyle fontStyle = FontStyle.Regular;
        
        if (style.Bold) fontStyle |= FontStyle.Bold;
        if (style.Italic) fontStyle |= FontStyle.Italic;
        if (style.Underline) fontStyle |= FontStyle.Underline;
        
        float fontSize = style.FontSize ?? currentFont.Size;
        
        richTextBox.SelectionFont = new Font(currentFont.FontFamily, fontSize, fontStyle);
    }
    
    private struct OdtStyle
    {
        public bool Bold;
        public bool Italic;
        public bool Underline;
        public float? FontSize;
    }


    private void ImportPdf(string filePath, RichTextBox richTextBox)
    {
        richTextBox.Clear();
        StringBuilder sb = new StringBuilder();
        
        using (var document = PdfDocument.Open(filePath))
        {
            foreach (Page page in document.GetPages())
            {
                sb.AppendLine($"--- Page {page.Number} ---");
                sb.AppendLine(page.Text);
                sb.AppendLine();
            }
        }
        
        richTextBox.Text = sb.ToString();
        MessageBox.Show("PDF imported as plain text. Formatting and layout are not preserved.", 
            "Import Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    
    private void ImportLatex(string filePath, RichTextBox richTextBox)
    {
        latexImporter.ImportLatex(filePath, richTextBox);
    }
}
