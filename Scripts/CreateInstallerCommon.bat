@echo off

set "targetdotnet=%1"
if /i "%1" == "" ( set "targetdotnet=win-x64" )
set "configuration=Release"

set "ROOT_DIR=%~dp0..\"

REM Disable node reuse. Don't leave MSBuild.exe processes hanging around locking files after the build completes
set MSBUILDDISABLENODEREUSE=1

rd /Q /S "%ROOT_DIR%SQLSchemaCompare\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare\obj"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Core\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Core\obj"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Infrastructure\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Infrastructure\obj"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Services\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Services\obj"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Test\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.Test\obj"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.CLI\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.CLI\obj"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.UI\bin"
rd /Q /S "%ROOT_DIR%SQLSchemaCompare.UI\obj"

pushd "%ROOT_DIR%"

REM Cleanup solution
dotnet clean -c %configuration%
if ERRORLEVEL 1 ( popd & exit /b %ERRORLEVEL% )

echo.
echo     ___________________________
echo    /\                           \
echo    \_^|        Building         ^|
echo      ^|    SQLSchemaCompare     ^|
echo      ^|  _______________________^|_
echo       \_/_________________________/
echo.

dotnet restore --locked-mode
if ERRORLEVEL 1 ( popd & exit /b %ERRORLEVEL% )

dotnet build --no-restore SQLSchemaCompare.UI -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 ( popd & exit /b %ERRORLEVEL% )

echo.
echo     ____________________________
echo    /\                           \
echo    \_^|       Publishing        ^|
echo      ^|    SQLSchemaCompare     ^|
echo      ^|  _______________________^|_
echo       \_/_________________________/
echo.

dotnet publish --no-build --no-restore SQLSchemaCompare.UI -r %targetdotnet% -c %configuration%
if ERRORLEVEL 1 ( popd & exit /b %ERRORLEVEL% )

popd
