@echo off
set "target=win"
REM set "ArrayTarget[win]=true"
REM set "ArrayTarget[linux]=true"
REM set "ArrayTarget[mac]=true"
REM :choosetarget
REM set /p target="Enter target [win/linux/mac]: "
REM if not defined ArrayTarget[%target%] ( goto:choosetarget )

set "publishDir=%~dp0.publish"
set "configuration=release"
REM set "ArrayConfig[release]=true"
REM set "ArrayConfig[debug]=true"
REM :chooseconfig
REM set /p configuration="Enter configuration [release/debug]: "
REM if not defined ArrayConfig[%configuration%] ( goto:chooseconfig )

if /i "%target%" == "win" ( set "targetdotnet=win-x64" )
if /i "%target%" == "linux" ( set "targetdotnet=linux-x64" )
if /i "%target%" == "mac" ( set "targetdotnet=osx-x64" )

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
if exist %~dp0installer\win-unpacked ( rmdir /S /Q %~dp0installer\win-unpacked )
if exist %~dp0installer\linux-unpacked ( rmdir /S /Q %~dp0installer\linux-unpacked )
if exist %~dp0installer\mac-unpacked ( rmdir /S /Q %~dp0installer\mac-unpacked )

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Publishing     ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo      \_/___________________/
echo.

dotnet msbuild %~dp0SQLCompare.UI\SQLCompare.UI.csproj /t:WebCompile
dotnet msbuild %~dp0SQLCompare.UI\SQLCompare.UI.csproj /t:BundleMinify

dotnet publish %~dp0SQLCompare.UI\SQLCompare.UI.csproj -f netcoreapp2.1 -r %targetdotnet% -c %configuration%

if ERRORLEVEL 1 goto:error

if "%configuration%" == "release" ( del %publishDir%\*.pdb )
REM TODO: remove other unnecessary files

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Packaging      ^|  
echo      ^|    electron       ^|  
echo      ^|  _________________^|_ 
echo      \_/___________________/
echo.

call npm --prefix SqlCompare run dist-%target%

if ERRORLEVEL 1 goto:error

REM Cleanup folders
if exist %publishDir% ( rmdir /S /Q %publishDir% )
REM if exist %~dp0installer\win-unpacked ( rmdir /S /Q %~dp0installer\win-unpacked )
REM if exist %~dp0installer\linux-unpacked ( rmdir /S /Q %~dp0installer\linux-unpacked )
REM if exist %~dp0installer\mac-unpacked ( rmdir /S /Q %~dp0installer\mac-unpacked )

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
