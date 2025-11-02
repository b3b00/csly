#!/bin/bash
echo "========================================"
echo "Building and running version comparison benchmark"
echo "========================================"
echo

cd "$(dirname "$0")"

echo "Building project..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo
echo "Running benchmark..."
echo "This may take several minutes..."
echo

dotnet run -c Release --no-build

echo
echo "========================================"
echo "Benchmark complete!"
echo "Results are in: BenchmarkDotNet.Artifacts/results"
echo "========================================"

