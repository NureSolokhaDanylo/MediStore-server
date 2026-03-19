@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..\..") do set "REPO_ROOT=%%~fI"
set "ENV_PATH_FILE=%SCRIPT_DIR%..\path_to_env"

if not exist "%ENV_PATH_FILE%" (
  echo File not found: "%ENV_PATH_FILE%"
  exit /b 1
)

set /p ENV_PATH=<"%ENV_PATH_FILE%"

if "%ENV_PATH%"=="" (
  echo Scripts/path_to_env is empty. Put absolute path to dev.env there.
  exit /b 1
)

docker compose -f "%REPO_ROOT%\compose.yaml" --env-file "%ENV_PATH%" --project-name medistoreserver up --build
