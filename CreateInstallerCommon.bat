@echo off

REM Bring dev tools into the PATH.
set "VsDevCmdPath=C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat"
if not exist "%VsDevCmdPath%" (
  set "VsDevCmdPath=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"
)
if not exist "%VsDevCmdPath%" (
  set "VsDevCmdPath=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\Common7\Tools\VsDevCmd.bat"
)
if not exist "%VsDevCmdPath%" (
  echo Unable to find VsDevCmd.bat
  echo.
  echo Press any key to close...
  pause > nul
  exit
)

call "%VsDevCmdPath%"

set "targetdotnet=%1"
if /i "%1" == "" ( set "targetdotnet=win-x64" )
set "configuration=release"
REM Disable node reuse. Don't leave MSBuild.exe processes hanging around locking files after the build completes
set MSBUILDDISABLENODEREUSE=1

rd /Q /S %~dp0\SQLSchemaCompare\bin
rd /Q /S %~dp0\SQLSchemaCompare\obj
rd /Q /S %~dp0\SQLSchemaCompare.Core\bin
rd /Q /S %~dp0\SQLSchemaCompare.Core\obj
rd /Q /S %~dp0\SQLSchemaCompare.Infrastructure\bin
rd /Q /S %~dp0\SQLSchemaCompare.Infrastructure\obj
rd /Q /S %~dp0\SQLSchemaCompare.Services\bin
rd /Q /S %~dp0\SQLSchemaCompare.Services\obj
rd /Q /S %~dp0\SQLSchemaCompare.Test\bin
rd /Q /S %~dp0\SQLSchemaCompare.Test\obj
rd /Q /S %~dp0\SQLSchemaCompare.UI\bin
rd /Q /S %~dp0\SQLSchemaCompare.UI\obj

REM Cleanup solution
msbuild %~dp0\SQLSchemaCompare.sln /t:Clean /p:Configuration=%configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo     ___________________________
echo    /\                           \
echo    \_^|        Building         ^|
echo      ^|    SQLSchemaCompare     ^|
echo      ^|  _______________________^|_
echo       \_/_________________________/
echo.

dotnet restore -r %targetdotnet%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

msbuild %~dp0\SQLSchemaCompare.sln /p:Configuration=%configuration%;RuntimeIdentifier=%targetdotnet%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

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
