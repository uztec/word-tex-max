# WordTexMax Installer Build Instructions

## Prerequisites

1. **Inno Setup 6** - Download from: https://jrsoftware.org/isdl.php
2. **.NET 8.0 SDK** - For building the application
3. **Windows** - Inno Setup runs on Windows only

## Building the Application

Before creating the installer, you need to build the application in Release mode:

```bash
cd WordTexMaxWinForms
dotnet publish -c Release -r win-x64 --self-contained false
```

Or for a self-contained build (includes .NET runtime):

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

## Creating the Installer

### Option 1: Using Inno Setup GUI

1. Open **Inno Setup Compiler**
2. File → Open → Select `WordTexMax-Setup.iss`
3. Build → Compile
4. Installer will be created in `installer/` folder

### Option 2: Using Command Line

```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" WordTexMax-Setup.iss
```

## Customization

### Change Version

Edit in `WordTexMax-Setup.iss`:
```
#define MyAppVersion "1.0.0"
```

### Add Custom Icon

1. Create `icon.ico` in the project root
2. Update the script line:
```
SetupIconFile=icon.ico
```

### Add License File

Create `LICENSE.txt` in the project root with your license text.

## Output

The installer will be created as:
```
installer/WordTexMax-Setup-1.0.0.exe
```

## Features Included

- ✅ .NET 8.0 Runtime check (prompts download if missing)
- ✅ Desktop shortcut (optional)
- ✅ Start Menu shortcuts
- ✅ Uninstaller
- ✅ Templates folder included
- ✅ All dependencies bundled
- ✅ Portuguese and English language support
- ✅ Modern wizard style
- ✅ 64-bit support

## Distribution

The generated `.exe` file is a single self-extracting installer that can be distributed to users.

## Notes

- If using `--self-contained true`, the installer will be larger (~150MB) but won't require .NET installation
- If using `--self-contained false`, the installer will be smaller (~5MB) but requires .NET 8.0 Runtime
- Custom templates created by users are preserved during updates
