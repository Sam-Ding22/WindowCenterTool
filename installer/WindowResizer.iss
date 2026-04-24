#define MyAppName "Window Resizer"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Window Resizer"
#define MyAppExeName "WindowResizerApp.exe"
#define MyPublishDir "..\WindowResizerApp\bin\Release\net7.0-windows\win-x64\publish"

[Setup]
AppId={{8C8A69B0-1E4A-4DB5-A5D2-711C954B1F8A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Window Resizer
DefaultGroupName=Window Resizer
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=.\dist
OutputBaseFilename=WindowResizerSetup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"
Name: "launchonlogin"; Description: "Launch Window Resizer when I sign in"; GroupDescription: "Startup options:"

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Window Resizer"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall Window Resizer"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Window Resizer"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Window Resizer"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
    ValueType: string; ValueName: "WindowResizerApp"; ValueData: """{app}\{#MyAppExeName}"""; \
    Flags: uninsdeletevalue; Tasks: launchonlogin
