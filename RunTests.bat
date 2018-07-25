@echo off

Choice /M "Perform build"

If Errorlevel 2 ( set "nobuild=--no-build" )

Choice /M "Run integration tests"

If Errorlevel 2 ( set "filter=--filter Category!=IntegrationTest" )

dotnet test %nobuild% --configuration release %filter% --collect:coverage .\SQLCompare.Test\SQLCompare.Test.csproj

:exit
echo Press any key to close...
pause > nul
