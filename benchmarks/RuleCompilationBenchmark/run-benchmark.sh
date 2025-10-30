#!/bin/bash

echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║    CSLY Parser - Rule Compilation Benchmark Runner              ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Please install .NET 8.0 SDK or later."
    exit 1
fi

echo "Building benchmark project in Release mode..."
echo ""
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Build failed. Please check the error messages above."
    exit 1
fi

echo ""
echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║                    Running Benchmarks                            ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "This may take several minutes. Please be patient..."
echo ""

dotnet run -c Release

echo ""
echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║                    Benchmark Complete!                           ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "Check the BenchmarkDotNet.Artifacts folder for detailed results."
echo ""

