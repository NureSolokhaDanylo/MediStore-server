@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%..\..\"
set "URL=%~1"
if "%URL%"=="" set "URL=http://localhost:14000/openapi/v1/openapi.json"
set "OUT_DIR=%REPO_ROOT%OpenApiHistory"
set "OUT_FILE=%REPO_ROOT%openapi.json"
set "TMP_FILE=%TEMP%\medistore_openapi_%RANDOM%_%RANDOM%.json"

if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

set "LAST_FILE="
for /f "delims=" %%F in ('dir /b /a-d /o:n "%OUT_DIR%\openapi.v*.json" 2^>nul') do set "LAST_FILE=%%F"

set "NEXT_VERSION=1"
if defined LAST_FILE (
  set "LAST_VERSION=!LAST_FILE:openapi.v=!"
  set "LAST_VERSION=!LAST_VERSION:.json=!"
  set /a NEXT_VERSION=1!LAST_VERSION! - 10000 + 1
)

set "PADDED_VERSION=0000!NEXT_VERSION!"
set "PADDED_VERSION=!PADDED_VERSION:~-4!"
set "VERSION_FILE=%OUT_DIR%\openapi.v!PADDED_VERSION!.json"

echo Downloading OpenAPI specification from %URL%...

curl --fail --show-error --silent ^
  -H "Accept: application/json" ^
  -H "Cache-Control: no-cache" ^
  -H "Pragma: no-cache" ^
  "%URL%" > "%TMP_FILE%"

if errorlevel 1 (
  if exist "%TMP_FILE%" del /q "%TMP_FILE%"
  echo Error: Failed to download OpenAPI spec. Is the server running on port 14000?
  exit /b 1
)

copy /y "%TMP_FILE%" "%VERSION_FILE%" >nul
move /y "%TMP_FILE%" "%OUT_FILE%" >nul

echo.
echo [OK] Saved to: %OUT_FILE%
echo [OK] Archived snapshot: %VERSION_FILE%
for %%A in ("%OUT_FILE%") do echo File size: %%~zA bytes
exit /b 0
