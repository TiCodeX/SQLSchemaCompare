@echo off

REM Bring dev tools into the PATH.
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"

set "targetdotnet=%1"
set "configuration=release"
set "publishDir=%~dp0\.publish"
set "certFile=TiCodeXCodeSigningCertificate.p12"
set "certPass=test1234"
set "certDesc=TiCodeX SA application"
set "timeUrl=http://timestamp.verisign.com/scripts/timstamp.dll"

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )

REM Clean solution
REM msbuild %~dp0\SQLCompare.sln /t:Clean /p:Configuration=Release
REM if ERRORLEVEL 1 exit /b %ERRORLEVEL%

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
set CSC_LINK=../%certFile%
set CSC_KEY_PASSWORD=%certPass%

signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Core.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Infrastructure.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Services.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.Views.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
if exist %publishDir%\SQLCompare.UI.exe (
  signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.exe
  if ERRORLEVEL 1 exit /b %ERRORLEVEL%
)

REM Clean debug files
del /Q %publishDir%\*.pdb
del /Q %publishDir%\*.xml
del /Q %publishDir%\Mapping.txt
del /Q %publishDir%\web.config
del /Q %publishDir%\createdump
REM TODO: remove other unnecessary files
