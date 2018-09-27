@echo off

set "target=linux-x64"
set "publishDir=%~dp0\.publish"

call CreateInstallerCommon.bat %target%

if ERRORLEVEL 1 goto:error

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Packaging      ^|  
echo      ^|    electron       ^|  
echo      ^|     DOCKER        ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

REM Cleanup folders
if exist %~dp0\installer\linux-unpacked ( rmdir /S /Q %~dp0\installer\linux-unpacked )

REM Create a volume for caching
docker volume create electron-cache

docker run --rm^
   -v electron-cache:/root/.cache^
   -v %~dp0:/project^
   -w /project/SQLCompare^
   --env ELECTRON_CACHE="/root/.cache/electron"^
   --env ELECTRON_BUILDER_CACHE="/root/.cache/electron-builder"^
   electronuserland/builder^
   npm run dist-%target%

if ERRORLEVEL 1 goto:error

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
if exist %~dp0\installer\linux-unpacked ( rmdir /S /Q %~dp0\installer\linux-unpacked )

if ERRORLEVEL 1 goto:error

REM Create latest-linux.yml
for /f "tokens=1,2" %%a in (%~dp0\SQLCompare\package.json) do (
    if %%a=="version": (
        set version=%%b
        goto:versionfound
    )
)
:versionfound
echo version: %version:~1,-2%> %~dp0\installer\latest-linux.yml

if ERRORLEVEL 1 goto:error

echo.
echo.
echo DONE.

REM processes done correctly
goto:exit

:error
echo.
echo.
echo FAILED.

:exit
REM Workaround to avoid VisualStudio complaining about published for a different target
dotnet restore

echo Press any key to close...
pause > nul
