@echo off

set "target=osx-x64"
set "publishDir=%~dp0\.publish"
set "electronDir=%~dp0\SQLCompare"
set "remoteIp=192.168.247.128"
set "remoteUser=ticodex"
set "remotePass=test1234"
set "remoteDir=/private/tmp/sqlcompare-build"

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
pscp -pw %remotePass% %~dp0\%certFile% %remoteUser%@%remoteIp%:%remoteDir%

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
   cd %remoteDir%/SQLCompare;^
   chmod -R a+rwx ./node_modules;^
   chmod +x ../.publish/SQLCompare.UI;^
   export CSC_LINK=%remoteDir%/%certFile%;^
   export CSC_KEY_PASSWORD=%certPass%;^
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

REM Create backup of latest-mac.yml
for /f "tokens=1-2" %%a in (%~dp0\installer\latest-mac.yml) do (
    set "version=%%b"
    goto:versionfound
)
:versionfound
copy /Y %~dp0\installer\latest-mac.yml %~dp0\installer\latest-mac-%version%.yml

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
