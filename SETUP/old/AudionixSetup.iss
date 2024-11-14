; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Audionix"
#define MyAppVersion "0.0.3.0"
#define MyAppPublisher "Greg Davis"
#define MyAppExeName "Audionix.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{5F470A96-C8FE-44D8-BDEB-53BD404AF16B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=C:\Users\gdavis\OneDrive\CODE\CURRENT PROJECTS\Audionix\SETUP
OutputBaseFilename=AudionixSetup
SetupIconFile=C:\Users\gdavis\OneDrive\CODE\CURRENT PROJECTS\Audionix\Audionix\Audionix.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableDirPage=auto
UsePreviousAppDir=yes
CreateUninstallRegKey=no
UpdateUninstallLogAppName=no



[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "C:\Users\gdavis\OneDrive\CODE\CURRENT PROJECTS\Audionix\Audionix\bin\Release\net8.0\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\gdavis\OneDrive\CODE\CURRENT PROJECTS\Audionix\Audionix\bin\Release\net8.0\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: {sys}\sc.exe; Parameters: "create Audionix start= auto binPath= ""{app}\Audionix.exe""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "start Audionix" ; Flags: runhidden ;

[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop Audionix" ; Flags: runhidden ; RunOnceId: "StopService"
Filename: {sys}\sc.exe; Parameters: "delete Audionix" ; Flags: runhidden ; RunOnceId: "DelService"