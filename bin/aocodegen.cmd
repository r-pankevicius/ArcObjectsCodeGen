@echo off
@set compiled_exe=%~dp0%..\src\ArcObjectsCodeGen\bin\Debug\ArcObjectsCodeGen.exe

@if not exist "%compiled_exe%" (
	@echo Could not find %compiled_exe%
	@echo Build it first.
	exit /b 100
)

%compiled_exe% %*