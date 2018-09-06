@echo off

set "targetdotnet=win-x64"
set "publishDir=%~dp0\.publish"
set "configuration=release"
set "electronScript=dist-win"

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
if exist %~dp0\installer\win-unpacked ( rmdir /S /Q %~dp0\installer\win-unpacked )

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Publishing     ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

dotnet publish %~dp0\SQLCompare.UI\SQLCompare.UI.csproj -r %targetdotnet% -c %configuration%

if ERRORLEVEL 1 goto:error

if "%configuration%" == "release" ( del %publishDir%\*.pdb )
REM TODO: remove other unnecessary files

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Packaging      ^|  
echo      ^|    electron       ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

call npm --prefix SqlCompare run %electronScript%

if ERRORLEVEL 1 goto:error

for /f "tokens=1-2" %%a in (%~dp0\installer\latest.yml) do (
    set "version=%%b"
    goto:versionfound
)
:versionfound
copy /Y %~dp0\installer\latest.yml %~dp0\installer\latest-%version%.yml

if ERRORLEVEL 1 goto:error

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
REM if exist %~dp0installer\win-unpacked ( rmdir /S /Q %~dp0installer\win-unpacked )

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
