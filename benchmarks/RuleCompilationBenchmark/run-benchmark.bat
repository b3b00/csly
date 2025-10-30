@echo off
echo ╔══════════════════════════════════════════════════════════════════╗
echo ║    CSLY Parser - Rule Compilation Benchmark Runner              ║
echo ╚══════════════════════════════════════════════════════════════════╝
echo.

REM Check if .NET SDK is installed
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found. Please install .NET 8.0 SDK or later.
    pause
    exit /b 1
)

echo Building benchmark project in Release mode...
echo.
dotnet build -c Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed. Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo ╔══════════════════════════════════════════════════════════════════╗
echo ║                    Running Benchmarks                            ║
echo ╚══════════════════════════════════════════════════════════════════╝
echo.
echo This may take several minutes. Please be patient...
echo.

dotnet run -c Release

echo.
echo ╔══════════════════════════════════════════════════════════════════╗
echo ║                    Benchmark Complete!                           ║
echo ╚══════════════════════════════════════════════════════════════════╝
echo.
echo Check the BenchmarkDotNet.Artifacts folder for detailed results.
echo.
pause

