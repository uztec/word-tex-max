# WordTexMax - Word-like LaTeX Editor

Multiple implementations of a WYSIWYG editor that generates LaTeX code.

## Projects

### 1. WordTexMaxWinForms (Recommended - True WYSIWYG)
**Platform**: Windows (or Linux with Wine)  
**Technology**: .NET 8 Windows Forms  
**Editor**: RichTextBox (native WYSIWYG)

**Features**:
- ✅ True WYSIWYG editing (like MS Word)
- ✅ Click Bold → text becomes bold immediately
- ✅ Full formatting support
- ✅ RTF to LaTeX conversion

**Build**:
```bash
cd WordTexMaxWinForms
dotnet build
dotnet run  # On Windows
```

### 2. WordTexMax (Avalonia - Cross-platform)
**Platform**: Linux, Windows, macOS  
**Technology**: .NET 8 Avalonia UI  
**Editor**: AvaloniaEdit (code editor style)

**Features**:
- ✅ Cross-platform
- ⚠️ Not true WYSIWYG (uses Markdown syntax)
- ✅ Works on Linux without Wine

**Build**:
```bash
cd WordTexMax
dotnet run
```

### 3. WordTexMaxCpp (Qt - Requires Qt installation)
**Platform**: Linux, Windows, macOS  
**Technology**: C++ with Qt  
**Status**: ⚠️ Requires Qt6 installation

**Build**:
```bash
cd WordTexMaxCpp
mkdir build && cd build
cmake ..
make
./WordTexMaxCpp
```

## Recommendation
- **If you have Windows**: Use `WordTexMaxWinForms` (best WYSIWYG experience)
- **If you're on Linux only**: Use `WordTexMax` (Avalonia) - works now, but not true WYSIWYG
- **If you want to install Qt**: Use `WordTexMaxCpp` (good WYSIWYG with Qt's QTextEdit)

## Documentation
See `walkthrough.md` in the artifacts directory for detailed usage instructions.
