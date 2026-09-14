@echo off
REM start.bat
REM Sobe o PostgreSQL (Docker, com fallback pro servico nativo), aplica as
REM migrations pendentes e abre a API numa janela nova.

cd /d "%~dp0"

echo ==============================================
echo   Subindo PostgreSQL (Docker)...
echo ==============================================
docker compose up -d postgres

if errorlevel 1 (
    echo.
    echo [AVISO] Docker nao respondeu. Tentando o servico nativo do PostgreSQL...
    net start postgresql-x64-16 >nul 2>&1
    echo.
)

echo ==============================================
echo   Aplicando migrations...
echo ==============================================
dotnet ef database update --project src\SecondBrain.Infrastructure --startup-project src\SecondBrain.API

if errorlevel 1 (
    echo.
    echo [ERRO] Nao foi possivel aplicar as migrations.
    echo Verifique se o PostgreSQL esta no ar ^(Docker ou o servico nativo "postgresql-x64-16"^).
    echo.
    pause
    exit /b 1
)

echo.
echo ==============================================
echo   Abrindo a API numa janela nova...
echo ==============================================
start "SecondBrain - API" cmd /k "cd /d "%~dp0" && dotnet run --project src\SecondBrain.API"

timeout /t 4 /nobreak >nul

echo ==============================================
echo   Abrindo o Swagger...
echo ==============================================
start "" "http://localhost:5080/swagger"

echo.
echo ==============================================
echo   Tudo no ar!
echo   API:     http://localhost:5080
echo   Swagger: http://localhost:5080/swagger
echo ==============================================
echo.
pause
