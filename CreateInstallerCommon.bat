@echo off

REM Bring dev tools into the PATH.
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"

set "targetdotnet=%1"
if /i "%1" == "" ( set "targetdotnet=win-x64" )
set "configuration=release"
REM Disable node reuse. Don't leave MSBuild.exe processes hanging around locking files after the build completes
set MSBUILDDISABLENODEREUSE=1
REM Set environment variables used in electron builder
set CSC_LINK=../TiCodeXCodeSigningCertificate.p12
set CSC_KEY_PASSWORD=test1234

REM Cleanup solution
msbuild %~dp0\SQLSchemaCompare.sln /t:Clean /p:Configuration=%configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

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

echo.
echo     ___________________________
echo    /\                           \  
echo    \_^|        Building         ^|  
echo      ^|    SQLSchemaCompare     ^|  
echo      ^|  _______________________^|_ 
echo       \_/_________________________/
echo.

pushd %~dp0\SQLSchemaCompare
rd /Q /S node_modules
call npm install
popd

dotnet restore -r %targetdotnet%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

msbuild %~dp0\SQLSchemaCompare.sln /p:Configuration=%configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%

echo.
echo     ____________________________
echo    /\                           \  
echo    \_^|       Publishing        ^|  
echo      ^|    SQLSchemaCompare     ^|  
echo      ^|  _______________________^|_ 
echo       \_/_________________________/
echo.

rem BUG https://github.com/dotnet/cli/issues/10310
rem Workaround:
mkdir %~dp0\SQLSchemaCompare.UI\obj\Release\netcoreapp2.2\win-x64\
copy /Y %~dp0\SQLSchemaCompare.UI\obj\Release\netcoreapp2.2\*.dll %~dp0\SQLSchemaCompare.UI\obj\Release\netcoreapp2.2\win-x64\
copy /Y %~dp0\SQLSchemaCompare.UI\obj\Release\netcoreapp2.2\*.pdb %~dp0\SQLSchemaCompare.UI\obj\Release\netcoreapp2.2\win-x64\

dotnet publish --no-build --no-restore %~dp0\SQLSchemaCompare.UI\SQLSchemaCompare.UI.csproj -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 exit /b %ERRORLEVEL%
