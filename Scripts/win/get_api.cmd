@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%..\..\"
set "URL=%~1"
if "%URL%"=="" set "URL=http://localhost:14000/openapi/v1/openapi.json"
set "OUT_FILE=%REPO_ROOT%openapi.json"

echo Downloading OpenAPI specification from %URL%...

curl --fail --show-error --silent ^
  -H "Accept: application/json" ^
  -H "Cache-Control: no-cache" ^
  -H "Pragma: no-cache" ^
  "%URL%" > "%OUT_FILE%"

if errorlevel 1 (
  echo Error: Failed to download OpenAPI spec. Is the server running on port 14000?
  exit /b 1
)

echo.
echo [OK] Saved to: %OUT_FILE%
for %%A in ("%OUT_FILE%") do echo File size: %%~zA bytes
exit /b 0
