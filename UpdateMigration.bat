@echo off
setlocal
pushd "%~dp0"

set MIGRATIONS_PROJ=Infrastructure\Infrastructure.csproj
set STARTUP_PROJ=%MIGRATIONS_PROJ%
set CONTEXT=Infrastructure.AppDbContext

echo Applying migrations...
dotnet ef database update --project "%MIGRATIONS_PROJ%" --startup-project "%STARTUP_PROJ%" --context %CONTEXT% -v

if errorlevel 1 (
    echo.
    echo ERROR: Failed to update database
) else (
    echo.
    echo Database updated successfully.
)

echo.
echo Press any key to close this window...
pause >nul

popd
exit /b
