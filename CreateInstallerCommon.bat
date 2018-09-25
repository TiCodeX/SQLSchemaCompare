@echo off

REM Bring dev tools into the PATH.
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"

set "targetdotnet=%1"
set "configuration=release"
set "publishDir=%~dp0\.publish"

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )

REM Clean solution
msbuild %~dp0\SQLCompare.sln /t:Clean /p:Configuration=Release
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|     Building      ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

msbuild %~dp0\SQLCompare /p:Configuration=Release
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Publishing     ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

dotnet publish %~dp0\SQLCompare.UI\SQLCompare.UI.csproj -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

REM Clean debug files
del /Q %publishDir%\*.pdb
del /Q %publishDir%\*.xml
del /Q %publishDir%\Mapping.txt
del /Q %publishDir%\web.config
del /Q %publishDir%\sosdocsunix.txt
del /Q %publishDir%\createdump
REM TODO: remove other unnecessary files

REM Workaround to avoid VisualStudio complaining about published for a different target
dotnet restore
