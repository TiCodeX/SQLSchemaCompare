@echo off

Choice /M "Perform build"

If %ERRORLEVEL% == 1 ( set "build=true" )

Choice /M "Run integration tests"

If %ERRORLEVEL% == 2 ( set "filter=--filter Category!=IntegrationTest" )

if "%build%" == "true" (
    REM Bring dev tools into the PATH.
    call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
    REM Disable node reuse. Don't leave MSBuild.exe processes hanging around locking files after the build completes
    set MSBUILDDISABLENODEREUSE=1

    echo.
    echo     _____________________
    echo    /\                    \  
    echo    \_^|     Building      ^|  
    echo      ^|    SqlCompare     ^|  
    echo      ^|  _________________^|_ 
    echo       \_/___________________/
    echo.

    dotnet restore -r win-x64
    if ERRORLEVEL 1 goto:exit

    msbuild %~dp0\SQLCompare.sln /p:Configuration=Release
    if ERRORLEVEL 1 goto:exit
)

dotnet test --no-build --configuration release %filter% --collect:coverage %~dp0\SQLCompare.Test\SQLCompare.Test.csproj

:exit
echo Press any key to close...
pause > nul
