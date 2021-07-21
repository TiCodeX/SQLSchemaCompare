@echo off

set "target=osx-x64"
set "publishDir=%~dp0\.publish"
set "electronDir=%~dp0\SQLSchemaCompare"
set "remoteIp=10.0.10.205"
set "remoteUser=neolution"
set "remotePass=Neo-1234."
set "remoteDir=/private/tmp/sqlschemacompare-build"

call CreateInstallerCommon.bat %target%

if ERRORLEVEL 1 goto:error

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Preparing      ^|  
echo      ^|       OSX         ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

REM Cleanup folders
plink -pw %remotePass% %remoteUser%@%remoteIp% (^
    rm -rf %remoteDir%;^
    mkdir %remoteDir%;^
)

if ERRORLEVEL 1 goto:error

REM Copy files to OSX
pscp -pw %remotePass% -r %publishDir% %remoteUser%@%remoteIp%:%remoteDir%
pscp -pw %remotePass% -r %electronDir% %remoteUser%@%remoteIp%:%remoteDir%

if ERRORLEVEL 1 goto:error

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Packaging      ^|  
echo      ^|    electron       ^|  
echo      ^|       OSX         ^|  
echo      ^|  _________________^|_ 
echo       \_/___________________/
echo.

REM Create installer
plink -pw %remotePass% %remoteUser%@%remoteIp% (^
   export PATH=/usr/local/bin:$PATH;^
   cd %remoteDir%/SQLSchemaCompare;^
   chmod -R a+rwx ./node_modules;^
   chmod +x ../.publish/TiCodeX.SQLSchemaCompare.UI;^
   npm run dist-%target%;^
)

if ERRORLEVEL 1 goto:error

REM Copy generated files to local
mkdir %~dp0\installer
pscp -pw %remotePass% %remoteUser%@%remoteIp%:%remoteDir%/installer/*.zip %~dp0\installer\
pscp -pw %remotePass% %remoteUser%@%remoteIp%:%remoteDir%/installer/*.dmg %~dp0\installer\
pscp -pw %remotePass% %remoteUser%@%remoteIp%:%remoteDir%/installer/*.blockmap %~dp0\installer\
pscp -pw %remotePass% %remoteUser%@%remoteIp%:%remoteDir%/installer/*.yml %~dp0\installer\

if ERRORLEVEL 1 goto:error

REM Cleanup folders
plink -pw %remotePass% %remoteUser%@%remoteIp% rm -rf %remoteDir%
if exist %publishDir% ( rmdir /S /Q %publishDir% )

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
echo Press any key to close...
pause > nul
