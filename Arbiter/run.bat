@echo off
echo Building RCC Arbiter...
dotnet build

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful! Running arbiter...
    echo.
    dotnet run
) else (
    echo.
    echo Build failed!
    pause
)
