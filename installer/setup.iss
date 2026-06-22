; Inno Setup Script
; Work_Practice — Учебная практика

#define MyAppName "Work_Practice"
#define MyAppVersion "1.0"
#define MyAppPublisher "ТулГУ"
#define MyAppExeName "WorkPracticeLauncher.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Work_Practice
DefaultGroupName={#MyAppName}
OutputDir=C:\проверка связи\Work_Practice\installer
OutputBaseFilename=setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\Work_Practice.exe
PrivilegesRequired=lowest

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; WPF приложение
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\Work_Practice.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\Work_Practice.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion

; Консольный лаунчер
Source: "C:\проверка связи\Work_Practice\WorkPracticeLauncher\bin\Release\WorkPracticeLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\WorkPracticeLauncher\bin\Release\WorkPracticeLauncher.exe.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Work_Practice (WPF)"; Filename: "{app}\Work_Practice.exe"
Name: "{group}\Work_Practice (Лаунчер)"; Filename: "{app}\WorkPracticeLauncher.exe"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Work_Practice"; Filename: "{app}\WorkPracticeLauncher.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\WorkPracticeLauncher.exe"; Description: "Запустить лаунчер"; Flags: nowait postinstall skipifsilent
