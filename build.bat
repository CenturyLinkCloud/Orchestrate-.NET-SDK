@echo off
setlocal EnableExtensions

set MSBUILD=%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe
if exist "%MSBUILD%" goto RunBuild

goto Error_NoMsBuild

:RunBuild

set TARGET=%1
if (%TARGET%)==() set TARGET=Test

set VERBOSITY=%2
if (%VERBOSITY%)==() set VERBOSITY=minimal

"%MSBUILD%" /verbosity:%VERBOSITY% /nologo /m /t:%TARGET% %~dp0build/build.proj

if errorlevel 1 goto Error_BuildFailed

echo.
echo *** BUILD SUCCESSFUL ***
echo.
goto :EOF

:Error_BuildFailed
echo.
echo *** BUILD FAILED ***
echo.
exit /b 1

:Error_NoMsBuild
echo.
echo. ERROR: Unable to locate MSBuild.exe (expected location: %MSBUILD%)
echo.
exit /b 1
