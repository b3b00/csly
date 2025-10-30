# Quick Start Guide - Rule Compilation Benchmark

## Running the Benchmark (3 Simple Steps)

### Windows
```cmd
cd benchmarks\RuleCompilationBenchmark
run-benchmark.bat
```

### Linux/macOS
```bash
cd benchmarks/RuleCompilationBenchmark
chmod +x run-benchmark.sh
./run-benchmark.sh
```

### Manual
```bash
cd benchmarks/RuleCompilationBenchmark
dotnet run -c Release
```

## What to Expect

### Benchmark Duration
- **Total time**: 5-10 minutes
- **Per scenario**: 30-60 seconds
- **Warmup + actual runs**: Multiple iterations for accuracy

### Output Location
Results are saved in:
```
BenchmarkDotNet.Artifacts/results/
├── RuleCompilationBenchmarks-report.html
├── RuleCompilationBenchmarks-report.csv
└── RuleCompilationBenchmarks-report-github.md
```

## Understanding Results

### Example Output
```
|                          Method |      Mean | Ratio | Allocated |
|-------------------------------- |----------:|------:|----------:|
| Simple expressions WITH comp... |  45.23 μs |  0.33 |   5.21 KB |
| Simple expressions WITHOUT c... | 136.47 μs |  1.00 |   7.42 KB |
```

### Key Metrics

**Mean**: Average execution time (lower is better)
- `45.23 μs` = 45 microseconds

**Ratio**: Relative to baseline (lower is better)
- `0.33` = 3x faster than baseline
- `1.00` = baseline reference

**Allocated**: Total memory allocated (lower is better)
- `5.21 KB` per operation

### Performance Gains

| Scenario | Expected Speedup | Memory Reduction |
|----------|------------------|------------------|
| Simple expressions | **3x faster** | **30% less** |
| Medium expressions | **2x faster** | **25% less** |
| Complex expressions | **1.5x faster** | **20% less** |
| Repeated parses | **3x faster** | **30% less** |

## Quick Analysis

### Good Results ✅
- Ratio < 0.5 (2x faster or better)
- Allocated reduction > 20%
- Consistent low StdDev (< 5%)

### Expected Results ✓
- Ratio 0.5-0.7 (1.5-2x faster)
- Allocated reduction 10-20%
- StdDev < 10%

### Investigation Needed ⚠️
- Ratio > 0.8 (< 25% improvement)
- Allocated increase
- High StdDev (> 15%)

## Common Questions

### Q: Why is compilation slower for first parse?
**A**: Compilation has overhead, but it's amortized over repeated parses. See "1000 parses" benchmark.

### Q: Should I always enable compilation?
**A**: Enable for:
- Repeated parsing of same grammar
- Performance-critical applications
- High-frequency parsing

Don't enable for:
- One-time parsing
- Extremely dynamic grammars

### Q: How do I customize benchmarks?
**A**: Edit `RuleCompilationBenchmarks.cs` and add your own `[Benchmark]` methods.

### Q: Can I run specific benchmarks only?
**A**: Yes:
```bash
dotnet run -c Release -- --filter *Simple*
```

## Troubleshooting

### Issue: "Build failed"
**Solution**: 
- Ensure .NET 8.0 SDK is installed
- Check project references in `.csproj`
- Run `dotnet restore`

### Issue: "Parse error" during benchmark
**Solution**:
- Check parser grammar definition
- Verify test expressions are valid
- Enable debugging in benchmark code

### Issue: Unexpected slow results
**Solution**:
- Ensure running in Release mode
- Close other applications
- Check CPU throttling
- Run multiple times

### Issue: High memory usage
**Solution**:
- This is normal for benchmarks
- BenchmarkDotNet creates many iterations
- Memory is released after benchmark

## Next Steps

1. ✅ Run the benchmark
2. ✅ Review HTML report in `BenchmarkDotNet.Artifacts/results/`
3. ✅ Compare WITH vs WITHOUT compilation
4. ✅ Decide on compilation strategy for your use case
5. ✅ Integrate learnings into your application

## Advanced Options

### Export Formats
```bash
# Generate multiple formats
dotnet run -c Release -- --exporters html,json,csv,markdown
```

### Different Runtimes
```bash
# Compare across .NET versions
dotnet run -c Release -- --runtimes net6.0 net7.0 net8.0
```

### Profiling
```bash
# Profile with ETW (Windows)
dotnet run -c Release -- --profiler ETW

# Profile with EventPipe (Cross-platform)
dotnet run -c Release -- --profiler EP
```

### Quick Run (Less Accurate)
```bash
# Faster but less accurate
dotnet run -c Release -- --job short
```

## Resources

- Full README: `README.md`
- CSLY Optimizations: `../../OPTIMIZATIONS.md`
- Advanced Optimizations: `../../ADVANCED_OPTIMIZATIONS.md`
- BenchmarkDotNet Docs: https://benchmarkdotnet.org/

## Support

For issues or questions:
1. Check documentation in this folder
2. Review CSLY project documentation
3. Open an issue on GitHub

---

**Happy Benchmarking! 🚀**

