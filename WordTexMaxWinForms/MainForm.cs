using UzunTec.WordTexMax.Services;
using UzunTec.WordTexMax.Forms;

namespace UzunTec.WordTexMax;

public partial class MainForm : Form
{
    private RichTextBox editor;
    private TextBox preview;
    private ToolStrip toolStrip;
    private MenuStrip menuStrip;
    private SplitContainer splitContainer;
    private RtfToLatexConverter converter;
    private DocumentImporter importer;
    private DocumentExporter exporter;
    private LatexTemplateManager templateManager;

    public MainForm()
    {
        InitializeComponent();
        converter = new RtfToLatexConverter();
        importer = new DocumentImporter();
        exporter = new DocumentExporter();
        templateManager = new LatexTemplateManager();
    }

    private void InitializeComponent()
    {
        this.Text = "WordTexMax - WYSIWYG Editor";
        this.Size = new Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;

        // MenuStrip
        menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("File");
        
        fileMenu.DropDownItems.Add("Open...", null, MenuOpen_Click);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Save As RTF...", null, MenuSaveRtf_Click);
        fileMenu.DropDownItems.Add("Save As DOCX...", null, MenuSaveDocx_Click);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Export to PDF...", null, MenuExportPdf_Click);
        fileMenu.DropDownItems.Add("Export to LaTeX...", null, MenuExportLatex_Click);
        fileMenu.DropDownItems.Add("Export with Template...", null, MenuExportWithTemplate_Click);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
        
        menuStrip.Items.Add(fileMenu);

        // ToolStrip
        toolStrip = new ToolStrip();
        toolStrip.Items.Add(CreateButton("Bold", "B", BtnBold_Click, FontStyle.Bold));
        toolStrip.Items.Add(CreateButton("Italic", "I", BtnItalic_Click, FontStyle.Italic));
        toolStrip.Items.Add(CreateButton("Underline", "U", BtnUnderline_Click, FontStyle.Underline));
        toolStrip.Items.Add(CreateButton("Strikeout", "S", BtnStrikeout_Click, FontStyle.Strikeout));
        toolStrip.Items.Add(new ToolStripSeparator());
        
        var cmbHeading = new ToolStripComboBox("Heading");
        cmbHeading.Items.AddRange(new object[] { "Normal", "Heading 1", "Heading 2", "Heading 3" });
        cmbHeading.SelectedIndex = 0;
        cmbHeading.SelectedIndexChanged += CmbHeading_SelectedIndexChanged;
        toolStrip.Items.Add(cmbHeading);
        
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(CreateButton("Bullet List", "•", BtnBulletList_Click));
        toolStrip.Items.Add(CreateButton("Code Block", "{}", BtnCodeBlock_Click));
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(CreateButton("Generate LaTeX", "LaTeX", BtnGenerate_Click));

        // SplitContainer
        splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical
        };

