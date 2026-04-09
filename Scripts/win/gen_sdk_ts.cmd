@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%..\..\"
set "SPEC_FILE=%~1"
if "%SPEC_FILE%"=="" set "SPEC_FILE=%REPO_ROOT%openapi.json"
set "OUT_DIR=%REPO_ROOT%GeneratedClients\ts-fetch"

echo Generating TypeScript SDK from %SPEC_FILE%...
if exist "%OUT_DIR%" rmdir /s /q "%OUT_DIR%"
openapi-generator-cli generate ^
  -i "%SPEC_FILE%" ^
  -g typescript-fetch ^
  -o "%OUT_DIR%"

if errorlevel 1 exit /b 1
echo.
echo [OK] TypeScript SDK generated in: %OUT_DIR%
exit /b 0
