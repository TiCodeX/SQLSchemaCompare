@echo off

set "targetdotnet=%1"
set "configuration=release"
set "publishDir=%~dp0\.publish"

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Publishing     ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

dotnet publish %~dp0\SQLCompare.UI\SQLCompare.UI.csproj -r %targetdotnet% -c %configuration%

REM Clean debug files
del %publishDir%\*.pdb

REM TODO: remove other unnecessary files
