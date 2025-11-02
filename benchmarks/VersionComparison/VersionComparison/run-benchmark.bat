@echo off
echo ========================================
echo Building and running version comparison benchmark
echo ========================================
echo.

cd /d %~dp0

echo Building project...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

echo.
echo Running benchmark...
echo This may take several minutes...
echo.

dotnet run -c Release --no-build

echo.
echo ========================================
echo Benchmark complete!
echo Results are in: BenchmarkDotNet.Artifacts\results
echo ========================================
pause

