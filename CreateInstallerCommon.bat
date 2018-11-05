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
msbuild %~dp0\SQLSchemaCompare.sln /t:Clean /p:Configuration=Release
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

REM Cleanup generated javascript
del /Q %~dp0\SQLSchemaCompare\app.js
del /Q %~dp0\SQLSchemaCompare\app.min.js
del /Q /S %~dp0\SQLSchemaCompare.UI\wwwroot\js\*.js

REM Cleanup node_modules
rd /Q /S %~dp0\SQLSchemaCompare\node_modules

echo.
echo     ___________________________
echo    /\                           \  
echo    \_^|        Building         ^|  
echo      ^|    SQLSchemaCompare     ^|  
echo      ^|  _______________________^|_ 
echo       \_/_________________________/
echo.

pushd %~dp0\SQLSchemaCompare
call npm install
popd

dotnet restore -r %targetdotnet%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

msbuild %~dp0\SQLSchemaCompare.sln /p:Configuration=Release
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

REM Check generated files
if not exist %~dp0\SQLSchemaCompare\app.min.js (
  echo ERROR: app.min.js not generated
  exit /b 999
)
if not exist %~dp0\SQLSchemaCompare.UI\wwwroot\js\Index.min.js (
  echo ERROR: Index.min.js not generated
  exit /b 999
)
if not exist %~dp0\SQLSchemaCompare.UI\wwwroot\js\Login.min.js (
  echo ERROR: Login.min.js not generated
  exit /b 999
)

echo.
echo     ____________________________
echo    /\                           \  
echo    \_^|       Publishing        ^|  
echo      ^|    SQLSchemaCompare     ^|  
echo      ^|  _______________________^|_ 
echo       \_/_________________________/
echo.

dotnet publish --no-build --no-restore %~dp0\SQLSchemaCompare.UI\SQLSchemaCompare.UI.csproj -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo     ____________________________
echo    /\                           \  
echo    \_^|      Code Signing       ^|  
echo      ^|    SQLSchemaCompare     ^|  
echo      ^|  _______________________^|_ 
echo       \_/_________________________/
echo.

REM Set environment variables used in electron builder
set CSC_LINK=../%certFile%
set CSC_KEY_PASSWORD=%certPass%

signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.Core.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.Core.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.Infrastructure.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.Infrastructure.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.Services.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.Services.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.UI.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.UI.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.UI.Views.dll
signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.UI.Views.dll
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
if exist %publishDir%\TiCodeX.SQLSchemaCompare.UI.exe (
  signtool sign /f %certFile% /p %certPass% /t %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.UI.exe
  signtool sign /as /fd sha256 /f %certFile% /p %certPass% /tr %timeUrl% /d "%certDesc%" %publishDir%\TiCodeX.SQLSchemaCompare.UI.exe
  if ERRORLEVEL 1 exit /b %ERRORLEVEL%
)

REM Clean debug files
del /Q %publishDir%\*.pdb
del /Q %publishDir%\*.xml
del /Q %publishDir%\Mapping.txt
del /Q %publishDir%\web.config
REM TODO: remove other unnecessary files
