# Benchmarks Directory

This directory contains performance benchmarking projects for CSLY parser optimizations.

## Available Benchmarks

### 1. RuleCompilationBenchmark
**Location**: `RuleCompilationBenchmark/`  
**Purpose**: Demonstrates performance improvements from rule compilation  
**Framework**: BenchmarkDotNet  
**Status**: ✅ Ready to run

**Quick Start**:
```bash
cd RuleCompilationBenchmark
dotnet run -c Release
```

See [RuleCompilationBenchmark/QUICKSTART.md](RuleCompilationBenchmark/QUICKSTART.md) for details.

## Adding New Benchmarks

To add a new benchmark project:

1. Create new directory: `benchmarks/YourBenchmark/`
2. Add `.csproj` with BenchmarkDotNet reference
3. Create benchmark classes with `[Benchmark]` attributes
4. Add README and documentation
5. Update this file

### Template Structure
```
YourBenchmark/
├── YourBenchmark.csproj
├── Program.cs
├── YourBenchmarks.cs
├── README.md
└── QUICKSTART.md
```

## Running All Benchmarks

```bash
# Windows
for /d %d in (*) do (cd %d && dotnet run -c Release && cd ..)

# Linux/macOS
for dir in */; do (cd "$dir" && dotnet run -c Release); done
```

## Best Practices

### 1. Always Use Release Mode
```bash
dotnet run -c Release
```
Debug mode will give misleading results.

### 2. Close Other Applications
For accurate measurements, minimize background processes.

### 3. Multiple Runs
Run benchmarks multiple times to verify consistency.

### 4. Document Results
Save benchmark results in the project for future comparison.

### 5. Version Control
Commit benchmark code but not result files (add to `.gitignore`).

## CI/CD Integration

Add to your CI pipeline:

```yaml
# .github/workflows/benchmarks.yml
name: Benchmarks
on: [push, pull_request]
jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Run Benchmarks
        run: |
          cd benchmarks/RuleCompilationBenchmark
          dotnet run -c Release -- --job short --exporters json
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: benchmark-results
          path: benchmarks/**/BenchmarkDotNet.Artifacts/
```

## Benchmark Results Archive

Store historical results for tracking performance over time:

```
benchmarks/
├── results/
│   ├── 2025-10-27-rule-compilation.html
│   ├── 2025-11-15-further-optimizations.html
│   └── ...
```

## Resources

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [CSLY Optimizations Guide](../OPTIMIZATIONS.md)
- [Advanced Optimizations](../ADVANCED_OPTIMIZATIONS.md)

## Contributing

When adding benchmarks:
- Follow existing structure and naming
- Include comprehensive documentation
- Ensure reproducibility
- Add meaningful scenarios
- Document expected results

## License

Same as CSLY project license.

