@echo off
REM sync-obsidian.bat
REM Importa os verbetes do Segundo Cerebro (vault Obsidian) para o SecondBrain.
REM Precisa da API no ar - rode start.bat primeiro.

cd /d "%~dp0"

echo ==============================================
echo   Importando conhecimento do Obsidian...
echo ==============================================
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\sync-obsidian.ps1"

echo.
pause
