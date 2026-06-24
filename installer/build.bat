@echo off
echo === Сборка WPF ===
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "C:\Users\Danila\source\repos\Work_Practice\Work_Practice\Work_Practice.csproj" /t:Build /p:Configuration=Release
if %errorlevel% neq 0 (
    echo Ошибка сборки WPF!
    pause
    exit /b 1
)

echo === Сборка Launcher ===
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "C:\Users\Danila\source\repos\Work_Practice\WorkPracticeLauncher\WorkPracticeLauncher.csproj" /t:Build /p:Configuration=Release
if %errorlevel% neq 0 (
    echo Ошибка сборки Launcher!
    pause
    exit /b 1
)

echo === Сборка установщика ===
"C:\Inno Setup 6\ISCC.exe" "C:\Users\Danila\source\repos\Work_Practice\installer\setup.iss"
if %errorlevel% neq 0 (
    echo Ошибка сборки установщика!
    pause
    exit /b 1
)

echo === Готово! ===
echo setup.exe: C:\Users\Danila\source\repos\Work_Practice\installer\setup.exe
pause
