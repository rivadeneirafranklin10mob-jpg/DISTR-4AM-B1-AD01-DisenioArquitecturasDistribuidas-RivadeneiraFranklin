@echo off
title AD01 - Microservicios Comercio Multibodega - Franklin Rivadeneira (4AM)
echo ============================================================================
echo   INSTITUTO SUPERIOR TECNOLOGICO CORDILLERA - APLICACIONES DISTRIBUIDAS
echo   Estudiante: Franklin Rivadeneira (4AM) - Actividad Docente AD01
echo ============================================================================
echo.
echo Iniciando los 3 microservicios independientes:
echo   1. Catalogo.Api   -> http://localhost:5101  (Base de datos: CatalogoDB)
echo   2. Inventario.Api -> http://localhost:5102  (Base de datos: InventarioDB)
echo   3. Pedidos.Api    -> http://localhost:5103  (Base de datos: PedidosDB)
echo.

start "1. Catalogo.Api (Puerto 5101)" cmd /k "cd /d %~dp0Catalogo.Api && dotnet run --urls http://localhost:5101"
timeout /t 2 /nobreak >nul

start "2. Inventario.Api (Puerto 5102)" cmd /k "cd /d %~dp0Inventario.Api && dotnet run --urls http://localhost:5102"
timeout /t 2 /nobreak >nul

start "3. Pedidos.Api (Puerto 5103)" cmd /k "cd /d %~dp0Pedidos.Api && dotnet run --urls http://localhost:5103"

echo.
echo Todos los microservicios han sido lanzados en sus propias ventanas.
pause