        // Editor (RichTextBox)
        editor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11),
            AcceptsTab = true
        };
        editor.SelectionChanged += Editor_SelectionChanged;

        // Preview (TextBox)
        preview = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 10),
            ScrollBars = ScrollBars.Both,
            BackColor = Color.FromArgb(250, 250, 250)
        };

        splitContainer.Panel1.Controls.Add(editor);
        splitContainer.Panel2.Controls.Add(preview);

        this.Controls.Add(splitContainer);
        this.Controls.Add(toolStrip);
        this.Controls.Add(menuStrip);
        this.MainMenuStrip = menuStrip;
    }

    private ToolStripButton CreateButton(string tooltip, string text, EventHandler onClick, FontStyle? style = null)
    {
        var btn = new ToolStripButton(text)
        {
            ToolTipText = tooltip,
            AutoSize = true
        };
        if (style.HasValue)
            btn.Font = new Font(btn.Font, style.Value);
        btn.Click += onClick;
        return btn;
    }

    private void BtnBold_Click(object? sender, EventArgs e)
    {
        ToggleFontStyle(FontStyle.Bold);
    }

    private void BtnItalic_Click(object? sender, EventArgs e)
    {
        ToggleFontStyle(FontStyle.Italic);
    }

    private void BtnUnderline_Click(object? sender, EventArgs e)
    {
        ToggleFontStyle(FontStyle.Underline);
    }

    private void BtnStrikeout_Click(object? sender, EventArgs e)
    {
        ToggleFontStyle(FontStyle.Strikeout);
    }

    private void ToggleFontStyle(FontStyle style)
    {
        if (editor.SelectionFont != null)
        {
            FontStyle newStyle = editor.SelectionFont.Style ^ style;
            editor.SelectionFont = new Font(editor.SelectionFont, newStyle);
        }
        editor.Focus();
    }

    private void CmbHeading_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is ToolStripComboBox cmb && editor.SelectionFont != null)
        {
            float size = cmb.SelectedIndex switch
            {
                1 => 18, // H1
                2 => 16, // H2
                3 => 14, // H3
                _ => 11  // Normal
            };
            editor.SelectionFont = new Font(editor.SelectionFont.FontFamily, size, editor.SelectionFont.Style);
        }
        editor.Focus();
    }

    private void BtnBulletList_Click(object? sender, EventArgs e)
    {
        editor.SelectionBullet = !editor.SelectionBullet;
        editor.Focus();
    }

    private void BtnCodeBlock_Click(object? sender, EventArgs e)
    {
        if (editor.SelectionFont != null)
        {
            editor.SelectionFont = new Font("Courier New", 10);
            editor.SelectionBackColor = Color.FromArgb(240, 240, 240);
        }
        editor.Focus();
    }

    private void BtnGenerate_Click(object? sender, EventArgs e)
    {
        string latex = converter.ConvertToLatex(editor);
        preview.Text = latex;
    }

    private void Editor_SelectionChanged(object? sender, EventArgs e)
    {
        // Update toolbar button states based on current selection
        // (Optional enhancement)
    }

    // File Menu Handlers
    private void MenuOpen_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "All Supported|*.rtf;*.docx;*.doc;*.odt;*.pdf;*.tex|" +
                     "Rich Text Format|*.rtf|" +
                     "Word Document|*.docx|" +
                     "Word 97-2003|*.doc|" +
                     "OpenDocument|*.odt|" +
                     "PDF Document|*.pdf|" +
                     "LaTeX Document|*.tex",
            Title = "Open Document"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            importer.ImportDocument(dialog.FileName, editor);
        }
    }

    private void MenuSaveRtf_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Rich Text Format|*.rtf",
            Title = "Save as RTF"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            exporter.ExportToRtf(editor, dialog.FileName);
            MessageBox.Show("Document saved successfully!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void MenuSaveDocx_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Word Document|*.docx",
            Title = "Save as DOCX"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            exporter.ExportToDocx(editor, dialog.FileName);
            MessageBox.Show("Document saved successfully!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void MenuExportPdf_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PDF Document|*.pdf",
            Title = "Export to PDF"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            exporter.ExportToPdf(editor, dialog.FileName);
            MessageBox.Show("PDF exported successfully!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void MenuExportLatex_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "LaTeX Document|*.tex",
            Title = "Export to LaTeX"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            exporter.ExportToLatex(editor, dialog.FileName);
            MessageBox.Show("LaTeX exported successfully!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void MenuExportWithTemplate_Click(object? sender, EventArgs e)
    {
        using var templateDialog = new TemplateDialog(templateManager);
        
        if (templateDialog.ShowDialog() == DialogResult.OK)
        {
            if (templateDialog.SelectedTemplate == null)
            {
                MessageBox.Show("No template selected.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "LaTeX Document|*.tex",
                Title = "Export with Template"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Get content without preamble
                string content = converter.ConvertToLatexContent(editor);
                
                // Apply template
                string latex = templateManager.ApplyTemplate(
                    templateDialog.SelectedTemplate, 
                    templateDialog.Metadata, 
                    content);
                
                File.WriteAllText(saveDialog.FileName, latex);
                
                MessageBox.Show("LaTeX exported with template successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
