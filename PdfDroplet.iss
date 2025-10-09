; Inno Setup Script for PdfDroplet
; https://jrsoftware.org/isinfo.php

#define MyAppName "PdfDroplet"
#define MyAppVersion "2.6"
#define MyAppPublisher "SIL International"
#define MyAppURL "https://github.com/sillsdev/pdfdroplet"
#define MyAppExeName "PdfDroplet.exe"
#define MyAppId "{{B5A0BE25-532D-4CF9-9BB6-B0D513B1186A}"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=DistFiles\license.rtf
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=output\installer
OutputBaseFilename=PdfDropletInstaller
; SetupIconFile=output\Release\win-x86\{#MyAppExeName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
MinVersion=6.1sp1
; Show images during setup
;WizardImageFile=
;WizardSmallImageFile=

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main executable
Source: "output\Release\win-x86\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Configuration and runtime files
Source: "output\Release\win-x86\*.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "output\Release\win-x86\*.config"; DestDir: "{app}"; Flags: ignoreversion

; All DLL files
Source: "output\Release\win-x86\*.dll"; DestDir: "{app}"; Flags: ignoreversion

; PDB files for debugging (optional but helpful)
Source: "output\Release\win-x86\*.pdb"; DestDir: "{app}"; Flags: ignoreversion

; Localization folders
Source: "output\Release\win-x86\cs\*"; DestDir: "{app}\cs"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\de\*"; DestDir: "{app}\de"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\es\*"; DestDir: "{app}\es"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\fr\*"; DestDir: "{app}\fr"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\it\*"; DestDir: "{app}\it"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\ja\*"; DestDir: "{app}\ja"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\ko\*"; DestDir: "{app}\ko"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\pl\*"; DestDir: "{app}\pl"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\pt-BR\*"; DestDir: "{app}\pt-BR"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\ru\*"; DestDir: "{app}\ru"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\tr\*"; DestDir: "{app}\tr"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\zh-Hans\*"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "output\Release\win-x86\zh-Hant\*"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion recursesubdirs createallsubdirs

; Runtime dependencies
Source: "output\Release\win-x86\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs

; Documentation and license files
Source: "DistFiles\about.htm"; DestDir: "{app}"; Flags: ignoreversion
Source: "DistFiles\instructions.htm"; DestDir: "{app}"; Flags: ignoreversion
Source: "DistFiles\*.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "DistFiles\AGPL License.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "DistFiles\images\*"; DestDir: "{app}\images"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Download and install WebView2 Runtime if not present (runs before launching the app)
Filename: "https://go.microsoft.com/fwlink/p/?LinkId=2124703"; Description: "Download and Install Microsoft Edge WebView2 Runtime (Required)"; Flags: shellexec runasoriginaluser postinstall; Check: not IsWebView2RuntimeInstalled

Filename: "{app}\{#MyAppExeName}"; Parameters: "-about"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsWebView2RuntimeInstalled: Boolean;
var
  Version: String;
begin
  // Check if WebView2 Runtime is installed by looking for the registry key
  // Check system-wide installation (HKLM)
  Result := RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
  if not Result then
    // Check per-user installation (HKCU)
    Result := RegQueryStringValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
  if not Result then
    // Check 64-bit HKLM location
    Result := RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  // Check if WebView2 Runtime is installed
  if not IsWebView2RuntimeInstalled then
  begin
    if MsgBox('This application requires Microsoft Edge WebView2 Runtime to display PDF previews.' + #13#10 + #13#10 +
              'WebView2 Runtime is not currently installed on your system.' + #13#10 + #13#10 +
              'Would you like to continue with the installation? You will be prompted to download WebView2 after installation completes.', 
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;
