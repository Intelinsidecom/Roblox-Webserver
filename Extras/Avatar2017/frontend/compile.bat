@echo off
echo Running npm run build...
call npm run build

if %errorlevel% neq 0 (
    echo Build failed with error code %errorlevel%.
    exit /b %errorlevel%
)

echo Build completed successfully. Starting the application...
call npm run start