using System.Text.Json;
using UzunTec.WordTexMax.Models;

namespace UzunTec.WordTexMax.Services;

public class LatexTemplateManager
{
    private readonly string templatesDirectory;
    private List<LatexTemplate> templates;

    public LatexTemplateManager()
    {
        templatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        Directory.CreateDirectory(templatesDirectory);
        templates = new List<LatexTemplate>();
        LoadPredefinedTemplates();
        LoadCustomTemplates();
    }

    public List<LatexTemplate> GetAllTemplates() => templates;

    public LatexTemplate? GetTemplate(string name)
    {
        return templates.FirstOrDefault(t => t.Name == name);
    }

    public void SaveCustomTemplate(LatexTemplate template)
    {
        var customDir = Path.Combine(templatesDirectory, "custom");
        Directory.CreateDirectory(customDir);
        
        var fileName = $"{template.Name.Replace(" ", "_").ToLower()}.json";
        var filePath = Path.Combine(customDir, fileName);
        
        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText(filePath, json);
        
        // Reload templates
        LoadCustomTemplates();
    }

    private void LoadPredefinedTemplates()
    {
        // General templates
        templates.Add(CreateArticleTemplate());
        templates.Add(CreateReportTemplate());
        templates.Add(CreateBookTemplate());
        templates.Add(CreateBeamerTemplate());
        templates.Add(CreateLetterTemplate());
        templates.Add(CreateCVTemplate());
        
        // Scientific/Academic templates
        templates.Add(CreateIEEETemplate());
        templates.Add(CreateACMTemplate());
        templates.Add(CreateThesisTemplate());
        templates.Add(CreateDissertationTemplate());
        templates.Add(CreateConferencePaperTemplate());
        templates.Add(CreateJournalArticleTemplate());
    }


    private void LoadCustomTemplates()
    {
        var customDir = Path.Combine(templatesDirectory, "custom");
        if (!Directory.Exists(customDir)) return;

        foreach (var file in Directory.GetFiles(customDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var template = JsonSerializer.Deserialize<LatexTemplate>(json);
                if (template != null && !templates.Any(t => t.Name == template.Name))
                {
                    templates.Add(template);
                }
            }
            catch
            {
                // Skip invalid templates
            }
        }
    }

    private LatexTemplate CreateArticleTemplate()
    {
        return new LatexTemplate
        {
            Name = "Article",
            DocumentClass = "article",
            ClassOptions = new List<string> { "12pt", "a4paper" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=1in" } },
                new() { Name = "graphicx" },
                new() { Name = "hyperref", Options = new() { "colorlinks=true", "linkcolor=blue" } },
                new() { Name = "amsmath" },
                new() { Name = "listings" },
                new() { Name = "xcolor" },
                new() { Name = "ulem" }
            },
            Preamble = "\\title{{{TITLE}}}\n\\author{{{AUTHOR}}}\n\\date{{{DATE}}}",
            IncludeAbstract = true
        };
    }

    private LatexTemplate CreateReportTemplate()
    {
        return new LatexTemplate
        {
            Name = "Report",
            DocumentClass = "report",
            ClassOptions = new List<string> { "12pt", "a4paper" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=1in" } },
                new() { Name = "graphicx" },
                new() { Name = "fancyhdr" },
                new() { Name = "titlesec" },
                new() { Name = "hyperref" },
                new() { Name = "listings" },
                new() { Name = "xcolor" }
            },
            Preamble = "\\title{{{TITLE}}}\n\\author{{{AUTHOR}}}\n\\date{{{DATE}}}\n\\pagestyle{fancy}",
            IncludeAbstract = true
        };
    }

    private LatexTemplate CreateBookTemplate()
    {
        return new LatexTemplate
        {
            Name = "Book",
            DocumentClass = "book",
            ClassOptions = new List<string> { "12pt", "a4paper", "twoside" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=1in" } },
                new() { Name = "graphicx" },
                new() { Name = "fancyhdr" },
                new() { Name = "hyperref" },
                new() { Name = "listings" }
            },
            Preamble = "\\title{{{TITLE}}}\n\\author{{{AUTHOR}}}\n\\date{{{DATE}}}",
            IncludeAbstract = false
        };
    }

