cls

@ECHO OFF
set /p choice="Run with full logging(very long)? (Y, N) "
if "%choice%"=="y" goto full
if "%choice%"=="n" goto normal
@ECHO ON

:full
"Portrait Builder.exe" -logfull
goto exit

:normal
"Portrait Builder.exe" -log

:exit