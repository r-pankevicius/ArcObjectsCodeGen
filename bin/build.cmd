@set solution=%~dp0%..\ArcGisCodeGen.sln

@REM Find MSBuild, see https://github.com/microsoft/vswhere/wiki/Find-MSBuild

@if not exist "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" (
	@echo vswhere doesn't exist at "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
	exit /b 1
)

@setlocal enabledelayedexpansion

@for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -products * -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
@	set msbuild=%%i
)

@if "%msbuild%" == "" (
	@echo Could not find MSBuild.exe using vswhere.
	exit /b 2
)

@echo Found MSBuild at %msbuild%

call NuGet restore "%solution%"
@if errorlevel 1  (
	@echo Failed to execute "NuGet restore". Make sure NuGet.exe is on path.
	exit /b 10
)

"%msbuild%" "%solution%" /p:Configuration=Debug;Platform="Any CPU" /verbosity:minimal
@if errorlevel 1  (
	@echo Error during build.
	exit /b 11
)

exit /b 0
