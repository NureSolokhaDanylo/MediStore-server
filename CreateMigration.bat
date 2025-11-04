@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Script to create EF Core migration with timestamped name
REM Usage: double-click or run from solution root

pushd "%~dp0"

REM Get reliable timestamp via PowerShell: yyyyMMdd_HHmmss
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set TS=%%i

set MIGRATIONS_PROJ=Infrastructure\Infrastructure.csproj
set STARTUP_PROJ=%MIGRATIONS_PROJ%
set CONTEXT=Infrastructure.AppDbContext
set NAME=Mig_%TS%

echo Creating migration %NAME% ...
 dotnet ef migrations add %NAME% --project "%MIGRATIONS_PROJ%" --startup-project "%STARTUP_PROJ%" --context %CONTEXT% -v
if errorlevel 1 goto :error

echo Migration created: %NAME%
echo Next: run UpdateMigration.bat to apply it to DB.
echo.
echo Press any key to close this window...
pause >nul

popd
exit /b 0

:error
echo.
echo ERROR: Failed to create migration

echo Press any key to close this window...
pause >nul
popd
exit /b 1
