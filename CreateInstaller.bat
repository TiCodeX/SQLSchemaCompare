@echo off
set "ArrayTarget[win]=true"
set "ArrayTarget[linux]=true"
set "ArrayTarget[mac]=true"
:choosetarget
set /p target="Enter target [win/linux/mac]: "
if not defined ArrayTarget[%target%] ( goto:choosetarget )

set "configuration=release"
REM set "ArrayConfig[release]=true"
REM set "ArrayConfig[debug]=true"
REM :chooseconfig
REM set /p configuration="Enter configuration [release/debug]: "
REM if not defined ArrayConfig[%configuration%] ( goto:chooseconfig )

if /i "%target%" == "win" ( set "targetdotnet=win-x64" )
if /i "%target%" == "linux" ( set "targetdotnet=linux-x64" )
if /i "%target%" == "mac" ( set "targetdotnet=osx-x64" )

if exist %~dp0SQLCompare.UI\dist ( rmdir /S /Q %~dp0SQLCompare.UI\dist )

echo.
echo     _____________________
echo    /\                    \  
echo    \_^|    Publishing     ^|  
echo      ^|    SqlCompare     ^|  
echo      ^|  _________________^|_ 
echo      \_/___________________/
echo.

set PublishOutputPath="dist\%configuration%"

dotnet publish %~dp0SQLCompare.UI\SQLCompare.UI.csproj -f netcoreapp2.0 -o %PublishOutputPath% -r %targetdotnet% -c %configuration%

if ERRORLEVEL 1 goto:error

if "%configuration%" == "release" ( del %~dp0SQLCompare.UI\%PublishOutputPath%\*.pdb )
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

REM cleanup
if exist %~dp0SQLCompare.UI\dist ( rmdir /S /Q %~dp0SQLCompare.UI\dist )
if exist %~dp0SQLCompare\dist\win-unpacked ( rmdir /S /Q %~dp0SQLCompare\dist\win-unpacked )
if exist %~dp0SQLCompare\dist\linux-unpacked ( rmdir /S /Q %~dp0SQLCompare\dist\linux-unpacked )
if exist %~dp0SQLCompare\dist\mac-unpacked ( rmdir /S /Q %~dp0SQLCompare\dist\mac-unpacked )

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
