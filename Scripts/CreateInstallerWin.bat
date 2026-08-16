@echo off

set "target=win-x64"

set "ROOT_DIR=%~dp0..\"
set "publishDir=%ROOT_DIR%.publish"

call "%~dp0CreateInstallerCommon.bat" %target%

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
if exist "%ROOT_DIR%installer\win-unpacked" ( rmdir /S /Q "%ROOT_DIR%installer\win-unpacked" )

call yarn --cwd "%ROOT_DIR%SQLSchemaCompare" dist-%target%

if ERRORLEVEL 1 goto:error

REM Cleanup folders
if exist "%publishDir%" ( rmdir /S /Q "%publishDir%" )
if exist "%ROOT_DIR%installer\win-unpacked" ( rmdir /S /Q "%ROOT_DIR%installer\win-unpacked" )

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
