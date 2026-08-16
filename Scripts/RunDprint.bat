@ECHO OFF
PUSHD "%~dp0\.."
SET "solutiondir=%CD%"
POPD

REM Capitalize file path for vs code
SET filepath=%1
SET "c1=%filepath:~0,1%"
FOR %%s IN (A B C) DO CALL SET "c1=%%c1:%%s=%%s%%"
SET filepath="%c1%%filepath:~1%"

REM Get current file directory
FOR /F "delims=" %%i IN (%filepath%) DO SET dirname="%%~dpi"

CD %dirname%

:loop

REM Stop if we are in the solution folder
IF %CD% EQU %solutiondir% (
   EXIT
)

IF EXIST .dprint.jsonc (
   yarn dprint fmt %filepath%
   EXIT
) ELSE (
   REM Search a dprint config on the parent folder
   CD ..
   GOTO loop
)
