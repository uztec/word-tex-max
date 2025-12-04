; WordTexMax Inno Setup Script
; Generates a Windows installer for WordTexMax application

#define MyAppName "WordTexMax"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "WordTexMax Team"
#define MyAppURL "https://github.com/yourrepo/wordtexmax"
#define MyAppExeName "WordTexMaxWinForms.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{A7B8C9D0-E1F2-4A5B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=installer
OutputBaseFilename=WordTexMax-Setup-{#MyAppVersion}
SetupIconFile=icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Main executable
Source: "WordTexMaxWinForms\bin\Release\net8.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; All DLLs and dependencies
Source: "WordTexMaxWinForms\bin\Release\net8.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "WordTexMaxWinForms\bin\Release\net8.0-windows\*.json"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "WordTexMaxWinForms\bin\Release\net8.0-windows\*.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "WordTexMaxWinForms\bin\Release\net8.0-windows\*.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
; Templates folder
Source: "WordTexMaxWinForms\bin\Release\net8.0-windows\Templates\*"; DestDir: "{app}\Templates"; Flags: ignoreversion recursesubdirs createallsubdirs
; Documentation
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  // Check if .NET 8.0 Runtime is installed
  Result := RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost') or
            RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedhost');
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;
  
  if not IsDotNetInstalled() then
  begin
    if MsgBox('.NET 8.0 Runtime is required but not installed.' + #13#10 + #13#10 +
              'Would you like to download it now?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0', '', '', SW_SHOW, ewNoWait, ErrorCode);
      Result := False;
    end
    else
    begin
      Result := False;
    end;
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Templates\custom"
