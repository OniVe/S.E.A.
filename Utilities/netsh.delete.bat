@echo off
set port=9000

echo Administrative permissions required. Detecting permissions...
net session >nul 2>&1
if %errorLevel% == 0 (
	echo Success: Administrative permissions confirmed.
	netsh http delete urlacl http://+:%port%/
) else (
	echo Failure: Current permissions inadequate.
)
pause >nul