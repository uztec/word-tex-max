using System.Collections.Generic;

namespace UzunTec.WordTexMax.Models;

public class LatexTemplate
{
    public string Name { get; set; } = "";
    public string DocumentClass { get; set; } = "article";
    public List<string> ClassOptions { get; set; } = new();
    public List<LatexPackage> Packages { get; set; } = new();
    public string Preamble { get; set; } = "";
    public string CustomCommands { get; set; } = "";
    public bool IncludeAbstract { get; set; } = false;
}

public class LatexPackage
{
    public string Name { get; set; } = "";
    public List<string> Options { get; set; } = new();
    
    public override string ToString()
    {
        if (Options.Count > 0)
        {
            return $"\\usepackage[{string.Join(",", Options)}]{{{Name}}}";
        }
        return $"\\usepackage{{{Name}}}";
    }
}

public class LatexMetadata
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Date { get; set; } = "\\today";
    public string Abstract { get; set; } = "";
    public string Keywords { get; set; } = "";
}
