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

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|   Code Signing    ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

REM Set environment variables used in electron builder
set CSC_LINK=../codesigningcert.pfx
set CSC_KEY_PASSWORD=test1234

signtool sign /f codesigningcert.pfx /p test1234 /t  http://timestamp.verisign.com/scripts/timstamp.dll /d "TiCodeX SA application" %publishDir%\SQLCompare.Core.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f codesigningcert.pfx /p test1234 /t  http://timestamp.verisign.com/scripts/timstamp.dll /d "TiCodeX SA application" %publishDir%\SQLCompare.Infrastructure.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f codesigningcert.pfx /p test1234 /t  http://timestamp.verisign.com/scripts/timstamp.dll /d "TiCodeX SA application" %publishDir%\SQLCompare.Services.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f codesigningcert.pfx /p test1234 /t  http://timestamp.verisign.com/scripts/timstamp.dll /d "TiCodeX SA application" %publishDir%\SQLCompare.UI.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f codesigningcert.pfx /p test1234 /t  http://timestamp.verisign.com/scripts/timstamp.dll /d "TiCodeX SA application" %publishDir%\SQLCompare.UI.exe
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f codesigningcert.pfx /p test1234 /t  http://timestamp.verisign.com/scripts/timstamp.dll /d "TiCodeX SA application" %publishDir%\SQLCompare.Ui.Views.dll
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
