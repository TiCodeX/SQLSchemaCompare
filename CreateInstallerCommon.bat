@echo off

set "targetdotnet=%1"
if /i "%1" == "" ( set "targetdotnet=win-x64" )
set "configuration=Release"
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
rd /Q /S %~dp0\SQLSchemaCompare.CLI\bin
rd /Q /S %~dp0\SQLSchemaCompare.CLI\obj
rd /Q /S %~dp0\SQLSchemaCompare.UI\bin
rd /Q /S %~dp0\SQLSchemaCompare.UI\obj

REM Cleanup solution
dotnet clean -c %configuration%
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

dotnet build --no-restore SQLSchemaCompare.UI -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo     ____________________________
echo    /\                           \
echo    \_^|       Publishing        ^|
echo      ^|    SQLSchemaCompare     ^|
echo      ^|  _______________________^|_
echo       \_/_________________________/
echo.

dotnet publish --no-build --no-restore SQLSchemaCompare.UI -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
