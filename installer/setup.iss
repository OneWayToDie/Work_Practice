; Inno Setup Script
; Work_Practice - Учебная практика

#define MyAppName "Work_Practice"
#define MyAppVersion "1.0"
#define MyAppPublisher "ТулГУ"

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
UninstallDisplayIcon={app}\WorkPracticeLauncher.exe
PrivilegesRequired=lowest

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; === Корень: exe и конфиги ===
Source: "C:\проверка связи\Work_Practice\WorkPracticeLauncher\bin\Release\WorkPracticeLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\WorkPracticeLauncher\bin\Release\WorkPracticeLauncher.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\Work_Practice.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\Work_Practice.exe.config"; DestDir: "{app}"; Flags: ignoreversion

; === lib: библиотеки ===
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}\lib"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\System.Buffers.dll"; DestDir: "{app}\lib"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\System.ComponentModel.Annotations.dll"; DestDir: "{app}\lib"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\System.Memory.dll"; DestDir: "{app}\lib"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\System.Numerics.Vectors.dll"; DestDir: "{app}\lib"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}\lib"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\bin\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}\lib"; Flags: ignoreversion

; === icons: иконки ===
Source: "C:\проверка связи\Work_Practice\Work_Practice\Resources\Images\Boxes.ico"; DestDir: "{app}\icons"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\Resources\Images\calculator.ico"; DestDir: "{app}\icons"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\Resources\Images\list.ico"; DestDir: "{app}\icons"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\Resources\Images\rocketMonitor.ico"; DestDir: "{app}\icons"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\Resources\Images\university.ico"; DestDir: "{app}\icons"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\Work_Practice\Resources\Images\warning.ico"; DestDir: "{app}\icons"; Flags: ignoreversion
Source: "C:\проверка связи\Work_Practice\WorkPracticeLauncher\terminal.ico"; DestDir: "{app}\icons"; Flags: ignoreversion

[Icons]
Name: "{group}\Work_Practice (WPF)"; Filename: "{app}\Work_Practice.exe"
Name: "{group}\Work_Practice (Лаунчер)"; Filename: "{app}\WorkPracticeLauncher.exe"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Work_Practice (WPF)"; Filename: "{app}\Work_Practice.exe"; IconFilename: "{app}\icons\rocketMonitor.ico"; Tasks: desktopicon
Name: "{autodesktop}\Work_Practice (Лаунчер)"; Filename: "{app}\WorkPracticeLauncher.exe"; IconFilename: "{app}\icons\terminal.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\WorkPracticeLauncher.exe"; Description: "Запустить лаунчер"; Flags: nowait postinstall skipifsilent