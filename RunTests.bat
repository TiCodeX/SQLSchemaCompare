@echo off

Choice /M "Perform build"

If Errorlevel 2 ( set "nobuild=--no-build" )

dotnet test %nobuild% --configuration release --filter Category!=IntegrationTest --collect:coverage .\SQLCompare.Test\SQLCompare.Test.csproj

:exit
echo Press any key to close...
pause > nul