    private LatexTemplate CreateBeamerTemplate()
    {
        return new LatexTemplate
        {
            Name = "Beamer Presentation",
            DocumentClass = "beamer",
            ClassOptions = new List<string>(),
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "graphicx" },
                new() { Name = "tikz" }
            },
            Preamble = "\\usetheme{Madrid}\n\\title{{{TITLE}}}\n\\author{{{AUTHOR}}}\n\\date{{{DATE}}}",
            CustomCommands = "\\AtBeginSection[]\n{\n  \\begin{frame}\n    \\tableofcontents[currentsection]\n  \\end{frame}\n}",
            IncludeAbstract = false
        };
    }

    private LatexTemplate CreateLetterTemplate()
    {
        return new LatexTemplate
        {
            Name = "Letter",
            DocumentClass = "letter",
            ClassOptions = new List<string> { "12pt" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=1in" } }
            },
            Preamble = "\\signature{{{AUTHOR}}}\n\\address{Your Address}",
            IncludeAbstract = false
        };
    }

    private LatexTemplate CreateCVTemplate()
    {
        return new LatexTemplate
        {
            Name = "CV/Resume",
            DocumentClass = "article",
            ClassOptions = new List<string> { "11pt", "a4paper" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=0.75in" } },
                new() { Name = "enumitem" },
                new() { Name = "hyperref" }
            },
            Preamble = "\\pagestyle{empty}\n\\setlength{\\parindent}{0pt}",
            CustomCommands = "\\newcommand{\\name}[1]{\\begin{center}\\Huge\\textbf{#1}\\end{center}\\vspace{0.5em}}",
            IncludeAbstract = false
        };
    }

    public string ApplyTemplate(LatexTemplate template, LatexMetadata metadata, string content)
    {
        var sb = new System.Text.StringBuilder();
        
        // Document class
        var classOptions = string.Join(",", template.ClassOptions);
        if (!string.IsNullOrEmpty(classOptions))
        {
            sb.AppendLine($"\\documentclass[{classOptions}]{{{template.DocumentClass}}}");
        }
        else
        {
            sb.AppendLine($"\\documentclass{{{template.DocumentClass}}}");
        }
        
        sb.AppendLine();
        
        // Packages
        foreach (var package in template.Packages)
        {
            sb.AppendLine(package.ToString());
        }
        
        sb.AppendLine();
        
        // Custom commands
        if (!string.IsNullOrEmpty(template.CustomCommands))
        {
            sb.AppendLine(template.CustomCommands);
            sb.AppendLine();
        }
        
        // Preamble with metadata
        var preamble = template.Preamble
            .Replace("{{TITLE}}", metadata.Title)
            .Replace("{{AUTHOR}}", metadata.Author)
            .Replace("{{DATE}}", metadata.Date);
        sb.AppendLine(preamble);
        
        sb.AppendLine();
        sb.AppendLine("\\begin{document}");
        
        // Title page
        if (template.DocumentClass == "beamer")
        {
            sb.AppendLine("\\frame{\\titlepage}");
        }
        else if (!string.IsNullOrEmpty(metadata.Title))
        {
            sb.AppendLine("\\maketitle");
        }
        
        sb.AppendLine();
        
        // Abstract
        if (template.IncludeAbstract && !string.IsNullOrEmpty(metadata.Abstract))
        {
            sb.AppendLine("\\begin{abstract}");
            sb.AppendLine(metadata.Abstract);
            sb.AppendLine("\\end{abstract}");
            sb.AppendLine();
        }
        
        // Content
        sb.AppendLine(content);
        
        sb.AppendLine();
        sb.AppendLine("\\end{document}");
        
        return sb.ToString();
    }
    
    // Scientific/Academic Templates
    
    private LatexTemplate CreateIEEETemplate()
    {
        return new LatexTemplate
        {
            Name = "IEEE Conference Paper",
            DocumentClass = "IEEEtran",
            ClassOptions = new List<string> { "conference" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "cite" },
                new() { Name = "amsmath" },
                new() { Name = "amssymb" },
                new() { Name = "amsfonts" },
                new() { Name = "algorithmic" },
                new() { Name = "graphicx" },
                new() { Name = "textcomp" },
                new() { Name = "xcolor" },
                new() { Name = "hyperref" }
            },
            Preamble = "\\title{{{TITLE}}}\\n\\author{\\IEEEauthorblockN{{{AUTHOR}}}\\n\\IEEEauthorblockA{Your Institution\\\\Email: author@institution.edu}}",
            IncludeAbstract = true,
            CustomCommands = "% IEEE specific commands\n\\hyphenation{op-tical net-works semi-conduc-tor}"
        };
    }
    
    private LatexTemplate CreateACMTemplate()
    {
        return new LatexTemplate
        {
            Name = "ACM Article",
            DocumentClass = "acmart",
            ClassOptions = new List<string> { "sigconf" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "booktabs" },
                new() { Name = "graphicx" },
                new() { Name = "hyperref" }
            },
            Preamble = "\\acmConference[CONFERENCE]{Conference Name}{Month Year}{City, Country}\\n\\title{{{TITLE}}}\\n\\author{{{AUTHOR}}}\\n\\affiliation{\\n  \\institution{Your Institution}\\n  \\city{City}\\n  \\country{Country}\\n}\\n\\email{author@institution.edu}",
            IncludeAbstract = true,
            CustomCommands = "\\copyrightyear{2024}\n\\acmYear{2024}\n\\setcopyright{acmlicensed}"
        };
    }
    
    private LatexTemplate CreateThesisTemplate()
    {
        return new LatexTemplate
        {
            Name = "Master's Thesis",
            DocumentClass = "report",
            ClassOptions = new List<string> { "12pt", "a4paper", "oneside" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=1in", "top=1.5in" } },
                new() { Name = "setspace" },
                new() { Name = "graphicx" },
                new() { Name = "amsmath" },
                new() { Name = "amssymb" },
                new() { Name = "natbib" },
                new() { Name = "hyperref", Options = new() { "hidelinks" } },
                new() { Name = "fancyhdr" },
                new() { Name = "titlesec" }
            },
            Preamble = "\\title{{{TITLE}}}\\n\\author{{{AUTHOR}}}\\n\\date{{{DATE}}}\\n\\onehalfspacing\\n\\pagestyle{fancy}\\n\\fancyhf{}\\n\\fancyhead[R]{\\thepage}",
            IncludeAbstract = true,
            CustomCommands = "% Thesis specific\n\\newcommand{\\university}{Your University}\n\\newcommand{\\department}{Department Name}\n\\newcommand{\\degree}{Master of Science}"
        };
    }
    
    private LatexTemplate CreateDissertationTemplate()
    {
        return new LatexTemplate
        {
            Name = "PhD Dissertation",
            DocumentClass = "book",
            ClassOptions = new List<string> { "12pt", "a4paper", "twoside", "openright" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=1in", "top=1.5in", "bottom=1.5in" } },
                new() { Name = "setspace" },
                new() { Name = "graphicx" },
                new() { Name = "amsmath" },
                new() { Name = "amssymb" },
                new() { Name = "amsthm" },
                new() { Name = "natbib" },
                new() { Name = "hyperref", Options = new() { "hidelinks" } },
                new() { Name = "fancyhdr" },
                new() { Name = "titlesec" },
                new() { Name = "tocloft" }
            },
            Preamble = "\\title{{{TITLE}}}\\n\\author{{{AUTHOR}}}\\n\\date{{{DATE}}}\\n\\doublespacing\\n\\pagestyle{fancy}",
            IncludeAbstract = true,
            CustomCommands = "% Dissertation specific\n\\newtheorem{theorem}{Theorem}[chapter]\n\\newtheorem{lemma}[theorem]{Lemma}\n\\newtheorem{proposition}[theorem]{Proposition}\n\\newtheorem{corollary}[theorem]{Corollary}"
        };
    }
    
    private LatexTemplate CreateConferencePaperTemplate()
    {
        return new LatexTemplate
        {
            Name = "Conference Paper (Generic)",
            DocumentClass = "article",
            ClassOptions = new List<string> { "10pt", "twocolumn" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "inputenc", Options = new() { "utf8" } },
                new() { Name = "geometry", Options = new() { "margin=0.75in" } },
                new() { Name = "graphicx" },
                new() { Name = "amsmath" },
                new() { Name = "amssymb" },
                new() { Name = "cite" },
                new() { Name = "hyperref", Options = new() { "colorlinks=true", "linkcolor=blue", "citecolor=blue" } },
                new() { Name = "balance" }
            },
            Preamble = "\\title{{{TITLE}}}\\n\\author{{{AUTHOR}} \\\\\\\\ Your Institution \\\\\\\\ \\texttt{author@institution.edu}}\\n\\date{}",
            IncludeAbstract = true,
            CustomCommands = "% Conference paper formatting\n\\setlength{\\columnsep}{0.25in}"
        };
    }
    
    private LatexTemplate CreateJournalArticleTemplate()
    {
        return new LatexTemplate
        {
            Name = "Journal Article (Elsevier)",
            DocumentClass = "elsarticle",
            ClassOptions = new List<string> { "review", "12pt" },
            Packages = new List<LatexPackage>
            {
                new() { Name = "lineno" },
                new() { Name = "graphicx" },
                new() { Name = "amsmath" },
                new() { Name = "amssymb" },
                new() { Name = "hyperref" }
            },
            Preamble = "\\journal{Journal Name}\\n\\title{{{TITLE}}}\\n\\author[inst1]{{{AUTHOR}}}\\n\\address[inst1]{Your Institution, City, Country}\\n\\ead{author@institution.edu}",
            IncludeAbstract = true,
            CustomCommands = "\\linenumbers\n% Keywords\n\\begin{keyword}\n{{KEYWORDS}}\n\\end{keyword}"
        };
    }
}
