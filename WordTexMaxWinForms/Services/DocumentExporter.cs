using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Xceed.Words.NET;

namespace UzunTec.WordTexMax.Services;

public class DocumentExporter
{
    public void ExportToPdf(RichTextBox richTextBox, string filePath)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Content().Column(column =>
                {
                    column.Spacing(5);
                    
                    // Parse RichTextBox content
                    string text = richTextBox.Text;
                    int pos = 0;
                    
                    while (pos < text.Length)
                    {
                        richTextBox.Select(pos, 1);
                        char currentChar = text[pos];
                        
                        if (currentChar == '\n')
                        {
                            pos++;
                            continue;
                        }
                        
                        // Get formatting
                        Font? font = richTextBox.SelectionFont;
                        
                        if (font != null)
                        {
                            // Find continuous text with same formatting
                            int start = pos;
                            while (pos < text.Length && 
                                   text[pos] != '\n' &&
                                   richTextBox.SelectionFont?.Style == font.Style &&
                                   richTextBox.SelectionFont?.Size == font.Size)
                            {
                                pos++;
                                if (pos < text.Length)
                                    richTextBox.Select(pos, 1);
                            }
                            
                            string segment = text.Substring(start, pos - start);
                            
                            column.Item().Text(txt =>
                            {
                                var span = txt.Span(segment);
                                
                                if (font.Bold) span.Bold();
                                if (font.Italic) span.Italic();
                                if (font.Underline) span.Underline();
                                if (font.Strikeout) span.Strikethrough();
                                
                                span.FontSize(font.Size);
                            });
                        }
                        else
                        {
                            pos++;
                        }
                    }
                });
            });
        })
        .GeneratePdf(filePath);
    }

    public void ExportToDocx(RichTextBox richTextBox, string filePath)
    {
        var doc = DocX.Create(filePath);
        
        string text = richTextBox.Text;
        var lines = text.Split('\n');
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                doc.InsertParagraph();
                continue;
            }
            
            var paragraph = doc.InsertParagraph(line);
            
            // Apply formatting from first character (simplified)
            if (line.Length > 0)
            {
                richTextBox.Select(richTextBox.GetFirstCharIndexFromLine(0), 1);
                Font? font = richTextBox.SelectionFont;
                
                if (font != null)
                {
                    if (font.Bold) paragraph.Bold();
                    if (font.Italic) paragraph.Italic();
                    if (font.Underline) paragraph.UnderlineStyle(Xceed.Document.NET.UnderlineStyle.singleLine);
                    paragraph.FontSize(font.Size);
                }
            }
        }
        
        doc.Save();
    }

    public void ExportToLatex(RichTextBox richTextBox, string filePath)
    {
        var converter = new RtfToLatexConverter();
        string latex = converter.ConvertToLatex(richTextBox);
        File.WriteAllText(filePath, latex);
    }

    public void ExportToRtf(RichTextBox richTextBox, string filePath)
    {
        richTextBox.SaveFile(filePath, RichTextBoxStreamType.RichText);
    }
}
