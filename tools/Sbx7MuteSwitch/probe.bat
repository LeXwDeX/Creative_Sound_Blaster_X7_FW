@echo off
setlocal
cd /d "%~dp0bin\Release\net472"
if not exist Sbx7MuteSwitch.exe (
  echo Sbx7MuteSwitch.exe not found.
  echo Build the tool first, or use the packaged folder.
  pause
  exit /b 1
)
echo This only reads the device state. It does not change volume or firmware.
echo.
Sbx7MuteSwitch.exe --probe
echo.
echo If you see "Probe succeeded", continue with test-direct-off.bat.
pause