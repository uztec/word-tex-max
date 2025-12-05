using System;
using System.Drawing;
using System.Windows.Forms;
using UzunTec.WordTexMax.Models;
using UzunTec.WordTexMax.Services;


namespace UzunTec.WordTexMax.Forms;

public class TemplateDialog : Form
{
    private ComboBox cmbTemplates;
    private TextBox txtTitle;
    private TextBox txtAuthor;
    private TextBox txtDate;
    private TextBox txtAbstract;
    private TextBox txtPreview;
    private Button btnOk;
    private Button btnCancel;

    private LatexTemplateManager templateManager;
    public LatexTemplate? SelectedTemplate { get; private set; }
    public LatexMetadata Metadata { get; private set; }

    public TemplateDialog(LatexTemplateManager manager)
    {
        templateManager = manager;
        Metadata = new LatexMetadata();
        InitializeComponent();
        LoadTemplates();
    }

    private void InitializeComponent()
    {
        this.Text = "Export with LaTeX Template";
        this.Size = new Size(700, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(10)
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Template selection
        mainLayout.Controls.Add(new Label { Text = "Template:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
        cmbTemplates = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbTemplates.SelectedIndexChanged += CmbTemplates_SelectedIndexChanged;
        mainLayout.Controls.Add(cmbTemplates, 1, 0);

        // Title
        mainLayout.Controls.Add(new Label { Text = "Title:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
        txtTitle = new TextBox { Dock = DockStyle.Fill };
        mainLayout.Controls.Add(txtTitle, 1, 1);

        // Author
        mainLayout.Controls.Add(new Label { Text = "Author:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
        txtAuthor = new TextBox { Dock = DockStyle.Fill };
        mainLayout.Controls.Add(txtAuthor, 1, 2);

        // Date
        mainLayout.Controls.Add(new Label { Text = "Date:", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
        txtDate = new TextBox { Dock = DockStyle.Fill, Text = "\\today" };
        mainLayout.Controls.Add(txtDate, 1, 3);

        // Abstract
        mainLayout.Controls.Add(new Label { Text = "Abstract:", TextAlign = ContentAlignment.TopRight, Padding = new Padding(0, 5, 0, 0) }, 0, 4);
        txtAbstract = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };
        mainLayout.Controls.Add(txtAbstract, 1, 4);

        // Preview
        mainLayout.Controls.Add(new Label { Text = "Preview:", TextAlign = ContentAlignment.TopRight, Padding = new Padding(0, 5, 0, 0) }, 0, 5);
        txtPreview = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 9),
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        mainLayout.Controls.Add(txtPreview, 1, 5);

        // Buttons
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
        btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
        btnOk.Click += BtnOk_Click;

        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        mainLayout.Controls.Add(btnPanel, 1, 6);

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Template
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Title
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Author
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Date
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Abstract
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Preview
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Buttons

        this.Controls.Add(mainLayout);
        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
    }

    private void LoadTemplates()
    {
        var templates = templateManager.GetAllTemplates();
        cmbTemplates.Items.Clear();

        foreach (var template in templates)
        {
            cmbTemplates.Items.Add(template.Name);
        }

        if (cmbTemplates.Items.Count > 0)
        {
            cmbTemplates.SelectedIndex = 0;
        }
    }

    private void CmbTemplates_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbTemplates.SelectedItem == null) return;

        var templateName = cmbTemplates.SelectedItem.ToString();
        var template = templateManager.GetTemplate(templateName ?? "");

        if (template != null)
        {
            UpdatePreview(template);
        }
    }

    private void UpdatePreview(LatexTemplate template)
    {
        var preview = new System.Text.StringBuilder();

        // Document class
        var classOptions = string.Join(",", template.ClassOptions);
        if (!string.IsNullOrEmpty(classOptions))
        {
            preview.AppendLine($"\\documentclass[{classOptions}]{{{template.DocumentClass}}}");
        }
        else
        {
            preview.AppendLine($"\\documentclass{{{template.DocumentClass}}}");
        }

        preview.AppendLine();

        // Show first few packages
        var packageCount = Math.Min(5, template.Packages.Count);
        for (int i = 0; i < packageCount; i++)
        {
            preview.AppendLine(template.Packages[i].ToString());
        }

        if (template.Packages.Count > 5)
        {
            preview.AppendLine($"... and {template.Packages.Count - 5} more packages");
        }

        preview.AppendLine();
        preview.AppendLine(template.Preamble);

        if (!string.IsNullOrEmpty(template.CustomCommands))
        {
            preview.AppendLine();
            preview.AppendLine("% Custom commands:");
            preview.AppendLine(template.CustomCommands);
        }

        txtPreview.Text = preview.ToString();
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (cmbTemplates.SelectedItem == null)
        {
            MessageBox.Show("Please select a template.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.DialogResult = DialogResult.None;
            return;
        }

        var templateName = cmbTemplates.SelectedItem.ToString();
        SelectedTemplate = templateManager.GetTemplate(templateName ?? "");

        Metadata.Title = txtTitle.Text;
        Metadata.Author = txtAuthor.Text;
        Metadata.Date = txtDate.Text;
        Metadata.Abstract = txtAbstract.Text;
    }
}
