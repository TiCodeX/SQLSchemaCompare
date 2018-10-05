@echo off

REM Bring dev tools into the PATH.
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"

set "targetdotnet=%1"
if /i "%1" == "" ( set "targetdotnet=win-x64" )
set "configuration=release"
set "publishDir=%~dp0\.publish"
set "certFile=TiCodeXCodeSigningCertificate.p12"
set "certPass=test1234"
set "certDesc=TiCodeX SA application"
set "timeUrl=http://timestamp.comodoca.com"
REM Disable node reuse. Don't leave MSBuild.exe processes hanging around locking files after the build completes
set MSBUILDDISABLENODEREUSE=1

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )

REM Cleanup solution
msbuild %~dp0\SQLCompare.sln /t:Clean /p:Configuration=Release
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

REM Cleanup generated javascript
del /Q %~dp0\SQLCompare\app.js
del /Q %~dp0\SQLCompare\app.min.js
del /Q /S %~dp0\SQLCompare.UI\wwwroot\js\*.js

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|     Building      ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

dotnet restore -r %targetdotnet%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

msbuild %~dp0\SQLCompare.sln /p:Configuration=Release
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

REM Check generated files
if not exist %~dp0\SQLCompare\app.min.js (
  echo ERROR: app.min.js not generated
  exit /b 999
)
if not exist %~dp0\SQLCompare.UI\wwwroot\js\Index.min.js (
  echo ERROR: Index.min.js not generated
  exit /b 999
)
if not exist %~dp0\SQLCompare.UI\wwwroot\js\Login.min.js (
  echo ERROR: Login.min.js not generated
  exit /b 999
)

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Publishing     ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

dotnet publish --no-build --no-restore %~dp0\SQLCompare.UI\SQLCompare.UI.csproj -r %targetdotnet% -c %configuration%
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
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Core.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Infrastructure.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Infrastructure.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Services.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.Services.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.Views.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.Views.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
if exist %publishDir%\SQLCompare.UI.exe (
  signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.exe
  signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\SQLCompare.UI.exe
  if ERRORLEVEL 1 exit /b %ERRORLEVEL%
)

REM Clean debug files
del /Q %publishDir%\*.pdb
del /Q %publishDir%\*.xml
del /Q %publishDir%\Mapping.txt
del /Q %publishDir%\web.config
del /Q %publishDir%\createdump
REM TODO: remove other unnecessary files
