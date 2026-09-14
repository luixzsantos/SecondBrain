@echo off
REM stop.bat
REM Encerra a API e derruba o PostgreSQL do Docker (se estiver rodando por ele).
REM So dar 2 cliques neste arquivo.

cd /d "%~dp0"

echo ==============================================
echo   Encerrando processo na porta 5080 (API)...
echo ==============================================
for /f "tokens=5" %%p in ('netstat -ano ^| findstr :5080 ^| findstr LISTENING') do (
    taskkill /PID %%p /F >nul 2>&1
)

echo ==============================================
echo   Derrubando o PostgreSQL (docker compose down)...
echo ==============================================
docker compose down

echo.
echo ==============================================
echo   Tudo encerrado.
echo   (Se o Postgres estava rodando pelo servico nativo em vez do Docker,
echo    ele continua no ar - "net stop postgresql-x64-16" pra parar tambem.)
echo ==============================================
echo.
pause
