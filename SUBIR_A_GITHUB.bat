@echo off
title Subir Proyecto AD01 a GitHub - Franklin Rivadeneira
cd /d "%~dp0"
echo ============================================================================
echo   SUBIENDO PROYECTO A GITHUB:
echo   https://github.com/rivadeneirafranklin10mob-jpg/DISTR-4AM-B1-AD01-DisenioArquitecturasDistribuidas-RivadeneiraFranklin.git
echo ============================================================================
echo.
echo Si se abre una ventana de "GitHub Sign in", haz clic en "Sign in with your browser"
echo y autoriza el acceso en tu navegador donde ya tienes abierta tu cuenta.
echo.
git push -u origin main
echo.
echo ============================================================================
echo   PROCESO FINALIZADO. Revisa tu repositorio en GitHub.
echo ============================================================================
pause
