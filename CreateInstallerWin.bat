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

call yarn --cwd SQLSchemaCompare dist-%target%

if ERRORLEVEL 1 goto:error

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
if exist %~dp0\installer\win-unpacked ( rmdir /S /Q %~dp0\installer\win-unpacked )

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
echo Press any key to close...
pause > nul
