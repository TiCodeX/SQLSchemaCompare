@echo off

set "target=win-x64"
set "publishDir=%~dp0\.publish"

call CreateInstallerCommon.bat %target%

if ERRORLEVEL 1 goto:error

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Packaging      ^|  
echo      ^|    electron       ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

REM Cleanup folders
if exist %~dp0\installer\win-unpacked ( rmdir /S /Q %~dp0\installer\win-unpacked )

call npm --prefix SqlCompare run dist-%target%

if ERRORLEVEL 1 goto:error

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
if exist %~dp0\installer\win-unpacked ( rmdir /S /Q %~dp0\installer\win-unpacked )

if ERRORLEVEL 1 goto:error

REM Create backup of latest.yml
for /f "tokens=1-2" %%a in (%~dp0\installer\latest.yml) do (
    set "version=%%b"
    goto:versionfound
)
:versionfound
copy /Y %~dp0\installer\latest.yml %~dp0\installer\latest-%version%.yml

if ERRORLEVEL 1 goto:error

echo.
echo.
echo DONE.

REM processes done correctly
goto:exit

:error
echo.
echo.
echo FAILED.

:exit
REM Workaround to avoid VisualStudio complaining about published for a different target
dotnet restore

echo Press any key to close...
pause > nul
