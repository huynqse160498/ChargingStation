@echo off
echo ========================================
echo   Starting Charging Station Demo
echo ========================================
echo.
echo Starting application...
dotnet run --project ChargingStationSystem
echo.
echo Application started! Opening browser...
timeout /t 3 /nobreak >nul
start http://localhost:5028/demo-websocket.html
echo.
echo Browser opened! Press Ctrl+C to stop the server.

