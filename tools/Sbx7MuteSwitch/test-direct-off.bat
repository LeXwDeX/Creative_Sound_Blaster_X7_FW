@echo off
setlocal
cd /d "%~dp0bin\Release\net472"
if not exist Sbx7MuteSwitch.exe (
  echo Sbx7MuteSwitch.exe not found.
  pause
  exit /b 1
)
echo Keep the amplifier or headphone volume LOW for this first test.
echo This will: mute - wait 1s - turn Direct Mode OFF - wait 1s - restore mute.
echo.
pause
Sbx7MuteSwitch.exe --mode direct --value off --hold-ms 1000
echo.
echo Did the pop disappear, stay the same, or change?
echo Please report the result.
pause