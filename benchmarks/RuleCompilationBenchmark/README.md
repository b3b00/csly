# Rule Compilation Benchmark

This benchmark project demonstrates the performance improvements gained by compiling parsing rules in CSLY using Expression Trees.

## Overview

The benchmark compares parsing performance with and without rule compilation across different complexity levels:

- **Simple expressions**: `1 + 2`, `3 * 4`
- **Medium expressions**: `1 + 2 * 3 - 4 / 2`
- **Complex expressions**: `((1 + 2) * (3 + 4)) / ((5 - 6) + (7 * 8))`

## Running the Benchmark

### Prerequisites

- .NET 8.0 SDK or later
- BenchmarkDotNet package (installed automatically)

### Build and Run

```bash
cd benchmarks/RuleCompilationBenchmark
dotnet build -c Release
dotnet run -c Release
```

**Important**: Always run benchmarks in Release mode for accurate results.

### Command Line Options

BenchmarkDotNet supports various command-line options:

```bash
# Run specific benchmark
dotnet run -c Release -- --filter *Simple*

# Run with specific job
dotnet run -c Release -- --job short

# Export results
dotnet run -c Release -- --exporters json,html
```

## Metrics Measured

### Time Metrics
- **Mean**: Average execution time
- **Median**: Middle value of execution times
- **StdDev**: Standard deviation (consistency)
- **Min/Max**: Fastest and slowest execution

### Memory Metrics
- **Allocated**: Total memory allocated
- **Gen0/Gen1/Gen2**: Garbage collection counts

### Rank
- Relative performance ranking (1 = fastest)

## Expected Results

Based on the optimization implementation, you should see:

| Scenario | Expected Improvement |
|----------|---------------------|
| **Simple expressions** | 2-3x faster |
| **Medium expressions** | 1.5-2x faster |
| **Complex expressions** | 1.3-1.8x faster |
| **Repeated parses** | 2-3x faster |

### Memory Improvements
- **Allocations**: 20-30% reduction
- **GC pressure**: 30-40% reduction

## Interpreting Results

### Example Output

```
|                          Method |      Mean |     Error |    StdDev |    Median | Ratio | Rank |    Gen0 | Allocated |
|-------------------------------- |----------:|----------:|----------:|----------:|------:|-----:|--------:|----------:|
| Simple expressions WITH comp... |  45.23 μs |  0.892 μs |  0.835 μs |  45.15 μs |  0.33 |    1 |  1.2207 |   5.21 KB |
| Simple expressions WITHOUT c... | 136.47 μs |  2.703 μs |  3.011 μs | 135.88 μs |  1.00 |    2 |  3.6621 |   7.42 KB |
```

**Key observations**:
- **Ratio < 1.0**: Faster than baseline
- **Lower Gen0**: Fewer garbage collections
- **Lower Allocated**: Less memory used

## Benchmark Scenarios Explained

### 1. Simple Expressions
Tests basic terminal rule matching and simple operations. Shows maximum benefit of compilation.

### 2. Medium Expressions
Tests operator precedence and mixed operations. Shows balanced improvement.

### 3. Complex Expressions
Tests nested parentheses and complex trees. Shows compilation benefits in deep recursion.

### 4. Single Parse
Tests one-time parsing cost including compilation overhead.

### 5. Repeated Parse (1000x)
Tests amortization of compilation cost over many parses. Best demonstrates real-world benefits.

## Understanding Compilation Benefits

### What Gets Compiled

The `RuleCompiler` optimizes:
1. **Simple terminal rules**: Direct token matching without interpretation
2. **Simple sequences**: Pre-calculated clause information
3. **Pre-bound delegates**: Cached rule information for faster access

### What Doesn't Get Compiled

Complex rules with:
- Choices (alternations)
- Repetitions (ZeroOrMore, OneOrMore)
- Deep recursion

These fall back to interpretation but still benefit from cached information.

## Troubleshooting

### Benchmark Hangs
- Check parser configuration is correct
- Verify grammar has no infinite loops
- Ensure test data is valid

### Unexpected Results
- Run in Release mode (not Debug)
- Close other applications
- Run multiple times for consistency
- Check for thermal throttling

### Compilation Errors
If compilation fails, the parser automatically falls back to interpretation. Check:
- Rule complexity
- Grammar structure
- Parser configuration

## Advanced Usage

### Custom Scenarios

Add your own benchmark scenarios by:

1. Create test data in `Setup()`
2. Add benchmark method with `[Benchmark]` attribute
3. Parse your custom expressions

Example:
```csharp
[Benchmark(Description = "My custom scenario")]
public void MyScenario_WithCompilation()
{
    var tokens = CreateTokens("your expression");
    var result = _parserWithCompilation.Parse(tokens);
}
```

### Performance Profiling

For deeper analysis:

```bash
# Run with profiler
dotnet run -c Release -- --profiler ETW

# Use specific runtime
dotnet run -c Release -- --runtimes net6.0 net7.0 net8.0
```

## Results Interpretation Guide

### Time Improvements

- **> 2x faster**: Excellent - simple rules heavily optimized
- **1.5-2x faster**: Good - mixed rule types
- **1.2-1.5x faster**: Moderate - complex rules
- **< 1.2x faster**: Minimal - may not justify compilation overhead

### Memory Improvements

- **> 30% reduction**: Excellent
- **20-30% reduction**: Good
- **10-20% reduction**: Moderate
- **< 10% reduction**: Minimal

### When to Enable Compilation

✅ **Enable if**:
- Parsing same grammar repeatedly
- Simple to moderate rule complexity
- Performance is critical
- Memory pressure is high

❌ **Don't enable if**:
- One-time parsing only
- Extremely complex grammar
- Dynamic rule generation
- Memory is abundant and performance adequate

## CI/CD Integration

To run benchmarks in CI/CD:

```bash
# Quick benchmark (less accurate but faster)
dotnet run -c Release -- --job short --filter *Simple*

# Export results for reporting
dotnet run -c Release -- --exporters json --artifacts ./benchmark-results
```

## Contributing

To add new benchmark scenarios:
1. Follow existing naming conventions
2. Add descriptive comments
3. Update this README with new scenarios
4. Ensure benchmarks complete in reasonable time

## References

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [CSLY Optimizations Guide](../../OPTIMIZATIONS.md)
- [Advanced Optimizations](../../ADVANCED_OPTIMIZATIONS.md)

## License

Same as CSLY project license.

