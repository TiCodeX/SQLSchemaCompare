@echo off
set filter=
set RunDockerTests=

Choice /M "Run integration tests"

If %ERRORLEVEL% == 2 (
    set "filter=--filter Category!=IntegrationTest"
    goto:start
)

Choice /M "Include docker tests"

If %ERRORLEVEL% == 1 (
    set RunDockerTests=true
)

:start

REM dotnet restore -r win-x64
REM if ERRORLEVEL 1 goto:exit

dotnet build SQLSchemaCompare.Test --configuration release
if ERRORLEVEL 1 goto:exit

dotnet test SQLSchemaCompare.Test --no-build --configuration release %filter%

:exit
echo Press any key to close...
pause > nul
